/*! @file RadioSelector.cs
 * @brief Enumをラジオボタンで選択する要素
 * $Id: $
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Web;
using System.Text;
using System.Collections.Generic;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   Enumをラジオボタンで選択する要素
/// </summary>
/// <remarks>
///   <para>
///     Enum要素数分のラジオボタンを一気に描画します。
///   </para>
/// </remarks>
public class RadioSelector<T> : TranslatableWebControl
    where T : struct {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public RadioSelector() : base() {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public RadioSelector(Translatable tr) : base(tr) {}

    /// <summary>
    ///   選択された値。Valueと同じだが、enumにキャストされている
    /// </summary>
    public T Selected {
        get {
            if(Value == null)
                return default(T);
            return (T)Value;
        }
        set { Value = (T)value; }
    }

    /// <summary>
    ///   縦に並べるかどうか
    /// </summary>
    public bool Vertical = false;

    /// <summary>
    ///   選択肢に表示しない値の一覧
    /// </summary>
    public T[] ExceptionList = null;

    /// <summary>
    ///   選択肢の表示名
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定がない場合は、typeof(T).Name+"."+T.ToString() を翻訳したもの
    ///   </para>
    /// </remarks>
    public Dictionary<T,string> Text = null;

    /// <summary>
    /// 　ラジオボタン Enum名表示
    /// 　<remarks>
    /// 　　true:Enum名表示 false:Enum名非表示
    /// 　</remarks>
    /// </summary>
    public bool ShowEnumName = true;

    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(String.IsNullOrEmpty(Name)) {
            Name = typeof(T).Name;
        }
        if(!Visible) {
            sb.Append("<input type=\"hidden\" name=\"");
            sb.Append(Name);
            sb.Append("\" value=\"");
            sb.Append(Selected.ToString());
            sb.Append("\"");
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
        T[] list = (T[])Enum.GetValues(typeof(T));
        foreach(T x in list) {
            if(ExceptionList != null) {
                bool ex = false;
                foreach(T i in ExceptionList) {
                    if(x.ToString() == i.ToString())
                        ex = true;
                }
                if(ex)
                    continue;
            }
            string val = x.ToString();
            string id = Name+"-"+val;
            sb.Append("<span class=\"radio\" style=\"white-space:nowrap\">");
            sb.Append("<input type=\"radio\" name=\"");
            sb.Append(Name);
            sb.Append("\" id=\"");
            sb.Append(id);
            sb.Append("\" value=\"");
            sb.Append(val);
            sb.Append("\"");
            if(val == Selected.ToString())
                sb.Append(" checked=\"checked\"");
            if(!String.IsNullOrEmpty(OnClick)) {
                sb.Append(" onclick=\"");
                sb.Append(OnClick);
                sb.Append("\"");
            }
            sb.Append("/><label for=\"");
            sb.Append(id);
            sb.Append("\">");
            string vv;
            if((Text == null) || !Text.TryGetValue(x, out vv)){
                if(ShowEnumName)
                    vv = _(typeof(T).Name+"."+val);
                else
                    vv = _(val);
            }
            sb.Append(HE(vv));
            sb.Append("</label></span>");
            if(Vertical)
                sb.Append("<br/>");
            else
                sb.Append(" ");
        }
        sb.Append("</div>");
        RenderInLineError(sb);
        return sb;
    }

    public override void Fetch(HttpPage page, object defaultValue=null) {
        T val;
        if(Enum.TryParse<T>(page.Fetch(Name,""), out val)) {
            Value = val;
        } else {
            Value = defaultValue;
        }
    }

}

} // End of namespace
