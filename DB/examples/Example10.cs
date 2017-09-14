/*! @file Example10.cs
 * $Id: Example10.cs 1890 2014-06-05 04:34:56Z shibuya $
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
            tbl.Join("サンプル役職", "ID", "ID");
            // 名前の翻訳をT1という仮テーブル名で行なう
            tbl.JoinAs("T1", "サンプル翻訳", "日本語", "名前");
            // 役職の翻訳をT2という仮テーブル名で行なう
            tbl.JoinAs("T2", "サンプル翻訳", "T2.日本語=サンプル役職.役職");

            tbl.SetColumns("T1.英語", "T2.英語");

            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("Name:{0},  Title:{1}", rec["T1.英語"], rec["T2.英語"]);
            }

        }

        return 0;
    }
}
