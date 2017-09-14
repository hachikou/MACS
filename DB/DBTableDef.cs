/*
  * データベーステーブル定義
  * $Id: DBTableDef.cs 1890 2014-06-05 04:34:56Z shibuya $
  *
  * Copyright (C) 2011-2012 Microbrains Inc. All rights reserved.
  * This code was designed and coded by SHIBUYA K.
  */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using MACS;

namespace MACS.DB {

/// <summary>
///   データベーステーブル定義
/// </summary>
public partial class DBTableDef {

    /// <summary>
    ///   テーブル定義XMLファイルを指定ディレクトリから読み込む
    /// </summary>
    /// <param name="dir">XMLファイル群が格納されているディレクトリ名</param>
    /// <returns>読み込んだテーブル数</returns>
    /// <remarks>
    ///   <para>
    ///     指定ディレクトリ内の"table*.xml"というパターンの名前のファイルを
    ///     全て読み込む。
    ///     同じIDの定義が重複している場合には以前の定義は上書きされる。
    ///     つまり、同じディレクトリを指定すると定義のリロードになる。
    ///   </para>
    /// </remarks>
    public static int LoadAll(string dir) {
        return Load(dir, "table*.xml");
    }

    /// <summary>
    ///   テーブル定義XMLファイルを指定ディレクトリから読み込む
    /// </summary>
    /// <param name="dir">XMLファイル群が格納されているディレクトリ名</param>
    /// <param name="filepattern">取り込むファイル名のパターン</param>
    /// <returns>読み込んだテーブル数</returns>
    /// <remarks>
    ///   <para>
    ///     同じIDの定義が重複している場合には以前の定義は上書きされる。
    ///   </para>
    /// </remarks>
    public static int Load(string dir, string filepattern) {
        int c = 0;
        lock(tableList) {
            foreach(DBTableDef def in GetTableDefs(dir, filepattern)) {
                bool exist = false;
                for(int i = 0; i < tableList.Count; i++) {
                    if(tableList[i].Id == def.Id) {
                        tableList[i] = def;
                        exist = true;
                    }
                }
                if(!exist)
                    tableList.Add(def);
                c++;
            }
        }
        return c;
    }

    /// <summary>
    ///   テーブル定義一覧
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     LoadAllまたはLoadで読み込まれたテーブル定義の一覧。
    ///   </para>
    /// </remarks>
    public static List<DBTableDef> TableList {
        get { return tableList; }
    }

    /// <summary>
    ///   テーブル名またはIDを指定してテーブル定義を獲得する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定した名前またはIDのテーブル定義が存在しない場合はnullを返す。
    ///   </para>
    /// </remarks>
    public static DBTableDef Get(string name) {
        lock(tableList) {
            foreach(DBTableDef def in tableList) {
                if(def.Name == name)
                    return def;
            }
            foreach(DBTableDef def in tableList) {
                if(def.Id == name)
                    return def;
            }
            return null;
        }
    }


    /// <summary>
    ///   テーブルID名
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     定義ファイル名のディレクトリ名と拡張子を除いたもの
    ///   </para>
    /// </remarks>
    public string Id {
        get {
            if(String.IsNullOrEmpty(xmlfile))
                return "";
            return Path.GetFileNameWithoutExtension(xmlfile);
        }
        set {
            xmlfile = value+".xml";
        }
    }

    /// <summary>
    ///   テーブル名
    /// </summary>
    public string Name {
        get { return name; }
        set { name = value; }
    }

    /// <summary>
    ///   テーブル内容
    /// </summary>
    public string Expr {
        get { return String.IsNullOrEmpty(expr)?name:expr; }
        set { expr = value; }
    }

    /// <summary>
    ///   説明文
    /// </summary>
    public string Note {
        get { return note; }
        set { note = value; }
    }

    /// <summary>
    ///   カラム定義
    /// </summary>
    public DBColumnDef[] Columns {
        get { return columns; }
    }

    /// <summary>
    ///   カラム名一覧
    /// </summary>
    public string[] ColumnNames {
        get {
            string[] res = new string[columns.Length];
            for(int i = 0; i < columns.Length; i++)
                res[i] = columns[i].Name;
            return res;
        }
    }

