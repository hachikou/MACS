/*! @file Example6.cs
 * $Id: Example6.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // 結果を1件だけ抽出する
            DataArray rec = tbl.GetRecordData("ID", 1);

            // 上記は以下のコードと同じ動作をする
            // tbl.SetCondition("ID", 1);
            // List<DataArray> recs = tbl.GetData(1);
            // DataArray rec;
            // if(recs.Count > 0)
            //     rec = recs[0];
            // else
            //     rec = null;

            Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);

            // 該当件数だけを獲得する
            tbl.SetCondition(DBCondition.Code.GreaterOrEqual, "ID", 100);
            int n = tbl.GetCount();
            Console.WriteLine("該当件数は{0}", n);

            // 該当件数が巨大であることが見込まれる場合の処理方法
            tbl.SetCondition(DBCondition.Code.GreaterThan, "ID", 0);
            using(DBReader reader = tbl.Query(0,0)) {
                string[] val;
                while((val = reader.Get()) != null) {
                    rec = new DataArray(tbl.Columns, val);
                    Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);
                }
            }
        }

        return 0;
    }
}
