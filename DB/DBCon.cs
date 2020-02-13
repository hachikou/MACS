/// DBCon: DB接続を取り扱うクラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Data;
using System.Data.Common;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
#if USE_MYSQL
using MySql.Data.MySqlClient;
#endif
#if USE_ORACLE
using System.Data.OracleClient;
#endif
#if USE_POSTGRESQL
using Npgsql;
#endif
using MACS;

namespace MACS.DB {

/// <summary>
///   DB接続を取り扱うクラス。
///   ADO.NETのDbConnectをもっと簡便に扱えるようにラッピングしたもの。
/// </summary>
public class DBCon : Loggable, IDisposable {

    /// <summary>
    ///   DBの種類
    /// </summary>
    public enum Type {
        Invalid = 0,
        MySQL,
        Oracle,
        PostgreSQL,
    }

    /// <summary>
    ///   初期データベースを指定する時の特別な名前
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     CREATE DATABASEをするためにDBConを使う場合などに、接続データベース名
    ///     として使用してください。
    ///   </para>
    /// </remarks>
    public const string DEFAULT_DATABASE = "__DEFAULT_DATABASE__";

    /// <summary>
    ///   デフォルトのタイムアウト時間
    /// </summary>
    public const int DefaultCommandTimeout = 180;

    /// <summary>
    ///   Loggerにデバッグログを残すかどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     エラーログは本設定にかかわらず記録されます。
    ///   </para>
    /// </remarks>
    public bool UseDebugLog = DefaultUseDebugLog;

    /// <summary>
    ///   コンソールにデバッグログを出すかどうか
    /// </summary>
    public bool UseConsoleLog = DefaultUseConsoleLog;

    /// <summary>
    ///   Loggerにデバッグログを残すかどうか（デフォルト値）
    /// </summary>
    public static bool DefaultUseDebugLog = false;

    /// <summary>
    ///   コンソールにデバッグログを出すかどうか（デフォルト値）
    /// </summary>
    public static bool DefaultUseConsoleLog = false;

    /// <summary>
    ///   SQL実行時間が長すぎる警告ログを記録する閾値（ミリ秒）
    /// </summary>
    public int WarnThresholdTime = DefaultWarnThresholdTime;
    
    /// <summary>
    ///   SQL実行時間が長すぎる警告ログを記録する閾値（ミリ秒）（デフォルト値）
    /// </summary>
    public static int DefaultWarnThresholdTime = 10000;
    
    
    /// <summary>
    ///   DB種別を文字列からパースする
    /// </summary>
    public static Type ToType(string str) {
        if(String.IsNullOrEmpty(str)) {
#if USE_MYSQL
            return Type.MySQL;
#else
#if USE_ORACLE
            return Type.Oracle;
#else
#if USE_POSTGRESQL
            return Type.PostgreSQL;
#else
            return Type.Invalid;
#endif
#endif
#endif
        }
        switch(str.ToUpper()) {
#if USE_MYSQL
        case "MYSQL":
            return Type.MySQL;
#endif
#if USE_ORACLE
        case "ORACLE":
            return Type.Oracle;
#endif
#if USE_POSTGRESQL
        case "POSTGRESQL":
            return Type.PostgreSQL;
#endif
        default:
            return Type.Invalid;
        }
    }

    /// <summary>
    ///   DBと接続する。
    /// </summary>
    public DBCon(Type dbtype_, string server, string dbname_, string user, string passwd) {
        _open(dbtype_, server, dbname_, user, passwd);
    }

    /// <summary>
    ///   INIファイルの設定に従ってDBと接続する
    /// </summary>
    public DBCon(IniFile inifile, string prefix="db") {
        _open(inifile, prefix);
    }

    /// <summary>
    ///   DBConPoolの接続を使ってDBConを作成する
    /// </summary>
    public DBCon(DBConPool pool_) {
        if(pool_.MaxPoolSize == 0) {
            _open(pool_.DBType, pool_.Server, pool_.DBName, pool_.User, pool_.Passwd);
            return;
        }
        pool = pool_;
        dbtype = pool.DBType;
        dbname = pool.DBName;
        con = pool.GetDbConnection();
        fetchConnectionId();
    }

