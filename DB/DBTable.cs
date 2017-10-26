/*
  * テーブルアクセス
  * $Id: DBTable.cs 1890 2014-06-05 04:34:56Z shibuya $
  *
  * Copyright (C) 2011-2012 Microbrains Inc. All rights reserved.
  * This code was designed and coded by SHIBUYA K.
  */

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.IO;
using MACS;

namespace MACS.DB {

/// <summary>
///   DBテーブルアクセスユーティリティクラス
/// </summary>
/// <remarks>
///   <para>
///     DBTableDefにデータベースアクセス機能を付加するラッパークラス。
///     本クラスを用いると、SQL文を直接書く事無く、簡単にデータベースアクセス
///     コードを書くことができる。
///   </para>
/// </remarks>
public class DBTable {

#region コンストラクタ

    /// <summary>
    ///   テーブル名を指定したコンストラクタ
    /// </summary>
    /// <param name="dbcon_">接続DB</param>
    /// <param name="name_">テーブル名</param>
    /// <remarks>
    ///   <para>
    ///     テーブル名はIDでも構わない。
    ///     あらかじめテーブル定義をDBTableDef.LoadAll()またはDBTableDef.Load()
    ///     で読み込んでおく事。
    ///   </para>
    /// </remarks>
    public DBTable(DBCon dbcon_, string name_): this(dbcon_, name_, name_) {}


    /// <summary>
    ///   テーブル名を指定したコンストラクタ（テーブル別名付き）
    /// </summary>
    /// <param name="dbcon_">接続DB</param>
    /// <param name="name_">テーブル名</param>
    /// <param name="alias_">テーブル別名</param>
    /// <remarks>
    ///   <para>
    ///     テーブル名はIDでも構わない。
    ///     あらかじめテーブル定義をDBTableDef.LoadAll()またはDBTableDef.Load()
    ///     で読み込んでおく事。
    ///   </para>
    /// </remarks>
    public DBTable(DBCon dbcon_, string name_, string alias_) {
        if(alias_ == null)
            throw new ArgumentNullException("Table alias name must not be null.");
        dbcon = dbcon_;
        def = getTableDef(name_);
        tbl = null;
        name = alias_;
        columns = null;
        sqlcolumns = null;
        sortcolumns = null;
        condition = null;
        having = null;
        forupdate = false;
        calcrows = false;
        distinct = false;
    }

    /// <summary>
    ///   テーブル定義を指定したコンストラクタ
    /// </summary>
    /// <param name="dbcon_">接続DB</param>
    /// <param name="def_">テーブル定義</param>
    public DBTable(DBCon dbcon_, DBTableDef def_) {
        dbcon = dbcon_;
        def = def_;
        tbl = null;
        name = def.Name;
        columns = null;
        sqlcolumns = null;
        sortcolumns = null;
        condition = null;
        having = null;
        forupdate = false;
        calcrows = false;
        distinct = false;
    }

    /// <summary>
    ///   テーブル定義を指定したコンストラクタ（テーブル別名付き）
    /// </summary>
    /// <param name="dbcon_">接続DB</param>
    /// <param name="def_">テーブル定義</param>
    /// <param name="name_">テーブル別名</param>
    public DBTable(DBCon dbcon_, DBTableDef def_, string name_) {
        dbcon = dbcon_;
        def = def_;
        tbl = null;
        name = name_;
        columns = null;
        sqlcolumns = null;
        sortcolumns = null;
        condition = null;
        having = null;
        forupdate = false;
        calcrows = false;
        distinct = false;
    }

    /// <summary>
    ///   テーブル定義を指定したコンストラクタ（テーブル別名付き）
    /// </summary>
    /// <param name="dbcon_">接続DB</param>
    /// <param name="tbl_">DBテーブルアクセスユーティリティクラス</param>
    /// <param name="name_">テーブル別名</param>
    public DBTable(DBCon dbcon_, DBTable tbl_, string name_) {
        dbcon = dbcon_;
        def = null;
        tbl = tbl_;
        name = name_;
        columns = null;
        sqlcolumns = null;
        sortcolumns = null;
        condition = null;
        having = null;
        forupdate = false;
        calcrows = false;
        distinct = false;
    }

#endregion


#region 対象カラム

    /// <summary>
    ///   対象カラム指定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     nullの場合は全カラムが指定される。
    ///     デフォルトは全カラム指定。
    ///     カラム名には、SQLでサポートされる式を指定してもよい。
    ///   </para>
    /// </remarks>
    public string[] Columns {
        get {
            if(columns == null) {
                string[] res = new string[0];
                if(def != null) {
                    res = new string[def.Columns.Length];
                    for(int i = 0; i < def.Columns.Length; i++)
                        res[i] = def.Columns[i].Name;
                }else if(tbl != null) {
                    res = new string[tbl.Columns.Length];
                    for(int i = 0; i < tbl.Columns.Length; i++)
                        res[i] = tbl.Columns[i];
                }
                return res;
            }
            return columns;
        }
        set {
            SetColumns(value);
        }
    }

    /// <summary>
    ///   指定カラムを持っているかどうか
    /// </summary>
    public bool ContainsColumn(string col) {
        foreach(string c in Columns) {
            if(c == col)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   全カラムを対象にする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     他テーブルがJOINされている場合でも、基本テーブルの全カラムだけが
    ///     設定される。
    ///   </para>
    /// </remarks>
    public DBTable SetAllColumns() {
        if(def != null)
            columns = def.ColumnNames;
        else if(tbl != null)
            columns = tbl.Columns;
        sqlcolumns = null;
        return this;
    }

    /// <summary>
    ///   rowidと全カラムを対象にする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     常に先頭カラムがrowidになる。
    ///   </para>
    /// </remarks>
    public DBTable SetAllColumnsWithRowId() {
        if(def != null) {
            columns = new string[def.Columns.Length+1];
            columns[0] = "rowid";
            for(int i = 0; i < def.Columns.Length; i++)
                columns[i+1] = def.Columns[i].Name;
        }else if(tbl != null) {
            columns = new string[tbl.Columns.Length+1];
            columns[0] = "rowid";
            for(int i = 0; i < tbl.Columns.Length; i++)
                columns[i+1] = tbl.Columns[i];
        }
        sqlcolumns = null;
        return this;
    }

    /// <summary>
    ///   カラムをセットする（1つだけ）
    /// </summary>
    public DBTable SetColumn(string col) {
        return SetColumns(col);
    }

    /// <summary>
    ///   カラムを追加する
    /// </summary>
    public DBTable AddColumn(string col) {
        return AddColumns(col);
    }

    /// <summary>
    ///   カラムをセットする（複数）
    /// </summary>
    public DBTable SetColumns(params string[] cols) {
        columns = null;
        sqlcolumns = null;
        return AddColumns(cols);
    }

    /// <summary>
    ///   カラムを追加する（複数）
    /// </summary>
    public DBTable AddColumns(params string[] cols) {
        if(cols == null)
            return this;
        string[] newcolumns;
        string[] newsqlcolumns;
        int idx;
        if(columns == null) {
            newcolumns = new string[cols.Length];
            newsqlcolumns = new string[cols.Length];
            idx = 0;
        } else {
            newcolumns = new string[columns.Length+cols.Length];
            newsqlcolumns = new string[columns.Length+cols.Length];
            for(int i = 0; i < columns.Length; i++){
                newcolumns[i] = columns[i];
                newsqlcolumns[i] = (sqlcolumns==null)?columns[i]:sqlcolumns[i];
            }
            idx = columns.Length;
        }
        for(int i = 0; i < cols.Length; i++) {
            string c = cols[i];
            int pos = c.IndexOf(" as ", StringComparison.OrdinalIgnoreCase);
            if(pos < 0) {
                newcolumns[idx+i] = c;
                newsqlcolumns[idx+i] = c;
            } else {
                newcolumns[idx+i] = c.Substring(pos+4);
                newsqlcolumns[idx+i] = c;
            }
        }
        columns = newcolumns;
        sqlcolumns = newsqlcolumns;
        return this;
    }

    /// <summary>
    ///   JOINしているテーブルの全カラムを対象に加える
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     他テーブル名（別名がある場合は別名）がカラム名の前につく。
    ///   </para>
    /// </remarks>
    public DBTable AddAllJoinColumns() {
        if(joinList != null) {
            foreach(JoinTable jt in joinList)
                AddAllJoinColumns(jt.name);
        }
        return this;
    }

    /// <summary>
    ///   指定した名称のJOINテーブルの全カラムを対象に加える
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     他テーブル名（別名がある場合は別名）がカラム名の前につく。
    ///   </para>
    /// </remarks>
    public DBTable AddAllJoinColumns(string joinname) {
        if(joinList == null)
            throw new ArgumentException("This DBTable does not have joined tables");
        foreach(JoinTable jt in joinList) {
            if(jt.name == joinname) {
                string[] jcolumns;
                if(jt.def != null)
                    jcolumns = jt.def.ColumnNames;
                else if(jt.tbl != null)
                    jcolumns = jt.tbl.Columns;
                else
                    throw new ArgumentException("This joined table does not have valid column definition");
                string[] newcolumns = new string[jcolumns.Length];
                for(int i = 0; i < jcolumns.Length; i++)
                    newcolumns[i] = joinname+"."+jcolumns[i];
                return AddColumns(newcolumns);
            }
        }
        throw new ArgumentException(String.Format("Table {0} is not joined to this table.", joinname));
    }

    /// <summary>
    ///   JOINしているテーブルの全カラムを対象に加える
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     重複する名前のカラムが既にある場合には、他テーブル名（別名がある場合は別名）がカラム名の前につく。
    ///   </para>
    /// </remarks>
    public DBTable AddAllJoinColumnsUniqName() {
        if(joinList != null) {
            foreach(JoinTable jt in joinList)
                AddAllJoinColumnsUniqName(jt.name);
        }
        return this;
    }

    /// <summary>
    ///   指定した名称のJOINテーブルの全カラムを対象に加える
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     重複する名前のカラムが既にある場合には、他テーブル名（別名がある場合は別名）がカラム名の前につく。
    ///   </para>
    /// </remarks>
    public DBTable AddAllJoinColumnsUniqName(string joinname) {
        if(joinList == null)
            throw new ArgumentException("This DBTable does not have joined tables");
        foreach(JoinTable jt in joinList) {
            if(jt.name == joinname) {
                string[] jcolumns;
                if(jt.def != null)
                    jcolumns = jt.def.ColumnNames;
                else if(jt.tbl != null)
                    jcolumns = jt.tbl.Columns;
                else
                    throw new ArgumentException("This joined table does not have valid column definition");
                string[] newcolumns = new string[jcolumns.Length];
                for(int i = 0; i < jcolumns.Length; i++) {
                    bool duplicated = false;
                    string[] currentColumns = Columns;
                    for(int j = 0; j < currentColumns.Length; j++) {
                        if(currentColumns[j] == jcolumns[i]) {
                            duplicated = true;
                            break;
                        }
                    }
                    if(duplicated)
                        newcolumns[i] = joinname+"."+jcolumns[i];
                    else
                        newcolumns[i] = jcolumns[i];
                }
                return AddColumns(newcolumns);
            }
        }
        throw new ArgumentException(String.Format("Table {0} is not joined to this table.", joinname));
    }

    /// <summary>
    ///   ソート対象カラム指定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ソート対象カラムは複数指定できる。
    ///     nullの場合はソート指定無し。
    ///     デフォルトはソート指定無し。
    ///   </para>
    /// </remarks>
    public string[] SortColumns {
        get { return sortcolumns; }
        set { sortcolumns = value; }
    }

    /// <summary>
    ///   ソート対象カラムを指定する。
    /// </summary>
    /// <param name="columnname">カラム名</param>
    /// <param name="ascendflag">昇順のときtrue</param>
    public DBTable AddSortColumn(string columnname, bool ascendflag) {
        if(sortcolumns == null) {
            sortcolumns = new string[1];
        } else {
            string[] newsortcolumns = new string[sortcolumns.Length+1];
            for(int i = 0; i < sortcolumns.Length; i++)
                newsortcolumns[i] = sortcolumns[i];
            sortcolumns = newsortcolumns;
        }
        sortcolumns[sortcolumns.Length-1] = expandColumnName(columnname);
        if(!ascendflag)
            sortcolumns[sortcolumns.Length-1] += " DESC";
        return this;
    }

    /// <summary>
    ///   指定した名前のカラムの定義を獲得する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     "テーブル名.カラム名"の形式の時には、Joinしているテーブルも探して
    ///     カラム定義を獲得する。
    ///   </para>
    /// </remarks>
    public DBColumnDef GetColumnDef(string colname) {
        int idx = colname.IndexOf(" as ", StringComparison.OrdinalIgnoreCase);
        if(idx >= 0) {
            colname = colname.Substring(0, idx);
        }
        idx = colname.IndexOf('.');
        if(idx >= 0) {
            string tblname = colname.Substring(0,idx);
            colname = colname.Substring(idx+1);
            if(tblname == name) {
                if(def != null)
                    return def.GetColumnDef(colname);
                else if(tbl != null)
                    return tbl.GetColumnDef(colname);
            }
            if(joinList == null)
                return null;
            foreach(JoinTable jt in joinList) {
                if(tblname == jt.name) {
                    if(jt.def != null)
                        return jt.def.GetColumnDef(colname);
                    else if(jt.tbl != null)
                        return jt.tbl.GetColumnDef(colname);
                }
            }
            return null;
        }
        if(joinList != null) {
            if((colname == "rowid") || ((def != null) && def.HasColumn(colname)) || ((tbl != null) && tbl.ContainsColumn(colname))) {
                if(def != null)
                    return def.GetColumnDef(colname);
                else if(tbl != null)
                    return tbl.GetColumnDef(colname);
            }
            foreach(JoinTable jt in joinList) {
                if((jt.def != null) && jt.def.HasColumn(colname))
                    return jt.def.GetColumnDef(colname);
                else if((jt.tbl != null) && jt.tbl.ContainsColumn(colname))
                    return jt.tbl.GetColumnDef(colname);
            }
        }
        if(def != null)
            return def.GetColumnDef(colname);
        else if(tbl != null)
            return tbl.GetColumnDef(colname);
        else
            throw new ArgumentException("DBTable or DBTableDef must not be null.");           
    }

#endregion

#region 検索条件

    /// <summary>
    ///   検索条件指定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     SQLのWHERE句内の式として有効な文字列を指定する。
    ///     nullの場合条件無し。
    ///     デフォルトは条件無し。
    ///   </para>
    /// </remarks>
    public string Condition {
        get { return condition.ToString(); }
        set {
            if(String.IsNullOrEmpty(value))
                condition = null;
            else
                condition = new StringBuilder(value);
        }
    }

    /// <summary>
    ///   検索条件を指定する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定した文字列が、現在の検索条件式になる。
    ///   </para>
    /// </remarks>
    public DBTable SetCondition(string cond) {
        ClearCondition();
        return AddCondition(cond);
    }

    /// <summary>
    ///   検索条件を指定する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Equals, DBCondition.Code.NotEquals,
    ///     DBCondition.Code.Contains, DBCondition.Code.NotContains,
    ///     DBCondition.Code.StartsWith, DBCondition.Code.EndsWith,
    ///     DBCondition.Code.GreaterOrEqual, DBCondition.Code.GreaterThan,
    ///     DBCondition.Code.LessOrEqual, DBCondition.Code.LessThan,
    ///     DBCondition.Code.CollateEquals, DBCondition.Code.NotCollateEquals,
    ///     DBCondition.Code.CollateContains, DBCondition.Code.NotCollateContains,
    ///     DBCondition.Code.CollateStartsWith, DBCondition.Code.CollateEndsWith,
    ///     のいずれか。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable SetCondition(DBCondition.Code conditioncode, string columnname, object columnvalue) {
        ClearCondition();
        return AddCondition(conditioncode, columnname, columnvalue);
    }

    /// <summary>
    ///   検索条件を指定する。Between, NotBetween用。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     現在、DBCondition.Code.Between, DBCondition.Code.NotBetween のみ有効。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue1">カラム値1</param>
    /// <param name="columnvalue2">カラム値2</param>
    public DBTable SetCondition(DBCondition.Code conditioncode, string columnname, object columnvalue1, object columnvalue2) {
        ClearCondition();
        return AddCondition(conditioncode, columnname, columnvalue1, columnvalue2);
    }

    /// <summary>
    ///   検索条件を追加する。Exists, NotExists用
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Exists, DBCondition.Code.NotExists のいずれか。
    ///     第2引数はサブクエリとなるSELECT文の文字列か、サブクエリの条件を指定
    ///     したDBTable。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="subquery">条件サブクエリ</param>
    public DBTable SetCondition(DBCondition.Code conditioncode, object subquery) {
        ClearCondition();
        return AddCondition(conditioncode, subquery);
    }

    /// <summary>
    ///   等値検索条件を指定する。
    /// </summary>
    /// <param name="columnnames">カラム名一覧</param>
    /// <param name="columnvalues">カラム値一覧</param>
    /// <remarks>
    ///   <para>
    ///     columnnamesで指定したカラムの値が、それぞれvaluesで指定した値である
    ///     ことを検索条件にする。
    ///     columnnamesとcolumnvaluesの長さは同じでなければならない。
    ///   </para>
    /// </remarks>
    public DBTable SetCondition(string[] columnnames, object[] columnvalues) {
        ClearCondition();
        return AddCondition(columnnames, columnvalues);
    }

    /// <summary>
    ///   等値検索条件を指定する。
    /// </summary>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable SetCondition(string columnname, object columnvalue) {
        ClearCondition();
        return AddCondition(DBCondition.Code.Equals, columnname, columnvalue);
    }

