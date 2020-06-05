/// IniFile: INIファイルツール.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   INI形式のファイルを取り扱うオブジェクト
/// </summary>
/// <remarks>
///   <para>
///     カギカッコによるセクションが最初にひとつだけあるINIファイルには対応して
///     いるが、複数のセクションがあるINIファイル形式には対応していない。
///   </para>
/// </remarks>
public class IniFile {

    /// <summary>
    ///   INIファイルを読み取り、データベースを作成する。
    /// </summary>
    /// <param name="filename">ファイル名</param>
    /// <param name="enc">ファイルエンコード (optional)</param>
    /// <param name="distfile">INIファイルのテンプレート (optional)</param>
    /// <remarks>
    ///   <para>
    ///     指定ファイルが存在していなくてもコンストラクションは成功します。
    ///     この場合、のちにWriteを実行するとそのファイルが作成されます。
    ///     encを指定しないかnullを指定すると、FileUtil.DefaultEncoding が使われ
    ///     ます。
    ///     distfileを指定すると、そのファイルを使って Upgradeが実行されます。
    ///   </para>
    /// </remarks>
    public IniFile(string filename, Encoding enc=null, string distfile=null) {
        m_mutex = new object();
        m_filename = filename;
        m_enc = enc;
        if(!String.IsNullOrEmpty(distfile))
            _upgrade(distfile);
    }

    /// <summary>
    ///   指定StreamReaderから読み取り、データベースを作成する。
    /// </summary>
    public IniFile(StreamReader sr) {
        m_mutex = new object();
        m_filename = null;
        m_enc = null;
        _reload(sr);
    }

    /// <summary>
    ///   空のデータベースを作成する。
    ///   SectionIniFile用
    /// </summary>
    internal IniFile(Encoding enc) {
        m_mutex = new object();
        m_filename = null;
        m_enc = enc;
    }
    /// <summary>
    ///   空のデータベースを作成する。
    ///   デフォルトエンコーディング版。
    ///   SectionIniFile用
    /// </summary>
    internal IniFile() {
        m_mutex = new object();
        m_filename = null;
        m_enc = null;
    }

    /// <summary>
    ///   ファイル名
    /// </summary>
    public string FileName {
        get { return m_filename; }
        set { m_filename = value; }
    }

