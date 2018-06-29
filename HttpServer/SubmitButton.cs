/// SubmitButton: input submit要素.
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
///   input submit要素
/// </summary>
/// <remarks>
///   <para>
///     ボタンタイプが"submit"である以外はButtonクラスと同じ。
///   </para>
/// </remarks>
public class SubmitButton : Button {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public SubmitButton(string name, string id, string text) : base(name, id, text) {
        Type = "submit";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public SubmitButton(string name, string text) : base(name, text) {
        Type = "submit";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public SubmitButton(string name) : base(name) {
        Type = "submit";
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public SubmitButton() : base() {
        Type = "submit";
    }

}

} // End of namespace
