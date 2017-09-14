/*! @file Example11.cs
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System.Text;
using SCS;

class Program {
    public static int Main(string[] args) {
        HttpServer sv = new HttpServer("SampleHttpServer", "*", 8888);
        sv.AddPage("/example11", typeof(SamplePage));
        sv.DefaultPage = "/example11";
        sv.Run(4);
        return 0;
    }
}

// HTMLコンテンツ以外を返す例:
public class SamplePage : HttpPage {
    public override void PageLoad(string param) {
        StringBuilder data = new StringBuilder();

        // CSVデータを作る
        for(int i = 0; i < 20; i++)
            data.Append(string.Format("{0},{1},{2}\n", i, i*i, i*i*i));

        // あらかじめバイト列にしておく
        byte [] buf = Encoding.ASCII.GetBytes(data.ToString());

        // HTTPヘッダを調整する。
        Response.ContentType = "application/octet-stream";
        Response.AddHeader("Content-Disposition", "attachment; filename=hogehoge.csv");

        // それを出力
        Response.OutputStream.Write(buf, 0, buf.Length);

        // すでに出力を行なった事をHttpPageに通知する
        IsRendered = true;
    }
}