    /// <summary>
    ///   等値検索条件を指定する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    public DBTable SetCondition(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2) {
        ClearCondition();
        AddCondition(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddCondition(DBCondition.Code.Equals, columnname2, columnvalue2);
        return this;
    }

    /// <summary>
    ///   等値検索条件を指定する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    /// <param name="columnname3">カラム名</param>
    /// <param name="columnvalue3">カラム値</param>
    public DBTable SetCondition(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2,
                                string columnname3, object columnvalue3) {
        ClearCondition();
        AddCondition(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddCondition(DBCondition.Code.Equals, columnname2, columnvalue2);
        AddCondition(DBCondition.Code.Equals, columnname3, columnvalue3);
        return this;
    }

    /// <summary>
    ///   検索条件を追加する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定した文字列が、現在の検索条件式の後に" AND "で付け足される。
    ///   </para>
    /// </remarks>
    public DBTable AddCondition(string cond) {
        return addCondition(ref condition, cond);
    }

    /// <summary>
    ///   検索条件を追加する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Equals, DBCondition.Code.NotEquals,
    ///     DBCondition.Code.Contains, DBCondition.Code.NotContains,
    ///     DBCondition.Code.StartsWith, DBCondition.Code.EndsWith,
    ///     DBCondition.Code.GreaterOrEqual, DBCondition.Code.GreaterThan,
    ///     DBCondition.Code.LessOrEqual, DBCondition.Code.LessThan,
    ///     DBCondition.Code.In, DBCondition.Code.NotIn,
    ///     DBCondition.Code.CollateEquals, DBCondition.Code.NotCollateEquals,
    ///     DBCondition.Code.CollateContains, DBCondition.Code.NotCollateContains,
    ///     DBCondition.Code.CollateStartsWith, DBCondition.Code.CollateEndsWith,
    ///     のいずれか。
    ///     DBCondition.Code.InまたはDBCondition.Code.NotInの場合は、第3引数の
    ///     カラム値はDBTable(サブクエリを生成)、DataArray、配列のいずれかで
    ///     なければならない。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable AddCondition(DBCondition.Code conditioncode, string columnname, object columnvalue) {
        return addCondition(ref condition, conditioncode, columnname, columnvalue);
    }

    /// <summary>
    ///   検索条件を追加する。Between, NotBetween用。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Between, DBCondition.Code.NotBetween のみ有効。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue1">カラム値1</param>
    /// <param name="columnvalue2">カラム値2</param>
    public DBTable AddCondition(DBCondition.Code conditioncode, string columnname, object columnvalue1, object columnvalue2) {
        return addCondition(ref condition, conditioncode, columnname, columnvalue1, columnvalue2);
    }

    /// <summary>
    ///   検索条件を追加する。Exists, NotExists用
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Exists, DBCondition.Code.NotExists のいずれか。
    ///     第2引数はサブクエリとなるSELECT文の文字列か、サブクエリの条件を指定
    ///     したDBTable。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="subquery">条件サブクエリ</param>
    public DBTable AddCondition(DBCondition.Code conditioncode, object subquery) {
        return addCondition(ref condition, conditioncode, subquery);
    }

    /// <summary>
    ///   等値検索条件を追加する。
    /// </summary>
    /// <param name="columnnames">カラム名一覧</param>
    /// <param name="columnvalues">カラム値一覧</param>
    /// <remarks>
    ///   <para>
    ///     columnnamesで指定したカラムの値が、それぞれvaluesで指定した値である
    ///     ことを検索条件に追加する。
    ///     columnnamesとcolumnvaluesの長さは同じでなければならない。
    ///   </para>
    /// </remarks>
    public DBTable AddCondition(string[] columnnames, object[] columnvalues) {
        return addCondition(ref condition, columnnames, columnvalues);
    }

    /// <summary>
    ///   等値検索条件を追加する。
    /// </summary>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable AddCondition(string columnname, object columnvalue) {
        return AddCondition(DBCondition.Code.Equals, columnname, columnvalue);
    }

    /// <summary>
    ///   等値検索条件を追加する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    public DBTable AddCondition(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2) {
        AddCondition(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddCondition(DBCondition.Code.Equals, columnname2, columnvalue2);
        return this;
    }

    /// <summary>
    ///   等値検索条件を追加する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    /// <param name="columnname3">カラム名</param>
    /// <param name="columnvalue3">カラム値</param>
    public DBTable AddCondition(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2,
                                string columnname3, object columnvalue3) {
        AddCondition(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddCondition(DBCondition.Code.Equals, columnname2, columnvalue2);
        AddCondition(DBCondition.Code.Equals, columnname3, columnvalue3);
        return this;
    }

    /// <summary>
    ///   検索条件を取得する。
    /// </summary>
    public string GetCondition(string cond) {
        StringBuilder condition = new StringBuilder();
        addCondition(ref condition, cond);

        return condition.ToString();
    }

    /// <summary>
    ///   検索条件を取得する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Equals, DBCondition.Code.NotEquals,
    ///     DBCondition.Code.Contains, DBCondition.Code.NotContains,
    ///     DBCondition.Code.StartsWith, DBCondition.Code.EndsWith,
    ///     DBCondition.Code.GreaterOrEqual, DBCondition.Code.GreaterThan,
    ///     DBCondition.Code.LessOrEqual, DBCondition.Code.LessThan,
    ///     DBCondition.Code.In, DBCondition.Code.NotIn,
    ///     DBCondition.Code.CollateEquals, DBCondition.Code.NotCollateEquals,
    ///     DBCondition.Code.CollateContains, DBCondition.Code.NotCollateContains,
    ///     DBCondition.Code.CollateStartsWith, DBCondition.Code.CollateEndsWith,
    ///     のいずれか。
    ///     DBCondition.Code.InまたはDBCondition.Code.NotInの場合は、第3引数の
    ///     カラム値はDBTable(サブクエリを生成)、DataArray、配列のいずれかで
    ///     なければならない。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public string GetCondition(DBCondition.Code conditioncode, string columnname, object columnvalue) {
        StringBuilder condition = new StringBuilder();
        addCondition(ref condition, conditioncode, columnname, columnvalue);
        
        return condition.ToString();
    }

    /// <summary>
    ///   検索条件を取得する。Between, NotBetween用。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Between, DBCondition.Code.NotBetween のみ有効。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue1">カラム値1</param>
    /// <param name="columnvalue2">カラム値2</param>
    public string GetCondition(DBCondition.Code conditioncode, string columnname, object columnvalue1, object columnvalue2) {
        StringBuilder condition = new StringBuilder();
        addCondition(ref condition, conditioncode, columnname, columnvalue1, columnvalue2);
        
        return condition.ToString();
    }

    /// <summary>
    ///   検索条件を取得する。Exists, NotExists用
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Exists, DBCondition.Code.NotExists のいずれか。
    ///     第2引数はサブクエリとなるSELECT文の文字列か、サブクエリの条件を指定
    ///     したDBTable。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="subquery">条件サブクエリ</param>
    public string GetCondition(DBCondition.Code conditioncode, object subquery) {
        StringBuilder condition = new StringBuilder();
        addCondition(ref condition, conditioncode, subquery);
        
        return condition.ToString();
    }

