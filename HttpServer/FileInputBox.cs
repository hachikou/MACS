/// FileInputBox: input file要素.
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
///   input file要素
/// </summary>
/// <remarks>
///   <para>
///     テキストモードが"file"である以外はTextBoxクラスと同じ。
///   </para>
/// </remarks>
public class FileInputBox : TextBox {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public FileInputBox(string name, string id, int maxlength) : base(name, id, maxlength) {
        TextMode = "file";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public FileInputBox(string name, int maxlength) : base(name, maxlength) {
        TextMode = "file";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public FileInputBox(string name, string id) : base(name, id) {
        TextMode = "file";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public FileInputBox(string name) : base(name) {
        TextMode = "file";
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public FileInputBox() : base() {
        TextMode = "file";
    }

}

} // End of namespace
