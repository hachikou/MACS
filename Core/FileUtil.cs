/*! @file FileUtil.cs
 * @brief 安全なファイルのオープンを提供するクラス
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;

namespace MACS {

/// <summary>
///   安全なファイルのオープンを提供するクラス
/// </summary>
public class FileUtil {

    /// <summary>
    ///   ファイルアクセス時のデフォルトエンコーディング。SJIS。
    /// </summary>
    public static Encoding DefaultEncoding = Encoding.GetEncoding("Shift_JIS");

    /// <summary>
    ///   ファイルオープンリトライ待ち時間
    /// </summary>
    private const int RetryWait = 10; // msec

    /// <summary>
    ///   ファイルオープンリトライ回数
    /// </summary>
    private const int MaxRetry = 10*1000/RetryWait;

    /// <summary>
    ///   指定ファイルをテキスト書き込み用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Writeのロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamWriter Writer(string filename){
        return Writer(filename, null, false);
    }

    /// <summary>
    ///   指定ファイルを指定エンコーディングでテキスト書き込み用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Write禁止のロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamWriter Writer(string filename, Encoding enc){
        return Writer(filename, enc, false);
    }

    /// <summary>
    ///   指定ファイルをテキスト追加書き込み用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Writeのロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamWriter AppendWriter(string filename){
        return Writer(filename, null, true);
    }

    /// <summary>
    ///   指定ファイルを指定エンコーディングでテキスト追加書き込み用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Write禁止のロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamWriter AppendWriter(string filename, Encoding enc){
        return Writer(filename, enc, true);
    }

    public static StreamWriter Writer(string filename, Encoding enc, bool appendflag){
        FileMode mode;
        FileAccess access;
        if(appendflag){
            mode = FileMode.Append;
            access = FileAccess.Write;
        } else {
            mode = FileMode.Create;
            access = FileAccess.ReadWrite;
        }
        if(enc == null)
            enc = DefaultEncoding;

        int retry = MaxRetry;
        while(retry > 0){
            try {
                StreamWriter sw = new StreamWriter(new FileStream(filename, mode, access, FileShare.None), enc);
                return sw;
            } catch(DirectoryNotFoundException e){
                //Console.WriteLine(e.Message);
                e.GetType();
                return null;
            } catch(UnauthorizedAccessException e){
                //Console.WriteLine(e.Message);
                e.GetType();
                return null;
            } catch(IOException) {
                // アクセス排他の場合
                // go through.
            }
            retry--;
            Thread.Sleep(RetryWait);
        }
        return null;
    }


    /// <summary>
    ///   指定ファイルを指定エンコーディングでテキスト追加書き込み用にオープンする。
    ///   ファイルのReadアクセスロックをかけない版。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamWriter SharedAppendWriter(string filename, Encoding enc){
        if(enc == null)
            enc = DefaultEncoding;

        int retry = MaxRetry;
        while(retry > 0){
            try {
                StreamWriter sw = new StreamWriter(new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read), enc);
                return sw;
            } catch(DirectoryNotFoundException e){
                //Console.WriteLine(e.Message);
                e.GetType();
                return null;
            } catch(UnauthorizedAccessException e){
                //Console.WriteLine(e.Message);
                e.GetType();
                return null;
            } catch(IOException) {
                // アクセス排他の場合
                // go through.
            }
            retry--;
            Thread.Sleep(RetryWait);
        }
        return null;
    }


    /// <summary>
    ///   指定ファイルをテキスト読み出し用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにWrite禁止のロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamReader Reader(string filename){
        return Reader(filename, null);
    }

    /// <summary>
    ///   指定ファイルを指定エンコーディングでテキスト読み出し用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにWrite禁止のロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static StreamReader Reader(string filename, Encoding enc){
        if(enc == null)
            enc = DefaultEncoding;
        int retry = MaxRetry;
        while(retry > 0){
            try {
                StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read), enc);
                return sr;
            } catch(FileNotFoundException){
                return null;
            } catch(DirectoryNotFoundException){
                return null;
            } catch(UnauthorizedAccessException){
                return null;
            } catch(IOException) {
                // go through.
            }
            retry--;
            Thread.Sleep(RetryWait);
        }
        return null;
    }

    /// <summary>
    ///   指定ファイルをバイナリ書き込み用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Writeのロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static FileStream BinaryWriter(string filename){
        return BinaryWriter(filename, FileMode.Create);
    }

    /// <summary>
    ///   指定ファイルをバイナリ追加書き込み用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Writeのロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static FileStream BinaryAppendWriter(string filename){
        return BinaryWriter(filename, FileMode.Append);
    }

    /// <summary>
    ///   指定ファイルをバイナリ上書き用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Writeのロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static FileStream BinaryOverwriteWriter(string filename){
        return BinaryWriter(filename, FileMode.OpenOrCreate);
    }

    /// <summary>
    ///   指定ファイルをバイナリ上書き用にオープンする。もしファイルが書き
    ///   込み禁止状態ならば、書き込み許可状態に変更してからオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにRead/Writeのロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///     オープン前にファイルが書き込み禁止状態である場合、書き込み許可に
    ///     強制的に変えてからオープンします。このとき、引数iswritableに
    ///     falseがセットされます。
    ///   </para>
    /// </remarks>
    public static FileStream BinaryOverwriteWriter(string filename, out bool iswritable){
        iswritable = true;
        FileInfo fi = new FileInfo(filename);
        if(fi.Exists && fi.IsReadOnly){
            try {
                fi.IsReadOnly = false;
                iswritable = false;
            } catch(Exception e){
                Console.WriteLine("Failed to set writable on "+filename+": "+e.Message);
            }
        }
        return BinaryWriter(filename, FileMode.OpenOrCreate);
    }

    public static FileStream BinaryWriter(string filename, FileMode fmode){
        int retry = MaxRetry;
        FileAccess access;
        if(fmode == FileMode.Append)
            access = FileAccess.Write;
        else
            access = FileAccess.ReadWrite;
        while(retry > 0){
            try {
                FileStream fs = new FileStream(filename, fmode, access, FileShare.None);
                return fs;
            } catch(DirectoryNotFoundException e){
                //Console.WriteLine(e.Message);
                e.GetType();
                return null;
            } catch(UnauthorizedAccessException e){
                //Console.WriteLine(e.Message);
                e.GetType();
                return null;
            } catch(IOException) {
                // アクセス排他の場合
                // go through.
            }
            retry--;
            Thread.Sleep(RetryWait);
        }
        return null;
    }

    /// <summary>
    ///   指定ファイルをバイナリ読み出し用にオープンする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルにWrite禁止のロックをかけてオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static FileStream BinaryReader(string filename) {
        return BinaryReader(filename, false);
    }

    /// <summary>
    ///   指定ファイルをバイナリ読み出し用にオープンする。アクセス排他なし版。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルをReadWrite許可の状態でオープンします。
    ///     指定ファイルに既にロックがかかっているときには、10msec置きに10秒
    ///     までリトライします。
    ///     ファイルオープンに失敗するとnullを返します。
    ///   </para>
    /// </remarks>
    public static FileStream BinarySharedReader(string filename) {
        return BinaryReader(filename, true);
    }

    private static FileStream BinaryReader(string filename, bool shareflag) {
        int retry = MaxRetry;
        while(retry > 0){
            try {
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, shareflag?FileShare.ReadWrite:FileShare.Read);
                return fs;
            } catch(FileNotFoundException){
                return null;
            } catch(DirectoryNotFoundException){
                return null;
            } catch(UnauthorizedAccessException){
                return null;
            } catch(IOException) {
                // go through.
            }
            retry--;
            Thread.Sleep(RetryWait);
        }
        return null;
    }

    /// <summary>
    ///   ファイルをコピーする。コピーの際にはファイルにアクセスロックがかかる。
    /// </summary>
    public static bool Copy(string src, string dst) {
        using(FileStream fsrc = BinaryReader(src)){
            if(fsrc == null)
                return false;
            using(FileStream fdst = BinaryWriter(dst)) {
                if(fdst == null)
                    return false;
                int len;
                byte[] buf = new byte[1024*4];
                while((len = fsrc.Read(buf, 0, buf.Length)) > 0)
                    fdst.Write(buf, 0, len);
                fdst.Close();
                fsrc.Close();
            }
        }
        return true;
    }

    /// <summary>
    ///   ストリームからファイルにコピーする。
    /// </summary>
    public static bool Copy(Stream src, string dst) {
        try {
            using(FileStream fdst = BinaryWriter(dst)){
                if(fdst == null)
                    return false;
                Copy(src, fdst);
                fdst.Close();
            }
            return true;
        } catch (Exception) {
            // just ignore
        }
        return false;
    }

    /// <summary>
    ///   ファイルからストリームにコピーする。
    /// </summary>
    public static bool Copy(string src, Stream dst) {
        try {
            using(FileStream fsrc = BinaryReader(src)){
                if(fsrc == null)
                    return false;
                Copy(fsrc, dst);
                fsrc.Close();
            }
            return true;
        } catch (Exception) {
            // just ignore
        }
        return false;
    }

    /// <summary>
    ///   ストリームからストリームにコピーする。
    /// </summary>
    public static bool Copy(Stream src, Stream dst) {
        try {
            int len;
            byte[] buf = new byte[1024*4];
            while((len = src.Read(buf, 0, buf.Length)) > 0)
                dst.Write(buf, 0, len);
            return true;
        } catch (Exception) {
            // just ignore
        }
        return false;
    }

    /// <summary>
    ///   ファイル内容をBase64文字列に変換する
    /// </summary>
    public static string FileToBase64String(string src) {
        // ファイルをbyte型配列としてすべて読み込む
        try {
            using(FileStream fs = new FileStream(src, FileMode.Open, FileAccess.Read)) {
                byte [] buf = new byte [fs.Length];
                fs.Read(buf, 0, (int) fs.Length);
                fs.Close();

                // Base64で文字列に変換
                return Convert.ToBase64String(buf);
            }
        } catch (Exception) {
            // just ignore
        }
        return "";
    }
    
    /// <summary>
    ///   ファイル内容をBase64文字列に変換する
    /// </summary>
    public static string FileToBase64String(Stream src) {
        // ファイルをbyte型配列としてすべて読み込む
        try {
            byte[] buf = new byte[src.Length];
            src.Read(buf, 0, (int) src.Length);

            // Base64で文字列に変換
            return Convert.ToBase64String(buf);
        } catch (Exception) {
            // just ignore
        }
        return "";
    }
    
    /// <summary>
    ///   最終変更日時が指定日時より昔のファイルを削除する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルパターンの詳細は、Directory.GetFiles(string, string)メソッドのファイルパターンを参照。
    ///     ただし、nullまたは空文字列を指定すると、全ファイル ("*")の意味になる。
    ///   </para>
    /// </remarks>
    /// <param name="path">ディレクトリ名</param>
    /// <param name="pattern">ファイルパターン</param>
    /// <param name="before">最終変更日時がこの日時より古いファイルを削除する</param>
    /// <returns>削除したファイルの数</returns>
    public static int Purge(string path, string pattern, DateTime before) {
        if(string.IsNullOrEmpty(pattern))
            pattern = "*";
        int ndeleted = 0;
        try {
            foreach(string f in Directory.GetFiles(path, pattern)){
                FileInfo fi = new FileInfo(f);
                if(fi.LastWriteTime < before){
                    try {
                        fi.Delete();
                        ndeleted++;
                    } catch(Exception) {
                        // just ignore
                    }
                }
            }
        } catch(Exception) {
            // just ignore
        }
        return ndeleted;
    }


    /// <summary>
    ///   最終変更日時が指定秒数より昔のファイルを削除する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルパターンの詳細は、Directory.GetFiles(string, string)メソッドのファイルパターンを参照。
    ///     ただし、nullまたは空文字列を指定すると、全ファイル ("*")の意味になる。
    ///   </para>
    /// </remarks>
    /// <param name="path">ディレクトリ名</param>
    /// <param name="pattern">ファイルパターン</param>
    /// <param name="seconds">変更日時がこの秒数より古いファイルを削除する</param>
    /// <returns>削除したファイルの数</returns>
    public static int Purge(string path, string pattern, int seconds) {
        return Purge(path, pattern, DateTime.Now.AddSeconds(-seconds));
    }


    /// <summary>
    ///   ファイル名の大文字小文字を無視して、実ファイル名を探索する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定ファイルが存在しない場合には引数をそのまま返す。
    ///   </para>
    /// </remarks>
    /// <param name="filepath">ファイル名（ディレクトリ名を含んでもよい）</param>
    /// <returns>実ファイル名</returns>
    public static string GetRealName(string filepath) {
        if(String.IsNullOrEmpty(filepath))
            return filepath;
        if(filepath == "/")
            return filepath;
        if(File.Exists(filepath) || Directory.Exists(filepath))
            return filepath;
        return GetRealName(GetRealName(Path.GetDirectoryName(filepath)), Path.GetFileName(filepath));
    }

    /// <summary>
    ///   GetRealName(string filepath)の下請け。
    /// </summary>
    private static string GetRealName(string dir, string file) {
        if(String.IsNullOrEmpty(dir)) {
            dir = ".";
        }
        if(Directory.Exists(dir)) {
            string xfile = file.ToLower();
            foreach(string i in Directory.GetFiles(dir)) {
                string ii = Path.GetFileName(i);
                if(xfile == ii.ToLower())
                    return i;
            }
            foreach(string i in Directory.GetDirectories(dir)) {
                string ii = Path.GetFileName(i);
                if(xfile == ii.ToLower())
                    return i;
            }
        }
        return Path.Combine(dir,file);
    }

    /// <summary>
    ///   ファイル名ワイルドカード文字(*,?)を展開してマッチするファイル名の一覧を返す
    /// </summary>
    public static string[] ExpandWildCard(string path, bool ignoreCase=true) {
        //Console.WriteLine("ExpandWildCard: '{0}'", path);
        List<string> res = new List<string>();
        string dir = Path.GetDirectoryName(path);
        string file = Path.GetFileName(path);
        //Console.WriteLine("ExpandWildCard:   dir='{0}', file='{1}'", dir, file);
        if(!String.IsNullOrEmpty(dir) && (dir.Contains("*") || dir.Contains("?"))) {
            foreach(string d in ExpandWildCard(dir)) {
                if (Directory.Exists(d)) {
                    foreach (string x in ExpandWildCard(Path.Combine(d, file))) {
                        res.Add(x);
                    }
                }
            }
            return res.ToArray();
        }
        if(String.IsNullOrEmpty(dir))
            dir = ".";
        if(!Directory.Exists(dir))
            return res.ToArray();
        string pat_str = "^"+Regex.Escape(file).Replace(@"\*",".*").Replace(@"\?",".")+"$";
        Regex pat;
        if(ignoreCase)
            pat = new Regex(pat_str, RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
        else
            pat = new Regex(pat_str);
        try {
            foreach(string d in Directory.EnumerateDirectories(dir))
                if(pat.IsMatch(Path.GetFileName(d)))
                    res.Add(d);
            foreach(string f in Directory.EnumerateFiles(dir))
                if(pat.IsMatch(Path.GetFileName(f)))
                    res.Add(f);
        } catch(UnauthorizedAccessException) {
            // just ignore.
        }
        return res.ToArray();
    }

    /// <summary>
    ///   ディレクトリ内の総ファイルサイズを得る
    /// </summary>
    /// <param name="dir">ディレクトリ名</param>
    /// <param name="depth">サブディレクトリをたどる深さ。0の場合サブディレクトリをたどらない</param>
    /// <returns>ファイルサイズ (byte)</returns>
    /// <remarks>
    ///   <para>
    ///     シンボリックリンクは正しくたどれません。
    ///   </para>
    /// </remarks>
    public static long GetDirectorySize(string dir, int depth=99) {
        return GetDirectorySize(new DirectoryInfo(dir));
    }

    /// <summary>
    ///   ディレクトリ内の総ファイルサイズを得る
    /// </summary>
    /// <param name="di">ディレクトリ情報</param>
    /// <param name="depth">サブディレクトリをたどる深さ。0の場合サブディレクトリをたどらない</param>
    /// <returns>ファイルサイズ (byte)</returns>
    /// <remarks>
    ///   <para>
    ///     シンボリックリンクは正しくたどれません。
    ///   </para>
    /// </remarks>
    public static long GetDirectorySize(DirectoryInfo di, int depth=99) {
        long totalSize = 0;
        try {
            foreach(FileInfo fi in di.EnumerateFiles()) {
                totalSize += fi.Length;
            }
            if(depth <= 0)
                return totalSize;
            foreach(DirectoryInfo fi in di.EnumerateDirectories()) {
                totalSize += GetDirectorySize(fi, depth-1);
            }
        } catch(Exception) {
            // just ignore.
        }
        return totalSize;
    }
}

} // End of namespace