    /// <summary>
    ///   等値検索条件を取得する。
    /// </summary>
    /// <param name="columnnames">カラム名一覧</param>
    /// <param name="columnvalues">カラム値一覧</param>
    /// <remarks>
    ///   <para>
    ///     columnnamesで指定したカラムの値が、それぞれvaluesで指定した値である
    ///     ことを検索条件に追加する。
    ///     columnnamesとcolumnvaluesの長さは同じでなければならない。
    ///   </para>
    /// </remarks>
    public string GetCondition(string[] columnnames, object[] columnvalues) {
        StringBuilder condition = new StringBuilder();
        addCondition(ref condition, columnnames, columnvalues);
        
        return condition.ToString();
    }

    /// <summary>
    ///   検索条件を空にする
    /// </summary>
    public DBTable ClearCondition() {
        return clearCondition(ref condition);
    }

    /// <summary>
    ///   検索条件が空かどうか
    /// </summary>
    public bool IsNullCondition {
        get { return isNullCondition(ref condition); }
    }

#endregion

#region HAVING条件

    /// <summary>
    ///   HAVING条件指定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     SQLのHAVING句内の式として有効な文字列を指定する。
    ///     nullの場合条件無し。
    ///     デフォルトは条件無し。
    ///   </para>
    /// </remarks>
    public string Having {
        get { return having.ToString(); }
        set {
            if(String.IsNullOrEmpty(value))
                having = null;
            else
                having = new StringBuilder(value);
        }
    }

    /// <summary>
    ///   HAVING条件を指定する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定した文字列が、現在のHAVING条件式になる。
    ///   </para>
    /// </remarks>
    public DBTable SetHaving(string cond) {
        ClearHaving();
        return AddHaving(cond);
    }

    /// <summary>
    ///   HAVING条件を指定する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Equals, DBCondition.Code.NotEquals,
    ///     DBCondition.Code.Contains, DBCondition.Code.NotContains,
    ///     DBCondition.Code.StartsWith, DBCondition.Code.EndsWith,
    ///     DBCondition.Code.GreaterOrEqual, DBCondition.Code.GreaterThan,
    ///     DBCondition.Code.LessOrEqual, DBCondition.Code.LessThan,
    ///     DBCondition.Code.CollateEquals, DBCondition.Code.NotCollateEquals,
    ///     DBCondition.Code.CollateContains, DBCondition.Code.NotCollateContains,
    ///     DBCondition.Code.CollateStartsWith, DBCondition.Code.CollateEndsWith,
    ///     のいずれか。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable SetHaving(DBCondition.Code conditioncode, string columnname, object columnvalue) {
        ClearHaving();
        return AddHaving(conditioncode, columnname, columnvalue);
    }

    /// <summary>
    ///   HAVING条件を指定する。Between, NotBetween用。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     現在、DBCondition.Code.Between, DBCondition.Code.NotBetween のみ有効。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue1">カラム値1</param>
    /// <param name="columnvalue2">カラム値2</param>
    public DBTable SetHaving(DBCondition.Code conditioncode, string columnname, object columnvalue1, object columnvalue2) {
        ClearHaving();
        return AddHaving(conditioncode, columnname, columnvalue1, columnvalue2);
    }

    /// <summary>
    ///   HAVING条件を追加する。Exists, NotExists用
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Exists, DBCondition.Code.NotExists のいずれか。
    ///     第2引数はサブクエリとなるSELECT文の文字列か、サブクエリの条件を指定
    ///     したDBTable。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="subquery">条件サブクエリ</param>
    public DBTable SetHaving(DBCondition.Code conditioncode, object subquery) {
        ClearHaving();
        return AddHaving(conditioncode, subquery);
    }

    /// <summary>
    ///   等値HAVING条件を指定する。
    /// </summary>
    /// <param name="columnnames">カラム名一覧</param>
    /// <param name="columnvalues">カラム値一覧</param>
    /// <remarks>
    ///   <para>
    ///     columnnamesで指定したカラムの値が、それぞれvaluesで指定した値である
    ///     ことをHAVING条件にする。
    ///     columnnamesとcolumnvaluesの長さは同じでなければならない。
    ///   </para>
    /// </remarks>
    public DBTable SetHaving(string[] columnnames, object[] columnvalues) {
        ClearHaving();
        return AddHaving(columnnames, columnvalues);
    }

    /// <summary>
    ///   等値HAVING条件を指定する。
    /// </summary>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable SetHaving(string columnname, object columnvalue) {
        ClearHaving();
        return AddHaving(DBCondition.Code.Equals, columnname, columnvalue);
    }

    /// <summary>
    ///   等値HAVING条件を指定する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    public DBTable SetHaving(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2) {
        ClearHaving();
        AddHaving(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddHaving(DBCondition.Code.Equals, columnname2, columnvalue2);
        return this;
    }

    /// <summary>
    ///   等値HAVING条件を指定する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    /// <param name="columnname3">カラム名</param>
    /// <param name="columnvalue3">カラム値</param>
    public DBTable SetHaving(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2,
                                string columnname3, object columnvalue3) {
        ClearHaving();
        AddHaving(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddHaving(DBCondition.Code.Equals, columnname2, columnvalue2);
        AddHaving(DBCondition.Code.Equals, columnname3, columnvalue3);
        return this;
    }

    /// <summary>
    ///   HAVING条件を追加する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定した文字列が、現在のHAVING条件式の後に" AND "で付け足される。
    ///   </para>
    /// </remarks>
    public DBTable AddHaving(string cond) {
        return addCondition(ref having, cond);
    }

    /// <summary>
    ///   HAVING条件を追加する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Equals, DBCondition.Code.NotEquals,
    ///     DBCondition.Code.Contains, DBCondition.Code.NotContains,
    ///     DBCondition.Code.StartsWith, DBCondition.Code.EndsWith,
    ///     DBCondition.Code.GreaterOrEqual, DBCondition.Code.GreaterThan,
    ///     DBCondition.Code.LessOrEqual, DBCondition.Code.LessThan,
    ///     DBCondition.Code.In, DBCondition.Code.NotIn,
    ///     DBCondition.Code.CollateEquals, DBCondition.Code.NotCollateEquals,
    ///     DBCondition.Code.CollateContains, DBCondition.Code.NotCollateContains,
    ///     DBCondition.Code.CollateStartsWith, DBCondition.Code.CollateEndsWith,
    ///     のいずれか。
    ///     DBCondition.Code.InまたはDBCondition.Code.NotInの場合は、第3引数の
    ///     カラム値はDBTable(サブクエリを生成)、DataArray、配列のいずれかで
    ///     なければならない。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable AddHaving(DBCondition.Code conditioncode, string columnname, object columnvalue) {
        return addCondition(ref having, conditioncode, columnname, columnvalue);
    }

    /// <summary>
    ///   HAVING条件を追加する。Between, NotBetween用。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Between, DBCondition.Code.NotBetween のみ有効。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue1">カラム値1</param>
    /// <param name="columnvalue2">カラム値2</param>
    public DBTable AddHaving(DBCondition.Code conditioncode, string columnname, object columnvalue1, object columnvalue2) {
        return addCondition(ref having, conditioncode, columnname, columnvalue1, columnvalue2);
    }

    /// <summary>
    ///   HAVING条件を追加する。Exists, NotExists用
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     第1引数は比較条件を示す。
    ///     DBCondition.Code.Exists, DBCondition.Code.NotExists のいずれか。
    ///     第2引数はサブクエリとなるSELECT文の文字列か、サブクエリの条件を指定
    ///     したDBTable。
    ///   </para>
    /// </remarks>
    /// <param name="conditioncode">比較条件</param>
    /// <param name="subquery">条件サブクエリ</param>
    public DBTable AddHaving(DBCondition.Code conditioncode, object subquery) {
        return addCondition(ref having, conditioncode, subquery);
    }

    /// <summary>
    ///   等値HAVING条件を追加する。
    /// </summary>
    /// <param name="columnnames">カラム名一覧</param>
    /// <param name="columnvalues">カラム値一覧</param>
    /// <remarks>
    ///   <para>
    ///     columnnamesで指定したカラムの値が、それぞれvaluesで指定した値である
    ///     ことをHAVING条件に追加する。
    ///     columnnamesとcolumnvaluesの長さは同じでなければならない。
    ///   </para>
    /// </remarks>
    public DBTable AddHaving(string[] columnnames, object[] columnvalues) {
        return addCondition(ref having, columnnames, columnvalues);
    }

    /// <summary>
    ///   等値HAVING条件を追加する。
    /// </summary>
    /// <param name="columnname">カラム名</param>
    /// <param name="columnvalue">カラム値</param>
    public DBTable AddHaving(string columnname, object columnvalue) {
        return AddHaving(DBCondition.Code.Equals, columnname, columnvalue);
    }

    /// <summary>
    ///   等値HAVING条件を追加する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    public DBTable AddHaving(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2) {
        AddHaving(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddHaving(DBCondition.Code.Equals, columnname2, columnvalue2);
        return this;
    }

    /// <summary>
    ///   等値HAVING条件を追加する。
    /// </summary>
    /// <param name="columnname1">カラム名</param>
    /// <param name="columnvalue1">カラム値</param>
    /// <param name="columnname2">カラム名</param>
    /// <param name="columnvalue2">カラム値</param>
    /// <param name="columnname3">カラム名</param>
    /// <param name="columnvalue3">カラム値</param>
    public DBTable AddHaving(string columnname1, object columnvalue1,
                                string columnname2, object columnvalue2,
                                string columnname3, object columnvalue3) {
        AddHaving(DBCondition.Code.Equals, columnname1, columnvalue1);
        AddHaving(DBCondition.Code.Equals, columnname2, columnvalue2);
        AddHaving(DBCondition.Code.Equals, columnname3, columnvalue3);
        return this;
    }

    /// <summary>
    ///   HAVING条件を空にする
    /// </summary>
    public DBTable ClearHaving() {
        return clearCondition(ref having);
    }

    /// <summary>
    ///   HAVING条件が空かどうか
    /// </summary>
    public bool IsNullHaving {
        get { return isNullCondition(ref having); }
    }

#endregion

