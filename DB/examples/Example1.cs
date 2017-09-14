/*! @file Example1.cs
 * $Id: Example1.cs 1890 2014-06-05 04:34:56Z shibuya $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;

class Program {
    public static int Main(string[] args) {

        // DBConのデバッグログをコンソールに出す
        DBCon.UseConsoleLog = true;
        // SCSLibのOpeLogを使ってデバッグログを出力する場合は、
        // DBCon.UseDebugLog = true;

        // DBと接続する。
        using(DBCon db = new DBCon(DBCon.Type.PostgreSQL, // データベースの種類
                                   "localhost",     // データベースサーバ
                                   "mydb",          // データベース名
                                   "myuser",          // 接続ユーザ名
                                   "mypass")){        // 接続パスワード
            // SQL文を実行
            db.Execute("CREATE TABLE IF NOT EXISTS サンプルテーブル (ID INTEGER, 名前 VARCHAR(32), 電話番号 VARCHAR(16))");

            // INSERT文の実行例
            int n;
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO サンプルテーブル");
            sql.Append(" (ID,名前,電話番号)");
            sql.Append(" VALUES (1,'テスト一郎','03-4567-8901')");
            n = db.Execute(sql); // StringBuilderを直接渡すことができる。
            Console.WriteLine("{0}件登録しました", n);

            // usingを使っているので明示的にクローズする必要は無いが、念のため
            db.Close();
        }

        return 0;
    }
}
