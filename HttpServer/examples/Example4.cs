/*! @file Example4.cs
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using SCS;

class Program {
    public static int Main(string[] args) {
        HttpServer sv = new HttpServer("SampleHttpServer", "*", 8888);

        // HTMLテンプレートは、template ディレクトリ内にある。
        HttpTemplatePage.SetTemplateDir("template");

        // "/example4" にアクセスすると、SamplePageクラスのPageLoadメソッドが呼び出される
        sv.AddPage("/example4", typeof(SamplePage));

        sv.DefaultPage = "/example4";
        sv.Run(4);
        return 0;
    }
}

// テンプレートを使って動的に生成するページの例:
// HttpTemplatePageの派生クラスを用意する
public class SamplePage : HttpTemplatePage {
    public override void PageLoad(string param) {

        // テンプレート変数"hoge"に、paramを代入する。
        Assign("hoge", HE(param)); // HTTPエスケープをしておく

        // パブリックなメンバを持つオブジェクトを値にセットする。
        Mugyo mugyo = new Mugyo(124, 346);
        Assign("mugyo", mugyo);

        // テンプレートを出力する。
        // ページ名.html（この場合"example4.html"）または、クラス名.html（この場合"SamplePage.html"）がレンダリングされる。
        RenderTemplate();

        // テンプレートファイル名を指定する場合は、 RenderTemplate(ファイル名) を使う。
    }

}

// パブリックなメンバを持つクラスの例
public class Mugyo {
    public int x;
    public int y;

    public Mugyo(int x_, int y_) {
        x = x_;
        y = y_;
    }

    public int xy {
        get { return x*y; }
    }

}
