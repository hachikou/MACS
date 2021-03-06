/// DebugTool: デバッグ用ユーティリティ.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace MACS {

/// <summary>
///   デバッグ作業用ユーティリティ
/// </summary>
public static class DebugTool {

    /// <summary>
    ///   例外のスタックトレース文字列を得る。
    /// </summary>
    public static string GetStackTrace(Exception ex) {
        StringBuilder sb = new StringBuilder();
        sb.Append(ex.Message);
        sb.Append("\n");
        sb.Append("Exception class: ");
        int count = 0;
        Type t = ex.GetType();
        while((t != null) && (count < 4)) {
            if(count > 0)
                sb.Append(" - ");
            sb.Append(t.FullName);
            t = t.BaseType;
            count++;
        }
        sb.Append('\n');
        sb.Append("StackTrace:\n");
        sb.Append(ex.StackTrace);
        return sb.ToString();
    }
}

} // End of namespace
