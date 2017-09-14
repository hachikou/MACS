/*! @file SectionIniFile.cs
 *	@brief セクション分けされたINIファイルツール
 *
 * @author shibuya@ncad.co.jp
 * @date 2010/02/19
 * @version $Id: SectionIniFile.cs 68 2011-06-16 05:17:50Z shibuya $
 *
 * Copyright (C) 2011-2015 Nippon C.A.D. Co.,Ltd. All rights reserved.
 */

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   []でセクション分けされたINI形式のファイルを取り扱うオブジェクト
/// </summary>
public class SectionIniFile {

    private readonly string m_filename;
    private readonly Encoding m_enc;
    private SortedDictionary<string, IniFile> m_section;
    private DateTime m_lasttime;
    private object m_mutex;

    /// <summary>
    ///   INIファイルを読み取り、データベースを作成する。
    /// </summary>
    public SectionIniFile(string filename, Encoding enc) {
        m_mutex = new object();
        m_filename = filename;
        m_enc = enc;
        _reload();
    }
    /// <summary>
    ///   INIファイルを読み取り、データベースを作成する。
    ///   デフォルトエンコーディング版。
    /// </summary>
    public SectionIniFile(string filename) {
        m_mutex = new object();
        m_filename = filename;
        m_enc = null;
        _reload();
    }

    /// <summary>
    ///   ファイル名
    /// </summary>
    public string FileName {
        get { return m_filename; }
    }

    /// <summary>
    ///   INIファイルを読み取る。
    /// </summary>
    public bool Reload() {
        lock(m_mutex){
            return _reload();
        }
    }

    /// <summary>
    ///   INIファイルを書き出す。
    /// </summary>
    /// <param name="makebak">バックアップファイルを作るかどうか。</param>
    /// <param name="tmpdir">一時保存ファイルを作るディレクトリ名。nullを指定したり省略した場合はINIファイルと同じディレクトリが使われる。</param>
    /// <remarks>
    ///   <para>
    ///     既存ファイルに書き出す場合、そのファイルに書かれている内容のうち、
    ///     本データベースで値を持っている定義行以外のすべての行は、そのまま
    ///     保持される。（コメント行や空行は変更されない。）
    ///   </para>
    /// </remarks>
    public bool Write(bool makebak=true, string tmpdir=null) {
        lock(m_mutex){
            if(m_filename == null)
                return false;
            string tmpfilename;
            if(String.IsNullOrEmpty(tmpdir)) {
                tmpfilename = m_filename;
            } else {
                tmpfilename = Path.Combine(tmpdir,Path.GetFileName(m_filename));
            }
            tmpfilename += "."+rnd.Next(1000000).ToString();
            try {
                // まず元ファイルを一時ファイルにコピーする
                if(File.Exists(m_filename)) {
                    if(!FileUtil.Copy(m_filename, tmpfilename))
                        return false;
                    if(makebak)
                        FileUtil.Copy(m_filename, m_filename+".bak");
                }
                using(StreamWriter sw = FileUtil.Writer(m_filename, m_enc)){
                    if(sw == null)
                        return false;
                    if(File.Exists(tmpfilename)) {
                        using(StreamReader sr = FileUtil.Reader(tmpfilename, m_enc)) {
                            if(sr == null)
                                return false;
                            string sectionname = "";
                            IniFile section = m_section[sectionname];
                            while(!sr.EndOfStream) {
                                string line = sr.ReadLine().Trim();
                                if(line.StartsWith("#") || line.StartsWith(";")) {
                                    sw.WriteLine(line);
                                    continue;
                                }
                                if(line.StartsWith("[") && line.EndsWith("]")) {
                                    // セクション内の残りのkey-valueを書き出す
                                    foreach(string key in section.GetKeys()) {
                                        sw.WriteLine(key+"="+section.Get(key));
                                    }
                                    m_section.Remove(sectionname);
                                    sectionname = line.Substring(1,line.Length-2).Trim();
                                    if(m_section.ContainsKey(sectionname))
                                        section = m_section[sectionname];
                                    else
                                        section = new IniFile(); //ダミー
                                    sw.WriteLine(line);
                                    continue;
                                }
                                string[] keyval = line.Split("=".ToCharArray(), 2);
                                if(keyval.Length == 2) {
                                    string key = keyval[0].Trim();
                                    sw.WriteLine(key+"="+section.Get(key, keyval[1].Trim()));
                                    section.Remove(key);
                                } else {
                                    sw.WriteLine(line);
                                }
                            }
                            sr.Close();

                            // セクション内の残りのkey-valueを書き出す
                            foreach(string key in section.GetKeys()) {
                                sw.WriteLine(key+"="+section.Get(key));
                            }
                            m_section.Remove(sectionname);
                        }
                    }
                    // 残りのセクションを書き出す
                    foreach(string sectionname in m_section.Keys){
                        if(sectionname != ""){
                            sw.Write("[");
                            sw.Write(sectionname);
                            sw.WriteLine("]");
                        }
                        IniFile ini = m_section[sectionname];
                        foreach(string key in ini.GetKeys()){
                            sw.Write(key);
                            sw.Write("=");
                            sw.WriteLine(ini.Get(key));
                        }
                    }
                    sw.Close();
                }
            } finally {
                if(File.Exists(tmpfilename))
                    File.Delete(tmpfilename);
            }
            return _reload();
        }
    }