    /// <summary>
    ///   セクション名
    /// </summary>
    public string Section {
        get { return m_sectionname; }
        set { m_sectionname = value; }
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
    ///   INIファイルの1行を読み取る。
    ///   SectionIniFile用。
    /// </summary>
    internal bool LoadLine(string line){
        lock(m_mutex){
            return _loadline(line);
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
        lock(m_mutex) {
            return _write(m_filename, makebak, tmpdir);
        }
    }

    /// <summary>
    ///   INIファイルをアップグレードする。
    /// </summary>
    /// <param name="srcfile">アップグレードのベースとなるファイル。</param>
    /// <param name="makebak">バックアップファイルを作るかどうか。</param>
    /// <param name="tmpdir">一時保存ファイルを作るディレクトリ名。nullを指定したり省略した場合はINIファイルと同じディレクトリが使われる。</param>
    /// <remarks>
    ///   <para>
    ///     ベースとなるファイルに書かれている内容のうち、本データベースで値を
    ///     持っている定義行以外のすべての行は、そのまま保持される。（コメント行
    ///     や空行は変更されない。）
    ///     ベースファイルの定義値と本データベースの現在の値が異なる場合には、
    ///     ベースファイルの定義行がコメントとして残る。
    ///   </para>
    /// </remarks>
    public bool Upgrade(string srcfile, bool makebak=true, string tmpdir=null) {
        lock(m_mutex) {
            _reload_ifneeded();
            return _write(srcfile, makebak, tmpdir);
        }
    }
        
    /// <summary>
    ///   指定したキーに対応する値を返す。デフォルトは""。
    /// </summary>
    public string Get(string key) {
        return Get(key,"");
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。文字列版。
    /// </summary>
    public string Get(string key, string def) {
        lock(m_mutex){
            _reload_ifneeded();
            string val;
            if((m_data != null) && m_data.TryGetValue(key, out val))
                return val;
            return def;
        }
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。整数版。
    /// </summary>
    public int Get(string key, int def) {
        return StringUtil.ToInt(Get(key, def.ToString()), def);
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。浮動小数点数版。
    /// </summary>
    public double Get(string key, double def) {
        try {
            return double.Parse(Get(key, def.ToString()));
        } catch(Exception){
            // just ignore.
        }
        return def;
    }
    /// <summary>
    ///   指定したキーに対応する値を返す。 true/false版。
    /// </summary>
    public bool Get(string key, bool def) {
        return GetBoolFromString(Get(key, ""), def);
    }

    /// <summary>
    ///   指定したキーに対応する値をセットする。文字列版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string key, string val) {
        lock(m_mutex){
            _reload_ifneeded();
            if(val == null) {
                m_data.Remove(key);
            } else {
                m_data[key] = val;
            }
            m_lasttime = DateTime.Now;
        }
    }
    /// <summary>
    ///   指定したキーに対応する値をセットする。整数版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string key, int val) {
        Set(key, val.ToString());
    }
    /// <summary>
    ///   指定したキーに対応する値をセットする。浮動小数点数版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string key, double val) {
        Set(key, val.ToString());
    }
    /// <summary>
    ///   指定したキーに対応する値をセットする。true/false版。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Set(string key, bool val) {
        Set(key, val?"yes":"no");
    }

    /// <summary>
    ///   指定したキーの登録を削除する。指定したキーが登録されていないときは
    ///   なにもしない。
    ///   ファイルへの書き込みは行なわない。
    /// </summary>
    public void Remove(string key) {
        Set(key, null);
    }

    /// <summary>
    ///   全てのキーの一覧を返す。
    /// </summary>
    public string[] GetKeys() {
        lock(m_mutex){
            _reload_ifneeded();
            if(m_data == null)
                return new string[0];
            string[] keys = new string[m_data.Count];
            m_data.Keys.CopyTo(keys,0);
            return keys;
        }
    }

    /// <summary>
    ///   最終更新日時
    /// </summary>
    public DateTime LastTime {
        get {
            lock(m_mutex){
                _reload_ifneeded();
                return m_lasttime;
            }
        }
    }


    /// <summary>
    ///   文字列からbool値を判断する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     valが"yes","Yes","YES","y","Y","true","True","TRUE","t","T","1"のとき、trueを、
    ///     valが"no","No","NO","n","N","false","False","FALSE","f","F","0"のとき、falseを、
    ///     それ以外のときはdefを返す。
    ///   </para>
    /// </remarks>
    public static bool GetBoolFromString(string val, bool def) {
        switch(val) {
        case "yes":
        case "Yes":
        case "YES":
        case "y":
        case "Y":
        case "true":
        case "True":
        case "TRUE":
        case "t":
        case "T":
        case "1":
            return true;
        case "no":
        case "No":
        case "NO":
        case "n":
        case "N":
        case "false":
        case "False":
        case "FALSE":
        case "f":
        case "F":
        case "0":
            return false;
        default:
            return def;
        }
    }


    private string m_filename;
    private readonly Encoding m_enc;
    private Dictionary<string, string> m_data;
    private string m_sectionname;
    private DateTime m_lasttime;
    private object m_mutex;

    private void _reload_ifneeded() {
        if((m_data == null)
           || ((m_filename != null) && File.Exists(m_filename) && (m_lasttime < File.GetLastWriteTime(m_filename)))){
            _reload();
        }
    }

    private bool _reload() {
        if(m_data == null)
            m_data = new Dictionary<string, string>();
        else
            m_data.Clear();
        m_sectionname = null;
        m_lasttime = DateTime.MinValue;
        if((m_filename == null) || !File.Exists(m_filename))
            return false;
        using(StreamReader sr = FileUtil.Reader(m_filename, m_enc)){
            if(!_reload(sr))
                return false;
            sr.Close();
        }
        m_lasttime = File.GetLastWriteTime(m_filename);
        return true;
    }

    private bool _reload(StreamReader sr) {
        if(sr == null)
            return false;
        while(!sr.EndOfStream) {
            _loadline(sr.ReadLine().Trim());
        }
        return true;
    }

    private bool _loadline(string line){
        if(line.StartsWith("#") || line.StartsWith(";"))
            return false;
        if(line.StartsWith("[")){
            int idx = line.IndexOf(']');
            if(idx > 1)
                m_sectionname = line.Substring(1,idx-1);
            return true;
        }
        string[] keyval = line.Split("=".ToCharArray(), 2);
        if(keyval.Length != 2)
            return false;
        if(m_data == null)
            m_data = new Dictionary<string, string>();
        m_data[keyval[0].Trim()] = keyval[1].Trim().Replace("\\n", "\n");
        return true;
    }

    private bool _write(string srcfile, bool makebak, string tmpdir) {
        if(m_filename == null)
            return false;
        bool upgradeMode = (srcfile != m_filename);
        string tmpfilename;
        if(String.IsNullOrEmpty(tmpdir)) {
            tmpfilename = m_filename;
        } else {
            tmpfilename = Path.Combine(tmpdir,Path.GetFileName(m_filename));
        }
        tmpfilename += "."+rnd.Next(1000000).ToString();
        try {
            // まず元ファイルを一時ファイルにコピーする
            if(File.Exists(srcfile)) {
                if(!FileUtil.Copy(srcfile, tmpfilename))
                    return false;
            }
            if(makebak && File.Exists(m_filename))
                FileUtil.Copy(m_filename, m_filename+".bak");
            using(StreamWriter sw = FileUtil.Writer(m_filename, m_enc)){
                if(sw == null)
                    return false;
                if(m_data == null)
                    m_data = new Dictionary<string,string>(); // ダミーで空データを書き出す
                if(File.Exists(tmpfilename)) {
                    using(StreamReader sr = FileUtil.Reader(tmpfilename, m_enc)){
                        if(sr == null)
                            return false;
                        while(!sr.EndOfStream) {
                            string line = sr.ReadLine().Trim();
                            if(line.StartsWith("#") || line.StartsWith(";") || line.StartsWith("[")) {
                                sw.WriteLine(line);
                                continue;
                            }
                            string[] keyval = line.Split("=".ToCharArray(), 2);
                            if(keyval.Length != 2) {
                                sw.WriteLine(line);
                                continue;
                            }
                            string key = keyval[0].Trim();
                            string val;
                            if(m_data.TryGetValue(key, out val)) {
                                string xval = keyval[1].Trim().Replace("\\n", "\n");
                                if(val == xval) {
                                    sw.WriteLine(line);
                                } else {
                                    if(upgradeMode)
                                        sw.WriteLine("#"+line);
                                    sw.WriteLine(key + "=" + val.Replace("\n", "\\n").Replace("\r",""));
                                }
                                m_data.Remove(key);
                            } else {
                                sw.WriteLine(line);
                            }
                        }
                        sr.Close();
                    }
                } else {
                    if(m_sectionname != null)
                        sw.WriteLine("[" + m_sectionname + "]");
                }
                foreach(string key in m_data.Keys) {
                    sw.WriteLine(key + "=" + m_data[key].Replace("\n", "\\n").Replace("\r", ""));
                }
                sw.Close();
            }
        } finally {
            if(File.Exists(tmpfilename))
                File.Delete(tmpfilename);
        }
        return _reload();
    }

    private void _upgrade(string distfile) {
        if(File.Exists(distfile)) {
            if(File.Exists(m_filename)) {
                // .distファイルのほうが新しい場合、.iniファイルをアップグレードする
                if(File.GetLastWriteTime(m_filename) < File.GetLastWriteTime(distfile))
                    Upgrade(distfile);
            } else {
                // .iniファイルがないときは.distファイルをコピーする
                FileUtil.Copy(distfile, m_filename);
            }
        }
    }

    private static Random rnd = new Random();


#region SELFTEST
#if SELFTEST
    public static int Main(string[] args) {
        string inifilename = "IniFileTest.ini";
        
        // サンプルのINIファイルを作る
        using(StreamWriter sw = FileUtil.Writer(inifilename)) {
            sw.WriteLine("# Sample INI file");
            sw.WriteLine("");
            sw.WriteLine("hogehoge = mogera");
            sw.WriteLine("hogehoge2 = mogera2");
            sw.WriteLine("# hogehoge3 = mogera3");
            sw.WriteLine("hogehoge4 = 314");
        }

        // IniFileを作って、ちゃんと定義されているか確認
        IniFile ini = new IniFile(inifilename);
        if(ini.Get("hogehoge") == "mogera")
            Console.WriteLine("hogehoge: OK");
        else
            Console.WriteLine("hogehoge: NG");
        if(ini.Get("hogehoge2") == "mogera2")
            Console.WriteLine("hogehoge2: OK");
        else
            Console.WriteLine("hogehoge2: NG");
        if(ini.Get("hogehoge3") == "")
            Console.WriteLine("hogehoge3: OK");
        else
            Console.WriteLine("hogehoge3: NG");
        if(ini.Get("hogehoge4",0) == 314)
            Console.WriteLine("hogehoge4: OK");
        else
            Console.WriteLine("hogehoge4: NG");

        // 定義を追加して書き出してみる
        ini.Set("piyopiyo", "punyo");
        ini.Set("hogehoge4", 512);
        ini.Write();
        using(StreamReader sr = FileUtil.Reader(inifilename)) {
            while(!sr.EndOfStream) {
                Console.WriteLine(sr.ReadLine());
            }
        }

        // Upgradeの試験
        using(StreamWriter sw = FileUtil.Writer(inifilename+".dist")) {
            sw.WriteLine("# Upgraded Sample INI file");
            sw.WriteLine("");
            sw.WriteLine("hogehoge = mogerachu");
            sw.WriteLine("hogehoge2 = mogera2");
            sw.WriteLine("# added hogehoge3.");
            sw.WriteLine("hogehoge3 = mogera3");
            sw.WriteLine("hogehoge4 = 314");
        }
        ini.Upgrade(inifilename+".dist");
        using(StreamReader sr = FileUtil.Reader(inifilename)) {
            while(!sr.EndOfStream) {
                Console.WriteLine(sr.ReadLine());
            }
        }

        return 0;
    }
#endif
#endregion

}

} // End of namespace
