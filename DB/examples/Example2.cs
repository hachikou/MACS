/*! @file Example2.cs
 * $Id: Example2.cs 1890 2014-06-05 04:34:56Z shibuya $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;

class Program {
    public static int Main(string[] args) {

        DBCon.UseConsoleLog = true;

        // INIファイルの設定に従ってDBと接続する。
        IniFile conf = new IniFile("Example.ini");
        using(DBCon db = new DBCon(conf)) {

            // クエリの実行例
            string sql = "SELECT 名前,電話番号 FROM サンプルテーブル WHERE ID<5";
            using(DBReader reader = db.Query(sql)) {
                // レコードが読み取れる間、ループを回す
                while(reader.Read()) {
                    Console.WriteLine("なまえ:{0}、でんわ:{1}",
                                      reader.GetString(0),
                                      reader.GetString(1));
                }
                // usingを使っているので明示的にクローズする必要は無いが、念のため
                reader.Close();
            }
        }

        return 0;
    }
}
