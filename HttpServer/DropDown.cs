/// DropDown: プルダウンメニュー要素.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Web;
using System.Text;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   プルダウンメニュー要素
/// </summary>
/// <remarks>
///   <para>
///     Listプロパティを使うと、値の選択肢を簡単に設定できます。
///
///     テンプレート中では、{名前 List="値1:表示名1,値2:表示名2,..."} のように、
///     選択肢を設定できます。
///   </para>
/// </remarks>
public class DropDown : TranslatableWebControl {

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
            return "";
        }
    }
    
    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;


    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public DropDown(string name, string id, object value) : base(name, id) {
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public DropDown(string name, string value) : base(name) {
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public DropDown(string name) : base(name) {}

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public DropDown() : base() {}


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        sb.Append("<select");
        CommonOptions(sb);
        if(OnChange != null){
            sb.Append(" onchange=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        sb.Append(">");
        if(Values != null){
            string v = (Value == null)?"":Value.ToString();
            if(Labels == null)
                Labels = Values;
            for(int i = 0; i < Values.Length; i++){
                sb.Append("<option value=\"");
                sb.Append(HE(Values[i]));
                sb.Append("\"");
                if(Values[i] == v)
                    sb.Append(" selected=\"selected\"");
                sb.Append(">");
                sb.Append(HE(_(Labels[i])));
                sb.Append("</option>");
            }
        }
        sb.Append("</select>");
        RenderInLineError(sb);
        return sb;
    }

}

} // End of namespace
