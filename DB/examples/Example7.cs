/*! @file Example7.cs
 * $Id: Example7.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // サブクエリ用のDBTableを作る
            // （役職が'技術課長'であるレコードのIDをサンプル役職テーブルから得る）
            DBTable subtbl = new DBTable(db, "サンプル役職");
            subtbl.SetColumn("ID");
            subtbl.SetCondition("役職", "技術課長");

            // メインクエリ用のDBTableを作る
            DBTable tbl = new DBTable(db, "サンプルテーブル");
            tbl.SetColumns("名前", "電話番号");

            // 検索条件にサブクエリを使う
            // （IDがサブクエリの結果のいずれかである）
            tbl.SetCondition(DBCondition.Code.In, "ID", subtbl);

            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);
            }

            // 同じ検索をExists条件を使って行なってみる
            // （IDがサンプルテーブルのIDと等しく、役職が技術課長であるレコードをサンプル役職テーブルから探す）
            subtbl = new DBTable(db, "サンプル役職");
            subtbl.SetColumn("ID");
            subtbl.SetCondition("ID", "!サンプルテーブル.ID",
                                "役職", "技術課長");

            tbl = new DBTable(db, "サンプルテーブル");
            tbl.SetColumns("名前", "電話番号");
            // サブクエリの結果が存在するレコードを探す
            tbl.SetCondition(DBCondition.Code.Exists, subtbl);

            foreach(DataArray rec in tbl.GetData()) {
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);
            }

        }

        return 0;
    }
}
