/*! @file PasswordBox.cs
 * @brief input password要素
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
///   input password要素
/// </summary>
/// <remarks>
///   <para>
///     テキストモードが"password"である以外はTextBoxクラスと同じ。
///   </para>
/// </remarks>
public class PasswordBox : TextBox {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public PasswordBox(string name, string id, int maxlength) : base(name, id, maxlength) {
        TextMode = "password";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public PasswordBox(string name, int maxlength) : base(name, maxlength) {
        TextMode = "password";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public PasswordBox(string name, string id) : base(name, id) {
        TextMode = "password";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public PasswordBox(string name) : base(name) {
        TextMode = "password";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public PasswordBox() : base() {
        TextMode = "password";
    }

}

} // End of namespace
