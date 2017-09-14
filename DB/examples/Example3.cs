/*! @file Example3.cs
 * $Id: Example3.cs 1890 2014-06-05 04:34:56Z shibuya $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;

class Program {
    public static int Main(string[] args) {

        // テーブル定義群を読み込む
        DBTableDef.LoadAll("tabledef");
        // ファイル名パターンを指定する場合は、
        // DBTableDef.Load("tabledef", "table*.xml");
        // のようにする。

        // テーブル定義の一覧
        foreach(DBTableDef tabledef in DBTableDef.TableList) {
            Console.WriteLine("TABLE: {0}", tabledef.Name);
        }

        // テーブル名を指定してテーブル定義を獲得する
        DBTableDef def = DBTableDef.Get("サンプルテーブル");
        Console.WriteLine("テーブル{0}は、次のカラムを持つ:", def.Name);

        // カラムの一覧
        foreach(DBColumnDef col in def.Columns) {
            Console.WriteLine("{0}: {1}({2}.{3})", col.Name, col.Type, col.Length, col.FractionalLength);
        }

        return 0;
    }
}
