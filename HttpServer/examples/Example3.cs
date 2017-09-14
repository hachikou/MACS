/*! @file Example3.cs
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

        // "/xxxx" にアクセスすると、html/xxxx ファイルを返す。
        sv.AddStaticPage("/", "html");

        // "/css/xxxx" にアクセスすると、css/xxxx ファイルを返す。
        // このディレクトリ内のファイルのContent-Typeは text/css 固定。
        HttpStaticPage csspage = new HttpStaticPage("css");
        csspage.SetContentType("text/css");
        sv.AddStaticPage("/css", csspage);

        // "/document/xxxx" にアクセスすると、doc/xxxx ファイルを返す。
        // このディレクトリ内では".rtf"という拡張子のファイルのContent-Typeを"application/rtf"にする。
        HttpStaticPage docpage = new HttpStaticPage("doc");
        csspage.SetContentType(".rtf", "application/rtf");
        sv.AddStaticPage("/document", docpage);

        sv.DefaultPage = "/index.html";
        sv.Run(4);
        return 0;
    }
}
