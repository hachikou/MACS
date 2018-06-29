/// Button: input button要素.
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
///   input button要素
/// </summary>
public class Button : TranslatableWebControl {

    /// <summary>
    ///   ボタン表示文字列
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字列のみを格納できる以外は、Valueと同じ。
    ///   </para>
    /// </remarks>
    public string Text {
        get { return (Value == null)?"":Value.ToString(); }
        set { Value = value; }
    }

    /// <summary>
    ///   ボタンのタイプ
    /// </summary>
    public string Type = "button";

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public Button(string name, string id, string text) : base(name, id) {
        Text = text;
        CssClass = "button";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public Button(string name, string text) : base(name) {
        Text = text;
        CssClass = "button";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public Button(string name) : base(name) {
        CssClass = "button";
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public Button() : base() {
        CssClass = "button";
    }


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        sb.Append("<input type=\"");
        sb.Append(Type);
        sb.Append("\" value=\"");
        sb.Append(HE(_(Text)));
        sb.Append("\"");
        CommonOptions(sb);
        sb.Append(" />");
        return sb;
    }

}

} // End of namespace
