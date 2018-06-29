/// TemplateFlag: テンプレート内でフラグに使うダミー要素.
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
///   TemplateFlag要素
/// </summary>
/// <remarks>
///   <para>
///     本クラスはHTMLのフォーム要素ではありません。
///
///     テンプレートの{!if 変数名}で簡単に扱うことができるように用意されました。
///   </para>
/// </remarks>
public class TemplateFlag : WebControl {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public TemplateFlag() : base() {}

    /// <summary>
    ///   レンダリング
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Disabledに応じて"true"または"false"を出力する。
    ///   </para>
    /// </remarks>
    public override StringBuilder Render(StringBuilder sb) {
        if(!Visible)
            return sb;
        if(Disabled)
            sb.Append("false");
        else
            sb.Append("true");
        return sb;
    }

}

} // End of namespace
