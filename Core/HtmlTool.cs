/// HtmlTool: HTMLコード作成に便利なユーティリティ集.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Web;
using System.Text;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   HTMLコード作成に便利なユーティリティ集。
/// </summary>
/// <remarks>
///   <para>
///     全てグローバルな関数であり、本来はクラス化する必要が無い。
///     コード表記を簡略化するためのネームスペース代りに使っているにすぎない。
///   </para>
/// </remarks>
public class HtmlTool : Loggable {

    /// <summary>
    ///   URLエンコーディングした文字列を返す。
    /// </summary>
    public static string UE(string txt) {
        if(txt == null)
            return "";
        return HttpUtility.UrlEncode(txt, Encoding.GetEncoding("UTF-8"));
    }

    /// <summary>
    ///   URLエンコーディングされた文字列をデコードして返す。
    /// </summary>
    public static string EU(string txt) {
        if(txt == null)
            return "";
        return HttpUtility.UrlDecode(txt, Encoding.GetEncoding("UTF-8"));
    }

    /// <summary>
    ///   HTMLエンコーディングした文字列を返す。
    /// </summary>
    public static string HE(string txt) {
        if(txt == null)
            return "";
        //return HttpUtility.HtmlEncode(txt); // HtmlEncodeは日本語も&#変換してしまう。
        StringBuilder sb = new StringBuilder();
        foreach(char ch in txt){
            switch(ch){
            case '&':
                sb.Append("&amp;");
                break;
            case '<':
                sb.Append("&lt;");
                break;
            case '>':
                sb.Append("&gt;");
                break;
            case '\"':
                sb.Append("&quot;");
                break;
            case '\'':
                sb.Append("&#39;");
                break;
            default:
                sb.Append(ch);
                break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    ///   HTMLエンコーディングされた文字列をデコードして返す。
    /// </summary>
    public static string EH(string txt) {
        if(txt == null)
            return "";
        return HttpUtility.HtmlDecode(txt);
    }

    /// <summary>
    ///   クォート文字をエスケープした文字列を返す。
    /// </summary>
    public static string QE(string txt) {
        if(txt == null)
            return "";
        StringBuilder sb = new StringBuilder();
        foreach(char ch in txt) {
            bool flag = false;
            foreach(KeyValuePair<char,string> kv in QEDict) {
                if(ch == kv.Key) {
                    sb.Append(kv.Value);
                    flag = true;
                    break;
                }
            }
            if(!flag)
                sb.Append(ch);
        }
        return sb.ToString();
        //return txt.Replace("&", "&amp;").Replace("\\", "&#92;").Replace("\"", "&quot;").Replace("'", "&#39;");
    }

    private static readonly Dictionary<char,string> QEDict = new Dictionary<char,string>() {
        {'\n', "\\n"},
        {'\f', "\\f"},
        {'\b', "\\b"},
        {'\r', "\\r"},
        {'\t', "\\t"},
        {'\'', "\\'"},
        {'"', "\\\""},
        {'\\',"\\\\"}
    };

    /// <summary>
    ///   日時表示をフォーマットしなおす
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     年を表示しない。
    ///   </para>
    /// </remarks>
    public static string DT(string txt) {
        if(txt == null)
            return "";
        int i = txt.IndexOf('/');
        if(i >= 0)
            return txt.Substring(i + 1).Replace(" ", "&nbsp;");
        return txt;
    }

    /// <summary>
    ///   HTML checkbox input要素タグを生成する。
    /// </summary>
    public static string HtmlCheckBox(string name, string txt, bool flag) {
        StringBuilder sb = new StringBuilder();
        HtmlCheckBox(sb, name, null, txt, flag, false);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML checkbox input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlCheckBox(StringBuilder sb, string name, string txt, bool flag) {
        return HtmlCheckBox(sb, name, null, txt, flag, false);
    }
    /// <summary>
    ///   HTML checkbox input要素タグを生成する。value付き。
    /// </summary>
    public static string HtmlCheckBox(string name, object val, string txt, bool flag) {
        StringBuilder sb = new StringBuilder();
        HtmlCheckBox(sb, name, val, txt, flag, false);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML checkbox input要素タグをStringBuilderに書き込む。value付き。
    /// </summary>
    public static StringBuilder HtmlCheckBox(StringBuilder sb, string name, object val, string txt, bool flag) {
        return HtmlCheckBox(sb, name, val, txt, flag, false);
    }
    /// <summary>
    ///   HTML checkbox input要素タグを生成する。disabledフラグ付き。
    /// </summary>
    public static string HtmlCheckBox(string name, string txt, bool flag, bool disableflag) {
        StringBuilder sb = new StringBuilder();
        HtmlCheckBox(sb, name, null, txt, flag, disableflag);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML checkbox input要素タグをStringBuilderに書き込む。disabledフラグ付き。
    /// </summary>
    public static StringBuilder HtmlCheckBox(StringBuilder sb, string name, string txt, bool flag, bool disableflag) {
        return HtmlCheckBox(sb, name, null, txt, flag, disableflag);
    }
    /// <summary>
    ///   HTML checkbox input要素タグを生成する。CSS以外全属性指定。
    /// </summary>
    public static string HtmlCheckBox(string name, object val, string txt, bool flag, bool disableflag) {
        StringBuilder sb = new StringBuilder();
        HtmlCheckBox(sb, name, val, txt, flag, disableflag);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML checkbox input要素タグをStringBuilderに書き込む。CSS以外全属性指定。
    /// </summary>
    public static StringBuilder HtmlCheckBox(StringBuilder sb, string name, object val, string txt, bool flag, bool disableflag) {
        return HtmlCheckBox(sb, name, val, null, txt, flag, disableflag);
    }
    /// <summary>
    ///   HTML checkbox input要素タグをStringBuilderに書き込む。全属性指定。
    /// </summary>
    public static StringBuilder HtmlCheckBox(StringBuilder sb, string name, object val, string cssclass, string txt, bool flag, bool disableflag) {
        if(name == null)
            name = "";
        sb.Append("<input type=\"checkbox\" id=\"checkbox_");
        sb.Append(QE(name));
        sb.Append("\" name=\"");
        sb.Append(QE(name));
        sb.Append("\"");
        if(cssclass != null){
            sb.Append(" class=\"");
            sb.Append(cssclass);
            sb.Append("\"");
        }
        if(val != null){
            sb.Append(" value=\"");
            sb.Append(QE(val.ToString()));
            sb.Append("\"");
        }
        if(flag)
            sb.Append(" checked=\"checked\"");
        if(disableflag)
            sb.Append(" disabled=\"disabled\"");
        sb.Append(" />");
        if((txt != null) && (txt != "")){
            sb.Append("<label for=\"checkbox_");
            sb.Append(QE(name));
            sb.Append("\"> ");
            sb.Append(HE(txt));
            sb.Append("</label>");
        }
        return sb;
    }

    /// <summary>
    ///   HTML radio input要素タグを生成する。
    /// </summary>
    public static string HtmlRadio(string name, object value, string txt, bool flag) {
        StringBuilder sb = new StringBuilder();
        HtmlRadio(sb, name, value, txt, flag, false);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML radio input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlRadio(StringBuilder sb, string name, object value, string txt, bool flag) {
        return HtmlRadio(sb, name, value, txt, flag, false);
    }
    /// <summary>
    ///   HTML radio input要素タグを生成する。disabledフラグ付き。
    /// </summary>
    public static string HtmlRadio(string name, object value, string txt, bool checkflag, bool disableflag) {
        StringBuilder sb = new StringBuilder();
        HtmlRadio(sb, name, value, txt, checkflag, disableflag);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML radio input要素タグをStringBuilderに書き込む。disabledフラグ付き。
    /// </summary>
    public static StringBuilder HtmlRadio(StringBuilder sb, string name, object value, string txt, bool checkflag, bool disableflag) {
        return HtmlRadio(sb, name, name+"_"+value, value, null, txt, checkflag, disableflag, null);
    }
    /// <summary>
    ///   HTML radio input要素タグをStringBuilderに書き込む。全属性指定。
    /// </summary>
    public static StringBuilder HtmlRadio(StringBuilder sb, string name, string label, object value, string cssclass, string txt, bool checkflag, bool disableflag, string onclick) {
        if(name == null)
            name = "";
        sb.Append("<input type=\"radio\" value=\"");
        sb.Append(QE(value.ToString()));
        sb.Append("\" name=\"");
        sb.Append(QE(name));
        sb.Append("\"");
        if(label != null){
            sb.Append(" id=\"");
            sb.Append(QE(label));
            sb.Append("\"");
        }
        if(checkflag)
            sb.Append(" checked=\"checked\"");
        if(disableflag)
            sb.Append(" disabled=\"disabled\"");
        if(onclick != null){
            sb.Append(" onclick=\"");
            sb.Append(onclick);
            sb.Append("\"");
        }
        sb.Append(" />");
        if((txt != null) && (txt != "")){
            sb.Append("<label for=\"");
            sb.Append(QE(label));
            sb.Append("\"> ");
            sb.Append(HE(txt));
            sb.Append("</label>");
        }
        return sb;
    }

    /// <summary>
    ///   HTML option要素タグを生成する。
    /// </summary>
    public static string HtmlOption(string value, string txt, bool flag) {
        StringBuilder sb = new StringBuilder();
        HtmlOption(sb, value, txt, flag);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML option要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlOption(StringBuilder sb, string value, string txt, bool flag) {
        sb.Append("<option value=\"");
        sb.Append(QE(value));
        sb.Append("\"");
        if(flag)
            sb.Append(" selected=\"selected\"");
        sb.Append(">");
        sb.Append(HE(txt));
        sb.Append("</option>");
        return sb;
    }

    /// <summary>
    ///   HTML button input要素タグを生成する。(他ページへに遷移する版）
    /// </summary>
    public static string HtmlAnchorButton(string name, string txt, string cssclass, string loc) {
        StringBuilder sb = new StringBuilder();
        HtmlAnchorButton(sb, name, txt, cssclass, loc);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML button input要素タグをStringBuilderに書き込む。(他ページへに遷移する版）
    /// </summary>
    public static StringBuilder HtmlAnchorButton(StringBuilder sb, string name, string txt, string cssclass, string loc) {
        if(name == null)
            name = "";
        sb.Append("<input type=\"button\" class=\"");
        sb.Append(cssclass);
        sb.Append("\" name=\"");
        sb.Append(QE(name));
        sb.Append("\" value=\"");
        sb.Append(QE(txt));
        sb.Append("\" onclick='location.href=\"");
        sb.Append(UE(loc));
        sb.Append("\"' />");
        return sb;
    }

    /// <summary>
    ///   HTML text input要素タグを生成する。
    /// </summary>
    public static string HtmlInput(string name, object value) {
        StringBuilder sb = new StringBuilder();
        HtmlInput(sb, name, value, null, false, 255);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML text input要素タグを生成する。CSSクラス指定版。
    /// </summary>
    public static string HtmlInput(string name, object value, string cssclass) {
        StringBuilder sb = new StringBuilder();
        HtmlInput(sb, name, value, cssclass, false, 255);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML text input要素タグを生成する。CSSクラスと最大文字数指定版。
    /// </summary>
    public static string HtmlInput(string name, object value, string cssclass, int maxlength) {
        StringBuilder sb = new StringBuilder();
        HtmlInput(sb, name, value, cssclass, false, maxlength);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML text input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlInput(StringBuilder sb, string name, object value) {
        return HtmlInput(sb, name, value, null, false, 255);
    }
    /// <summary>
    ///   HTML text input要素タグをStringBuilderに書き込む。CSSクラス指定版。
    /// </summary>
    public static StringBuilder HtmlInput(StringBuilder sb, string name, object value, string cssclass) {
        return HtmlInput(sb, name, value, cssclass, false, 255);
    }
    /// <summary>
    ///   HTML text input要素タグをStringBuilderに書き込む。CSSクラスと最大文字数指定版。
    /// </summary>
    public static StringBuilder HtmlInput(StringBuilder sb, string name, object value, string cssclass, int maxlength) {
        return HtmlInput(sb, name, value, cssclass, false, maxlength);
    }
    /// <summary>
    ///   HTML text input要素タグをStringBuilderに書き込む。CSSクラス、disableフラグ、最大文字数指定版。
    /// </summary>
    public static StringBuilder HtmlInput(StringBuilder sb, string name, object value, string cssclass, bool disabled, int maxlength) {
        return HtmlInput(sb, "text", name, value, cssclass, disabled, maxlength);
    }
    /// <summary>
    ///   HTML text input要素タグをStringBuilderに書き込む。全属性指定版。
    /// </summary>
    public static StringBuilder HtmlInput(StringBuilder sb, string type, string name, object value, string cssclass, bool disabled, int maxlength) {
        if(name == null)
            name = "";
        sb.Append("<input type=\"");
        sb.Append(type);
        sb.Append("\" name=\"");
        sb.Append(QE(name));
        sb.Append("\"");
        if(cssclass != null){
            sb.Append(" class=\"");
            sb.Append(cssclass);
            sb.Append("\"");
        }
        if(maxlength > 0){
            sb.Append(" maxlength=\"");
            sb.Append(maxlength);
            sb.Append("\"");
        }
        if(value != null){
            sb.Append(" value=\"");
            sb.Append(QE(value.ToString()));
            sb.Append("\"");
        }
        if(disabled)
            sb.Append(" disabled=\"disabled\"");
        sb.Append(" />");
        return sb;
    }

    /// <summary>
    ///   HTML password input要素タグを生成する。
    /// </summary>
    public static string HtmlPassword(string name, string value) {
        StringBuilder sb = new StringBuilder();
        HtmlPassword(sb, name, value, null, 255);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML password input要素タグを生成する。CSSクラス指定版。
    /// </summary>
    public static string HtmlPassword(string name, string value, string cssclass) {
        StringBuilder sb = new StringBuilder();
        HtmlPassword(sb, name, value, cssclass, 255);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML password input要素タグを生成する。CSSクラスと最大文字数指定版。
    /// </summary>
    public static string HtmlPassword(string name, string value, string cssclass, int maxlength) {
        StringBuilder sb = new StringBuilder();
        HtmlPassword(sb, name, value, cssclass, maxlength);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML password input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlPassword(StringBuilder sb, string name, string value) {
        return HtmlPassword(sb, name, value, null, 255);
    }
    /// <summary>
    ///   HTML password input要素タグをStringBuilderに書き込む。CSSクラス指定版。
    /// </summary>
    public static StringBuilder HtmlPassword(StringBuilder sb, string name, string value, string cssclass) {
        return HtmlPassword(sb, name, value, cssclass, 255);
    }
    /// <summary>
    ///   HTML password input要素タグをStringBuilderに書き込む。CSSクラスと最大文字数指定版。
    /// </summary>
    public static StringBuilder HtmlPassword(StringBuilder sb, string name, string value, string cssclass, int maxlength) {
        return HtmlInput(sb, "password", name, value, cssclass, false, maxlength);
    }

    /// <summary>
    ///   HTML submit button input要素タグを生成する。
    /// </summary>
    public static string HtmlSubmit(string name, string text) {
        StringBuilder sb = new StringBuilder();
        HtmlSubmit(sb, name, text, null, null);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML submit button input要素タグを生成する。CSSクラス指定版。
    /// </summary>
    public static string HtmlSubmit(string name, string text, string cssclass) {
        StringBuilder sb = new StringBuilder();
        HtmlSubmit(sb, name, text, cssclass, null);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML submit button input要素タグを生成する。CSSクラスとOnClick指定版。
    /// </summary>
    public static string HtmlSubmit(string name, string text, string cssclass, string onclick) {
        StringBuilder sb = new StringBuilder();
        HtmlSubmit(sb, name, text, cssclass, onclick);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML submit button input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlSubmit(StringBuilder sb, string name, string text) {
        return HtmlSubmit(sb, name, text, null, null);
    }
    /// <summary>
    ///   HTML submit button input要素タグをStringBuilderに書き込む。CSSクラス指定版。
    /// </summary>
    public static StringBuilder HtmlSubmit(StringBuilder sb, string name, string text, string cssclass) {
        return HtmlSubmit(sb, name, text, cssclass, null);
    }
    /// <summary>
    ///   HTML submit button input要素タグをStringBuilderに書き込む。CSSクラスとOnClick指定版。
    /// </summary>
    public static StringBuilder HtmlSubmit(StringBuilder sb, string name, string text, string cssclass, string onclick) {
        return HtmlButton(sb, "submit", name, text, cssclass, onclick);
    }

    /// <summary>
    ///   HTML button input要素タグを生成する。
    /// </summary>
    public static string HtmlButton(string type, string name, string text, string cssclass, string onclick) {
        StringBuilder sb = new StringBuilder();
        HtmlButton(sb, type, name, text, cssclass, onclick);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML button input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlButton(StringBuilder sb, string type, string name, string text, string cssclass, string onclick) {
        if(name == null)
            name = "";
        sb.Append("<input type=\"");
        sb.Append(type);
        sb.Append("\"");
        if(cssclass != null){
            sb.Append(" class=\"");
            sb.Append(cssclass);
            sb.Append("\"");
        }
        sb.Append("name=\"");
        sb.Append(QE(name));
        sb.Append("\" value=\"");
        sb.Append(QE(text));
        sb.Append("\"");
        if(onclick != null){
            sb.Append(" onclick=\"");
            sb.Append(onclick);
            sb.Append("\"");
        }
        sb.Append(" />");
        return sb;
    }

    /// <summary>
    ///   HTML hidden input要素タグを生成する。
    /// </summary>
    public static string HtmlHidden(string name, string value) {
        StringBuilder sb = new StringBuilder();
        HtmlHidden(sb, name, value);
        return sb.ToString();
    }
    /// <summary>
    ///   HTML hidden input要素タグをStringBuilderに書き込む。
    /// </summary>
    public static StringBuilder HtmlHidden(StringBuilder sb, string name, string value) {
        if(name == null)
            name = "";
        sb.Append("<input type=\"hidden\" name=\"");
        sb.Append(QE(name));
        sb.Append("\" value=\"");
        sb.Append(QE(value));
        sb.Append("\" />");
        return sb;
    }


}

} // End of namespace
