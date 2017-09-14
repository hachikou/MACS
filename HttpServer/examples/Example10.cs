/*! @file Example10.cs
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

        // "/xxxx" にアクセスすると、html/xxxxファイルがあればそれを返し、
        // なければ html.csに埋め込まれた/xxxxファイルを返す。
        HttpStaticPage page = new HttpStaticPage("html");
        page.SetBuiltinContents(html.Contents);
        sv.AddStaticPage("/", page);

        sv.DefaultPage = "/index.html";

        sv.Run(4);
        return 0;
    }
}
