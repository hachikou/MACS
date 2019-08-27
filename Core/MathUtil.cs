/// MathUtil: 数値演算ユーティリティクラス.
///
/// Copyright (C) 2019 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Collections.Generic;

namespace MACS {

/// <summary>
///   数値演算ユーティリティクラス
/// </summary>
public static class MathUtil {

    /// <summary>
    ///   安全にdoubleをintに変換する（切り捨て）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     小数点下6桁で四捨五入してから小数点以下を切り捨てる。
    ///     これにより、2進数丸め誤差で生じる、0.99999999998 のような数を 1と
    ///     して取り扱うことができる。
    ///     例：(int)(110/1.1)は100ではなく99になるが、ToInt(110/1.1)は100になる。
    ///     小数点下6桁にしたのは、32bit符号付整数値の上限に近い、1100000000/1.1
    ///     が正しく計算できるように考慮したものです。
    ///   </para>
    /// </remarks>
    public static int ToInt(this double x) {
        if(x >= 0)
            return (int)(x+0.0000005);
        else
            return (int)(x-0.0000005);
    }

    /// <summary>
    ///   安全にdoubleをintに変換する（切り捨て）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     四捨五入を行なう小数点下の桁数を指定したToInt()
    ///   </para>
    /// </remarks>
    public static int ToInt(this double x, int digits) {
        return (int)Math.Round(x, digits);
    }

}
} // End of namespace
