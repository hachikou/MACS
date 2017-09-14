/*! @file Example6.cs
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
        sv.AddPage("/example6", typeof(SamplePage));
        sv.DefaultPage = "/example6";
        sv.Run(4);
        return 0;
    }
}

public class SamplePage : HttpTemplatePage {

    public override void PageLoad(string param) {

        // hogeをフラグに使う
        if(param == "/hoge")
            Assign("hoge", true);
        else
            Assign("hoge", false);

        // mogeは変数の内容をチェックする
        Assign("moge", param);

        RenderTemplate();
    }
}
