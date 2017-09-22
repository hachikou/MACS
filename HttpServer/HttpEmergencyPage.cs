/*! @file HttpEmergencyPage.cs
 * @brief HttpServer内ワーカーが枯渇した時の非常ページ表示クラス
 * $Id: $
 *
 * Copyright (C) 2017 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   非常表示ページレンダラ
/// </summary>
public class HttpEmergencyPage : HttpPage {

    public override void PageLoad(string param) {
        Response.StatusCode = 500;
        StringBuilder body = new StringBuilder();
        body.Append("<h1>SERVER ERROR</h1>");
        body.Append("<p>Sorry, but the server couldn't dispatch your request.</p>");
        body.Append("<p>Maybe there have been many heavy requests and the server exhaust all working threads.</p>");
        body.Append("<p>Please try again later.</p>");
        body.Append("</body>");
        RenderBody("SERVER ERROR", body.ToString());
    }
    
}

} // End of namespace
