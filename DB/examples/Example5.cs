/*! @file Example5.cs
 * $Id: Example5.cs 1890 2014-06-05 04:34:56Z shibuya $
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
            tbl.SetColumns("名前", "電話番号");

            // 等値以外の検索条件を指定する
            tbl.SetCondition(DBCondition.Code.LessOrEqual, "ID", 3);

            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            tbl.SetCondition(DBCondition.Code.Between, "ID", 1, 9);
            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            tbl.SetCondition(DBCondition.Code.Contains, "名前", "一郎");
            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            tbl.SetCondition(DBCondition.Code.EndsWith, "名前", "一郎");
            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            string[] namelist = new string[]{"テスト一郎", "テスト次郎"};
            tbl.SetCondition(DBCondition.Code.In, "名前", namelist);
            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            tbl.SetCondition("名前", "テスト一郎",
                             "電話番号", "03-4567-8901");
            // 上記は次と同じ。
            // tbl.SetCondition("名前", "テスト一郎");
            // tbl.AddCondition("電話番号", "03-4567-8901");
            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            tbl.SetCondition(DBCondition.Code.CollateEquals, "名前", "てすと一郎");
            foreach(DataArray rec in tbl.GetData())
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);


        }

        return 0;
    }
}
