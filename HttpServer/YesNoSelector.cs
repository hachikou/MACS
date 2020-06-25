/// YesNoSelector: Yes/Noをラジオボタンで選択する要素.
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
///   Yes/Noをラジオボタンで選択する要素
/// </summary>
public class YesNoSelector: TranslatableWebControl {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public YesNoSelector(string name, string id, bool value) : base(name, id) {
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public YesNoSelector(string name, bool value) : base(name) {
        Value = value;
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public YesNoSelector(string name) : base(name) {}

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
    ///   選択された値の表示名。
    /// </summary>
    public string SelectedText {
        get {
            if(Selected)
                return _(YesString);
            else
                return _(NoString);
        }
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
    ///   yesとnoの選択肢の表示順を逆にする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     通常は、yes, noの順番です。
    ///   </para>
    /// </remarks>
    public bool SwapOrder = false;
    
    /// <summary>
    ///   変更時に呼び出されるJavaScript
    /// </summary>
    public string OnChange;

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

        if(SwapOrder) {
            showRadio(sb, false);
            sb.Append(" ");
            showRadio(sb, true);
        } else {
            showRadio(sb, true);
            sb.Append(" ");
            showRadio(sb, false);
        }

        sb.Append("</div>");
        RenderInLineError(sb);
        return sb;
    }

    public override void Fetch(HttpPage page, object defaultValue=null) {
        if(defaultValue == null)
            defaultValue = "";
        Value = StringUtil.ToBool(page.Fetch(Name,defaultValue.ToString()));
    }

    private void showRadio(StringBuilder sb, bool yesno) {
        string id = Name+"-"+(yesno?"true":"false");
        sb.Append("<span class=\"radio\" style=\"white-space:nowrap\">");
        sb.Append("<input type=\"radio\" name=\"");
        sb.Append(Name);
        sb.Append("\" id=\"");
        sb.Append(id);
        sb.Append("\" value=\"");
        sb.Append(yesno?"yes":"no");
        sb.Append("\"");
        if(Selected == yesno)
            sb.Append(" checked=\"checked\"");
        if(!String.IsNullOrEmpty(OnClick)) {
            sb.Append(" onclick=\"");
            sb.Append(OnClick);
            sb.Append("\"");
        }
        if(!String.IsNullOrEmpty(OnChange)) {
            sb.Append(" onclick=\"");
            sb.Append(OnChange);
            sb.Append("\"");
        }
        if(Disabled)
            sb.Append(" disabled=\"disabled\"");
        sb.Append("/><label for=\"");
        sb.Append(id);
        sb.Append("\">");
        sb.Append(HE(_(yesno?YesString:NoString)));
        sb.Append("</label></span>");
    }
}

} // End of namespace
