/// TranslatableWebControl: 翻訳機能つきWebControl.
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
///   翻訳機能付きWebControl
/// </summary>
public abstract class TranslatableWebControl : WebControl {

    /// <summary>
    ///   要素名、IDを指定したコンストラクタ
    /// </summary>
    public TranslatableWebControl(string name, string id) : base(name,id) {}

    /// <summary>
    ///   要素名、IDを指定したコンストラクタ（翻訳機指定付き）
    /// </summary>
    public TranslatableWebControl(string name, string id, Translatable tr) : base(name,id) {
        Translator = tr;
    }

    /// <summary>
    ///   要素名だけを指定したコンストラクタ
    /// </summary>
    public TranslatableWebControl(string name) : base(name) {}

    /// <summary>
    ///   要素名だけを指定したコンストラクタ（翻訳機指定付き）
    /// </summary>
    public TranslatableWebControl(string name, Translatable tr) : base(name) {
        Translator = tr;
    }

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public TranslatableWebControl() : base() {}

    /// <summary>
    ///   翻訳機指定コンストラクタ
    /// </summary>
    public TranslatableWebControl(Translatable tr) : base() {
        Translator = tr;
    }

    /// <summary>
    ///   翻訳機
    /// </summary>
    public Translatable Translator;

    /// <summary>
    ///   翻訳をする
    /// </summary>
    protected string _(string txt) {
        if(Translator == null)
            return txt;
        return Translator._(txt);
    }

}

} // End of namespace
