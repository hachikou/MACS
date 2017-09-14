/*! @file Example13.cs
 * $Id: Example13.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // 変更するレコードの内容を格納するDataArrayを用意する
            DataArray rec = new DataArray(tbl.Columns, null);
            rec["電話番号"] = "01-2345-6789";

            // レコードを更新する
            int n = tbl.Update(rec);
            Console.WriteLine("{0}レコード更新しました", n);

            // 念のためにレコードを読み出してみる
            rec = tbl.GetRecordData("ID", 1000);
            if(rec != null) {
                Console.WriteLine("なまえ:{0}、でんわ:{1}", rec["名前"], rec["電話番号"]);
            }

        }

        return 0;
    }
}