#region JOIN

    /// <summary>
    ///   他テーブルのJoin指定
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(string othertable, string othercolumnname, string columnname) {
        Join(getTableDef(othertable), othercolumnname, columnname);
    }

    /// <summary>
    ///   他テーブルのJoin指定
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        Join(getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2);
    }

    /// <summary>
    ///   他テーブルのJoin指定
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        Join(getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2, othercolumnname3, columnname3);
    }

    /// <summary>
    ///   他テーブルのJoin指定。Join条件指定版
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="cond">Join条件指定文字列</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(string othertable, string cond) {
        Join(getTableDef(othertable), cond);
    }

    /// <summary>
    ///   他テーブルのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, string othertable, string othercolumnname, string columnname) {
        JoinAs(othername, getTableDef(othertable), othercolumnname, columnname);
    }

    /// <summary>
    ///   他テーブルのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        JoinAs(othername, getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2);
    }

    /// <summary>
    ///   他テーブルのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        JoinAs(othername, getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2, othercolumnname3, columnname3);
    }

    /// <summary>
    ///   他テーブルのJoin指定。Join条件指定版。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="cond">Join条件指定文字列</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, string othertable, string cond) {
        JoinAs(othername, getTableDef(othertable), cond);
    }

    /// <summary>
    ///   他テーブルのInner Join指定
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    public void InnerJoin(string othertable, string othercolumnname, string columnname) {
        InnerJoin(getTableDef(othertable), othercolumnname, columnname);
    }

    /// <summary>
    ///   他テーブルのInner Join指定
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    public void InnerJoin(string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        InnerJoin(getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2);
    }

    /// <summary>
    ///   他テーブルのInner Join指定
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    public void InnerJoin(string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        InnerJoin(getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2, othercolumnname3, columnname3);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。Join条件指定版
    /// </summary>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="cond">Join条件指定文字列</param>
    public void InnerJoin(string othertable, string cond) {
        InnerJoin(getTableDef(othertable), cond);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    public void InnerJoinAs(string othername, string othertable, string othercolumnname, string columnname) {
        InnerJoinAs(othername, getTableDef(othertable), othercolumnname, columnname);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    public void InnerJoinAs(string othername, string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        InnerJoinAs(othername, getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    public void InnerJoinAs(string othername, string othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        InnerJoinAs(othername, getTableDef(othertable), othercolumnname1, columnname1, othercolumnname2, columnname2, othercolumnname3, columnname3);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。Join条件指定版。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブル名</param>
    /// <param name="cond">Join条件指定文字列</param>
    public void InnerJoinAs(string othername, string othertable, string cond) {
        InnerJoinAs(othername, getTableDef(othertable), cond);
    }

    /// <summary>
    ///   他テーブルのJoin指定
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(DBTableDef othertable, string othercolumnname, string columnname) {
        JoinAs(othertable.Name, othertable, othercolumnname, columnname);
    }

    /// <summary>
    ///   他テーブルのJoin指定
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        JoinAs(othertable.Name, othertable, othercolumnname1, columnname1, othercolumnname2, columnname2);
    }

    /// <summary>
    ///   他テーブルのJoin指定
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        JoinAs(othertable.Name, othertable, othercolumnname1, columnname1, othercolumnname2, columnname2, othercolumnname3, columnname3);
    }

    /// <summary>
    ///   他テーブルのJoin指定。Join条件指定版
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="cond">Join条件指定文字列</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void Join(DBTableDef othertable, string cond) {
        JoinAs(othertable.Name, othertable, cond);
    }

    /// <summary>
    ///   他テーブルのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTableDef othertable, string othercolumnname, string columnname) {
        JoinAs(othername, othertable, String.Format("{0}.{1}={2}", othername, othercolumnname, expandColumnName(columnname,true)));
    }

    /// <summary>
    ///   他テーブルのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        JoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true)));
    }

    /// <summary>
    ///   他テーブルのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        JoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4} AND {0}.{5}={6}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true), othercolumnname3, expandColumnName(columnname3,true)));
    }

    /// <summary>
    ///   他テーブルのJoin指定。Join条件指定版。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="cond">Join条件指定文字列</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTableDef othertable, string cond) {
        if(joinList == null)
            joinList = new List<JoinTable>();
        joinList.Add(new JoinTable(othername, othertable, "LEFT", cond));
    }

    /// <summary>
    ///   他テーブルのInner Join指定
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    public void InnerJoin(DBTableDef othertable, string othercolumnname, string columnname) {
        InnerJoinAs(othertable.Name, othertable, othercolumnname, columnname);
    }

    /// <summary>
    ///   他テーブルのInner Join指定
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    public void InnerJoin(DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        InnerJoinAs(othertable.Name, othertable, othercolumnname1, columnname1, othercolumnname2, columnname2);
    }

    /// <summary>
    ///   他テーブルのInner Join指定
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    public void InnerJoin(DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        InnerJoinAs(othertable.Name, othertable, othercolumnname1, columnname1, othercolumnname2, columnname2, othercolumnname3, columnname3);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。Join条件指定版
    /// </summary>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="cond">Join条件指定文字列</param>
    public void InnerJoin(DBTableDef othertable, string cond) {
        InnerJoinAs(othertable.Name, othertable, cond);
    }

    /// <summary>
    ///   他テーブルのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname">他テーブルの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    public void InnerJoinAs(string othername, DBTableDef othertable, string othercolumnname, string columnname) {
        InnerJoinAs(othername, othertable, String.Format("{0}.{1}={2}", othername, othercolumnname, expandColumnName(columnname,true)));
    }

    /// <summary>
    ///   他テーブルのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    public void InnerJoinAs(string othername, DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        InnerJoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true)));
    }

    /// <summary>
    ///   他テーブルのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="othercolumnname1">他テーブルの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">他テーブルの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">他テーブルの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    public void InnerJoinAs(string othername, DBTableDef othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        InnerJoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4} AND {0}.{5}={6}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true), othercolumnname3, expandColumnName(columnname3,true)));
    }

    /// <summary>
    ///   他テーブルのInner Join指定。Join条件指定版。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">他テーブルのテーブル別名</param>
    /// <param name="othertable">他テーブルのDBTableDef</param>
    /// <param name="cond">Join条件指定文字列</param>
    public void InnerJoinAs(string othername, DBTableDef othertable, string cond) {
        if(joinList == null)
            joinList = new List<JoinTable>();
        joinList.Add(new JoinTable(othername, othertable, "INNER", cond));
    }

    /// <summary>
    ///   サブクエリのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="othercolumnname">サブクエリの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTable othertable, string othercolumnname, string columnname) {
        JoinAs(othername, othertable, String.Format("{0}.{1}={2}", othername, othercolumnname, expandColumnName(columnname,true)));
    }

    /// <summary>
    ///   サブクエリのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="othercolumnname1">サブクエリの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">サブクエリの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTable othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        JoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true)));
    }

    /// <summary>
    ///   サブクエリのJoin指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="othercolumnname1">サブクエリの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">サブクエリの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">サブクエリの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTable othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        JoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4} AND {0}.{5}={6}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true), othercolumnname3, expandColumnName(columnname3,true)));
    }

    /// <summary>
    ///   サブクエリのJoin指定。Join条件指定版。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="cond">Join条件指定文字列</param>
    /// <remarks>
    ///   <para>
    ///     LEFT OUTER JOINされます。
    ///   </para>
    /// </remarks>
    public void JoinAs(string othername, DBTable othertable, string cond) {
        if(joinList == null)
            joinList = new List<JoinTable>();
        joinList.Add(new JoinTable(othername, othertable, "LEFT", cond));
    }

    /// <summary>
    ///   サブクエリのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="othercolumnname">サブクエリの項目名</param>
    /// <param name="columnname">上記項目とjoinする自テーブルの項目名</param>
    public void InnerJoinAs(string othername, DBTable othertable, string othercolumnname, string columnname) {
        InnerJoinAs(othername, othertable, String.Format("{0}.{1}={2}", othername, othercolumnname, expandColumnName(columnname,true)));
    }

    /// <summary>
    ///   サブクエリのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="othercolumnname1">サブクエリの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">サブクエリの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    public void InnerJoinAs(string othername, DBTable othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2) {
        InnerJoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true)));
    }

    /// <summary>
    ///   サブクエリのInner Join指定。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="othercolumnname1">サブクエリの項目名1</param>
    /// <param name="columnname1">上記項目1とjoinする自テーブルの項目名1</param>
    /// <param name="othercolumnname2">サブクエリの項目名2</param>
    /// <param name="columnname2">上記項目2とjoinする自テーブルの項目名2</param>
    /// <param name="othercolumnname3">サブクエリの項目名3</param>
    /// <param name="columnname3">上記項目3とjoinする自テーブルの項目名3</param>
    public void InnerJoinAs(string othername, DBTable othertable, string othercolumnname1, string columnname1, string othercolumnname2, string columnname2, string othercolumnname3, string columnname3) {
        InnerJoinAs(othername, othertable, String.Format("{0}.{1}={2} AND {0}.{3}={4} AND {0}.{5}={6}", othername, othercolumnname1, expandColumnName(columnname1,true), othercolumnname2, expandColumnName(columnname2,true), othercolumnname3, expandColumnName(columnname3,true)));
    }

    /// <summary>
    ///   サブクエリのInner Join指定。Join条件指定版。テーブル別名をつける版。
    /// </summary>
    /// <param name="othername">サブクエリのテーブル別名</param>
    /// <param name="othertable">サブクエリのDBTable</param>
    /// <param name="cond">Join条件指定文字列</param>
    public void InnerJoinAs(string othername, DBTable othertable, string cond) {
        if(joinList == null)
            joinList = new List<JoinTable>();
        joinList.Add(new JoinTable(othername, othertable, "INNER", cond));
    }


#endregion

#region 検索オプション

    /// <summary>
    ///   GROUP BY句を付ける。
    /// </summary>
    public void GroupBy(params string[] groupby_) {
        groupby = groupby_;
    }

    /// <summary>
    ///   SELECT実行時に"FOR UPDATE"オプションを付けるかどうか
    /// </summary>
    public bool ForUpdate {
        get { return forupdate; }
        set { forupdate = value; }
    }

    /// <summary>
    ///   SELECT実行時に"DISTINCT"オプションを付けるかどうか
    /// </summary>
    public bool Distinct {
        get { return distinct; }
        set { distinct = value; }
    }

    /// <summary>
    ///   カラム値指定時に特殊指定用prefix文字列
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カラム値を指定する際にこの文字列を先頭につけると、この文字列より後の
    ///     文字列がそのまま使われます。
    ///   </para>
    /// </remarks>
    public string EscapeValuePrefix = DefaultEscapeValuePrefix;

    /// <summary>
    ///   EscapeValuePrefixのデフォルト値
    /// </summary>
    public static string DefaultEscapeValuePrefix = "!";

#endregion

