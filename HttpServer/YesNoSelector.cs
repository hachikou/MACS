/*! @file RadioSelector.cs
 * @brief Yes/Noをラジオボタンで選択する要素
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
///   Yes/Noをラジオボタンで選択する要素
/// </summary>
public class YesNoSelector: TranslatableWebControl {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public YesNoSelector() : base() {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public YesNoSelector(Translatable tr) : base(tr) {}

    /// <summary>
    ///   選択された値。Valueと同じだが、boolにキャストされている
    /// </summary>
    public bool Selected {
        get {
            if(Value == null)
                return false;
            return (bool)Value;
        }
        set { Value = value; }
    }

    /// <summary>
    ///   yesの時の表示文字（これがTranslatorで翻訳される）
    /// </summary>
    public string YesString = "Yes";

    /// <summary>
    ///   noの時の表示文字（これがTranslatorで翻訳される）
    /// </summary>
    public string NoString = "No";

    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(String.IsNullOrEmpty(Name)) {
            Name = this.GetType().Name;
        }
        if(!Visible) {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name);
            sb.AppendFormat("\" value=\"{0}\"", Selected?"yes":"no");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID);
                sb.Append("\"");
            }
            sb.Append("/>");
            return sb;
        }
        sb.Append("<div class=\"radiogroup");
        if(CssClass != null){
            sb.Append(" ");
            sb.Append(CssClass);
        }
        sb.Append("\"");
        if(!String.IsNullOrEmpty(ID)) {
            sb.Append(" id=\"");
            sb.Append(ID);
            sb.Append("\"");
        }
        sb.Append(">");

        string id = Name+"-true";
        sb.Append("<span class=\"radio\" style=\"white-space:nowrap\">");
        sb.Append("<input type=\"radio\" name=\"");
        sb.Append(Name);
        sb.Append("\" id=\"");
        sb.Append(id);
        sb.Append("\" value=\"yes\"");
        if(Selected)
            sb.Append(" checked=\"checked\"");
        if(!String.IsNullOrEmpty(OnClick)) {
            sb.Append(" onclick=\"");
            sb.Append(OnClick);
            sb.Append("\"");
        }
        if(Disabled)
            sb.Append(" disabled=\"disabled\"");
        sb.Append("/><label for=\"");
        sb.Append(id);
        sb.Append("\">");
        sb.Append(HE(_(YesString)));
        sb.Append("</label></span>");

        sb.Append(" ");

        id = Name+"-false";
        sb.Append("<span class=\"radio\" style=\"white-space:nowrap\">");
        sb.Append("<input type=\"radio\" name=\"");
        sb.Append(Name);
        sb.Append("\" id=\"");
        sb.Append(id);
        sb.Append("\" value=\"no\"");
        if(!Selected)
            sb.Append(" checked=\"checked\"");
        if(!String.IsNullOrEmpty(OnClick)) {
            sb.Append(" onclick=\"");
            sb.Append(OnClick);
            sb.Append("\"");
        }
        if(Disabled)
            sb.Append(" disabled=\"disabled\"");
        sb.Append("/><label for=\"");
        sb.Append(id);
        sb.Append("\">");
        sb.Append(HE(_(NoString)));
        sb.Append("</label></span>");

        sb.Append("</div>");
        RenderInLineError(sb);
        return sb;
    }

    public override void Fetch(HttpPage page, object defaultValue=null) {
        if(defaultValue == null)
            defaultValue = "";
        Value = StringUtil.ToBool(page.Fetch(Name,defaultValue.ToString()));
    }
}

} // End of namespace
