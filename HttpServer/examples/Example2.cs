/*! @file Example2.cs
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
        // "/example2" にアクセスすると、SamplePageクラスのPageLoadメソッドが呼び出される
        sv.AddPage("/example2", typeof(SamplePage));
        sv.DefaultPage = "/example2";
        sv.Run(4);
        return 0;
    }
}

// 動的に生成するページの例:
// HttpPageの派生クラスを用意する
public class SamplePage : HttpPage {
    public override void PageLoad(string param) {
        StringBuilder body = new StringBuilder();

        // フォームを作る
        body.Append("<form method='post' action='sample'>");

        // ラジオボタンを作る
        HtmlRadio(body, "weather", "fine", "晴れ", false);
        HtmlRadio(body, "weather", "bad", "雨", false);

        body.Append("<br/>");

        // Submitボタンを作る
        HtmlSubmit(body, "send", "送信");

        body.Append("</form>\n");

        // HTMLエスケープの例
        body.Append(HE("<<< &&& >>>"));

        // フォームの入力データを取り込む。
        string weather = Fetch("weather");

        body.Append("<p>");
        switch(weather){
        case "fine":
            body.Append("よい天気ですね");
            break;
        case "bad":
            body.Append("悪い天気ですね");
            break;
        }
        body.Append("</p>\n");

        // それを出力する。
        RenderBody("Example2", body.ToString());
    }
}
