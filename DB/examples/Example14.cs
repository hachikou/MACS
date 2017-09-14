/*! @file Example14.cs
 * $Id: Example14.cs 1890 2014-06-05 04:34:56Z shibuya $
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
            tbl.SetAllColumns();
            tbl.SetCondition("ID", 1000);
            // 検索実行時に FOR UPDATE オプションを付ける
            tbl.ForUpdate = true;

            // まず既存レコードの獲得を試みる
            bool newbie = false;
            DataArray rec = tbl.GetOneData();
            if(rec == null) {
                // 既存レコードは無い
                rec = new DataArray(tbl.Columns, null);
                // 新規レコード時の値をセットする
                rec["ID"] = "1000";
                rec["名前"] = "七師権兵衛";
                newbie = true;
            }
            // 変更する値をセットする
            rec["電話番号"] = "098-765-4321";

            if(newbie) {
                tbl.Insert(rec);
                Console.WriteLine("レコードを追加しました");
            } else {
                tbl.Update(rec);
                Console.WriteLine("レコードを更新しました");
            }

        }

        return 0;
    }
}
