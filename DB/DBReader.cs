/// DBReader: テーブルアクセス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
#if USE_MYSQL
using MySql.Data.Types;
#endif
#if USE_POSTGRESQL
using Npgsql;
#endif
using MACS;

namespace MACS.DB {

/// <summary>
///   DBテーブルアクセス: DbDataReaderのラッパークラス
/// </summary>
/// <remarks>
///   <para>
///     データ読み出し時に適切な文字列に変換して読み出す。
///   </para>
/// </remarks>
public class DBReader: IDisposable {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DBConのQueryメソッドが作成する。
    ///   </para>
    /// </remarks>
    public DBReader(DbDataReader reader_) {
        reader = reader_;
    }

    /// <summary>
    ///   ダミーコンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本コンストラクタで作成したDBReaderに対する読み取りは全て空データを返す
    ///   </para>
    /// </remarks>
    public DBReader() {
        reader = null;
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~DBReader() {
        Dispose();
    }

    /// <summary>
    ///   カラム数
    /// </summary>
    public int FieldCount {
        get {
            if(reader == null)
                return 0;
            return reader.FieldCount;
        }
    }

    /// <summary>
    ///   使用リソース解放
    /// </summary>
    public void Dispose() {
        if(reader != null) {
            reader.Dispose();
            reader = null;
        }
    }

    /// <summary>
    ///   使用リソース解放
    /// </summary>
    public void Close() {
        Dispose();
    }

    /// <summary>
    ///   次のレコードを読み取る
    /// </summary>
    public bool Read() {
        if(reader == null)
            return false;
        return reader.Read();
    }

    /// <summary>
    ///   次のレコードを読み取り、全カラムの値を文字列にしたものを返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     次のレコードが無い場合はnullを返す。
    ///   </para>
    /// </remarks>
    public string[] Get() {
        if(!Read())
            return null;
        string[] item = new string[reader.FieldCount];
        for(int i = 0; i < item.Length; i++)
            item[i] = GetString(i);
        return item;
    }

    /// <summary>
    ///   i番目のカラムの値がヌルかどうかを返す
    /// </summary>
    public bool IsNull(int i) {
        if(reader == null)
            return true;
        return reader.IsDBNull(i);
    }

    /// <summary>
    ///   i番目のカラムの値を文字列として返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     NULLの場合、空文字列を返す。
    ///   </para>
    /// </remarks>
    public string GetString(int i) {
        try {
            if(IsNull(i)) {
                //Console.WriteLine("NULL");
                return "";
            }
            //Console.WriteLine("{0}:{1}", reader.GetDataTypeName(i), reader.GetString(i));
            switch(reader.GetDataTypeName(i).ToUpper()) {
            case "DATE":
                {
                    DateTime dt = reader.GetDateTime(i);
                    if((dt.Year <= 1) && (dt.Month <= 1) && (dt.Day <= 1))
                        return "";
                    return dt.ToString("yyyy/MM/dd");
                }
            case "DATETIME":
            case "TIMESTAMP":
                {
                    DateTime dt = reader.GetDateTime(i);
                    if((dt.Year <= 1) && (dt.Month <= 1) && (dt.Day <= 1))
                        return "";
                    if((dt.Hour == 0) && (dt.Minute == 0) && (dt.Second == 0))
                        return dt.ToString("yyyy/MM/dd");
                    return dt.ToString("yyyy/MM/dd HH:mm:ss");
                }
            case "TINYINT":
                {
                    string x = reader.GetString(i);
                    if("False" == x)
                        return "0";
                    if("True" == x)
                        return "1";
                    return x;
                }
            default:
                var val = reader.GetValue(i);
                if(val is Byte[])
                    return System.Text.Encoding.ASCII.GetString((byte[])val);
                else
                    return val.ToString();
            }
        }
#if USE_MYSQL
        catch(MySqlConversionException) {
            // MySqlコネクタが0000-00-00をエラーにしてしまうため。
            return "";
        }
#endif
        catch(Exception ex) {
            throw ex;
        }
    }

    /// <summary>
    ///   i番目のカラムの値を整数値として返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     NULLの場合、0を返す。
    ///   </para>
    /// </remarks>
    public int GetInt(int i) {
        if(IsNull(i))
            return 0;
        return reader.GetInt32(i);
    }

    /// <summary>
    ///   i番目のカラムの値をLONG整数値として返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     NULLの場合、0を返す。
    ///   </para>
    /// </remarks>
    public long GetLong(int i) {
        if(IsNull(i))
            return 0L;
        return reader.GetInt64(i);
    }

    /// <summary>
    ///   i番目のカラムの値を浮動小数点値として返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     NULLの場合、0.0を返す。
    ///   </para>
    /// </remarks>
    public double GetDouble(int i) {
        if(IsNull(i))
            return 0.0;
        return reader.GetDouble(i);
    }

    /// <summary>
    ///   i番目のカラムの値をDateTimeとして返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     NULLの場合、0001/01/01 00:00:00を返す。
    ///   </para>
    /// </remarks>
    public DateTime GetDateTime(int i) {
        try {
            if(IsNull(i))
                return new DateTime(0L);
            return reader.GetDateTime(i);
        }
#if USE_MYSQL
        catch(MySqlConversionException) {
            // MySqlコネクタが0000-00-00をエラーにしてしまうため。
            return new DateTime(0L);
        }
#endif
        catch(Exception ex) {
            throw ex;
        }
    }

    /// <summary>
    ///   カラム名の一覧を得る
    /// </summary>
    public string[] GetColumns() {
        List<string> list = new List<string>();
        for(int i = 0; i < reader.FieldCount; i++)
            list.Add(reader.GetName(i));
        return list.ToArray();
    }

    /// <summary>
    ///   i番目のカラムの名前を返す
    /// </summary>
    public string GetColumnName(int i) {
        return reader.GetName(i);
    }

    private DbDataReader reader;
}

} // End of namespace
