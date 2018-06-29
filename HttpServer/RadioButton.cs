/// RadioButton: input radio要素.
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
///   input radio要素
/// </summary>
/// <remarks>
///   <para>
///     RadioButtonは例外的にウィジェット変数名と値を読み出すフォーム要素名が異なる。
///
///     フォームの値を読み出す時には、GroupNameプロパティで指定した名前で読み出すこと。
///   </para>
/// </remarks>
public class RadioButton : TranslatableWebControl {

    /// <summary>
    ///   選択されているかどうか
    /// </summary>
    public bool Checked = false;

    /// <summary>
    ///   選択されているかどうか。Checkedと全く同じ。
    /// </summary>
    public bool Selected {
        get { return Checked; }
        set { Checked = value; }
    }

    /// <summary>
    ///   表示文字列
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Valueは選択されている時にアプリケーションに送られる文字列、Textは表示上の文字列。
    ///   </para>
    /// </remarks>
    public string Text;

    /// <summary>
    ///   ボタングループ名。Nameと全く同じ。
    /// </summary>
    public string GroupName {
        get { return Name; }
        set { Name = value; }
    }


    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioButton(string name, string id, string text, object value) : base(name, id) {
        Text = text;
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioButton(string name, string text, object value) : base(name) {
        Text = text;
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioButton(string name, string text) : base(name) {
        Text = text;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public RadioButton(string name) : base(name) {}

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public RadioButton() : base() {}


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        if(ID == null){
            //ID = "checkbox_";
            //if(Name != null)
            //    ID += Name;
            if(Name == null)
                ID = "checkbox_";
            else
                ID = Name+"_";
            if(Value == null){
                ID += m_seq.ToString();
                m_seq++;
            }else{
                ID += UE(Value.ToString());
            }
        }
        sb.Append("<input type=\"radio\"");
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

    private static int m_seq = 0;

}

} // End of namespace
