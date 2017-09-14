/*! @file Example1.cs
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using SCS;

class Program {
    public static int Main(string[] args) {
        // ポート8888番でHTTPサーバを動かす
        HttpServer sv = new HttpServer("SampleHttpServer", "*", 8888);

        // "/xxxx" にアクセスすると、html/xxxx ファイルを返す。
        sv.AddStaticPage("/", "html");

        // "/sample" にアクセスすると、SamplePageクラスのPageLoadメソッドが呼び出される
        sv.AddPage("/sample", typeof(SamplePage));

        // デフォルトは"/sample"にアクセスさせる。
        sv.DefaultPage = "/sample";

        // 4個のワーカーで、HTTPサービスを処理する。
        sv.Run(4); // 4 workers
        // 原則として、ここには戻ってこない。
        return 0;
    }
}

// 動的に生成するページの例:
// HttpPageの派生クラスを用意する
public class SamplePage : HttpPage {
    public override void PageLoad(string param) {
        // HTMLのbodyを作成する。
        string body = string.Format("Param = '{0}'", param);
        // それを出力する。
        RenderBody("Example1", body);
        // ヘッダから全て出力する場合は、Render(string) を使う。
    }
}