#region レコード取出し

    /// <summary>
    ///   DBからレコードを取り出す
    /// </summary>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    public List<string[]> Get(int limit=0, int offset=0) {
        List<string[]> res = new List<string[]>();
        using(DBReader reader = Query(limit, offset)) {
            string[] item;
            while((item = reader.Get()) != null) {
                res.Add(item);
            }
        }
        return res;
    }

    /// <summary>
    ///   DBからレコードを取り出す。総件数検出機能付き
    /// </summary>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    /// <param name="totalcount">limitを無視した時の総レコード数を格納する変数</param>
    public List<string[]> Get(int limit, int offset, out int totalcount) {
        calcrows = true;
        totalcount = 0;
        List<string[]> res = new List<string[]>();
        using(DBReader reader = Query(limit, offset)) {
            string[] item;
            while((item = reader.Get()) != null) {
                res.Add(item);
                if((totalcount == 0) && dbcon.IsPostgreSQL) {
                    totalcount = StringUtil.ToInt(item[item.Length-1]);
                }
            }
        }
        if(dbcon.IsMySQL) {
            using(DBReader reader = dbcon.Query("SELECT FOUND_ROWS()")) {
                totalcount = StringUtil.ToInt(reader.Get()[0]);
            }
        }
        calcrows = false;
        return res;
    }

    /// <summary>
    ///   DBからレコードを取り出す
    /// </summary>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    public List<DataArray> GetData(int limit=0, int offset=0) {
        List<DataArray> res = new List<DataArray>();
        using(DBReader reader = Query(limit, offset)) {
            string[] item;
            while((item = reader.Get()) != null) {
                res.Add(new DataArray(Columns, item));
            }
        }
        return res;
    }

    /// <summary>
    ///   DBからレコードを取り出す。総件数検出機能付き
    /// </summary>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    /// <param name="totalcount">limitを無視した時の総レコード数を格納する変数</param>
    public List<DataArray> GetData(int limit, int offset, out int totalcount) {
        calcrows = true;
        totalcount = 0;
        List<DataArray> res = new List<DataArray>();
        using(DBReader reader = Query(limit, offset)) {
            string[] item;
            while((item = reader.Get()) != null) {
                res.Add(new DataArray(Columns, item));
                if((totalcount == 0) && dbcon.IsPostgreSQL) {
                    totalcount = StringUtil.ToInt(item[item.Length-1]);
                }
            }
        }
        if(dbcon.IsMySQL) {
            using(DBReader reader = dbcon.Query("SELECT FOUND_ROWS()")) {
                totalcount = StringUtil.ToInt(reader.Get()[0]);
            }
        }
        calcrows = false;
        return res;
    }

    /// <summary>
    ///   DBから1件だけレコードを取り出す。条件に合致するレコードが無い場合にはnullを返す。
    /// </summary>
    public string[] GetOne() {
        using(DBReader reader = Query(1, 0)) {
            return reader.Get();
        }
    }

    /// <summary>
    ///   DBから1件だけレコードを取り出す。条件に合致するレコードが無い場合にはnullを返す。
    /// </summary>
    public DataArray GetOneData() {
        using(DBReader reader = Query(1, 0)) {
            string[] item = reader.Get();
            if(item == null)
                return null;
            return new DataArray(Columns, item);
        }
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを1件取り出す
    /// </summary>
    /// <param name="columnname">検索キーにするカラム名の一覧</param>
    /// <param name="columnvalue">検索条件値。columnnameと同じ長さの配列を指定すること</param>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public string[] GetRecord(string[] columnname, object[] columnvalue) {
        ClearCondition();
        AddCondition(columnname, columnvalue);
        return GetOne();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを1件取り出す。
    ///   検索キーカラムを1つだけ指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public string[] GetRecord(string columnname, object columnvalue) {
        ClearCondition();
        AddCondition(columnname, columnvalue);
        return GetOne();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを取り出す。
    ///   検索キーカラムを2つ指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public string[] GetRecord(string columnname1, object columnvalue1,
                              string columnname2, object columnvalue2) {
        ClearCondition();
        AddCondition(columnname1, columnvalue1);
        AddCondition(columnname2, columnvalue2);
        return GetOne();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを取り出す。
    ///   検索キーカラムを3つ指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public string[] GetRecord(string columnname1, object columnvalue1,
                              string columnname2, object columnvalue2,
                              string columnname3, object columnvalue3) {
        ClearCondition();
        AddCondition(columnname1, columnvalue1);
        AddCondition(columnname2, columnvalue2);
        AddCondition(columnname3, columnvalue3);
        return GetOne();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを1件取り出す
    /// </summary>
    /// <param name="columnname">検索キーにするカラム名の一覧</param>
    /// <param name="columnvalue">検索条件値。columnnameと同じ長さの配列を指定すること</param>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public DataArray GetRecordData(string[] columnname, object[] columnvalue) {
        ClearCondition();
        AddCondition(columnname, columnvalue);
        return GetOneData();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを1件取り出す。
    ///   検索キーカラムを1つだけ指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public DataArray GetRecordData(string columnname, object columnvalue) {
        ClearCondition();
        AddCondition(columnname, columnvalue);
        return GetOneData();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを取り出す。
    ///   検索キーカラムを2つ指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public DataArray GetRecordData(string columnname1, object columnvalue1,
                                   string columnname2, object columnvalue2) {
        ClearCondition();
        AddCondition(columnname1, columnvalue1);
        AddCondition(columnname2, columnvalue2);
        return GetOneData();
    }

    /// <summary>
    ///   指定カラムが指定値であるレコードを取り出す。
    ///   検索キーカラムを3つ指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionが勝手にセットされることに注意。
    ///   </para>
    /// </remarks>
    public DataArray GetRecordData(string columnname1, object columnvalue1,
                                   string columnname2, object columnvalue2,
                                   string columnname3, object columnvalue3) {
        ClearCondition();
        AddCondition(columnname1, columnvalue1);
        AddCondition(columnname2, columnvalue2);
        AddCondition(columnname3, columnvalue3);
        return GetOneData();
    }

    /// <summary>
    ///   DBを検索する
    /// </summary>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    public DBReader Query(int limit=0, int offset=0) {
        return dbcon.Query(GetQuerySql(limit, offset));
    }

    /// <summary>
    ///   DBを検索するSQLコードを返す
    /// </summary>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    public string GetQuerySql(int limit, int offset) {
        StringBuilder sql = new StringBuilder();
        GetQuerySql(sql, limit, offset);
        return sql.ToString();
    }

    /// <summary>
    ///   DBを検索するSQLコードをStringBuilderに追加する
    /// </summary>
    /// <param name="sql">SQLコードを追加するStringBuilder</param>
    /// <param name="limit">最大取り出し件数。0の場合無制限</param>
    /// <param name="offset">何件目以降を返すか。先頭レコードは0</param>
    public void GetQuerySql(StringBuilder sql, int limit, int offset) {
        sql.Append("SELECT ");
        if(dbcon.IsMySQL && calcrows)
            sql.Append("SQL_CALC_FOUND_ROWS ");
        if(columns == null)
            SetAllColumns();
        if(sqlcolumns == null)
            sqlcolumns = columns;
        if(distinct)
            sql.Append("DISTINCT ");
        bool first = true;
        foreach(string col in sqlcolumns) {
            if(first)
                first = false;
            else
                sql.Append(",");
            sql.Append(expandColumnName(col));
        }
        if(dbcon.IsPostgreSQL && calcrows)
            sql.Append(",COUNT(*) OVER()");
        sql.Append(" FROM ");
        if(def != null) {
            sql.Append(def.Name);
            if(def.Name != name) {
                sql.Append(" AS ");
                sql.Append(name);
            }
        }else if(tbl != null) {
            sql.Append("(");
            sql.Append(tbl.GetQuerySql(0,0));
            sql.Append(")");
            sql.Append(" AS ");
            sql.Append(name);
        }
        expandJoin(sql);
        if(!IsNullCondition) {
            sql.Append(" WHERE ");
            sql.Append(condition.ToString());
        }
        if(groupby != null) {
            sql.Append(" GROUP BY ");
            bool gfirst = true;
            foreach(string gb in groupby) {
                if(gfirst)
                    gfirst = false;
                else
                    sql.Append(',');
                sql.Append(expandColumnName(gb));
            }
        }
        if(!IsNullHaving) {
            sql.Append(" HAVING ");
            sql.Append(having.ToString());
        }
        if(sortcolumns != null) {
            sql.Append(" ORDER BY ");
            first = true;
            foreach(string col in sortcolumns) {
                if(first)
                    first = false;
                else
                    sql.Append(",");
                sql.Append(col);
            }
        }
        if((limit > 0) || (offset > 0)) {
            sql.Append(" LIMIT ");
            if(limit > 0)
                sql.Append(limit);
            else
                sql.Append(Int32.MaxValue);
            sql.Append(" OFFSET ");
            sql.Append(offset);
        }
        if(forupdate)
            sql.Append(" FOR UPDATE");
    }

    /// <summary>
    ///   条件に合致するレコードの件数を獲得する
    /// </summary>
    public int GetCount() {
        StringBuilder sql = new StringBuilder();
        sql.Append("SELECT COUNT(1) FROM ");
        if(def != null) {
            sql.Append(def.Name);
            if(def.Name != name) {
                sql.Append(" AS ");
                sql.Append(name);
            }
        }else if(tbl != null) {
            sql.Append("(");
            sql.Append(tbl.GetQuerySql(0,0));
            sql.Append(")");
            sql.Append(" AS ");
            sql.Append(name);
        }
        if(groupby != null)
            throw new ArgumentException("Can't count up records with group-by clause.");
        expandJoin(sql);
        if(!IsNullCondition) {
            sql.Append(" WHERE ");
            sql.Append(condition.ToString());
        }
        return StringUtil.ToInt(dbcon.QueryString(sql), 0);
    }

#endregion

