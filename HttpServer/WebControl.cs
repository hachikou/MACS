/// WebControl: HTMLのForm要素コード作成に便利なユーティリティクラス.
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
///   HTMLのForm要素コード作成に便利なユーティリティクラス。
///   Button, TextBox などのクラスの共通部分。
/// </summary>
public abstract class WebControl {

    /// <summary>
    ///   ウィジェットの持つ値
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ToString()を実装するオブジェクトでなければいけない。
    ///   </para>
    /// </remarks>
    public object Value;

    /// <summary>
    ///   ウィジェットの名前
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一般的には、アプリケーション内のウィジェット変数名と同じになる。
    ///   </para>
    /// </remarks>
    public string Name;

    /// <summary>
    ///   ウィジェットのID名
    /// </summary>
    public string ID;

    /// <summary>
    ///   CSSのクラス名
    /// </summary>
    public string CssClass;

    /// <summary>
    ///   CSSクラスを追加する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     すでにそのクラスが設定されている場合には何もしない
    ///   </para>
    /// </remarks>
    public void AddCssClass(string classname) {
        if(String.IsNullOrEmpty(CssClass)) {
            CssClass = classname;
            return;
        }
        foreach(string i in CssClass.Split(" ".ToCharArray())) {
            if(i == classname)
                return;
        }
        CssClass = CssClass+" "+classname;
    }

    /// <summary>
    ///   CSSクラスを削除する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     そのクラスが設定されていない場合には何もしない
    ///   </para>
    /// </remarks>
    public void RemoveCssClass(string classname) {
        if(String.IsNullOrEmpty(CssClass)) {
            return;
        }
        StringBuilder sb = new StringBuilder();
        foreach(string i in CssClass.Split(" ".ToCharArray())) {
            if(i != classname) {
                if(sb.Length > 0)
                    sb.Append(' ');
                sb.Append(i);
            }
        }
        CssClass = sb.ToString();
    }

    /// <summary>
    ///   入力禁止属性
    /// </summary>
    public bool Disabled = false;

    /// <summary>
    ///   入力許可属性
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Disabledの逆を示すプロパティ。
    ///   </para>
    /// </remarks>
    public bool Enabled {
        get { return !Disabled; }
        set { Disabled = !value; }
    }

    /// <summary>
    ///   クリック時に呼び出されるJavaScript
    /// </summary>
    public string OnClick;

    /// <summary>
    ///   ウィジェットの表示幅
    /// </summary>
    public object Width;

    /// <summary>
    ///   ウィジェットの表示属性
    /// </summary>
    public bool Visible = true;

    /// <summary>
    ///   インラインエラー表示をするかどうか
    /// </summary>
    public bool InLineError = false;

    /// <summary>
    ///   エラー時のメッセージ
    /// </summary>
    public string ErrorMessage;

    /// <summary>
    ///   要素名、IDを指定したコンストラクタ
    /// </summary>
    public WebControl(string name, string id) {
        Name = name;
        ID = id;
    }

    /// <summary>
    ///   要素名だけを指定したコンストラクタ
    /// </summary>
    public WebControl(string name) {
        Name = name;
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public WebControl() {}

    /// <summary>
    ///   レンダリングする
    /// </summary>
    public override string ToString() {
        return Render(new StringBuilder()).ToString();
    }

    /// <summary>
    ///   レンダリングする（StringBuilder版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     派生クラスで実装する事。
    ///   </para>
    /// </remarks>
    public abstract StringBuilder Render(StringBuilder sb);

    /// <summary>
    ///   Formの値をValueに取り込む
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     テンプレートHTML内で Name= を指定している場合、正しい値を読み取る
    ///     ことができません。（Fetchをする時点ではテンプレートをパースしてい
    ///     ないため。）
    ///   </para>
    /// </remarks>
    public virtual void Fetch(HttpPage page, object defaultValue=null) {
        string val = page.Fetch(Name, null);
        if(val == null)
            Value = defaultValue;
        else if(defaultValue is int)
            Value = StringUtil.ToInt(val);
        else if(defaultValue is double)
            Value = StringUtil.ToDouble(val);
        else if(defaultValue is bool)
            Value = StringUtil.ToBool(val);
        else
            Value = val;
    }


    /// <summary>
    ///   タグ内の共通オプションをレンダリングする
    /// </summary>
    /// <remarks>
    ///   <para>
    //    Valueはレンダリングされない事に注意。
    ///   </para>
    /// </remarks>
    protected void CommonOptions(StringBuilder sb) {
        if(Name != null){
            sb.Append(" name=\"");
            sb.Append(HE(Name));
            sb.Append("\"");
        }
        if(ID != null){
            sb.Append(" id=\"");
            sb.Append(HE(ID));
            sb.Append("\"");
        }
        if(CssClass != null){
            sb.Append(" class=\"");
            sb.Append(CssClass);
            sb.Append("\"");
        }
        if(Disabled)
            sb.Append(" disabled=\"disabled\"");
        if(Width != null){
            sb.Append(" style=\"width:");
            sb.Append(Width.ToString());
            if(typeof(int).IsInstanceOfType(Width))
                sb.Append("px");
            sb.Append(";\"");
        }
        if(OnClick != null){
            sb.Append(" onclick=\"");
            sb.Append(OnClick);
            sb.Append("\"");
        }
    }

    /// <summary>
    ///   エラーメッセージをレンダリングする
    /// </summary>
    /// <remarks>
    ///   <para>
    //    InLineErrorがfalseの時や、ErrorMessageがセットされていない時はなにもしない。
    ///   </para>
    /// </remarks>
    protected void RenderInLineError(StringBuilder sb) {
        if(!InLineError || String.IsNullOrEmpty(ErrorMessage))
            return;
        sb.Append("<span class=\"error\">");
        sb.Append(HE(ErrorMessage));
        sb.Append("</span>");
    }

    /// <summary>
    ///   クォートをエスケープする。
    ///   派生クラスでよく使うので便宜上用意している。
    /// </summary>
    protected static string QE(string x) {
        return HtmlTool.QE(x);
    }

    /// <summary>
    ///   HTMLエスケープする。
    ///   派生クラスでよく使うので便宜上用意している。
    /// </summary>
    protected static string HE(string x) {
        return HtmlTool.HE(x);
    }

    /// <summary>
    ///   URLエスケープする。
    ///   派生クラスでよく使うので便宜上用意している。
    /// </summary>
    protected static string UE(string x) {
        return HtmlTool.UE(x);
    }

}

} // End of namespace
