/*! @file HttpStackTracePage.cs
 * @brief デバッグ用スタックトレース表示ページレンダラ
 * $Id: $
 *
 * Copyright (C) 2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   デバッグ用スタックトレース表示ページレンダラ
/// </summary>
public class HttpStackTracePage : HttpPage {

    public HttpStackTracePage(Exception e)
        :base() {
        m_exception = e;
    }

    public override void PageLoad(string param) {
        SetNoCache();
        StringBuilder sb = new StringBuilder();
        sb.Append("<h1>");
        sb.Append(HE(m_exception.GetType().Name));
        sb.Append("</h1>\n");
        sb.Append("<div style='margin: 0px 0px 0px 20px; padding: 4px;'>");
        int count = 0;
        Type t = m_exception.GetType();
        while((t != null) && (count < 4)) {
            if(count > 0)
                sb.Append(" - ");
            sb.Append(HE(t.FullName));
            t = t.BaseType;
            count++;
        }
        sb.Append("</div>\n");
        sb.Append("<div style='border-style: solid; border-color: black; border-width: 2px; margin: 10px 0px 0px 20px; padding: 4px;'>");
        sb.Append(HE(m_exception.Message));
        sb.Append("</div>");
        sb.Append("<div style='border-style: solid; border-color: black; border-width: 2px; margin: 10px 0px 0px 20px; padding: 4px;'>");
        sb.Append("StackTrace:<pre>\n");
        sb.Append(HE(m_exception.StackTrace));
        sb.Append("</pre></div>");
        RenderBody(m_exception.GetType().Name, sb.ToString());
    }

    private Exception m_exception;
}

} // End of namespace
