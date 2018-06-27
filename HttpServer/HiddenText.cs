/// HiddenText: input hidden要素.
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
///   input hidden要素
/// </summary>
/// <remarks>
///   <para>
///     テキストモードが"hidden"である以外はTextBoxクラスと同じ。
///   </para>
/// </remarks>
public class HiddenText : TextBox {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HiddenText(string name, string id, int maxlength) : base(name, id, maxlength) {
        TextMode = "hidden";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HiddenText(string name, int maxlength) : base(name, maxlength) {
        TextMode = "hidden";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HiddenText(string name, string id) : base(name, id) {
        TextMode = "hidden";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HiddenText(string name) : base(name) {
        TextMode = "hidden";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public HiddenText() : base() {
        TextMode = "hidden";
    }

}

} // End of namespace
