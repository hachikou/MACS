/*! @file Example16.cs
 * $Id: Example16.cs 1890 2014-06-05 04:34:56Z shibuya $
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

            // サンプルテーブルをサブクエリとして使う
            DBTable subtbl = new DBTable(db, "サンプルテーブル");
            // INSERTするサンプル給与テーブルのカラムにあわせてカラム一覧を
            // セットしておく。
            subtbl.SetColumns("ID", "0.00");

            // サンプルテーブルに存在する全レコードに対応するサンプル給与レコードを作る
            DBTable tbl = new DBTable(db, "サンプル給与");
            tbl.SetColumns("ID", "給与");
            int n = tbl.Insert(subtbl);

            Console.WriteLine("{0}件追加しました", n);

        }

        return 0;
    }
}
