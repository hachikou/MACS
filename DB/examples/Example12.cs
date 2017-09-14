/*! @file Example12.cs
 * $Id: Example12.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // 新レコードの内容を格納するDataArrayを用意する
            DataArray rec = new DataArray(tbl.Columns, null);

            rec["ID"] = "1000"; // 数値の項目も文字列として入れておく
            rec["名前"] = "足柄金太郎";

            // 新レコードを追加する
            tbl.Insert(rec);

            // 念のために新レコードを読み出してみる
            rec = tbl.GetRecordData("ID", 1000);
            if(rec != null) {
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);
            }

        }

        return 0;
    }
}
