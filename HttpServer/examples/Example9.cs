/*! @file Example9.cs
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
        HttpTemplatePage.SetTemplateDir("template");
        sv.AddPage("/example9", typeof(SamplePage));
        sv.DefaultPage = "/example9";
        sv.Run(4);
        return 0;
    }
}

// WebControlを利用するテンプレートレンダラの例
public class SamplePage : HttpTemplatePage {

    // LiteralとTemplateFlagの例
    protected Literal foo;
    protected Literal foo2;
    protected TemplateFlag baa;

    public override void PageLoad(string param) {

        AssignWebControls();

        // {foo}はTextの値がそのままレンダリングされる。
        foo.Text = "おはよう";

        // {foo2}はspan要素でCSSクラスが指定される
        foo2.Text = "こんにちわ";
        foo2.CssClass = "reverse";

        // baaは {!if } で利用する
        baa.Disabled = false;

        RenderTemplate();
    }

}
