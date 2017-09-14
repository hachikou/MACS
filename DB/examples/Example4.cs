/*! @file Example4.cs
 * $Id: Example4.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // DBTable でテーブル定義を元にテーブルアクセス用オブジェクトを作る
            DBTable tbl = new DBTable(db, "サンプルテーブル");

            // 参照するカラムを指定する
            tbl.SetColumns("名前", "電話番号");

            // 検索条件を指定する
            tbl.SetCondition("ID", 1);

            // 検索をし、結果一覧を得る
            List<DataArray> recs = tbl.GetData();

            foreach(DataArray rec in recs) {
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);
            }

        }

        return 0;
    }
}
