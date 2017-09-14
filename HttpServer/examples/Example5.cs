/*! @file Example5.cs
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

        sv.AddPage("/example5", typeof(SamplePage));

        sv.DefaultPage = "/example5";
        sv.Run(4);
        return 0;
    }
}

public class SamplePage : HttpTemplatePage {
    public override void PageLoad(string param) {

        // 各種エスケープをする例
        Assign("foo", "'\"<>&\\");

        RenderTemplate();
    }

}
