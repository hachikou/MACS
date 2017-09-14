/*! @file Translator.cs
 * @brief 翻訳機
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
///   翻訳機
/// </summary>
public class Translator : Loggable {

    /// <summary>
    ///   辞書ファイルのディレクトリ名
    /// </summary>
    public static string DictionaryDirectory = "lang";

    /// <summary>
    ///   指定言語の翻訳機を獲得する
    /// </summary>
    public static Translator Get(string lang) {
        if(string.IsNullOrEmpty(lang))
            return Default;
        lock(g_mutex) {
            if(m_translators == null)
                m_translators = new Dictionary<string,Translator>();
            Translator t;
            if(m_translators.ContainsKey(lang)){
                t = m_translators[lang];
                if(t == null)
                    return Default;
                t.ReloadIfNeeded();
                return t;
            }
            string dictfile = Path.Combine(DictionaryDirectory, lang);
            if(!File.Exists(dictfile)){
                m_translators[lang] = null;
                return Default;
            }
            t = new Translator(dictfile);
            m_translators[lang] = t;
            return t;
        }
    }

    /// <summary>
    ///   指定言語の翻訳定義が存在するかどうかを返す
    /// </summary>
    public static bool Exists(string lang) {
        if(string.IsNullOrEmpty(lang))
            return false;
        lock(g_mutex) {
            if(m_translators == null)
                m_translators = new Dictionary<string,Translator>();
            if(m_translators.ContainsKey(lang)){
                return (m_translators[lang] != null);
            }
            string dictfile = Path.Combine(DictionaryDirectory, lang);
            if(!File.Exists(dictfile)){
                m_translators[lang] = null;
                return false;
            }
            m_translators[lang] = new Translator(dictfile);
            return true;
        }
    }

    /// <summary>
    ///   デフォルトの翻訳機
    /// </summary>
    public static Translator Default {
        get {
            if(m_default == null)
                m_default = new Translator();
            return m_default;
        }
    }


    /// <summary>
    ///   keyを翻訳した文字列を返す。
    /// </summary>
    public string this [ string key ] {
        get { return Trans(key,""); }
    }

    /// <summary>
    ///   keyを翻訳した文字列を返す（カテゴリ指定版）
    /// </summary>
    public string this [ string key, string category] {
        get { return Trans(key,category); }
    }

    /// <summary>
    ///   keyを翻訳した文字列を返す
    /// </summary>
    public string Trans(string key, string category) {
        if(m_dict == null)
            return key;
        Dictionary<string,string> d;
        string ret;
        if(m_dict.TryGetValue(category, out d)) {
            if(d.TryGetValue(key, out ret))
                return ret;
        }
        if((category != "") && m_dict[""].TryGetValue(key, out ret))
            return ret;
        return key;
    }

    /// <summary>
    ///   翻訳辞書をダンプアウトする（デバッグ用）
    /// </summary>
    public void Dump(TextWriter sw) {
        if(m_dict == null) {
            sw.WriteLine("# None");
            return;
        }
        foreach(string category in m_dict.Keys) {
            if(category != "")
                sw.WriteLine("[{0}]", category);
            foreach(KeyValuePair<string,string> kv in m_dict[category])
                sw.WriteLine("{0}::{1}", kv.Key, kv.Value);
        }
    }

    private static readonly String[] DictSeparator = new string[] {"::","|"};

    private static Dictionary<string,Translator> m_translators = null;
    private static Translator m_default = null;
    private static object g_mutex = new object();

    private Dictionary<string,Dictionary<string,string>> m_dict;
    private string m_filename;
    private DateTime m_filetime;

    private Translator() {}
    private Translator(string filename) {
        m_dict = new Dictionary<string,Dictionary<string,string>>();
        Load(filename);
    }

    private void ReloadIfNeeded() {
        if(File.Exists(m_filename) && (File.GetLastWriteTime(m_filename) != m_filetime))
            Load(m_filename);
    }

    private void Load(string filename) {
        LOG_INFO("Loading {0}", filename);
        m_filename = filename;
        m_dict.Clear();
        string category = "";
        Dictionary<string,string> dict = new Dictionary<string,string>();
        m_dict[category] = dict;
        using(StreamReader sr = FileUtil.Reader(filename, Encoding.UTF8)){
            if(sr == null)
                return;
            m_filetime = File.GetLastWriteTime(filename);
            while(!sr.EndOfStream){
                string line = sr.ReadLine().Trim();
                if((line.Length <= 0) || (line[0] == '#') || (line[0] == ';'))
                    continue;
                if(line[0] == '[') {
                    int idx = line.IndexOf(']');
                    if(idx > 0) {
                        category = line.Substring(1,idx-1);
                        if(!m_dict.TryGetValue(category, out dict)){
                            dict = new Dictionary<string,string>();
                            m_dict[category] = dict;
                        }
                        continue;
                    }
                }
                string[] kv = line.Split(DictSeparator,2,StringSplitOptions.None);
                if((kv[0].Length <= 0) || (kv[0][0] == '#') || (kv.Length != 2))
                    continue;
                string k = kv[0].TrimEnd();
                string v = kv[1].TrimStart();
                if((k == "") || (v == ""))
                    continue;
                dict[k] = v;
            }
            sr.Close();
        }
    }

}

} // End of namespace
