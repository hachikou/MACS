/*! @file Translatable.cs
 * @brief 翻訳機能を有するクラスインタフェース
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

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
