/*! @file Example8.cs
 * $Id: Example8.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // 複数のテーブルを連結して使う
            DBTable tbl = new DBTable(db, "サンプルテーブル");
            tbl.Join("サンプル役職", // 副テーブル名
                     "ID",           // 副テーブルのカラム名
                     "ID");          // 親テーブルのカラム名
            tbl.SetColumns("名前", "役職");

            // 検索条件を指定せず、全レコードを表示してみる
            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("なまえ:{0}、やくしょく:{1}", rec["名前"], rec["役職"]);
            }

            // 副テーブルのカラムを検索条件に使ってもよい
            tbl.SetCondition("役職", "技術課長");
            // tbl.SetCondition("サンプル役職.役職", "技術課長") と、テーブル名を明記してもよい。
            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("なまえ:{0}、やくしょく:{1}", rec["名前"], rec["役職"]);
            }

        }

        return 0;
    }
}