#region レコード追加削除

    /// <summary>
    ///   レコードを追加する。カラム名一覧を指定するバージョン。
    /// </summary>
    /// <param name="mycolumns">カラム名の一覧</param>
    /// <param name="rec">各カラムの値。mycolumnsと同じ長さの配列を指定しなければいけない</param>
    /// <remarks>
    ///   <para>
    ///     本オブジェクトのColumnsプロパティ値は無視される。
    ///     Conditionは無視される。
    ///     設定カラム値がnullの場合はテーブル定義時のデフォルト値がセットされる。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int Insert(string[] mycolumns, string[] rec) {
        return insert("INSERT", mycolumns, rec);
    }

    /// <summary>
    ///   レコードを追加する。カラム名一覧を指定するバージョン。
    /// </summary>
    /// <param name="mycolumns">カラム名の一覧</param>
    /// <param name="rec">各カラムの値。mycolumnsと同じ長さの配列を指定しなければいけない</param>
    /// <remarks>
    ///   <para>
    ///     主キーが一致するレコードが既に存在する場合、そのレコードを上書きする。
    ///     それ以外はInsertと同じ。
    ///   </para>
    /// </remarks>
    public int Replace(string[] mycolumns, string[] rec) {
        return insert("REPLACE", mycolumns, rec);
    }

    /// <summary>
    ///   レコードを追加する。
    /// </summary>
    /// <param name="rec">各カラムの値。Columnsと同じ長さの配列を指定しなければいけない</param>
    /// <remarks>
    ///   <para>
    ///     Conditionは無視される。
    ///     設定カラム値がnullの場合はテーブル定義時のデフォルト値がセットされる。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int Insert(string[] rec) {
        if(sqlcolumns == null)
            sqlcolumns = Columns;
        return Insert(sqlcolumns, rec);
    }

    /// <summary>
    ///   レコードを追加する。
    /// </summary>
    /// <param name="rec">各カラムの値。Columnsと同じ長さの配列を指定しなければいけない</param>
    /// <remarks>
    ///   <para>
    ///     主キーが一致するレコードが既に存在する場合、そのレコードを上書きする。
    ///     それ以外はInsertと同じ。
    ///   </para>
    /// </remarks>
    public int Replace(string[] rec) {
        if(sqlcolumns == null)
            sqlcolumns = Columns;
        return Replace(sqlcolumns, rec);
    }

    /// <summary>
    ///   レコードを追加する。DataArray指定バージョン
    /// </summary>
    /// <param name="rec">各カラムの値</param>
    /// <remarks>
    ///   <para>
    ///     recがカラム名指定を持つ場合、本オブジェクトのColumnsプロパティの値は
    ///     無視される。
    ///     recがカラム名指定を持たない場合、recのカラム数とColumnsの要素数が
    ///     一致しなければならない。
    ///     Conditionは無視される。
    ///     設定カラム値がnullの場合はテーブル定義時のデフォルト値がセットされる。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int Insert(DataArray rec) {
        string[] mycolumns = rec.Columns;
        if(mycolumns == null) {
            if(sqlcolumns == null)
                sqlcolumns = Columns;
            mycolumns = sqlcolumns;
        }
        return Insert(mycolumns, rec.Values);
    }

    /// <summary>
    ///   レコードを追加する。DataArray指定バージョン
    /// </summary>
    /// <param name="rec">各カラムの値</param>
    /// <remarks>
    ///   <para>
    ///     主キーが一致するレコードが既に存在する場合、そのレコードを上書きする。
    ///     それ以外はInsertと同じ。
    ///   </para>
    /// </remarks>
    public int Replace(DataArray rec) {
        string[] mycolumns = rec.Columns;
        if(mycolumns == null) {
            if(sqlcolumns == null)
                sqlcolumns = Columns;
            mycolumns = sqlcolumns;
        }
        return Replace(mycolumns, rec.Values);
    }

    /// <summary>
    ///   サブクエリを使ってレコードを追加する
    /// </summary>
    /// <param name="subquery">サブクエリを定義するDBTable</param>
    /// <param name="limit">サブクエリの最大検索件数。0は無制限</param>
    /// <param name="offset">サブクエリで何件目以降を返すか。先頭レコードは0</param>
    /// <remarks>
    ///   <para>
    ///     subquery.GetQuerySql(limit, offset)で得られるSELECT文を用いて
    ///     INSERTを実行する。
    ///     本オブジェクトのColumnsの要素数とsubqueryのColumnsの要素数は同じで
    ///     なければならない。
    ///     Conditionは無視される。
    ///     主キーが重複するとSQLエラー。
    ///   </para>
    /// </remarks>
    public int Insert(DBTable subquery, int limit, int offset) {
        return insert("INSERT", subquery, limit, offset);
    }

    /// <summary>
    ///   サブクエリを使ってレコードを追加する
    /// </summary>
    /// <param name="subquery">サブクエリを定義するDBTable</param>
    /// <param name="limit">サブクエリの最大検索件数。0は無制限</param>
    /// <param name="offset">サブクエリで何件目以降を返すか。先頭レコードは0</param>
    /// <remarks>
    ///   <para>
    ///     主キーが一致するレコードが既に存在する場合、そのレコードを上書きする。
    ///     それ以外はInsertと同じ。
    ///   </para>
    /// </remarks>
    public int Replace(DBTable subquery, int limit, int offset) {
        return insert("REPLACE", subquery, limit, offset);
    }

    /// <summary>
    ///   サブクエリを使ってレコードを追加する
    /// </summary>
    /// <param name="subquery">サブクエリを定義するDBTable</param>
    /// <remarks>
    ///   <para>
    ///     subquery.GetQuerySql(0,0)で得られるSELECT文を用いて
    ///     INSERTを実行する。
    ///     本オブジェクトのColumnsの要素数とsubqueryのColumnsの要素数は同じで
    ///     なければならない。
    ///     Conditionは無視される。
    ///     主キーが重複するとSQLエラー。
    ///   </para>
    /// </remarks>
    public int Insert(DBTable subquery) {
        return Insert(subquery, 0, 0);
    }

    /// <summary>
    ///   サブクエリを使ってレコードを追加する
    /// </summary>
    /// <param name="subquery">サブクエリを定義するDBTable</param>
    /// <remarks>
    ///   <para>
    ///     主キーが一致するレコードが既に存在する場合、そのレコードを上書きする。
    ///     それ以外はInsertと同じ。
    ///   </para>
    /// </remarks>
    public int Replace(DBTable subquery) {
        return Replace(subquery, 0, 0);
    }

    /// <summary>
    ///   レコードを更新する。カラム名一覧を指定するバージョン。
    /// </summary>
    /// <param name="mycolumns">カラム名の一覧</param>
    /// <param name="rec">各カラムの値。mycolumnsと同じ長さの配列を指定しなければいけない</param>
    /// <returns>更新したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     本オブジェクトのColumnsプロパティ値は無視される。
    ///     設定カラム値がnullの場合は値が更新されない。値をNULLにしたい場合には
    ///     "!NULL"と記述すること。
    ///     また、rowidカラムは更新されない。
    ///     Conditionに合致する全てのレコードが更新される。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int Update(string[] mycolumns, string[] rec) {
        if(mycolumns == null)
            throw new ArgumentNullException("No columns specified.");
        if(rec == null)
            throw new ArgumentNullException("No record data for update.");
        if(mycolumns.Length != rec.Length)
            throw new ArgumentException(String.Format("Invalid number of columns for update. (required={0}, provided={1})", mycolumns.Length, rec.Length));
        //if(joinList != null)
        //    throw new ArgumentException("Can't update records on joined table.");
        if(groupby != null)
            throw new ArgumentException("Can't update records with group-by clause.");
        if(IsNullCondition)
            throw new ArgumentException("Can't update records without condition.");
        if(def == null)
            throw new ArgumentException("Can't update on sub-query.");
        StringBuilder sql = new StringBuilder();
        sql.Append("UPDATE ");
        sql.Append(def.Name);
        if(def.Name != name) {
            sql.Append(" AS ");
            sql.Append(name);
        }
        expandJoin(sql);
        sql.Append(" SET ");
        bool first = true;
        for(int i = 0; i < mycolumns.Length; i++) {
            if((mycolumns[i] == "rowid") || (rec[i] == null))
                continue;
            if(first)
                first = false;
            else
                sql.Append(",");
            sql.Append(expandColumnName(mycolumns[i]));
            sql.Append("=");
            DBColumnDef coldef = GetColumnDef(mycolumns[i]);
            if(coldef == null)
                throw new ArgumentException(String.Format("Invalid column name ({0})", mycolumns[i]));
            sql.Append(ConvertValue(coldef, rec[i]));
        }
        sql.Append(" WHERE ");
        sql.Append(condition.ToString());
        return dbcon.Execute(sql);
    }

    /// <summary>
    ///   レコードを更新する。
    /// </summary>
    /// <param name="rec">各カラムの値。mycolumnsと同じ長さの配列を指定しなければいけない</param>
    /// <returns>更新したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     設定カラム値がnullの場合は値が更新されない。また、rowidカラムは更新されない。
    ///     Conditionに合致する全てのレコードが更新される。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int Update(string[] rec) {
        if(sqlcolumns == null)
            sqlcolumns = Columns;
        return Update(sqlcolumns, rec);
    }

    /// <summary>
    ///   レコードを更新する。DataArray指定バージョン
    /// </summary>
    /// <param name="rec">各カラムの値</param>
    /// <returns>更新したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     recがカラム名指定を持つ場合、本オブジェクトのColumnsプロパティの値は
    ///     無視される。
    ///     recがカラム名指定を持たない場合、recのカラム数とColumnsの要素数が
    ///     一致しなければならない。
    ///     設定カラム値がnullの場合は値が更新されない。また、rowidカラムは更新されない。
    ///     Conditionに合致する全てのレコードが更新される。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int Update(DataArray rec) {
        string[] mycolumns = rec.Columns;
        if(mycolumns == null) {
            if(sqlcolumns == null)
                sqlcolumns = Columns;
            mycolumns = sqlcolumns;
        }
        return Update(mycolumns, rec.Values);
    }

    /// <summary>
    ///   レコードを更新または追加する。カラム名一覧を指定するバージョン。
    /// </summary>
    /// <param name="mycolumns">カラム名の一覧</param>
    /// <param name="rec">各カラムの値。mycolumnsと同じ長さの配列を指定しなければいけない</param>
    /// <returns>更新したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     本オブジェクトのColumnsプロパティ値は無視される。
    ///     設定カラム値がnullの場合は値が更新されない。また、rowidカラムは更新されない。
    ///     Conditionに合致する全てのレコードが更新される。
    ///     Conditionに合致するレコードが存在しない場合、Insertが実行される。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int UpdateOrInsert(string[] mycolumns, string[] rec) {
        if(mycolumns == null)
            throw new ArgumentNullException("No columns specified.");
        if(rec == null)
            throw new ArgumentNullException("No record data for update.");
        if(mycolumns.Length != rec.Length)
            throw new ArgumentException(String.Format("Invalid number of columns for update. (required={0}, provided={1})", mycolumns.Length, rec.Length));
        if(joinList != null)
            throw new ArgumentException("Can't update records on joined table.");
        if(groupby != null)
            throw new ArgumentException("Can't update records with group-by clause.");
        if(IsNullCondition)
            throw new ArgumentException("Can't update records without condition.");
        if(def == null)
            throw new ArgumentException("Can't update records on sub-query.");
        StringBuilder sql = new StringBuilder();
        sql.Append("UPDATE ");
        sql.Append(def.Name);
        sql.Append(" SET ");
        bool first = true;
        for(int i = 0; i < mycolumns.Length; i++) {
            if((mycolumns[i] == "rowid") || (rec[i] == null))
                continue;
            if(first)
                first = false;
            else
                sql.Append(",");
            sql.Append(mycolumns[i]);
            sql.Append("=");
            DBColumnDef coldef = GetColumnDef(mycolumns[i]);
            if(coldef == null)
                throw new ArgumentException(String.Format("Invalid column name ({0})", mycolumns[i]));
            sql.Append(ConvertValue(coldef, rec[i]));
        }
        sql.Append(" WHERE ");
        sql.Append(condition.ToString());
        int recno = dbcon.Execute(sql);
        if(recno > 0)
            return recno;
        return Insert(mycolumns,rec);
    }

    /// <summary>
    ///   レコードを更新または追加する。
    /// </summary>
    /// <param name="rec">各カラムの値。Columnsと同じ長さの配列を指定しなければいけない</param>
    /// <returns>更新したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     設定カラム値がnullの場合は値が更新されない。また、rowidカラムは更新されない。
    ///     Conditionに合致する全てのレコードが更新される。
    ///     Conditionに合致するレコードが存在しない場合、Insertが実行される。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int UpdateOrInsert(string[] rec) {
        if(sqlcolumns == null)
            sqlcolumns = Columns;
        return UpdateOrInsert(sqlcolumns, rec);
    }

    /// <summary>
    ///   レコードを更新または追加する。DataArray指定バージョン
    /// </summary>
    /// <param name="rec">各カラムの値</param>
    /// <returns>更新したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     recがカラム名指定を持つ場合、本オブジェクトのColumnsプロパティの値は
    ///     無視される。
    ///     recがカラム名指定を持たない場合、recのカラム数とColumnsの要素数が
    ///     一致しなければならない。
    ///     設定カラム値がnullの場合は値が更新されない。また、rowidカラムは更新されない。
    ///     Conditionに合致する全てのレコードが更新される。
    ///     Conditionに合致するレコードが存在しない場合、Insertが実行される。
    ///     主キーが重複するとSQLエラー。
    ///     recで指定した値は、カラムの型に応じて正しいSQLのデータ表記に代えられる。
    ///     ただし、"!"で始まる文字列を指定した場合は"!"よりあとの文字列がそのまま使われる。（他項目の値を指定する時等に利用する。）
    ///   </para>
    /// </remarks>
    public int UpdateOrInsert(DataArray rec) {
        string[] mycolumns = rec.Columns;
        if(mycolumns == null) {
            if(sqlcolumns == null)
                sqlcolumns = Columns;
            mycolumns = sqlcolumns;
        }
        return UpdateOrInsert(mycolumns, rec.Values);
    }

    /// <summary>
    ///   レコードを削除する
    /// </summary>
    /// <returns>削除したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     Conditionで指定した全てのレコードが削除される。
    ///   </para>
    /// </remarks>
    public int Delete() {
        return Delete(0);
    }

    /// <summary>
    ///   レコードを削除する
    /// </summary>
    /// <returns>削除したレコード数</returns>
    /// <param name="limit">最大削除件数。0の場合無制限</param>
    /// <remarks>
    ///   <para>
    ///     Conditionで指定した全てのレコードが削除される。
    ///   </para>
    /// </remarks>
    public int Delete(int limit) {
        if(IsNullCondition)
            throw new ArgumentException("Can't delete records without condition.");
        if(def == null)
            throw new ArgumentException("Can't delete records on sub-query.");
        StringBuilder sql = new StringBuilder();
        sql.Append("DELETE ");
        if(joinList != null) {
            sql.Append(name);
            sql.Append(' ');
        }
        sql.Append("FROM ");
        sql.Append(def.Name);
        if(def.Name != name) {
            sql.Append(" AS ");
            sql.Append(name);
        }
        expandJoin(sql);
        sql.Append(" WHERE ");
        sql.Append(condition.ToString());
        if(limit > 0) {
            sql.Append(" LIMIT ");
            sql.Append(limit);
        }
        return dbcon.Execute(sql);
    }

    /// <summary>
    ///   全レコードを削除する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Conditionを無視し、すべてのレコードを削除します。
    ///     トランザクションは強制的にCommitされます。
    ///     Joinしたテーブルに用いることはできません。
    ///   </para>
    /// </remarks>
    public void Truncate() {
        if(joinList != null)
            throw new ArgumentException("Can't truncate joined tables.");
        if(def == null)
            throw new ArgumentException("Can't truncate on sub-query.");
        dbcon.Execute("TRUNCATE TABLE "+def.Name);
    }


#endregion