    /// <summary>
    ///   DB接続を持たないダミーのDBConを作成する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     このDBConに対するQueryは常に空リストを返し、Executeは何もしない。
    ///   </para>
    /// </remarks>
    public DBCon() {
        dbtype = Type.Invalid;
        dbname = "*INVALID*";
        con = null;
    }


    private void _open(Type dbtype_, string server, string dbname_, string user, string passwd) {
        dbtype = dbtype_;
        dbname = dbname_;
        if(String.IsNullOrEmpty(dbname))
            throw new ArgumentException("No Database name");
        if(dbname == DEFAULT_DATABASE) {
            switch(dbtype) {
#if USE_MYSQL
            case Type.MySQL:
                dbname = "mysql";
                break;
#endif
#if USE_POSTGRESQL
            case Type.PostgreSQL:
                dbname = "postgres";
                break;
#endif
            default:
                dbname = "";
                break;
            }
        }
        StringBuilder constr = new StringBuilder();
        switch(dbtype) {
#if USE_MYSQL
        case Type.MySQL:
            constr.Append("Server=");
            constr.Append(server);
            constr.Append(";Database=");
            constr.Append(dbname);
            constr.Append(";Uid=");
            constr.Append(user);
            constr.Append(";Pwd=");
            constr.Append(passwd);
            constr.Append(";CharSet=utf8");
            break;
#endif
#if USE_ORACLE
        case Type.Oracle:
            constr.Append("User ID=");
            constr.Append(user);
            constr.Append(";Password=");
            constr.Append(passwd);
            constr.Append(";Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=");
            constr.Append(server);
            constr.Append(")(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=");
            constr.Append(dbname);
            constr.Append(")))");
            break;
#endif
#if USE_POSTGRESQL
        case Type.PostgreSQL:
            constr.Append("Server=");
            constr.Append(server);
            constr.Append(";Port=5432;User Id=");
            constr.Append(user);
            constr.Append(";Password=");
            constr.Append(passwd);
            constr.Append(";Pooling=False");
            constr.Append(";Encoding=UNICODE");
            constr.Append(";Database=");
            constr.Append(dbname);
            break;
#endif
        default:
            // go through.
            break;
        }

        //log("Open "+constr.ToString());
        switch(dbtype) {
#if USE_MYSQL
        case Type.MySQL:
            con = new MySqlConnection(constr.ToString());
            break;
#endif
#if USE_ORACLE
        case Type.Oracle:
            con = new OracleConnection(constr.ToString());
            break;
#endif
#if USE_POSTGRESQL
        case Type.PostgreSQL:
            con = new NpgsqlConnection(constr.ToString());
            break;
#endif
        default:
            throw new ArgumentException("Unsupported DB type");
        }
        con.Open();
        fetchConnectionId();
    }

    private void _open(IniFile inifile, string prefix) {
        _open(ToType(inifile.Get(prefix+"engine","")),
              inifile.Get(prefix+"server", "localhost"),
              inifile.Get(prefix+"name", "dbname"),
              inifile.Get(prefix+"user", "username"),
              inifile.Get(prefix+"pass", "password"));
        commandtimeout = inifile.Get(prefix+"timeout", DefaultCommandTimeout);
        ignoreexception = inifile.Get(prefix+"ignore_exception", false);
    }

