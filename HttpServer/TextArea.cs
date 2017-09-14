/*! @file TextArea.cs
 * @brief textarea要素
 * $Id: $
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Web;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   textarea要素
/// </summary>
public class TextArea : WebControl {

    /// <summary>
    ///   入力テキスト。
    ///   文字列のみを扱う事以外はValueと同一。
    /// </summary>
    public string Text {
        get { return (Value == null)?"":Value.ToString(); }
        set { Value = value; }
    }

    /// <summary>
    ///   入力カラムサイズ
    /// </summary>
    public int Columns = 0;

    /// <summary>
    ///   入力行数
    /// </summary>
    public int Rows = 5;

    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TextArea(string name, string id) : base(name, id) {
        CssClass = "text";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TextArea(string name) : base(name) {
        CssClass = "text";
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public TextArea() : base() {
        CssClass = "text";
    }


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible) {
            return sb;
        }
        sb.Append("<textarea");
        if(Columns > 0)
            sb.AppendFormat(" cols={0}", Columns);
        if(Rows > 0)
            sb.AppendFormat(" rows={0}", Rows);
        if(!String.IsNullOrEmpty(OnChange)) {
            sb.Append(" onchange=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        CommonOptions(sb);
        sb.Append(">");
        if(Value != null){
            sb.Append(HE(Value.ToString()));
        }
        sb.Append("</textarea>");
        RenderInLineError(sb);
        return sb;
    }

}

} // namespace SCS

} // End of namespace