#region ダンプアウト

    /// <summary>
    ///   指定ファイルにCSVダンプアウトする
    /// </summary>
    /// <param name="limit">最大件数</param>
    /// <param name="offset">何軒目以降を出力するか。先頭レコードは0</param>
    /// <param name="filepath">出力ファイル名</param>
    /// <param name="columnNameFlag">カラム名を出力するかどうか</param>
    /// <returns>書き出したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     出力データ中のバックスラッシュ、カンマ、ダブルクォート、シャープは、
    ///     バックスラッシュを前置してエスケープします。
    ///   </para>
    /// </remarks>
    public int DumpCSV(string filepath, int limit=0, int offset=0, bool columnNameFlag=true) {
        using(StreamWriter sw = FileUtil.Writer(filepath, new UTF8Encoding())) {
            if(sw == null)
                throw new IOException(String.Format("Can't open {0} for writing.", filepath));
            return DumpCSV(sw, limit, offset, columnNameFlag);
        }
    }

    /// <summary>
    ///   指定ストリームにCSVダンプアウトする
    /// </summary>
    /// <param name="limit">最大件数</param>
    /// <param name="offset">何軒目以降を出力するか。先頭レコードは0</param>
    /// <param name="sw">出力先</param>
    /// <param name="columnNameFlag">カラム名を出力するかどうか</param>
    /// <returns>書き出したレコード数</returns>
    /// <remarks>
    ///   <para>
    ///     出力データ中のバックスラッシュ、カンマ、ダブルクォート、シャープは、
    ///     バックスラッシュを前置してエスケープします。
    ///   </para>
    /// </remarks>
    public int DumpCSV(StreamWriter sw, int limit=0, int offset=0, bool columnNameFlag=true) {
        if(columnNameFlag) {
            sw.Write("# number");
            foreach(string col in Columns) {
                sw.Write(",");
                sw.Write(csvEscape(col));
            }
            sw.WriteLine();
        }
        int c = 0;
        using(DBReader reader = Query(limit, offset)) {
            string[] item;
            while((item = reader.Get()) != null) {
                c++;
                sw.Write(c.ToString());
                foreach(string x in item) {
                    sw.Write(",");
                    sw.Write(csvEscape(x));
                }
                sw.WriteLine();
            }
        }
        return c;
    }

    /// <summary>
    ///   指定ファイルからCSVを読み取り、テーブルにINSERTする。
    /// </summary>
    /// <param name="filepath">読み取りファイル</param>
    /// <param name="ignoreBadRecord">カラム数不足,カラム数過多,INSERT時のDBエラーを無視するかどうか</param>
    /// <param name="testRun">true=ファイルフォーマットチェックだけ行ない、INSERTは実行しない。false=INSERTする。</param>
    /// <remarks>
    ///   <para>
    ///     dumpCSVで作成されたファイルを読み取ることを前提としています。
    ///     "# "で始まる最初の行をカラム定義として用います。
    ///     カラム定義行が無いまま"#"以外で始まる行を見つけた場合、全カラムが
    ///     カラム定義順に指定されたものとみなします。
    ///     カンマの前後の空白はデータの一部とみなされます。
    ///     本メソッドは、Columns定義とCondition定義を勝手に書き換えます。
    ///     Joinされたテーブルでは使用できません。
    ///   </para>
    /// </remarks>
    // <returns>INSERTしたレコード数</returns>
    public int LoadCSV(string filepath, bool ignoreBadRecord=false, bool testRun=false) {
        using(StreamReader sr = FileUtil.Reader(filepath, new UTF8Encoding())) {
            if(sr == null)
                throw new IOException(String.Format("Can't open {0} for reading.", filepath));
            return LoadCSV(sr, ignoreBadRecord, testRun);
        }
    }

    /// <summary>
    ///   指定ストリームからCSVを読み取り、テーブルにINSERTする。
    /// </summary>
    /// <param name="sr">読み取りストリーム</param>
    /// <param name="ignoreBadRecord">カラム数不足,カラム数過多,INSERT時のDBエラーを無視するかどうか</param>
    /// <param name="testRun">true=ファイルフォーマットチェックだけ行ない、INSERTは実行しない。false=INSERTする。</param>
    /// <remarks>
    ///   <para>
    ///     dumpCSVで作成されたファイルを読み取ることを前提としています。
    ///     "# "で始まる最初の行をカラム定義として用います。
    ///     カラム定義行が無いまま"#"以外で始まる行を見つけた場合、全カラムが
    ///     カラム定義順に指定されたものとみなします。
    ///     カンマの前後の空白はデータの一部とみなされます。
    ///     本メソッドは、Columns定義とCondition定義を勝手に書き換えます。
    ///     Joinされたテーブルでは使用できません。
    ///     データベーストランザクションは自動的にCommitされます。
    ///   </para>
    /// </remarks>
    // <returns>INSERTしたレコード数</returns>
    public int LoadCSV(StreamReader sr, bool ignoreBadRecord=false, bool testRun=false) {
        if(joinList != null)
            throw new ArgumentException("Can't load CSV records to joined table.");
        if(!testRun)
            dbcon.Begin();
        SetAllColumns();
        List<IndexAndName> coldefs = null;
        string[] cols = null;
        string[] vals = null;
        int count = 0;
        bool inHead = true;
        int lineno = 0;
        while(!sr.EndOfStream) {
            string line = sr.ReadLine();
            lineno++;
            if(line == "") // 空行は無視。
                continue;
            if(inHead && line.StartsWith("# ")) {
                // # の後に書かれているカラム名のうち、このテーブルのカラム名に
                // あるものだけを用いる。
                coldefs = new List<IndexAndName>();
                int idx = 0;
                foreach(string x in line.Substring(2).Split(",".ToCharArray())) {
                    if(idx == 0) { // 先頭のカラムは常に無視
                        idx++;
                        continue;
                    }
                    foreach(string c in columns) {
                        if(c == x) {
                            coldefs.Add(new IndexAndName(idx,x));
                            break;
                        }
                    }
                    idx++;
                }
                inHead = false;
                continue;
            }
            if(line.StartsWith("#")) // コメント行は無視
                continue;
            // データ行を見つけた
            inHead = false;
            if(coldefs == null) {
                // カラム定義行が無かったのでデフォルトを作成する
                coldefs = new List<IndexAndName>();
                for(int i = 0; i < columns.Length; i++) {
                    coldefs.Add(new IndexAndName(i+1, columns[i]));
                }
            }
            if(coldefs.Count == 0) {
                // カラム定義が異常
                if(!ignoreBadRecord)
                    throw new ArgumentException("Invalid CSV column definition");
                dbcon.LOG_NOTICE("Invalid CSV column definition, ignore this table.");
                break;
            }
            if(cols == null) {
                // 挿入するカラム名一覧
                cols = new string[coldefs.Count];
                for(int i = 0; i < coldefs.Count; i++)
                    cols[i] = coldefs[i].Name;
            }
            if(vals == null) {
                // 挿入するデータを格納する配列
                vals = new string[cols.Length];
            }
            // 挿入データを作成する
            string[] csv = csvSplit(line);
            bool good = true;
            for(int i = 0; i < coldefs.Count; i++) {
                int idx = coldefs[i].Index;
                if(csv.Length < idx) {
                    good = false;
                    break;
                }
                vals[i] = csv[idx];
            }
            if(!good) {
                if(!ignoreBadRecord) {
                    if(!testRun)
                        dbcon.Rollback();
                    throw new ArgumentException(String.Format("Invalid CSV record in line {0}", lineno));
                }
                dbcon.LOG_NOTICE("Invalid CSV record in line {0}, ignored.", lineno);
                continue;
            }
            try {
                if(!testRun)
                    Insert(cols, vals);
                count++;
            } catch(DbException ex) {
                if(!ignoreBadRecord) {
                    if(!testRun)
                        dbcon.Rollback();
                    throw ex;
                }
                dbcon.LOG_NOTICE("Failed to insert CSV record in line {0}, ignored: {1}", lineno, ex.Message);
                continue;
            }
        }
        if(!testRun)
            dbcon.Commit();
        return count;
    }

#endregion

#region テーブル属性

    /// <summary>
    ///   使用DB
    /// </summary>
    public DBCon Db {
        get { return dbcon; }
    }

    /// <summary>
    ///   テーブル名
    /// </summary>
    public string Name {
        get { return name; }
    }

#endregion

