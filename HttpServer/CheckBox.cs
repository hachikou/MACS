/*! @file CheckBox.cs
 * @brief input checkbox要素
 * $Id: $
 *
 * Copyright (C) 2008-2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Web;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   input checkbox要素
/// </summary>
public class CheckBox : TranslatableWebControl {

    /// <summary>
    ///   チェックされているかどうか
    /// </summary>
    public bool Checked = false;

    /// <summary>
    ///   表示文字列
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Valueはアプリケーションに送信される文字列。Textは表示上の文字列。
    ///   </para>
    /// </remarks>
    public string Text;

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, string id, string text, object value) : base(name, id) {
        Text = text;
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, string id, string text, object value, Translatable tr) : base(name, id, tr) {
        Text = text;
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, string text, object value) : base(name) {
        Text = text;
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, string text, object value, Translatable tr) : base(name, tr) {
        Text = text;
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, string text) : base(name) {
        Text = text;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, string text, Translatable tr) : base(name, tr) {
        Text = text;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name) : base(name) {}

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CheckBox(string name, Translatable tr) : base(name, tr) {}

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public CheckBox() : base() {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public CheckBox(Translatable tr) : base(tr) {}


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        if(ID == null){
            ID = "checkbox_";
            if(Name != null)
                ID += Name;
        }
        sb.Append("<input type=\"checkbox\"");
        if(Value != null){
            sb.Append(" value=\"");
            sb.Append(HE(Value.ToString()));
            sb.Append("\"");
        }
        CommonOptions(sb);
        if(Checked)
            sb.Append(" checked=\"checked\"");
        sb.Append(" />");
        if((Text != null) && (Text != "")){
            sb.Append("<label for=\"");
            sb.Append(HE(ID));
            sb.Append("\"> ");
            sb.Append(HE(_(Text)));
            sb.Append("</label>");
        }
        RenderInLineError(sb);
        return sb;
    }

    public override void Fetch(HttpPage page, object defaultValue=null) {
        foreach(string val in page.Fetch(Name, "").Split(",".ToCharArray())) {
            if(val == Value.ToString()) {
                Checked = true;
                return;
            }
        }
        Checked = false;
    }

}

} // End of namespace
