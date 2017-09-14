/*! @file Example7.cs
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
        sv.AddPage("/example7", typeof(SamplePage));
        sv.DefaultPage = "/example7";
        sv.Run(4);
        return 0;
    }
}

public class SamplePage : HttpTemplatePage {

    public override void PageLoad(string param) {

        // 配列を変数に代入する
        string[] mylist = new string[7];
        mylist[0] = "日曜日";
        mylist[1] = "月曜日";
        mylist[2] = "火曜日";
        mylist[3] = "水曜日";
        mylist[4] = "木曜日";
        mylist[5] = "金曜日";
        mylist[6] = "土曜日";
        Assign("mylist", mylist);

        RenderTemplate();
    }
}
