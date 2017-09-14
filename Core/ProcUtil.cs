/*! @file ProcUtil.cs
 * @brief 外部コマンド実行ユーティリティ
 * $Id: $
 *
 * Copyright (C) 2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MACS {

/// <summary>
///   外部コマンド実行ユーティリティ
/// </summary>
public static class ProcUtil {

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="timeout">最大実行待ち時間（ミリ秒）</param>
    /// <param name="args">コマンド引数（nullの場合コマンド引数無しで実行）</param>
    /// <param name="createNoWindow">trueでコマンドプロンプトを開かない</param>
    /// <param name="killOnTimeout">timeout時間待ってもプロセスが終了しないときにプロセスをkillするかどうか</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, int timeout, string[] args, bool createNoWindow=true, bool killOnTimeout=true) {
        StringBuilder argstr = new StringBuilder();
        if(args != null) {
            bool first = true;
            foreach(string a in args) {
                if(first)
                    first = false;
                else
                    argstr.Append(" ");
                argstr.Append(a);
            }
        }
        int ret;
        using(Process proc = new Process()) {
            proc.StartInfo = new ProcessStartInfo(cmd, argstr.ToString());
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.RedirectStandardOutput = false;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = createNoWindow;
            proc.Start();
            try {
                if(!proc.WaitForExit(timeout)) {
                    if(killOnTimeout) {
                        proc.Kill();
                        proc.WaitForExit(3000);
                    }
                }
                ret = proc.ExitCode;
            } catch(Exception) {
                ret = -1;
            }
            proc.Close();
        }
        return ret;
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="args">コマンド引数（nullの場合コマンド引数無しで実行）</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできない。スペースはコマンド引数の
    ///     区切りとみなされる。<br/>
    ///     コマンド実行タイムアウトはデフォルトの10000ミリ秒。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, string[] args) {
        return Exec(cmd, DefaultTimeout, args);
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="timeout">最大実行待ち時間（ミリ秒）</param>
    /// <returns>コマンドの終了コード</returns>
    public static int Exec(string cmd, int timeout) {
        return Exec(cmd, timeout, new string[0]);
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド実行タイムアウトはデフォルトの10000ミリ秒。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd) {
        return Exec(cmd, DefaultTimeout, new string[0]);
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="timeout">最大実行待ち時間（ミリ秒）</param>
    /// <param name="arg1">コマンド引数</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, int timeout, string arg1) {
        return Exec(cmd, timeout, new string[]{arg1});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="arg1">コマンド引数</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。<br/>
    ///     コマンド実行タイムアウトはデフォルトの10000ミリ秒。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, string arg1) {
        return Exec(cmd, DefaultTimeout, new string[]{arg1});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="timeout">最大実行待ち時間（ミリ秒）</param>
    /// <param name="arg1">コマンド引数1</param>
    /// <param name="arg2">コマンド引数2</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, int timeout, string arg1, string arg2) {
        return Exec(cmd, timeout, new string[]{arg1,arg2});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="arg1">コマンド引数1</param>
    /// <param name="arg2">コマンド引数2</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。<br/>
    ///     コマンド実行タイムアウトはデフォルトの10000ミリ秒。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, string arg1, string arg2) {
        return Exec(cmd, DefaultTimeout, new string[]{arg1, arg2});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="timeout">最大実行待ち時間（ミリ秒）</param>
    /// <param name="arg1">コマンド引数1</param>
    /// <param name="arg2">コマンド引数2</param>
    /// <param name="arg3">コマンド引数3</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, int timeout, string arg1, string arg2, string arg3) {
        return Exec(cmd, timeout, new string[]{arg1,arg2,arg3});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="arg1">コマンド引数1</param>
    /// <param name="arg2">コマンド引数2</param>
    /// <param name="arg3">コマンド引数3</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。<br/>
    ///     コマンド実行タイムアウトはデフォルトの10000ミリ秒。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, string arg1, string arg2, string arg3) {
        return Exec(cmd, DefaultTimeout, new string[]{arg1, arg2, arg3});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="timeout">最大実行待ち時間（ミリ秒）</param>
    /// <param name="arg1">コマンド引数1</param>
    /// <param name="arg2">コマンド引数2</param>
    /// <param name="arg3">コマンド引数3</param>
    /// <param name="arg4">コマンド引数4</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, int timeout, string arg1, string arg2, string arg3, string arg4) {
        return Exec(cmd, timeout, new string[]{arg1,arg2,arg3,arg4});
    }

    /// <summary>
    ///   外部コマンドを実行する
    /// </summary>
    /// <param name="cmd">実行コマンド名（パス名付きも可）</param>
    /// <param name="arg1">コマンド引数1</param>
    /// <param name="arg2">コマンド引数2</param>
    /// <param name="arg3">コマンド引数3</param>
    /// <param name="arg4">コマンド引数4</param>
    /// <returns>コマンドの終了コード</returns>
    /// <remarks>
    ///   <para>
    ///     コマンド引数にスペースを含む事はできません。スペースはコマンド引数の
    ///     区切りとみなされます。<br/>
    ///     コマンド実行タイムアウトはデフォルトの10000ミリ秒。
    ///   </para>
    /// </remarks>
    public static int Exec(string cmd, string arg1, string arg2, string arg3, string arg4) {
        return Exec(cmd, DefaultTimeout, new string[]{arg1, arg2, arg3, arg4});
    }


    /// <summary>
    ///   コマンド名と引数文字列を分離する
    /// </summary>
    public static void SplitCommand(string str, out string cmd, out string[] args) {
        if(str == null) {
            cmd = "";
            args = new string[0];
            return;
        }
        str = str.Trim();
        if(str == "") {
            cmd = "";
            args = new string[0];
            return;
        }
        Regex pat_spaces = new Regex(@"\s+");
        int ptr = 0;
        while((ptr = str.IndexOf(' ',ptr)) >= 0) {
            cmd = str.Substring(0,ptr);
            if(File.Exists(cmd)) {
                args = pat_spaces.Split(str.Substring(ptr+1));
                return;
            }
            ptr++;
        }
        string[] list = pat_spaces.Split(str);
        cmd = list[0];
        args = new string[list.Length-1];
        for(int i = 0; i < list.Length-1; i++) {
            args[i] = list[i+1];
        }
        return;
    }


    /// <summary>
    ///   外部コマンドを実行し、結果の標準出力文字列を文字列のリストで返す
    /// </summary>
    public static List<string> GetStdout(string cmd, string[] args) {
        StringBuilder argstr = new StringBuilder();
        if(args != null) {
            bool first = true;
            foreach(string a in args) {
                if(first)
                    first = false;
                else
                    argstr.Append(" ");
                argstr.Append(a);
            }
        }
        List<string> ret = new List<string>();
        using(Process proc = new Process()) {
            proc.StartInfo = new ProcessStartInfo(cmd, argstr.ToString());
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.StandardOutputEncoding = defaultEnc;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            while(!proc.StandardOutput.EndOfStream)
                ret.Add(proc.StandardOutput.ReadLine());
            proc.WaitForExit(DefaultTimeout);
            proc.Close();
        }
        return ret;
    }

    public static List<string> GetStdout(string cmd) {
        return GetStdout(cmd, new string[0]);
    }

    public static List<string> GetStdout(string cmd, string arg1) {
        return GetStdout(cmd, new string[]{arg1});
    }

    public static List<string> GetStdout(string cmd, string arg1, string arg2) {
        return GetStdout(cmd, new string[]{arg1, arg2});
    }

    public static List<string> GetStdout(string cmd, string arg1, string arg2, string arg3) {
        return GetStdout(cmd, new string[]{arg1, arg2, arg3});
    }

    public static List<string> GetStdout(string cmd, string arg1, string arg2, string arg3, string arg4) {
        return GetStdout(cmd, new string[]{arg1, arg2, arg3, arg4});
    }


    /// <summary>
    ///   プロセス完了待ち時間のデフォルト（ミリ秒）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     GetStdoutでは、標準出力を読み終えた後の待ち時間である点に注意。
    ///   </para>
    /// </remarks>
    public static int DefaultTimeout = 1000;

    /// <summary>
    ///   デフォルト文字エンコーディング。UTF8。
    /// </summary>
    private static Encoding defaultEnc = Encoding.UTF8;

#if SELFTEST
    public static void Main(string[] args) {
        //List<string> ret = ProcUtil.GetStdout(args[0], new string[]{args[1]});
        //foreach(string i in ret) {
        //    Console.WriteLine(i);
        //}
        Console.WriteLine("ExitCode = {0}", ProcUtil.Exec(args[0], new string[]{args[1]}));
    }
#endif

}

} // End of namespace
