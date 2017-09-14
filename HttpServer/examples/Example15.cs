/*! @file Example15.cs
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
        sv.AddPage("/example15", typeof(SamplePage));
        sv.DefaultPage = "/example15";
        sv.Run(4);
        return 0;
    }
}

// HttpValidationPageの派生クラスを作る。
public class SamplePage : HttpValidationPage {

    protected Literal message;
    protected TextBox numberfield;
    protected SubmitButton sendbutton;

    public override void PageLoad(string param) {
        AssignWebControls();

        if(Fetch("sendbutton") != null){
            // 入力をバリデーションしながら読み取る。
            int number = ValidateInt(Fetch("numberfield"), 0, 100, true, "数値", numberfield);

            // バリデーションエラー？
            if(IsValidationError){
                message.Text = ValidationMessage;
            } else {
                message.Text = HE(string.Format("{0}が入力されました。", number));
            }
        }

        RenderTemplate();
    }

}
