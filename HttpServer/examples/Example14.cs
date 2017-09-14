/*! @file Example14.cs
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using SCS;

class Program {
    public static int Main(string[] args) {
        HttpServer sv = new HttpServer("SampleHttpServer", "*", 8888);
        HttpTemplatePage.SetTemplateDir("template");

        // 使用言語を設定する
        HttpNlsSupport.DefaultLanguage = "auto";

        // "/xxxx" にアクセスすると、nls/xxxx.言語コード ファイルを返す。
        HttpStaticPage nlspage = new HttpStaticPage("nls");
        // 個別に使用言語を変えたい場合: nlspage.SetLanguage("es");
        sv.AddStaticPage("/", nlspage);

        // テンプレートの翻訳辞書ファイルのディレクトリを指定する。
        Translator.DictionaryDirectory = "lang";

        sv.AddPage("/example14", typeof(SamplePage));

        sv.DefaultPage = "/index.html";

        sv.Run(4); // 4 workers
        return 0;
    }
}

// メッセージの翻訳を行なう例:
public class SamplePage : HttpTemplatePage {

    protected Literal message;

    public SamplePage() {
        // 個別に使用言語を変えたい場合: SetLanguage("es");
    }

    public override void PageLoad(string param) {
        AssignWebControls();

        // _() メソッドは、言語コードに応じて翻訳を行なう。
        message.Text = _("おはようございます");

        RenderTemplate();

    }
}
