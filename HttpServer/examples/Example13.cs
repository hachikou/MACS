/*! @file Example13.cs
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
        sv.AddPage("/example13", typeof(SamplePage));
        sv.DefaultPage = "/example13";

        // セッション変数を保持しておく時間
        sv.SessionTimeout = 20; // sec.

        sv.Run(4);
        return 0;
    }
}

// セッション変数を使う例:
public class SamplePage : HttpTemplatePage {

    protected Literal message;
    protected TextBox username;
    protected SubmitButton loginbutton;
    protected SubmitButton logoutbutton;
    protected SubmitButton countbutton;

    public override void PageLoad(string param) {
        AssignWebControls();

        if(Fetch("loginbutton") != null) {
            // Session はセッション毎の ObjectDictionary
            Session["username"] = Fetch("username");
            Session["count"] = 0;
        }
        if(Fetch("logoutbutton") != null) {
            Session["username"] = null;
        }

        string uname = Session["username"] as string;
        if(string.IsNullOrEmpty(uname)) {
            // セッション変数"username"がセットされていない時はログイン中ではない
            loginbutton.Visible = true;
            logoutbutton.Visible = false;
            countbutton.Visible = false;
            message.Text = HE("名前を入力して下さい");
        } else {
            // ログイン中
            loginbutton.Visible = false;
            logoutbutton.Visible = true;
            countbutton.Visible = true;
            username.Text = uname;
            username.Disabled = true;
            int count = (int)Session["count"];
            if(Fetch("countbutton") != null) {
                count++;
                Session["count"] = count;
            }
            message.Text = HE(string.Format("Count = {0}", count));
        }

        RenderTemplate();

    }
}
