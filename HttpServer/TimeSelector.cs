/*! @file TimeSelector.cs
 * @brief 時刻入力ウィジェット
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
///   時刻入力ウィジェット
/// </summary>
/// <remarks>
///   <para>
///     時／分入力のプルダウンを表示します。
///     00:00を0とする分がValueになります。
///   </para>
/// </remarks>
public class TimeSelector : TranslatableWebControl {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public TimeSelector() : base() {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public TimeSelector(Translatable tr) : base(tr) {}

    /// <summary>
    ///   選択された値。Valueと同じだが、intにキャストされている。
    ///   00:00を0とする分。
    /// </summary>
    public int Selected {
        get {
            if(Value == null)
                return 0;
            if(Value is int)
                return (int)Value;
            return StringUtil.ToInt(Value.ToString());
        }
        set {
            Value = value;
        }
    }

    /// <summary>
    ///   時の選択範囲（始め）
    /// </summary>
    public int HourFrom = 0;

    /// <summary>
    ///   時の選択範囲（終わり）
    /// </summary>
    public int HourTo = 23;

    /// <summary>
    ///   分の間隔
    /// </summary>
    public int MinuteStep = 5;

    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(String.IsNullOrEmpty(Name)) {
            Name = "TimeSelector";
        }
        int val = Selected;
        int hh = val/60;
        int mm = val%60;
        if(!Visible) {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Hour");
            sb.Append("\" value=\"");
            sb.Append(hh.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Hour");
                sb.Append("\"");
            }
            sb.Append("/>");
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name+"Minute");
            sb.Append("\" value=\"");
            sb.Append(mm.ToString());
            sb.Append("\"");
            if(!String.IsNullOrEmpty(ID)) {
                sb.Append(" id=\"");
                sb.Append(ID+"Minute");
                sb.Append("\"");
            }
            sb.Append("/>");
            return sb;
        }
        sb.Append("<span class=\"timeselector");
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

        string hourFormat = _("TimeSelector.HourFormat");
        string hourSeparator = "";
        if(!hourFormat.Contains("{0}")) {
            hourFormat = "{0}";
            hourSeparator = ":";
        }
        string minuteFormat = _("TimeSelector.MinuteFormat");
        if(!minuteFormat.Contains("{0}")) {
            minuteFormat = "{0}";
        }

        sb.Append("<select name=\"");
        sb.Append(Name+"Hour");
        sb.Append("\"");
        if(!String.IsNullOrEmpty(OnChange)) {
            sb.Append(" onchange=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        sb.Append(">");
        for(int i = HourFrom; i <= HourTo; i++) {
            sb.Append("<option value=\"");
            sb.Append(i.ToString());
            sb.Append("\"");
            if(hh == i)
                sb.Append(" selected=\"selected\"");
            sb.Append(">");
            sb.Append(HE(String.Format(hourFormat,i)));
            sb.Append("</option>");
        }
        sb.Append("</select>");
        if(!String.IsNullOrEmpty(hourSeparator)) {
            sb.Append(" "+hourSeparator+" ");
        }

        sb.Append("<select name=\"");
        sb.Append(Name+"Minute");
        sb.Append("\"");
        if(!String.IsNullOrEmpty(OnChange)) {
            sb.Append(" onchange=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        sb.Append(">");
        if(MinuteStep <= 0)
            MinuteStep = 1;
        bool selected = false;
        for(int i = 0; i < 60; i += MinuteStep) {
            sb.Append("<option value=\"");
            sb.Append(i.ToString());
            sb.Append("\"");
            if(!selected && (i <= mm) && (mm < (i+MinuteStep))) {
                sb.Append(" selected=\"selected\"");
                selected = true;
            }
            sb.Append(">");
            sb.Append(HE(String.Format(minuteFormat,i)));
            sb.Append("</option>");
        }
        sb.Append("</select>");

        sb.Append("</span>");
        RenderInLineError(sb);
        return sb;
    }

    public override void Fetch(HttpPage page, object defaultValue=null) {
        int d,hh,mm;
        if(defaultValue == null) {
            d = 0;
        } else if(defaultValue is int) {
            d = (int)defaultValue;
        } else {
            d = StringUtil.ToInt(defaultValue.ToString());
        }
        hh = d/60;
        mm = d%60;
        Value = page.Fetch(Name+"Hour",hh)*60+page.Fetch(Name+"Minute",mm);
    }
}

} // End of namespace
