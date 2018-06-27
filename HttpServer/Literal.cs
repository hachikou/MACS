/// Literal: 文字列表示要素.
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
///   文字列表示要素
/// </summary>
/// <remarks>
///   <para>
///     CSSクラスが指定されていなければ、Valueそのものがレンダリングされる。
///
///     CSSクラスが指定されている場合は、spanタグで囲まれたValueの内容がレンダリングされる。
///   </para>
/// </remarks>
public class Literal : WebControl {

    /// <summary>
    ///   表示文字列。Valueと同じだが、stringにキャストされている
    /// </summary>
    public string Text {
        get { return (string)Value; }
        set { Value = (object)value; }
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public Literal(string text) : base() {
        Text = text;
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public Literal() : base() {}


    /// <summary>
    ///   レンダリング
    /// </summary>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        if(CssClass != null){
            sb.Append("<span class=\"");
            sb.Append(CssClass);
            sb.Append("\">");
        }
        if(Value != null)
            sb.Append(Value.ToString());
        if(CssClass != null){
            sb.Append("</span>");
        }
        return sb;
    }

}

} // End of namespace