    private void fetchConnectionId() {
        lock (connectionIdMutex) {
            connectionId++;
            if(connectionId >= 100000)
                connectionId = 10000;
            cid = connectionId;
        }
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~DBCon() {
        Dispose();
    }

    /// <summary>
    ///   使用リソースを解放する。
    /// </summary>
    public void Dispose() {
        try {
            Close();
        } catch(Exception) {
            // just ignore
        }
    }

    /// <summary>
    ///   DB接続を切断する。
    /// </summary>
    public void Close() {
        if(pool != null) {
            if(con != null) {
                lock(con) {
                    _close();
                }
            }
            pool.Free(con);
            con = null;
            return;
        }
        if(con == null)
            return;
        lock(con) {
            _close();
            con.Close();
            con = null;
            //log("Closed.");
        }
    }

    private void _close() {
        if(reader != null) {
            reader.Close();
            reader = null;
        }
        if(inTransaction)
            _rollback();
    }

    /// <summary>
    ///   接続DB名
    /// </summary>
    public string Name {
        get { return dbname; }
    }

    /// <summary>
    ///   DB種別
    /// </summary>
    public Type DBType {
        get { return dbtype; }
    }

    /// <summary>
    ///   SQL実行タイムアウト（秒）
    /// </summary>
    public int CommandTimeout {
        get { return commandtimeout; }
        set { commandtimeout = value; }
    }

    /// <summary>
    ///   SQL実行時の Exception を握りつぶすかどうか
    /// </summary>
    public bool IgnoreException {
        get { return ignoreexception; }
        set { ignoreexception = value; }
    }
    
    /// <summary>
    ///   DBと接続できているかどうか
    /// </summary>
    public bool Connected {
        get { return (con != null); }
    }

    /// <summary>
    ///   DBがMySQLかどうか
    /// </summary>
    public bool IsMySQL {
        get {
#if USE_MYSQL
            return (dbtype == Type.MySQL);
#else
            return false;
#endif
        }
    }

    /// <summary>
    ///   DBがORACLEかどうか
    /// </summary>
    public bool IsOracle {
        get {
#if USE_ORACLE
            return (dbtype == Type.Oracle);
#else
            return false;
#endif
        }
    }

    /// <summary>
    ///   DBがPostgreSQLかどうか
    /// </summary>
    public bool IsPostgreSQL {
        get {
#if USE_POSTGRESQL
            return (dbtype == Type.PostgreSQL);
#else
            return false;
#endif
        }
    }

    /// <summary>
    ///   現在実行中のSQL
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     現在実行中でない場合はnullを返す。
    ///   </para>
    /// </remarks>
    public string CurrentSQL {
        get { return currentSQL; }
    }
    
    /// <summary>
    ///   Query発行
    /// </summary>
    public DBReader Query(string sql) {
        if(con == null)
            return new DBReader(); // 何を読み取ってもNULLになるダミーリーダ
        lock(con) {
            log(sql);
            if(reader != null) {
                reader.Close();
                reader = null;
            }
            using(DbCommand cmd = con.CreateCommand()) {
                if(commandtimeout > 0)
                    cmd.CommandTimeout = commandtimeout;
                cmd.CommandText = sql;
                try {
                    currentSQL = sql;
                    timer.Restart();
                    reader = new DBReader(cmd.ExecuteReader());
                    timer.Stop();
                    if(timer.ElapsedMilliseconds >= WarnThresholdTime)
                        logWarning(String.Format("Heavy SQL: {0}msec: SQL={1}", timer.ElapsedMilliseconds, sql));
                    return reader;
                } catch(DbException e) {
                    logErr(String.Format("SQL Error: {0}: SQL={1}", e.Message, sql));
                    if(ignoreexception)
                        return new DBReader(); // 握りつぶすときはダミーリーダー
                    else
                        throw;
                } catch(Exception e) {
                    logErr(String.Format("{0}: {1}: SQL={2}", e.GetType().Name, e.Message, sql));
                    abort();
                    if(ignoreexception)
                        return new DBReader(); // 握りつぶすときはダミーリーダー
                    else
                        throw;
                } finally {
                    currentSQL = null;
                }
            }
        }
    }

    /// <summary>
    ///   Query発行
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     引数sqlはQuery実行後クリアされる。
    ///   </para>
    /// </remarks>
    public DBReader Query(StringBuilder sql) {
        try {
            return Query(sql.ToString());
        } finally {
            sql.Clear();
        }
    }

    /// <summary>
    ///   単一結果を返すQuery実行
    /// </summary>
    public string QueryString(string sql) {
        if(con == null)
            return "";
        lock(con) {
            log(sql);
            if(reader != null) {
                reader.Close();
                reader = null;
            }
            using(DbCommand cmd = con.CreateCommand()) {
                if(commandtimeout > 0)
                    cmd.CommandTimeout = commandtimeout;
                cmd.CommandText = sql;
                try {
                    currentSQL = sql;
                    return cmd.ExecuteScalar().ToString();
                } catch(DbException e) {
                    logErr(String.Format("SQL Error: {0}: SQL={1}", e.Message, sql));
                    if(ignoreexception)
                        return "";
                    else
                        throw;
                } catch(Exception e) {
                    logErr(String.Format("{0}: {1}: SQL={2}", e.GetType().Name, e.Message, sql));
                    abort();
                    if(ignoreexception)
                        return "";
                    else
                        throw;
                } finally {
                    currentSQL = null;
                }
            }
        }
    }

    /// <summary>
    ///   単一結果を返すQuery実行
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     引数sqlはQuery実行後クリアされる。
    ///   </para>
    /// </remarks>
    public string QueryString(StringBuilder sql) {
        try {
            return QueryString(sql.ToString());
        } finally {
            sql.Clear();
        }
    }

    /// <summary>
    ///   単一結果を返すQuery実行（整数版）
    /// </summary>
    public int QueryInt(string sql) {
        if(con == null)
            return 0;
        lock(con) {
            log(sql);
            if(reader != null) {
                reader.Close();
                reader = null;
            }
            using(DbCommand cmd = con.CreateCommand()) {
                if(commandtimeout > 0)
                    cmd.CommandTimeout = commandtimeout;
                cmd.CommandText = sql;
                try {
                    currentSQL = sql;
                    return StringUtil.ToInt(cmd.ExecuteScalar().ToString(), 0);
                } catch(DbException e) {
                    logErr(String.Format("SQL Error: {0}: SQL={1}", e.Message, sql));
                    if(ignoreexception)
                        return 0;
                    else
                        throw;
                } catch(Exception e) {
                    logErr(String.Format("{0}: {1}: SQL={2}", e.GetType().Name, e.Message, sql));
                    abort();
                    if(ignoreexception)
                        return 0;
                    else
                        throw;
                } finally {
                    currentSQL = null;
                }
            }
        }
    }

    /// <summary>
    ///   単一結果を返すQuery実行（整数版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     引数sqlはQuery実行後クリアされる。
    ///   </para>
    /// </remarks>
    public int QueryInt(StringBuilder sql) {
        try {
            return QueryInt(sql.ToString());
        } finally {
            sql.Clear();
        }
    }

    /// <summary>
    ///   単一結果を返すQuery実行（DateTime版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     検索結果、該当行が存在しないときやNULLのときはDateTime(0)を返す。
    ///   </para>
    /// </remarks>
    public DateTime QueryDateTime(string sql) {
        DBReader rd = Query(sql);
        if(!rd.Read())
            return new DateTime(0);
        return rd.GetDateTime(0);
    }

    /// <summary>
    ///   単一結果を返すQuery実行（DateTime）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     引数sqlはQuery実行後クリアされる。
    ///   </para>
    /// </remarks>
    public DateTime QueryDateTime(StringBuilder sql) {
        try {
            return QueryDateTime(sql.ToString());
        } finally {
            sql.Clear();
        }
    }

    /// <summary>
    ///   SQL実行
    /// </summary>
    public int Execute(string sql) {
        if(con == null)
            return 0;
        lock(con) {
            return _execute(sql);
        }
    }

    private int _execute(string sql) {
        log(sql);
        using(DbCommand cmd = con.CreateCommand()) {
            if(commandtimeout > 0)
                cmd.CommandTimeout = commandtimeout;
            cmd.CommandText = sql;
            try {
                currentSQL = sql;
                timer.Restart();
                int ret = cmd.ExecuteNonQuery();
                timer.Stop();
                if(timer.ElapsedMilliseconds >= WarnThresholdTime)
                    logWarning(String.Format("Heavy SQL: {0}msec: SQL={1}", timer.ElapsedMilliseconds, sql));
                return ret;
            } catch(DbException e) {
                logErr(String.Format("SQL Error: {0}: SQL={1}", e.Message, sql));
                if(ignoreexception)
                    return -1; // 握りつぶすときは -1
                else
                    throw;
            } catch(Exception e) {
                logErr(String.Format("{0}: {1}: SQL={2}", e.GetType().Name, e.Message, sql));
                abort();
                if(ignoreexception)
                    return -1; // 握りつぶすときは -1
                else
                    throw;
            } finally {
                currentSQL = null;
            }
        }
    }

    /// <summary>
    ///   SQL実行
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     引数sqlはQuery実行後クリアされる。
    ///   </para>
    /// </remarks>
    public int Execute(StringBuilder sql) {
        try {
            return Execute(sql.ToString());
        } finally {
            sql.Clear();
        }
    }

    /// <summary>
    ///   トランザクション開始
    /// </summary>
    public void Begin() {
        if(con == null) {
            if(ignoreexception) {
                logErr("Failed to start transaction (DB connection is lost)");
                return;
            }
            throw new DBConException("Failed to start transaction (DB connection is lost)");
        }
        lock(con) {
            if(inTransaction)
                return;
            _execute("START TRANSACTION");
            inTransaction = true;
        }
    }

    /// <summary>
    ///   トランザクションコミット
    /// </summary>
    public void Commit() {
        if(con == null) {
            if(ignoreexception) {
                logErr("DB connection has been lost while commiting");
                return;
            }
            throw new DBConException("DB connection has been lost while commiting");
        }
        lock(con) {
            if(reader != null) {
                reader.Close();
                reader = null;
            }
            if(!inTransaction)
                return;
            _execute("COMMIT");
            inTransaction = false;
        }
    }

    /// <summary>
    ///   トランザクションロールバック
    /// </summary>
    public void Rollback() {
        if(con == null)
            return;
        lock(con) {
            _rollback();
        }
    }

    private void _rollback() {
        if(reader != null) {
            reader.Close();
            reader = null;
        }
        if(!inTransaction)
            return;
        _execute("ROLLBACK");
        inTransaction = false;
    }

    /// <summary>
    ///   SQLの文字列定数を生成する
    /// </summary>
    public static string Literal(string txt) {
        if(txt == null)
            return "NULL";
        return "'"+Escape(txt)+"'";
    }

    /// <summary>
    ///   SQLの文字列定数として使ってはいけない文字をエスケープする
    /// </summary>
    public static string Escape(string txt) {
        if(txt.Length > 0) {
            txt = txt.Replace("'", "''");
            // 末尾の連続する'\'を処理する
            int bcount = 0;
            while((txt.Length > bcount) && (txt[txt.Length-bcount-1] == '\\'))
                bcount++;
            if(bcount%2 != 0)
                txt += "\\";
        }
        return txt;
    }

    /// <summary>
    ///   SQLのLIKE指定文字列として使ってはいけない文字をエスケープする
    /// </summary>
    public static string LikeEscape(string txt) {
        return txt.Replace("'", "''").Replace("\\", "\\\\").Replace("%", "\\%").Replace("_","\\_");
    }

    /// <summary>
    ///   SQLの日付指定文字列を得る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DateTime(0)は'0001-01-01'になる
    ///   </para>
    /// </remarks>
    public static string DateString(DateTime dt) {
        if(dt.Ticks == 0)
            return "0001-01-01";
        return dt.ToString("yyyy-MM-dd");
    }

    /// <summary>
    ///   SQLの日時指定文字列を得る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DateTime(0)は'0001-01-01 00:00:00'になる
    ///   </para>
    /// </remarks>
    public static string DateTimeString(DateTime dt) {
        if(dt.Ticks == 0)
            return "0001-01-01 00:00:00";
        return dt.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    ///   誕生日から年齢を算出するカラム定義文字列を得る
    /// </summary>
    /// <param name="columnName">年齢カラムに付ける名前</param>
    /// <param name="birthdayColumnName">誕生日のカラム名</param>
    /// <param name="baseDate">基準日</param>
    /// <param name="invalidValue">誕生日が不明な時の値</param>
    /// <remarks>
    ///   <para>
    ///     誕生日が1000年より前だったり、基準日より後だったりするときは、invalidValueを返します。
    ///   </para>
    /// </remarks>
    public string AgeColumn(string columnName, string birthdayColumnName, DateTime baseDate, int invalidValue=0) {
        return String.Format("CASE WHEN `{0}` IS NULL THEN {6} WHEN YEAR(`{0}`)<1000 THEN {6} WHEN `{0}`>'{1:D4}-{2:D2}-{3:D2}' THEN {6} ELSE TRUNCATE(({4}-YEAR(`{0}`)*10000-MONTH(`{0}`)*100-DAYOFMONTH(`{0}`))/10000,0) END AS {5}",
                             birthdayColumnName, baseDate.Year, baseDate.Month, baseDate.Day, baseDate.Year*10000+baseDate.Month*100+baseDate.Day, columnName, invalidValue);
    }

    /// <summary>
    ///   誕生日から年齢を算出するカラム定義文字列を得る
    /// </summary>
    /// <param name="columnName">年齢カラムに付ける名前</param>
    /// <param name="birthdayColumnName">誕生日のカラム名</param>
    /// <param name="invalidValue">誕生日が不明な時の値</param>
    /// <remarks>
    ///   <para>
    ///     誕生日が1000年より前だったり、基準日より後だったりするときは、invalidValueを返します。
    ///   </para>
    /// </remarks>
    public string AgeColumn(string columnName, string birthdayColumnName, int invalidValue=0) {
        return AgeColumn(columnName, birthdayColumnName, DateTime.Today, invalidValue);
    }

    /// <summary>
    ///   誕生日から学年年齢を算出するカラム定義文字列を得る
    /// </summary>
    /// <param name="columnName">年齢カラムに付ける名前</param>
    /// <param name="birthdayColumnName">誕生日のカラム名</param>
    /// <param name="baseDate">基準日</param>
    /// <param name="invalidValue">誕生日が不明な時の値</param>
    /// <remarks>
    ///   <para>
    ///     学年年齢とは、基準日が1月～3月の時はその前年の4月1日時点の年齢を、
    ///     4月～12月の時はその年の4月1日時点の年齢を指します。
    ///   </para>
    /// </remarks>
    public string SchoolAgeColumn(string columnName, string birthdayColumnName, DateTime baseDate, int invalidValue=0) {
        return AgeColumn(columnName, birthdayColumnName, new DateTime((baseDate.Month<4)?(baseDate.Year-1):baseDate.Year, 4, 1), invalidValue);
    }
    
    /// <summary>
    ///   誕生日から学年年齢を算出するカラム定義文字列を得る
    /// </summary>
    /// <param name="columnName">年齢カラムに付ける名前</param>
    /// <param name="birthdayColumnName">誕生日のカラム名</param>
    /// <param name="invalidValue">誕生日が不明な時の値</param>
    /// <remarks>
    ///   <para>
    ///     学年年齢とは、本日が1月～3月の時は前年の4月1日時点の年齢を、
    ///     4月～12月の時は本年の4月1日時点の年齢を指します。
    ///   </para>
    /// </remarks>
    public string SchoolAgeColumn(string columnName, string birthdayColumnName, int invalidValue=0) {
        return SchoolAgeColumn(columnName, birthdayColumnName, DateTime.Today, invalidValue);
    }

    /// <summary>
    ///   SQLを実行して実行結果をテキストストリームにHumanReadableな形で書き出す
    /// </summary>
    public void Execute(string sql, TextWriter w) {
        try {
            if(sql.StartsWith("select", StringComparison.InvariantCultureIgnoreCase)) {
                List<string[]> values = new List<string[]>();
                string[] columns;
                int[] fieldLen;
                using(DBReader reader = Query(sql)) {
                    columns = reader.GetColumns();
                    fieldLen = new int[columns.Length];
                    for(int i = 0; i < fieldLen.Length; i++){
                        fieldLen[i] = SJISDictionary.GetByteCount(columns[i]);
                    }
                    string[] value;
                    while((value = reader.Get()) != null) {
                        for(int i = 0; i < fieldLen.Length; i++) {
                            if(value[i] == null)
                                value[i] = "NULL";
                            int len = SJISDictionary.GetByteCount(value[i]);
                            if(fieldLen[i] < len)
                                fieldLen[i] = len;
                        }
                        values.Add(value);
                    }
                }
                for(int i = 0; i < fieldLen.Length; i++) {
                    w.Write('|');
                    string v = columns[i];
                    w.Write(' ');
                    w.Write(v);
                    for(int j = 0; j < fieldLen[i]-SJISDictionary.GetByteCount(v); j++)
                        w.Write(' ');
                    w.Write(' ');
                }
                w.WriteLine('|');
                for(int i = 0; i < fieldLen.Length; i++) {
                    w.Write('|');
                    for(int j = 0; j < fieldLen[i]+2; j++)
                        w.Write('-');
                }
                w.WriteLine('|');
                foreach(string[] val in values) {
                    for(int i = 0; i < fieldLen.Length; i++) {
                        w.Write('|');
                        string v = val[i];
                        w.Write(' ');
                        w.Write(v);
                        for(int j = 0; j < fieldLen[i]-SJISDictionary.GetByteCount(v); j++)
                            w.Write(' ');
                        w.Write(' ');
                    }
                    w.WriteLine('|');
                }
                w.WriteLine("Total {0} records.", values.Count);
            } else {
                int res = Execute(sql);
                w.WriteLine("{0} records affected.", res);
            }
        } catch(DbException ex) {
            w.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    ///   データベース名に使うことができる文字
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     PostgreSQLのコネクターが英大文字と小文字の区別を誤ることがあるため
    ///     英大文字を禁止しています。
    ///   </para>
    /// </remarks>
    public static readonly string ValidDatabaseNameChars = "0123456789abcdefghijklmnopqrstuvwxyz_";

    /// <summary>
    ///   データベース名として有効かどうか調べる
    /// </summary>
    public static bool IsValidDatabaseName(string newname) {
        if(String.IsNullOrEmpty(newname))
            return false;
        foreach(char ch in newname) {
            if(ValidDatabaseNameChars.IndexOf(ch) < 0)
                return false;
        }
        // 数字で始まる名前は使えない
        if((newname[0] >= '0') && (newname[0] <= '9'))
            return false;
        return true;
    }

    /// <summary>
    ///   データベースを新たに作る
    /// </summary>
    public void CreateDatabase(string newname, string user, string password) {
        switch(dbtype) {
#if USE_MYSQL
        case Type.MySQL:
            Execute("CREATE DATABASE "+newname+" CHARACTER SET utf8");
            Execute("GRANT ALL PRIVILEGES ON "+newname+".* TO "+user+"@localhost IDENTIFIED BY '"+password+"'");
            Execute("GRANT ALL PRIVILEGES ON "+newname+".* TO "+user+"@'%' IDENTIFIED BY '"+password+"'");
            break;
#endif
#if USE_POSTGRESQL
        case Type.PostgreSQL:
            try {
                Execute("DROP ROLE "+user);
            } catch(DbException) {
                // just ignore.
            }
            try {
                Execute("CREATE ROLE "+user+" WITH LOGIN PASSWORD '"+password+"'");
            } catch(DbException) {
                // just ignore.
            }
            Execute("CREATE DATABASE "+newname+" OWNER="+user+" ENCODING='utf8'");
            break;
#endif
        default:
            throw new InvalidOperationException("Invalid DB type or connection has been lost");
        }
    }

    /// <summary>
    ///   データベースのサイズを得る
    /// </summary>
    public long GetSize() {
        switch(dbtype) {
#if USE_MYSQL
        case Type.MySQL:
            return StringUtil.ToLong(QueryString("SELECT SUM(data_length+index_length) FROM information_schema.tables WHERE table_schema = '"+dbname+"'"));
#endif
#if USE_POSTGRESQL
        case Type.PostgreSQL:
            return StringUtil.ToLong(QueryString("SELECT pg_database_size('"+dbname+"')"));
#endif
        default:
            throw new InvalidOperationException("Invalid DB type or connection has been lost");
        }
    }

    /// <summary>
    ///   テーブルの複製を作成する
    /// </summary>
    /// <param name="src">コピー元テーブル名</param>
    /// <param name="dst">コピー先テーブル名</param>
    public void CopyTable(string src, string dst) {
        switch(dbtype) {
#if USE_MYSQL
        case Type.MySQL:
            Execute("CREATE TABLE "+dst+" (SELECT * FROM "+src+")");
            break;
#endif
#if USE_POSTGRESQL
        case Type.PostgreSQL:
            Execute("CREATE TABLE "+dst+" AS SELECT * FROM "+src);
            break;
#endif
        default:
            throw new InvalidOperationException("Invalid DB type or connection has been lost");
        }
    }

    /// <summary>
    ///   指定テーブルのカラム名一覧を得る
    /// </summary>
    public string[] GetColumnNameList(string tblname) {
        string query = null;
        switch(dbtype) {
#if USE_MYSQL
        case Type.MySQL:
            query = String.Format("SELECT column_name FROM information_schema.columns WHERE table_schema='{0}' AND table_name='{1}'", dbname, tblname);
            break;
#endif
#if USE_POSTGRESQL
        case Type.PostgreSQL:
            query = String.Format("SELECT column_name FROM information_schema.columns WHERE table_name='{0}'", tblname);
            break;
#endif
        default:
            throw new InvalidOperationException("Invalid DB type or connection has been lost");
        }
        List<string> cols = new List<string>();
        using(DBReader reader = Query(query)) {
            string[] item;
            while((item = reader.Get()) != null) {
                cols.Add(item[0]);
            }
        }
        return cols.ToArray();
    }

    /// <summary>
    ///   データベースの初期ユーザ名を得る
    /// </summary>
    public static string GetDefaultSuperUser(Type dbtype) {
        switch(dbtype) {
        case Type.MySQL:
            return "root";
        case Type.PostgreSQL:
            return "postgres";
        default:
            return "root";
        }
    }

    /// <summary>
    ///   データベースサーバのバージョン名
    /// </summary>
    public string Version {
        get {
            if(versionString != null)
                return versionString;
            getVersion();
            return versionString;
        }
    }

    /// <summary>
    ///   データベースバージョン番号
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     メジャー番号*10000+マイナー番号*100+サブマイナー番号
    ///   </para>
    /// </remarks>
    public int VersionNumber {
        get {
            if(versionNumber >= 0)
                return versionNumber;
            getVersion();
            return versionNumber;
        }
    }

    
    private Type dbtype;
    private string dbname;
    private DbConnection con;
    private DBReader reader = null;
    private volatile string currentSQL = null;
    private Stopwatch timer = new Stopwatch();
    private bool inTransaction = false;
    private int commandtimeout = 60;
    private bool ignoreexception = false;
    private static int connectionId = 10000;
    private static object connectionIdMutex = new object();
    private int cid;
    private DBConPool pool = null;
    private string versionString = null;
    private int versionNumber = -1;

    
    private void abort() {
        try {
            if(con != null) {
                _close();
                con.Close();
                con = null;
            }
        } catch(Exception) {
            // just ignore.
        }
    }
    
    private void log(string msg) {
        if(UseDebugLog)
            LOG_DEBUG(String.Format("[{0}] {1}", cid, msg));
        if(UseConsoleLog)
            Console.WriteLine("{0}: {1}", dbname, msg);
    }

    private void logWarning(string msg) {
        LOG_WARNING(String.Format("[{0}] {1}", cid, msg));
        if (UseConsoleLog)
            Console.WriteLine("{0}: {1}", dbname, msg);
    }

    private void logErr(string msg) {
        LOG_ERR(String.Format("[{0}] {1}", cid, msg));
        if (UseConsoleLog)
            Console.WriteLine("{0}: {1}", dbname, msg);
    }

    private Regex pat_version3 = new Regex(@"(\d+)\.(\d+)\.(\d+)");
    private Regex pat_version2 = new Regex(@"(\d+)\.(\d+)");
    
    private void getVersion() {
        Match mo;
        switch(dbtype) {
        case Type.MySQL:
        case Type.PostgreSQL:
            string r = QueryString("SELECT VERSION()");
            mo = pat_version3.Match(r);
            if(!mo.Success) {
                mo = pat_version2.Match(r);
                if(!mo.Success) {
                    versionString = "";
                    versionNumber = 0;
                }
            }
            versionString = mo.Groups[0].Value;
            if(mo.Groups.Count == 3)
                versionNumber = StringUtil.ToInt(mo.Groups[1].Value)*10000+StringUtil.ToInt(mo.Groups[2].Value)*100+StringUtil.ToInt(mo.Groups[3].Value);
            else
                versionNumber = StringUtil.ToInt(mo.Groups[1].Value)*10000+StringUtil.ToInt(mo.Groups[2].Value)*100;
            break;
        default:
            versionString = "";
            versionNumber = 0;
            break;
        }
    }
    
    protected override string GetCategoryName() {
        return dbname;
    }

    internal DbConnection DBConnection {
        get { return con; }
    }

}

/// <summary>
///   DBCon内で発生した異常事象用の例外
/// </summary>
/// <remarks>
///   <para>
///     DbExceptionクラスがabstractになっているので、本クラスを定義しました。
///     中身は特にありません。
///   </para>
/// </remarks>
public class DBConException : DbException {

    public DBConException() : base() {}
    public DBConException(string msg) : base(msg) {}
    
}

} // End of namespace