    /// <summary>
    ///   インデックス定義
    /// </summary>
    public DBIndexDef[] Indexes {
        get {
            if(indexes == null)
                return new DBIndexDef[0];
            return indexes;
        }
    }

    /// <summary>
    ///   定義XMLファイル名
    /// </summary>
    public string XmlFileName {
        get { return xmlfile; }
    }

    /// <summary>
    ///   Oracleデータベース上のスキーマ（オーナ）
    /// </summary>
    public string Owner {
        get { return owner; }
        set { owner = value; }
    }

    /// <summary>
    ///   Oracleデータベース上のスキーマ（オーナ）一覧
    /// </summary>
    public static readonly string[] OwnerList = {"SCHEME"};

    /// <summary>
    ///   空のコンストラクタ
    /// </summary>
    public DBTableDef() {}

    /// <summary>
    ///   テーブル名を指定したコンストラクタ
    /// </summary>
    public DBTableDef(string name_) {
        name = name_;
        owner = OwnerList[0];
    }

    /// <summary>
    ///   指定した名前のカラムを持つかどうか
    /// </summary>
    public bool HasColumn(string colname) {
        foreach(DBColumnDef col in columns) {
            if(colname.Equals(col.Name, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>
    ///   指定した名前のカラムの定義を獲得する
    /// </summary>
    public DBColumnDef GetColumnDef(string colname) {
        foreach(DBColumnDef col in columns) {
            if(colname.Equals(col.Name, StringComparison.OrdinalIgnoreCase))
                return col;
        }
        return null;
    }

    /// <summary>
    ///   主キーを持つかどうか
    /// </summary>
    public bool HasPrimaryKey {
        get {
            foreach(DBColumnDef col in columns) {
                if(col.PrimaryKey)
                    return true;
            }
            return false;
        }
    }


    /// <summary>
    ///   指定ファイルからテーブル定義を読み取る
    /// </summary>
    public void Load(string xmlfile_) {
        xmlfile = xmlfile_;
        columns = new DBColumnDef[0];
        try {
            using(XmlFile xml = new XmlFile(xmlfile, "table")) {
                XmlElement root = xml.Root;
                name = root.GetAttribute("name");
                expr = root.GetAttribute("expr");
                note = XmlFile.GetText(root, false).Trim();
                owner = root.GetAttribute("owner");
                orgName = root.GetAttribute("orgname");
                foreach(XmlElement el in xml.GetElements(root, "column")) {
                    DBColumnDef coldef = new DBColumnDef(el);
                    coldef.ByteSize = StringUtil.ToInt(el.GetAttribute("bytesize"),0);
                    coldef.OrgName = el.GetAttribute("orgname");
                    AddColumn(coldef);
                }
                foreach(XmlElement el in xml.GetElements(root, "index"))
                    AddIndex(new DBIndexDef(xml, el));
                xml.Close();
            }
        } catch(XmlException e) {
            throw new XmlException(String.Format("XMLファイル({0})の読み取りに失敗しました: {1}", xmlfile, e.Message));
        }
    }

    /// <summary>
    ///   指定ファイルに定義を書き出す
    /// </summary>
    public void SaveAs(string filepath) {
        using(StreamWriter sw = FileUtil.Writer(filepath, Encoding.UTF8)) {
            if(sw == null)
                throw new IOException("Can't open "+filepath+" for writing.");
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sw.WriteLine("<table expr=\"{0}\" name=\"{1}\">", Expr, Name);
            if(!String.IsNullOrEmpty(Note))
                sw.WriteLine(Note);
            foreach(DBColumnDef coldef in Columns) {
                sw.Write("  <column expr=\"{0}\" name=\"{1}\" type=\"{2}\" length=\"{3}\" ",
                         coldef.Expr, coldef.Name, coldef.Type, coldef.Length);
                if(coldef.FractionalLength > 0)
                    sw.Write("fractional=\"{0}\" ", coldef.FractionalLength);
                if((coldef.DefaultValue != null) && ((coldef.Type=="VARCHAR")||(coldef.DefaultValue != "")))
                    sw.Write("default=\"{0}\" ", coldef.DefaultValue);
                if(coldef.Nullable)
                    sw.Write("nullable=\"yes\" ");
                if(coldef.PrimaryKey)
                    sw.Write("pk=\"yes\" ");
                if(String.IsNullOrEmpty(coldef.Note))
                    sw.WriteLine("/>");
                else {
                    sw.WriteLine(">");
                    sw.WriteLine("  "+coldef.Note);
                    sw.WriteLine("</column>");
                }
            }
            // Indexを出力する
            foreach(DBIndexDef index in Indexes) {
                sw.Write("  <index");
                if(index.IsUnique)
                    sw.Write(" unique=\"yes\"");
                sw.WriteLine(">");
                foreach(string col in index.Columns) {
                    sw.Write("    <column name=\"");
                    sw.Write(col);
                    sw.WriteLine("\"/>");
                }
                sw.WriteLine("  </index>");
            }

            sw.WriteLine("</table>");

            sw.Close();
        }
    }

    /// <summary>
    ///   カラム追加
    /// </summary>
    public void AddColumn(DBColumnDef col) {
        if(columns == null) {
            columns = new DBColumnDef[1];
            columns[0] = col;
            return;
        }
        DBColumnDef[] newcolumns = new DBColumnDef[columns.Length+1];
        for(int i = 0; i < columns.Length; i++)
            newcolumns[i] = columns[i];
        newcolumns[newcolumns.Length-1] = col;
        columns = newcolumns;
    }

    /// <summary>
    ///   インデックス追加
    /// </summary>
    public void AddIndex(DBIndexDef ind) {
        if(indexes == null) {
            indexes = new DBIndexDef[1];
            indexes[0] = ind;
            return;
        }
        DBIndexDef[] newindexes = new DBIndexDef[indexes.Length+1];
        for(int i = 0; i < indexes.Length; i++)
            newindexes[i] = indexes[i];
        newindexes[newindexes.Length-1] = ind;
        indexes = newindexes;
    }


#region テーブル生成

    /// <summary>
    ///   テーブルを生成するSQL文を作る
    /// </summary>
    public string GenerateCreator(DBCon.Type dbtype=DBCon.Type.Invalid) {
#if USE_MYSQL
        if(dbtype == DBCon.Type.Invalid)
            dbtype = DBCon.Type.MySQL;
#endif
#if USE_POSTGRESQL
        if(dbtype == DBCon.Type.Invalid)
            dbtype = DBCon.Type.PostgreSQL;
#endif
        switch(dbtype) {
#if USE_MYSQL
        case DBCon.Type.MySQL:
            return GenerateCreatorMySQL();
#endif
#if USE_POSTGRESQL
        case DBCon.Type.PostgreSQL:
            return GenerateCreatorPostgreSQL();
#endif
        default:
            throw new ArgumentException("Unsupported DB type");
        }
    }


#if USE_MYSQL
    /// <summary>
    ///   テーブルを生成するSQL文を作る(MySQL用)
    /// </summary>
    public string GenerateCreatorMySQL() {
        StringBuilder sb = new StringBuilder();
        List<string> pk = new List<string>();
        foreach(DBColumnDef c in Columns) {
            if(c.PrimaryKey)
                pk.Add(c.Name);
        }
        sb.Append("CREATE TABLE IF NOT EXISTS ");
        sb.Append(Name);
        sb.Append(" (\n");
        int n = Columns.Length;
        foreach(DBColumnDef c in Columns) {
            string type = c.Type.ToUpper();
            if(type == "NUMBER") {
                if(c.Length == 0) {
                    type = "DECIMAL";
                } else if(c.FractionalLength == 0) {
                    if(c.Length == 1)
                        type = "TINYINT";
                    else if(c.Length <= 4)
                        type = "SMALLINT";
                    else if(c.Length <= 9)
                        type = "INT";
                    else
                        type = "BIGINT";
                } else {
                    type = String.Format("DECIMAL({0},{1})", c.Length, c.FractionalLength);
                }
            } else if(type.StartsWith("VARCHAR")) {
                type = String.Format("VARCHAR({0})", c.Length);
            } else if(type == "CHAR") {
                type = String.Format("CHAR({0})", c.Length);
            } else {
                // use its type
            }
            sb.Append("    ");
            sb.Append(c.Name);
            sb.Append(" ");
            sb.Append(type);
            if(!c.Nullable)
                sb.Append(" NOT NULL");
            if(c.DefaultValue != null) {
                if(c.Type.ToUpper() == "NUMBER")
                    sb.AppendFormat(" DEFAULT {0}", StringUtil.ToInt(c.DefaultValue));
                else if(c.DefaultValue == "NULL")
                    sb.Append(" DEFAULT NULL");
                else
                    sb.AppendFormat(" DEFAULT '{0}'", c.DefaultValue);
            }
            if(c.PrimaryKey && (pk.Count == 1))
                sb.Append(" PRIMARY KEY");
            n--;
            sb.Append(",\n");
        }
        sb.Append("    rowid BIGINT NOT NULL AUTO_INCREMENT");
        if(pk.Count == 0)
            sb.Append(" PRIMARY KEY");
        else
            sb.Append(" UNIQUE");
        n = pk.Count;
        if(n > 1) {
            sb.Append(",\n    PRIMARY KEY (");
            foreach(string i in pk) {
                sb.Append(i);
                if(--n > 0)
                    sb.Append(", ");
            }
            sb.Append(")");
        }
        foreach(DBIndexDef index in Indexes) {
            sb.Append(",\n    ");
            if(index.IsUnique)
                sb.Append("UNIQUE");
            else
                sb.Append("INDEX");
            sb.Append(" (");
            n = index.Columns.Length;
            foreach(string i in index.Columns) {
                sb.Append(i);
                if(--n > 0)
                    sb.Append(", ");
            }
            sb.Append(")");
        }
        sb.Append(");\n");

        if(!String.IsNullOrEmpty(orgName)) {
            sb.Append("CREATE OR REPLACE VIEW ");
            sb.Append(orgName);
            sb.Append("\n    (");
            foreach(DBColumnDef c in Columns) {
                sb.Append(String.IsNullOrEmpty(c.OrgName)?c.Name:c.OrgName);
                sb.Append(",");
            }
            sb.Append("rowid)\n");
            sb.AppendFormat("  AS SELECT * FROM {0};\n", Name);
        }
        return sb.ToString();
    }
#endif

#if USE_POSTGRESQL
    /// <summary>
    ///   テーブルを生成するSQL文を作る(PostgreSQL用)
    /// </summary>
    public string GenerateCreatorPostgreSQL() {
        StringBuilder sb = new StringBuilder();
        List<string> pk = new List<string>();
        foreach(DBColumnDef c in Columns) {
            if(c.PrimaryKey)
                pk.Add(c.Name);
        }
        sb.Append("CREATE TABLE IF NOT EXISTS ");
        sb.Append(Name);
        sb.Append(" (\n");
        int n = Columns.Length;
        foreach(DBColumnDef c in Columns) {
            string type = c.Type.ToUpper();
            if(type == "NUMBER") {
                if(c.Length == 0) {
                    type = "DECIMAL";
                } else if(c.FractionalLength == 0) {
                    if(c.Length <= 4)
                        type = "SMALLINT";
                    else if(c.Length <= 9)
                        type = "INTEGER";
                    else
                        type = "BIGINT";
                } else {
                    type = String.Format("DECIMAL({0},{1})", c.Length, c.FractionalLength);
                }
            } else if(type.StartsWith("VARCHAR")) {
                type = String.Format("VARCHAR({0})", c.Length);
            } else if(type == "CHAR") {
                type = String.Format("CHAR({0})", c.Length);
            } else if(type == "DATETIME") {
                type = "TIMESTAMP";
            } else {
                // use its type
            }
            sb.Append("    ");
            sb.Append(c.Name);
            sb.Append(" ");
            sb.Append(type);
            if(!c.Nullable) {
                sb.Append(" NOT NULL");
            }
            string defaultValue = c.DefaultValue;
            if((type == "TIMESTAMP") && String.IsNullOrEmpty(defaultValue))
                defaultValue = "0001-01-01 00:00:00";
            if(defaultValue != null) {
                if(c.Type.ToUpper() == "NUMBER")
                    sb.AppendFormat(" DEFAULT {0}", StringUtil.ToInt(defaultValue));
                else if(defaultValue == "NULL")
                    sb.Append(" DEFAULT NULL");
                else
                    sb.AppendFormat(" DEFAULT '{0}'", defaultValue);
            }
            if(c.PrimaryKey && (pk.Count == 1))
                sb.Append(" PRIMARY KEY");
            n--;
            sb.Append(",\n");
        }
        sb.Append("    rowid SERIAL");
        //if(pk.Count == 0)
        //    sb.Append(" PRIMARY KEY");
        //else
        //    sb.Append(" UNIQUE");
        n = pk.Count;
        if(n > 1) {
            sb.Append(",\n    PRIMARY KEY (");
            foreach(string i in pk) {
                sb.Append(i);
                if(--n > 0)
                    sb.Append(", ");
            }
            sb.Append(")");
        }
        sb.Append(");\n");
        int indexnum = 0;
        foreach(DBIndexDef index in Indexes) {
            indexnum++;
            sb.Append("CREATE ");
            if(index.IsUnique)
                sb.Append("UNIQUE ");
            sb.Append("INDEX ");
            sb.Append("INDEX_"+Name+"_"+indexnum.ToString());
            sb.Append(" ON ");
            sb.Append(Name);
            sb.Append(" (");
            n = index.Columns.Length;
            foreach(string i in index.Columns) {
                sb.Append(i);
                if(--n > 0)
                    sb.Append(", ");
            }
            sb.Append(");\n");
        }

        if(!String.IsNullOrEmpty(orgName)) {
            // orgName (aka VIEW name) is not supported.
        }

        return sb.ToString();
    }

#endif

    /// <summary>
    ///   テーブルを削除するSQL文を作る
    /// </summary>
    public string GenerateDropper(DBCon.Type dbtype=DBCon.Type.Invalid) {
#if USE_MYSQL
        if(dbtype == DBCon.Type.Invalid)
            dbtype = DBCon.Type.MySQL;
#endif
#if USE_POSTGRESQL
        if(dbtype == DBCon.Type.Invalid)
            dbtype = DBCon.Type.PostgreSQL;
#endif
        switch(dbtype) {
#if USE_MYSQL
        case DBCon.Type.MySQL:
            return GenerateDropperMySQL();
#endif
#if USE_POSTGRESQL
        case DBCon.Type.PostgreSQL:
            return GenerateDropperPostgreSQL();
#endif
        default:
            throw new ArgumentException("Unsupported DB type");
        }
    }

#if USE_MYSQL
    /// <summary>
    ///   テーブルを削除するSQL文を作る(MySQL用)
    /// </summary>
    public string GenerateDropperMySQL() {
        StringBuilder sb = new StringBuilder();
        sb.Append("DROP TABLE IF EXISTS ");
        sb.Append(Name);
        return sb.ToString();
    }
#endif

#if USE_POSTGRESQL
    /// <summary>
    ///   テーブルを削除するSQL文を作る(PostgreSQL用)
    /// </summary>
    public string GenerateDropperPostgreSQL() {
        StringBuilder sb = new StringBuilder();
        sb.Append("DROP TABLE IF EXISTS ");
        sb.Append(Name);
        return sb.ToString();
    }
#endif

#endregion

#region テーブル定義一括読み取り

    /// <summary>
    ///   指定ディレクトリ内から全テーブル定義を読み取る。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定ディレクトリ内の"table*.xml"というパターンの名前のファイルを
    ///     全て読み込む。
    ///   </para>
    /// </remarks>
    public static DBTableDef[] GetTableDefs(string dirname) {
        return GetTableDefs(dirname, "table*.xml");
    }

    /// <summary>
    ///   指定ディレクトリ内から全テーブル定義を読み取る。
    /// </summary>
    /// <param name="dirname">ディレクトリ名</param>
    /// <param name="filepattern">ファイル名パターン</param>
    public static DBTableDef[] GetTableDefs(string dirname, string filepattern) {
        string[] filelist = Directory.GetFiles(dirname, filepattern);
        int nfiles = filelist.Length;
        List<DBTableDef> list = new List<DBTableDef>();
        for(int i = 0; i < nfiles; i++) {
            if(filelist[i].EndsWith("~"))
                continue;
            DBTableDef td = new DBTableDef();
            try {
                td.Load(filelist[i]);
                list.Add(td);
            } catch(XmlException) {
                // just ignore.
            }
        }
        return list.ToArray();
    }

#endregion

#region private部

    private static List<DBTableDef> tableList = new List<DBTableDef>();

    private string xmlfile;
    private string name;
    private string expr;
    private string note;
    private string owner;
    private string orgName;
    private DBColumnDef[] columns;
    private DBIndexDef[] indexes;

#endregion

}

} // End of namespace
