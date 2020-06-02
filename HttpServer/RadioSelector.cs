/// RadioSelector: Enumをラジオボタンで選択する要素.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

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
    ///   選択された値の表示名
    /// </summary>
    public string SelectedText {
        get {
            string vv;
            if((Text == null) || !Text.TryGetValue(Selected, out vv)) {
                if(ShowEnumName)
                    vv = _(typeof(T).Name+"."+Selected.ToString());
                else
                    vv = _(Selected.ToString());
            }
            return vv;
        }
    }

    /// <summary>
    ///   ラジオボタン Enum名表示
    ///   <remarks>
    ///     true:Enum名表示 false:Enum名非表示
    ///   </remarks>
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

} // End of class RadioSelector<T>

public class RadioSelector : TranslatableWebControl {

    /// <summary>
    ///   値の配列
    /// </summary>
    public string[] Values;

    /// <summary>
    ///   表示文字列の配列
    /// </summary>
    public string[] Labels;

    /// <summary>
    ///   値がすでに存在するかどうか
    /// </summary>
    public bool ContainsValue(string txt) {
        if(Values == null)
            return false;
        foreach(string x in Values) {
            if(x == txt)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   表示文字列がすでに存在するかどうか
    /// </summary>
    public bool ContainsLabel(string txt) {
        if(Labels == null)
            return false;
        foreach(string x in Labels) {
            if(x == txt)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   表示文字列、値を追加する
    /// </summary>
    public void AddItem(string label, string value) {
        if((Labels == null) || (Values == null)) {
            Labels = new string[1]{label};
            Values = new string[1]{value};
            return;
        }
        string[] xlabels = new string[Labels.Length+1];
        string[] xvalues = new string[Values.Length+1];
        for(int i = 0; i < Labels.Length; i++)
            xlabels[i] = Labels[i];
        for(int i = 0; i < Values.Length; i++)
            xvalues[i] = Values[i];
        xlabels[xlabels.Length-1] = label;
        xvalues[xvalues.Length-1] = value;
        Labels = xlabels;
        Values = xvalues;
    }

    /// <summary>
    ///   全ての値と表示文字列のペアを "値:表示文字列,値:表示文字列,..."という文字列にまとめたもの
    /// </summary>
    public string List {
        get {
            StringBuilder sb = new StringBuilder();
            if(Values == null)
                return "";
            if(Labels == null)
                Labels = Values;
            for(int i = 0; i < Values.Length; i++){
                if(sb.Length > 0)
                    sb.Append(',');
                sb.Append(Values[i]);
                sb.Append(':');
                sb.Append(Labels[i]);
            }
            return sb.ToString();
        }
        set {
            string[] list = value.Split(",".ToCharArray());
            Values = new string[list.Length];
            Labels = new string[list.Length];
            for(int i = 0; i < list.Length; i++){
                string[] lv = list[i].Split(":".ToCharArray(),2);
                if(lv.Length == 2){
                    Values[i] = lv[0];
                    Labels[i] = lv[1];
                }else{
                    Values[i] = list[i];
                    Labels[i] = list[i];
                }
            }
        }
    }

    /// <summary>
    ///   選択されている項目の番号
    /// </summary>
    public int SelectedIndex {
        get {
            if(Values == null)
                return -1;
            string v = (Value == null)?"":Value.ToString();
            for(int i = 0; i < Values.Length; i++){
                if(Values[i] == v)
                    return i;
            }
            return 0;
        }
        set {
            if(Values == null)
                return;
            if(value < 0)
                Value = Values[0];
            else if(value < Values.Length)
                Value = Values[value];
            else
                Value = Values[Values.Length-1];
        }
    }

    /// <summary>
    ///   選択された値
    /// </summary>
    public string Selected {
        get { return (Value == null)?"":Value.ToString(); }
        set { Value = value; }
    }

    /// <summary>
    ///   選択されている項目の表示文字列
    /// </summary>
    public string Text {
        get {
            string v = (Value == null)?"":Value.ToString();
            for(int i = 0; i < Values.Length; i++){
                if(Values[i] == v)
                    return Labels[i];
            }
            return v;
        }
    }
    
    /// <summary>
    ///   縦に並べるかどうか
    /// </summary>
    public bool Vertical = false;


    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioSelector(string name, string id, object value) : base(name, id) {
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioSelector(string name, string value) : base(name) {
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioSelector(string name) : base(name) {}

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public RadioSelector() : base() {}


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
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
        for(int i = 0; i < Labels.Length; i++) {
            string label = Labels[i];
            string val = (i < Values.Length)?Values[i]:Values[Values.Length-1];
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
            sb.Append(HE(label));
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
} // End of class RadioSelector

} // End of namespace
