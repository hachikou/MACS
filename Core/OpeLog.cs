/*! @file OpeLog.cs
 * @brief 操作ログファイルを取り扱うオブジェクト
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace MACS {

/// <summary>
///   操作ログファイルを取り扱うオブジェクト
/// </summary>
public class OpeLog {

    /// <summary>
    ///   ログレベル
    /// </summary>
    public enum Level {
        DEBUG,
        INFO,
        NOTICE,
        WARNING,
        ERR,
        CRIT,
        ALERT,
        EMERG,
    };

    private readonly string m_filename;
    private readonly Encoding m_enc;
    private int m_size; // in kilobyte
    private int m_ages;
    private Level m_level;
    private Level m_consolelevel;
    private object m_mutex;


    /// <summary>
    ///   操作ログファイルオブジェクトを生成する。
    /// </summary>
    /// <param name="filename">ログファイル名（ディレクトリ名付き） </param>
    /// <remarks>
    ///   <para>
    ///     エンコーディングはデフォルト, ファイルサイズは1000kbytes, 保存世代数は9。
    ///   </para>
    /// </remarks>
    public OpeLog(string filename) {
        m_filename = filename;
        string dirname = Path.GetDirectoryName(m_filename);
        if(!string.IsNullOrEmpty(dirname) && !Directory.Exists(dirname))
            Directory.CreateDirectory(dirname);
        m_enc = null;
        m_size = 1000;
        m_ages = 9;
        m_level = Level.DEBUG;
        m_consolelevel = Level.EMERG;
        m_mutex = new object();
    }

    /// <summary>
    ///   操作ログファイルオブジェクトを生成する。
    /// </summary>
    /// <param name="filename">ログファイル名（ディレクトリ名付き） </param>
    /// <param name="enc">エンコーディング（null=デフォルト）</param>
    /// <param name="rotatesize">ファイルサイズ（kbytes）</param>
    /// <param name="ages">保存世代数</param>
    /// <remarks>
    ///   <para>
    ///     ファイルサイズが0以下の時には無制限として扱われる。
    ///     保存世代数の最大は99。それ以上を指定しても99として扱われる。
    ///     保存世代数が0以下の時には過去ログは保存されない。
    ///   </para>
    /// </remarks>
    public OpeLog(string filename, Encoding enc, int rotatesize, int ages) {
        m_mutex = new object();
        m_filename = filename;
        string dirname = Path.GetDirectoryName(m_filename);
        if(!string.IsNullOrEmpty(dirname) && !Directory.Exists(dirname))
            Directory.CreateDirectory(dirname);
        m_enc = enc;
        setRotateSize(rotatesize);
        setRotateAges(ages);
        m_level = Level.DEBUG;
        m_consolelevel = Level.EMERG;
    }

    /// <summary>
    ///   記録するログレベルを指定する。
    ///   指定したレベル以上のログメッセージがログファイルに記録される。
    /// </summary>
    public void SetLevel(Level lv) {
        lock(m_mutex){
            m_level = lv;
        }
    }

    /// <summary>
    ///   記録するログレベルを指定する。
    ///   指定したレベル以上のログメッセージがログファイルに記録される。
    /// </summary>
    public void SetLevel(string lv) {
        SetLevel(GetLevel(lv));
    }

    /// <summary>
    ///   現在のログレベルを返す
    /// </summary>
    public Level GetLevel() {
        return m_level;
    }

    /// <summary>
    ///   コンソール出力するログレベルを指定する。
    ///   指定したレベル以上のログメッセージがコンソールに出力される。
    /// </summary>
    public void SetConsoleLevel(Level lv) {
        lock(m_mutex){
            m_consolelevel = lv;
        }
    }

    /// <summary>
    ///   コンソール出力するログレベルを指定する。
    ///   指定したレベル以上のログメッセージがコンソールに出力される。
    /// </summary>
    public void SetConsoleLevel(string lv) {
        SetConsoleLevel(GetLevel(lv));
    }

    /// <summary>
    ///   レベル指定文字列からレベルを獲得する。
    /// </summary>
    public static Level GetLevel(string str) {
        try {
            return (Level)Enum.Parse(typeof(Level), str);
        } catch(ArgumentException){
            Console.WriteLine("Invalid log level ({0}), use DEBUG", str);
            return Level.DEBUG;
        }
    }


    /// <summary>
    ///   ログを追記する
    /// </summary>
    public void Log(string category, Level lv, string msg, bool ignoreNewLine=true) {
        lock(m_mutex){
            if(lv >= m_consolelevel)
                Console.WriteLine("{0}: {1}", category, msg);
            if(lv < m_level)
                return;
            if(m_size > 0){
                FileInfo fi = new FileInfo(m_filename);
                if(fi.Exists && (fi.Length/1000 >= m_size)){
                    try {
                        Rotate();
                    } catch(IOException e) {
                        Console.WriteLine("Can't rotate log file {0}: {1}", m_filename, e.Message);
                        return;
                    }
                }
            }
            StringBuilder txt = new StringBuilder();
            txt.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff "));
            txt.Append(category);
            txt.Append(' ');
            txt.Append(lv.ToString());
            txt.Append(' ');
            if(ignoreNewLine)
                txt.Append(msg.Replace('\n',' ').Replace('\r',' '));
            else
                txt.Append(msg);
            //Console.WriteLine(txt);
            StreamWriter sw = FileUtil.SharedAppendWriter(m_filename, m_enc);
            if(sw == null)
                return;
            sw.WriteLine(txt.ToString());
            sw.Close();
        }
    }

    /// <summary>
    ///   ログを追記する
    /// </summary>
    public void Log(string category, Level lv, string msg, params object[] objs) {
        Log(category, lv, String.Format(msg, objs));
    }

    /// <summary>
    ///   ローテートする
    /// </summary>
    public void Rotate() {
        removeOld();
        string basename = Path.Combine(Path.GetDirectoryName(m_filename), Path.GetFileNameWithoutExtension(m_filename))+".";
        string ext = Path.GetExtension(m_filename);
        int n = m_ages;
        while(n > 0){
            string fname = basename+n.ToString()+ext;
            string xfname;
            if(n > 1)
                xfname = basename+(n-1).ToString()+ext;
            else
                xfname = m_filename;
            File.Delete(fname);
            if(File.Exists(xfname))
                File.Move(xfname, fname);
            n--;
        }
        File.Delete(m_filename);
        Log("LOGGER", Level.INFO, "Operation log file is rotated.");
    }

    /// <summary>
    ///   ローテーションサイズをセットする。
    ///   単位はkilobyte。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ファイルサイズが0以下の時には無制限として扱われる。
    ///   </para>
    /// </remarks>
    public void SetRotateSize(int sz) {
        lock(m_mutex) {
            setRotateSize(sz);
        }
    }

    /// <summary>
    ///   ローテーションサイズを獲得する。
    /// </summary>
    public int GetRotateSize() {
        return m_size;
    }

    /// <summary>
    ///   ローテーション世代数をセットする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     保存世代数の最大は99。それ以上を指定しても99として扱われる。
    ///     保存世代数が0以下の時には過去ログは保存されない。
    ///     既に指定した世代数以上の過去ログファイルがある場合には削除される。
    ///   </para>
    /// </remarks>
    public void SetRotateAges(int ages) {
        lock(m_mutex) {
            setRotateAges(ages);
            removeOld();
        }
    }

    /// <summary>
    ///   ローテーション世代数を獲得する。
    /// </summary>
    public int GetRotateAges() {
        return m_ages;
    }

    /// <summary>
    ///   ログファイル名を獲得する。
    /// </summary>
    public string GetFileName() {
        return m_filename;
    }

    /// <summary>
    ///   過去ログファイル名を獲得する。
    /// </summary>
    public string GetFileName(int n) {
        if(n == 0)
            return m_filename;
        string basename = Path.Combine(Path.GetDirectoryName(m_filename), Path.GetFileNameWithoutExtension(m_filename))+".";
        string ext = Path.GetExtension(m_filename);
        return basename+n.ToString()+ext;
    }


    private void setRotateSize(int sz) {
        if(sz < 0)
            sz = 0;
        m_size = sz;
    }

    private void setRotateAges(int ages) {
        if(ages < 0)
            m_ages = 0;
        else if(ages < 100)
            m_ages = ages;
        else
            m_ages = 99;
    }

    private void removeOld() {
        string basename = Path.Combine(Path.GetDirectoryName(m_filename), Path.GetFileNameWithoutExtension(m_filename))+".";
        string ext = Path.GetExtension(m_filename);
        int n = m_ages+1;
        while(n < 100){
            File.Delete(basename+n.ToString()+ext);
            n++;
        }
    }

}

} // End of namespace
