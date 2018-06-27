/// Translatable: 翻訳機能を有するクラスインタフェース.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

namespace MACS {

/// <summary>
///   翻訳機能を有するクラスインタフェース
/// </summary>
public interface Translatable {

    /// <summary>
    ///   翻訳する
    /// </summary>
    string _(string msg);

}

} // End of namespace
