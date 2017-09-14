/*! @file Example8.cs
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
        sv.AddPage("/example8", typeof(SamplePage));
        sv.DefaultPage = "/example8";
        sv.Run(4);
        return 0;
    }
}

// WebControlを利用するテンプレートレンダラの例
public class SamplePage : HttpTemplatePage {

    // ウィジェットをメンバ変数として宣言する。
    // AssignWebControlsメソッドがいじれるように、protectedかpublicにしておく。
    protected TextBox mytext;
    protected SubmitButton sendbutton;

    public override void PageLoad(string param) {

        // ウィジェット用メンバ変数をテンプレート変数に自動割り当て
        AssignWebControls();
        // これで、メンバ変数 mytext, sendbutton にインスタンスが割り当てられ、
        // "mytext", "sendbutton"というテンプレート変数の値としてこれらのオブジェクトが割り当てられる。

        // フォームの入力値を取り込む。
        string x_mytext = Fetch("mytext");

        // ウィジェット変数のメンバにアクセスする事で、ウィジェットをコントロールできる。
        mytext.Value = x_mytext;
        if(x_mytext == "hoge")
            mytext.Disabled = true;

        RenderTemplate();
    }

}
