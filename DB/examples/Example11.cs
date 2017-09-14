/*! @file Example11.cs
 * $Id: Example11.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // 基本テーブルと同じテーブルをサブクエリに使う場合、
            // 名前を区別するためにテーブルの別名を付ける必要がある。
            DBTable subtbl = new DBTable(db, "サンプルテーブル", "SUBTBL");
            subtbl.SetColumn("ID");
            // 基本テーブルのID番号の次のID番号を持つレコードを探す
            subtbl.SetCondition("ID", "!サンプルテーブル.ID+1");

            DBTable tbl = new DBTable(db, "サンプルテーブル");
            tbl.SetColumns("ID", "名前");
            // 1つ次のID番号が無いレコードを抽出する
            tbl.SetCondition(DBCondition.Code.NotExists, subtbl);

            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("{0}: {1}", rec["ID"], rec["名前"]);
            }

        }

        return 0;
    }
}