    /// <summary>
    ///   指定したキーに対応する値を返す。デフォルトは""。
    /// </summary>
    public string Get(string sect, string key) {
        return Get(sect, key,"");
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。文字列版。
    /// </summary>
    public string Get(string sect, string key, string def) {
        lock(m_mutex){
            if(sect == null)
                sect = "";
            if((m_section == null)
               || (File.Exists(m_filename) && ( m_lasttime < File.GetLastWriteTime(m_filename)))){
                _reload();
            }
            if((m_section != null) && m_section.ContainsKey(sect))
                return m_section[sect].Get(key,def);
            return def;
        }
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。整数版。
    /// </summary>
    public int Get(string sect, string key, int def) {
        return StringUtil.ToInt(Get(sect, key, def.ToString()), def);
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。浮動小数点数版。
    /// </summary>
    public double Get(string sect, string key, double def) {
        try {
            return double.Parse(Get(sect, key, def.ToString()));
        } catch(Exception){
            // just ignore.
        }
        return def;
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。 true/false版。
    /// </summary>
    public bool Get(string sect, string key, bool def) {
        return IniFile.GetBoolFromString(Get(sect, key, ""), def);
    }

    /// <summary>
    ///   指定したキーに対応する値をセットする。文字列版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string sect, string key, string val) {
        lock(m_mutex){
            if(m_section == null)
                m_section = new SortedDictionary<string,IniFile>();
            if(!m_section.ContainsKey(sect))
                m_section[sect] = new IniFile(m_enc);
            m_section[sect].Set(key,val);
            m_lasttime = DateTime.Now;
        }
    }
    /// <summary>
    ///   指定したキーに対応する値をセットする。整数版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string sect, string key, int val) {
        Set(sect, key, val.ToString());
    }
    /// <summary>
    ///   指定したキーに対応する値をセットする。浮動小数点数版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string sect, string key, double val) {
        Set(sect, key, val.ToString());
    }
    /// <summary>
    ///   指定したキーに対応する値をセットする。true/false版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string sect, string key, bool val) {
        Set(sect, key, val?"yes":"no");
    }

    /// <summary>
    ///   セクションを丸ごと削除する
    /// </summary>
    public bool DeleteSection(string sect) {
        lock(m_mutex){
            if(m_section == null)
                return false;
            if(m_section.Remove(sect)){
                m_lasttime = DateTime.Now;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    ///   セクション名の一覧を獲得する。
    /// </summary>
    public string[] GetSections() {
        lock(m_mutex){
            if(m_section == null)
                return new string[0];
            string[] sections = new string[m_section.Count];
            m_section.Keys.CopyTo(sections,0);
            return sections;
        }
    }


    private bool _reload() {
        m_section = null;
        if(!File.Exists(m_filename))
            return false;
        using(StreamReader sr = FileUtil.Reader(m_filename, m_enc)){
            if(sr == null)
                return false;
            m_section = new SortedDictionary<string, IniFile>();
            IniFile current = new IniFile(m_enc);
            m_section[""] = current;
            while(!sr.EndOfStream) {
                string line = sr.ReadLine().Trim();
                if(line.StartsWith("[")){
                    int idx = line.IndexOf(']');
                    if(idx > 1){
                        string sectionname = line.Substring(1,idx-1);
                        current = new IniFile(m_enc);
                        m_section[sectionname] = current;
                    }
                    continue;
                }
                current.LoadLine(line);
            }
            sr.Close();
            m_lasttime = File.GetLastWriteTime(m_filename);
        }
        return true;
    }

    private static Random rnd = new Random();

}

} // End of namespace
