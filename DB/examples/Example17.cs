/*! @file Example17.cs
 * $Id: Example17.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // データベーストランザクションを開始する
            db.Begin();

            DBTable tbl = new DBTable(db, "サンプルテーブル");
            tbl.SetAllColumns();
            DataArray rec = new DataArray(tbl.Columns, null);
            rec["ID"] = "2000";
            rec["名前"] = "カルロス ゴーン";
            tbl.Insert(rec);

            // トランザクションをコミットするとデータベース変更が確定する
            db.Commit();

            foreach(DataArray xrec in tbl.GetData())
                Console.WriteLine("{0}: {1}", xrec["ID"], xrec["名前"]);

            // もう一度トランザクションを開始する
            db.Begin();

            rec["ID"] = "2001";
            rec["名前"] = "マイケル ウッドフォード";
            tbl.Insert(rec);

            // トランザクションをロールバックするとデータベース変更は破棄される
            db.Rollback();

            foreach(DataArray xrec in tbl.GetData())
                Console.WriteLine("{0}: {1}", xrec["ID"], xrec["名前"]);

            // もう一度トランザクションを開始する
            db.Begin();

            rec["ID"] = "2002";
            rec["名前"] = "堀江貴文";
            tbl.Insert(rec);
            
            // コミットしないまま DBConがDisposeされると、自動的にロールバックする
            if(rec["名前"] == "堀江貴文")
                throw new Exception("Aborted");

        }

        return 0;
    }
}