#region private部

    private DBTableDef getTableDef(string tblname) {
        if(tblname == null)
            throw new ArgumentNullException("Table name must not be null.");
        DBTableDef tabledef = DBTableDef.Get(tblname);
        if(tabledef == null)
            throw new ArgumentException(String.Format("No such table ({0})", tblname));
        return tabledef;
    }

    private DBTable addCondition(ref StringBuilder condvar, string cond) {
        if(String.IsNullOrEmpty(cond))
            return this;
        if(isNullCondition(ref condvar)) {
            condvar = new StringBuilder(cond);
            return this;
        }
        condvar.Append(" AND ");
        condvar.Append(cond);
        return this;
    }

    private DBTable addCondition(ref StringBuilder condvar, DBCondition.Code conditioncode, string columnname, object columnvalue) {
        if(String.IsNullOrEmpty(columnname))
            throw new ArgumentException("Column name is null.");
        if(condvar == null)
            condvar = new StringBuilder();
        if(condvar.Length > 0)
            condvar.Append(" AND ");
        if(columnvalue == null) {
            switch(conditioncode) {
            case DBCondition.Code.Equals:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(" IS NULL");
                break;
            case DBCondition.Code.NotEquals:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(" IS NOT NULL");
                break;
            default:
                throw new ArgumentException(String.Format("Invalid condition code ({0})", conditioncode.ToString()));
            }
        } else {
            DBColumnDef coldef = GetColumnDef(columnname);
            switch(conditioncode) {
            case DBCondition.Code.Equals:
                condvar.Append(expandColumnName(columnname));
                condvar.Append("=");
                condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                break;
            case DBCondition.Code.NotEquals:
                condvar.Append(expandColumnName(columnname));
                condvar.Append("<>");
                condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                break;
            case DBCondition.Code.Contains:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(" LIKE '%");
                condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                condvar.Append("%'");
                break;
            case DBCondition.Code.NotContains:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(" NOT LIKE '%");
                condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                condvar.Append("%'");
                break;
            case DBCondition.Code.StartsWith:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(" LIKE '");
                condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                condvar.Append("%'");
                break;
            case DBCondition.Code.EndsWith:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(" LIKE '%");
                condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                condvar.Append("'");
                break;
            case DBCondition.Code.GreaterOrEqual:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(">=");
                condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                break;
            case DBCondition.Code.GreaterThan:
                condvar.Append(expandColumnName(columnname));
                condvar.Append(">");
                condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                break;
            case DBCondition.Code.LessOrEqual:
                condvar.Append(expandColumnName(columnname));
                condvar.Append("<=");
                condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                break;
            case DBCondition.Code.LessThan:
                condvar.Append(expandColumnName(columnname));
                condvar.Append("<");
                condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                break;
            case DBCondition.Code.In:
                if(columnvalue is DBTable) {
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" IN (");
                    condvar.Append(((DBTable)columnvalue).GetQuerySql(0,0));
                } else if(columnvalue is DataArray) {
                    if(((DataArray)columnvalue).Length == 0) {
                        condvar.Append("(0=1");
                    } else {
                        condvar.Append(expandColumnName(columnname));
                        condvar.Append(" IN (");
                        bool first = true;
                        foreach(string x in ((DataArray)columnvalue).Values) {
                            if(first)
                                first = false;
                            else
                                condvar.Append(',');
                            condvar.Append(ConvertValue(coldef,x));
                        }
                    }
                } else if(columnvalue is Array) {
                    if(((Array)columnvalue).Length == 0) {
                        condvar.Append("(0=1");
                    } else {
                        condvar.Append(expandColumnName(columnname));
                        condvar.Append(" IN (");
                        bool first = true;
                        foreach(object x in (Array)columnvalue) {
                            if(first)
                                first = false;
                            else
                                condvar.Append(',');
                            condvar.Append(ConvertValue(coldef,x.ToString()));
                        }
                    }
                } else {
                    throw new ArgumentException("The type of the value for IN condition must be Array, DBTable or DataArray.");
                }
                condvar.Append(")");
                break;
            case DBCondition.Code.NotIn:
                if(columnvalue is DBTable) {
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT IN (");
                    condvar.Append(((DBTable)columnvalue).GetQuerySql(0,0));
                } else if(columnvalue is DataArray) {
                    if(((DataArray)columnvalue).Length == 0) {
                        condvar.Append("(1=1");
                    } else {
                        condvar.Append(expandColumnName(columnname));
                        condvar.Append(" NOT IN (");
                        bool first = true;
                        foreach(string x in ((DataArray)columnvalue).Values) {
                            if(first)
                                first = false;
                            else
                                condvar.Append(',');
                            condvar.Append(ConvertValue(coldef,x));
                        }
                    }
                } else if(columnvalue is Array) {
                    if(((Array)columnvalue).Length == 0) {
                        condvar.Append("(1=1");
                    } else {
                        condvar.Append(expandColumnName(columnname));
                        condvar.Append(" NOT IN (");
                        bool first = true;
                        foreach(object x in (Array)columnvalue) {
                            if(first)
                                first = false;
                            else
                                condvar.Append(',');
                            condvar.Append(ConvertValue(coldef,x.ToString()));
                        }
                    }
                } else {
                    throw new ArgumentException("The type of the value for NOT IN condition must be Array, DBTable or DataArray.");
                }
                condvar.Append(")");
                break;
            case DBCondition.Code.CollateEquals:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append("=");
                    condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                    condvar.Append(" COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" SIMILAR TO '");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.NotCollateEquals:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append("<>");
                    condvar.Append(ConvertValue(coldef, columnvalue.ToString()));
                    condvar.Append(" COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT SIMILAR TO '");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.CollateContains:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" LIKE '%");
                    condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                    condvar.Append("%' COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" SIMILAR TO '%");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("%'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.NotCollateContains:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT LIKE '%");
                    condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                    condvar.Append("%' COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT SIMILAR TO '%");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("%'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.CollateStartsWith:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" LIKE '");
                    condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                    condvar.Append("%' COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" SIMILAR TO '");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("%'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.NotCollateStartsWith:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT LIKE '");
                    condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                    condvar.Append("%' COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT SIMILAR TO '");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("%'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.CollateEndsWith:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" LIKE '%");
                    condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                    condvar.Append("' COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" SIMILAR TO '%");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            case DBCondition.Code.NotCollateEndsWith:
                switch(dbcon.DBType) {
                case DBCon.Type.MySQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT LIKE '%");
                    condvar.Append(DBCon.LikeEscape(columnvalue.ToString()));
                    condvar.Append("' COLLATE utf8_unicode_ci");
                    break;
                case DBCon.Type.PostgreSQL:
                    condvar.Append(expandColumnName(columnname));
                    condvar.Append(" NOT SIMILAR TO '%");
                    addSimilarToExpression(condvar, columnvalue.ToString());
                    condvar.Append("'");
                    break;
                default:
                    throw new InvalidOperationException(conditioncode.ToString()+" is not supported on "+dbcon.DBType.ToString());
                }
                break;
            default:
                throw new ArgumentException(String.Format("Invalid condition code ({0})", conditioncode.ToString()));
            }
        }
        return this;
    }

    private DBTable addCondition(ref StringBuilder condvar, DBCondition.Code conditioncode, string columnname, object columnvalue1, object columnvalue2) {
        if(String.IsNullOrEmpty(columnname))
            throw new ArgumentException("Column name is null.");
        if(condvar == null)
            condvar = new StringBuilder();
        if(condvar.Length > 0)
            condvar.Append(" AND ");
        DBColumnDef coldef = GetColumnDef(columnname);
        switch(conditioncode) {
        case DBCondition.Code.Between:
            condvar.Append(expandColumnName(columnname));
            condvar.Append(" BETWEEN ");
            condvar.Append(ConvertValue(coldef, columnvalue1.ToString()));
            condvar.Append(" AND ");
            condvar.Append(ConvertValue(coldef, columnvalue2.ToString()));
            break;
        case DBCondition.Code.NotBetween:
            condvar.Append(expandColumnName(columnname));
            condvar.Append(" NOT BETWEEN ");
            condvar.Append(ConvertValue(coldef, columnvalue1.ToString()));
            condvar.Append(" AND ");
            condvar.Append(ConvertValue(coldef, columnvalue2.ToString()));
            break;
        default:
            throw new ArgumentException(String.Format("Invalid condition code ({0})", conditioncode.ToString()));
        }
        return this;
    }

    private DBTable addCondition(ref StringBuilder condvar, DBCondition.Code conditioncode, object subquery) {
        if(condvar == null)
            condvar = new StringBuilder();
        if(condvar.Length > 0)
            condvar.Append(" AND ");
        switch(conditioncode) {
        case DBCondition.Code.Exists:
            condvar.Append(" EXISTS (");
            if(subquery is DBTable) {
                condvar.Append(((DBTable)subquery).GetQuerySql(0,0));
            } else {
                condvar.Append(subquery.ToString());
            }
            condvar.Append(")");
            break;
        case DBCondition.Code.NotExists:
            condvar.Append(" NOT EXISTS (");
            if(subquery is DBTable) {
                condvar.Append(((DBTable)subquery).GetQuerySql(0,0));
            } else {
                condvar.Append(subquery.ToString());
            }
            condvar.Append(")");
            break;
        default:
            throw new ArgumentException(String.Format("Invalid condition code ({0})", conditioncode.ToString()));
        }
        return this;
    }

    private DBTable addCondition(ref StringBuilder condvar, string[] columnnames, object[] columnvalues) {
        if((columnnames == null) || (columnvalues == null)
           || (columnnames.Length != columnvalues.Length))
            throw new ArgumentException("Invalid column names specification.");
        for(int i = 0; i < columnnames.Length; i++)
            addCondition(ref condvar, DBCondition.Code.Equals, columnnames[i], columnvalues[i]);
        return this;
    }

    private const string similarEscapeChars = "%_|*+?{,}()[]";

    private static void addSimilarToExpression(StringBuilder sb, string txt) {
        foreach(char ch in DBCon.Escape(txt)) {
            char[] chlist = StringUtil.GetSimilarChars(ch);
            if(chlist.Length <= 1) {
                if(similarEscapeChars.IndexOf(ch) >= 0)
                    sb.Append('\\');
                sb.Append(ch);
            } else {
                sb.Append('(');
                bool first = true;
                foreach(char xch in chlist) {
                    if(first)
                        first = false;
                    else
                        sb.Append('|');
                    if(similarEscapeChars.IndexOf(ch) >= 0)
                        sb.Append('\\');
                    sb.Append(xch);
                }
                sb.Append(')');
            }
        }
    }

    private DBTable clearCondition(ref StringBuilder condvar) {
        condvar = null;
        return this;
    }

    private bool isNullCondition(ref StringBuilder condvar) {
        return (condvar == null) || (condvar.Length == 0);
    }

    private int insert(string cmd, string[] mycolumns, string[] rec) {
        if(mycolumns == null)
            throw new ArgumentNullException("No columns specified.");
        if(rec == null)
            throw new ArgumentNullException("No record data to insert.");
        if(mycolumns.Length != rec.Length)
            throw new ArgumentException(String.Format("Invalid number of columns for insert. (required={0}, provided={1})", mycolumns.Length, rec.Length));
        if(joinList != null)
            throw new ArgumentException("Can't insert records into joined table.");
        if(groupby != null)
            throw new ArgumentException("Can't insert records with group-by clause.");
        if(def == null)
            throw new ArgumentException("Can't insert records on sub-query.");
        StringBuilder sql = new StringBuilder();
        sql.Append(cmd);
        sql.Append(" INTO ");
        sql.Append(def.Name);
        StringBuilder cols = new StringBuilder();
        StringBuilder vals = new StringBuilder();
        bool first = true;
        for(int i = 0; i < mycolumns.Length; i++) {
            if((mycolumns[i] == "rowid") || (rec[i] == null))
                continue;
            if(first) {
                first = false;
            } else {
                cols.Append(",");
                vals.Append(",");
            }
            DBColumnDef coldef = GetColumnDef(mycolumns[i]);
            if(coldef == null)
                throw new ArgumentException(String.Format("Invalid column name ({0})", mycolumns[i]));
            cols.Append(mycolumns[i]);
            vals.Append(ConvertValue(coldef, rec[i]));
        }
        sql.Append(" (");
        sql.Append(cols.ToString());
        sql.Append(") VALUES (");
        sql.Append(vals.ToString());
        sql.Append(")");
        return dbcon.Execute(sql);
    }

    private int insert(string cmd, DBTable subquery, int limit, int offset) {
        if(sqlcolumns == null)
            sqlcolumns = Columns;
        string[] mycolumns = sqlcolumns;
        if(mycolumns.Length != subquery.Columns.Length)
            throw new ArgumentException(String.Format("Invalid number of columns for insert. (required={0}, provided={1})", mycolumns.Length, subquery.Columns.Length));
        if(joinList != null)
            throw new ArgumentException("Can't insert records into joined table.");
        if(groupby != null)
            throw new ArgumentException("Can't insert records with group-by clause.");
        if(def == null)
            throw new ArgumentException("Can't insert records on sub-query.");
        StringBuilder sql = new StringBuilder();
        sql.Append(cmd);
        sql.Append(" INTO ");
        sql.Append(def.Name);
        sql.Append(" (");
        bool first = true;
        for(int i = 0; i < mycolumns.Length; i++) {
            if(first) {
                first = false;
            } else {
                sql.Append(",");
            }
            DBColumnDef coldef = GetColumnDef(mycolumns[i]);
            if(coldef == null)
                throw new ArgumentException(String.Format("Invalid column name ({0})", mycolumns[i]));
            sql.Append(mycolumns[i]);
        }
        sql.Append(") ");
        subquery.GetQuerySql(sql, limit, offset);
        return dbcon.Execute(sql);
    }


    private string ConvertValue(DBColumnDef col, string val) {
        if(val == null)
            return "NULL";
        if(!String.IsNullOrEmpty(EscapeValuePrefix) && val.StartsWith(EscapeValuePrefix))
            return val.Substring(EscapeValuePrefix.Length);
        val = val.Trim();
        if(col == null)
            return DBCon.Literal(val);
        if(col.Type == "NUMBER") {
            if(col.FractionalLength > 0) {
                double d = StringUtil.ToDouble(val);
                string fmt = String.Format("F{0}",col.FractionalLength);
                return d.ToString(fmt);
            } else {
                return StringUtil.ToInt(val).ToString();
            }
        }
        if(col.Type == "BOOLEAN")
            return StringUtil.ToInt(val).ToString();
        if((col.Type == "DATE")||(col.Type == "DATETIME")) {
            if(val == "")
                return DBCon.Literal("0001-01-01");
            else
                return DBCon.Literal(val);
        }
        return DBCon.Literal(val);
    }

    private string expandColumnName(string col, bool force=false) {
        StringBuilder sb = new StringBuilder();
        if((col.IndexOf(".",StringComparison.Ordinal) < 0) && (col.IndexOf(" as ",StringComparison.OrdinalIgnoreCase) < 0)) {
            if((col == "rowid") || ((def != null) && def.HasColumn(col)) || ((tbl != null) && tbl.ContainsColumn(col))) {
                if(force || (joinList != null) || ((def != null) && name != def.Name) || (tbl != null)) {
                    sb.Append(name);
                    sb.Append('.');
                }
            } else if(joinList != null){
                foreach(JoinTable jt in joinList) {
                    if(((jt.def != null) && jt.def.HasColumn(col)) || ((jt.tbl != null) && jt.tbl.ContainsColumn(col))) {
                        sb.Append(jt.name);
                        sb.Append('.');
                        break;
                    }
                }
            }
        }
        sb.Append(col);
        return sb.ToString();
    }

    private void expandJoin(StringBuilder sql) {
        if(joinList == null)
            return;
        foreach(JoinTable jt in joinList) {
            sql.Append(' ');
            sql.Append(jt.mode);
            sql.Append(" JOIN ");
            if(jt.def != null) {
                sql.Append(jt.def.Name);
                if(jt.def.Name != jt.name) {
                    sql.Append(" AS ");
                    sql.Append(jt.name);
                }
            } else if(jt.tbl != null) {
                sql.Append('(');
                jt.tbl.GetQuerySql(sql, 0, 0);
                sql.Append(')');
                if(jt.tbl.Name != jt.name) {
                    sql.Append(" AS ");
                    sql.Append(jt.name);
                }
            } else {
                throw new ArgumentException("This joined table does not have valid definition");
            }
            sql.Append(" ON ");
            sql.Append(jt.condition);
        }
    }
    
    private static string csvEscape(string str) {
        StringBuilder sb = new StringBuilder();
        foreach(char ch in str) {
            switch(ch) {
            case '\\':
            case ',':
            case '"':
            case '#':
                sb.Append('\\');
                sb.Append(ch);
                break;
            case '\n':
                sb.Append("\\n");
                break;
            case '\r':
                sb.Append("\\r");
                break;
            default:
                sb.Append(ch);
                break;
            }
        }
        return sb.ToString();
    }

    private static string[] csvSplit(string str) {
        List<string> rec = new List<string>();
        StringBuilder sb = new StringBuilder();
        bool inEscape = false;
        foreach(char ch in str) {
            if(inEscape) {
                switch(ch) {
                case 'n':
                    sb.Append('\n');
                    break;
                case 'r':
                    sb.Append('\r');
                    break;
                default:
                    sb.Append(ch);
                    break;
                }
                inEscape = false;
            } else { // not inEscape
                switch(ch) {
                case '\\':
                    inEscape = true;
                    break;
                case ',':
                    rec.Add(sb.ToString());
                    sb.Clear();
                    break;
                default:
                    sb.Append(ch);
                    break;
                }
            }
        }
        rec.Add(sb.ToString());
        return rec.ToArray();
    }


    private class JoinTable {
        public JoinTable(string name_, DBTableDef def_, string mode_, string condition_) {
            name = name_;
            def = def_;
            tbl = null;
            mode = mode_;
            condition = condition_;
        }
        public JoinTable(string name_, DBTable tbl_, string mode_, string condition_) {
            name = name_;
            def = null;
            tbl = tbl_;
            mode = mode_;
            condition = condition_;
        }
        public readonly string name;
        public readonly DBTableDef def;
        public readonly DBTable tbl;
        public readonly string mode;
        public readonly string condition;
    }

    private class IndexAndName {
        public readonly int Index;
        public readonly string Name;
        public IndexAndName(int index, string name) {
            Index = index;
            Name = name;
        }
    }

    private readonly DBCon dbcon;
    private readonly DBTableDef def;
    private readonly DBTable tbl;
    private readonly string name;
    private string[] columns;
    private string[] sqlcolumns;
    private string[] sortcolumns;
    private StringBuilder condition;
    private StringBuilder having;
    private List<JoinTable> joinList;
    private string[] groupby;
    private bool forupdate;
    private bool calcrows;
    private bool distinct;

#endregion

}

} // End of namespace
