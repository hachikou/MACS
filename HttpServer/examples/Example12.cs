/*! @file Example12.cs
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
        HttpTemplatePage.SetTemplateDir("template");
        sv.AddPage("/example12", typeof(SamplePage));
        sv.DefaultPage = "/example12";
        sv.Run(4);
        return 0;
    }
}

// ファイルアップロードを受信する例:
public class SamplePage : HttpTemplatePage {

    // 必ずしもFileInputBoxを使う必要はないが、使うとレンダリングが楽
    protected FileInputBox uploadfile;
    protected SubmitButton sendbutton;
    protected Literal message;

    public override void PageLoad(string param) {
        AssignWebControls();

        // 送信ボタンが押された時は、ファイルを受信する
        if(Fetch("sendbutton") != null) {

            // アップロードされたファイルは FetchFileメソッドで受け取る
            HttpPostedFile upfile = FetchFile("uploadfile");
            if(upfile != null){
                message.Text = HE(string.Format("Uploaded {0} ({1}bytes)", upfile.FileName, upfile.ContentLength));
                upfile.SaveAs("uploadedfile");
            }

        }

        RenderTemplate();

    }
}
