/*! @file DropDownSelector.cs
 * @brief Enumをプルダウンメニューで選択する要素
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
///   Enumをプルダウンメニューで選択する要素
/// </summary>
public class DropDownSelector<T> : TranslatableWebControl
    where T : struct {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public DropDownSelector() : base() {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public DropDownSelector(Translatable tr) : base(tr) {}

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
    ///   選択肢に含まない項目を指定する
    /// </summary>
    public void SetException(params T[] ex) {
        exceptionList = ex;
    }

    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;

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
        sb.Append("<select");
        CommonOptions(sb);
        if(OnChange != null){
            sb.Append(" onchange=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        sb.Append(">");
        T[] list = (T[])Enum.GetValues(typeof(T));
        foreach(T x in list) {
            bool ex = false;
            if(exceptionList != null) {
                foreach(T xx in exceptionList) {
                    if(x.Equals(xx)) {
                        ex = true;
                        break;
                    }
                }
            }
            if(ex)
                continue;
            string val = x.ToString();
            string id = Name+"-"+val;
            sb.Append("<option id=\"");
            sb.Append(id);
            sb.Append("\" value=\"");
            sb.Append(val);
            sb.Append("\"");
            if(val == Selected.ToString())
                sb.Append(" selected=\"selected\"");
            sb.Append(">");
            string vv;
            if((Text == null) || !Text.TryGetValue(x, out vv))
                vv = _(typeof(T).Name+"."+val);
            sb.Append(HE(vv));
            sb.Append("</option>");
        }
        sb.Append("</select>");
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


    private T[] exceptionList = null;

}

} // End of namespace
