/*! @file Example9.cs
 * $Id: Example9.cs 1890 2014-06-05 04:34:56Z shibuya $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Collections.Generic;

class Program {
    public static int Main(string[] args) {

        IniFile conf = new IniFile("Example.ini");
        DBCon.UseConsoleLog = true;
        DBTableDef.LoadAll("tabledef");

        using(DBCon db = new DBCon(conf)) {

            // LEFT OUTER JOINの例
            DBTable tbl = new DBTable(db, "サンプルテーブル");
            tbl.Join("サンプル役職", "ID", "ID");
            tbl.SetColumns("名前", "役職");

            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("なまえ:{0}、やくしょく:{1}", rec["名前"], rec["役職"]);
            }

            // INNER JOINの例
            tbl = new DBTable(db, "サンプルテーブル");
            tbl.InnerJoin("サンプル役職", "ID", "ID");
            tbl.SetColumns("名前", "役職");

            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("なまえ:{0}、やくしょく:{1}", rec["名前"], rec["役職"]);
            }

        }

        return 0;
    }
}
