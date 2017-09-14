/*! @file Example15.cs
 * $Id: Example15.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            DBTable tbl = new DBTable(db, "サンプルテーブル");
            tbl.SetCondition(DBCondition.Code.LessOrEqual, "ID", 10);

            // 条件に合致するレコードを削除する
            int n = tbl.Delete();
            Console.WriteLine("{0}件削除しました", n);

            // テーブルを連結して条件を指定する
            tbl = new DBTable(db, "サンプルテーブル");
            tbl.Join("サンプル役職", "ID", "ID");
            tbl.SetCondition("役職", "取締役");
            n = tbl.Delete();
            Console.WriteLine("{0}件削除しました", n);
        }

        return 0;
    }
}
