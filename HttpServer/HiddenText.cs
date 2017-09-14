/*! @file HiddenText.cs
 * @brief input hidden要素
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

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

} // namespace SCS

} // End of namespace
