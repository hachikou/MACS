/*! @file TextBox.cs
 * @brief input text要素
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Web;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   input text要素
/// </summary>
public class TextBox : WebControl {

    /// <summary>
    ///   入力テキスト。
    ///   文字列のみを扱う事以外はValueと同一。
    /// </summary>
    public string Text {
        get { return (Value == null)?"":Value.ToString(); }
        set { Value = value; }
    }

    /// <summary>
    ///   テキストモード
    /// </summary>
    public string TextMode = "text";

    /// <summary>
    ///   autocompleteフラグ
    /// </summary>
    public bool AutoComplete = false;

    /// <summary>
    ///   最大入力文字数
    /// </summary>
    public int MaxLength = 0;

    /// <summary>
    ///   表示サイズ（文字数）
    /// </summary>
    public int Size = 0;

    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TextBox(string name, string id, int maxlength) : base(name, id) {
        MaxLength = maxlength;
        CssClass = "text";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TextBox(string name, int maxlength) : base(name) {
        MaxLength = maxlength;
        CssClass = "text";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TextBox(string name, string id) : base(name, id) {
        CssClass = "text";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TextBox(string name) : base(name) {
        CssClass = "text";
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public TextBox() : base() {
        CssClass = "text";
    }


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        sb.Append("<input type=\"");
        sb.Append(TextMode);
        sb.Append("\"");
        sb.Append(" autocomplete=\"");
        sb.Append(AutoComplete?"on":"off");
        sb.Append("\"");
        if(MaxLength > 0){
            sb.Append(" maxlength=\"");
            sb.Append(MaxLength);
            sb.Append("\"");
        }
        if(Size > 0){
            sb.Append(" size=\"");
            sb.Append(Size);
            sb.Append("\"");
        }
        if(Value != null){
            sb.Append(" value=\"");
            sb.Append(HE(Value.ToString()));
            sb.Append("\"");
        }
        if(!String.IsNullOrEmpty(OnChange)) {
            sb.Append(" onchange=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        CommonOptions(sb);
        sb.Append(" />");
        RenderInLineError(sb);
        return sb;
    }

}

} // End of namespace
