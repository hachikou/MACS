/// ConfDict: INI形式文字列ツール.
///
/// Copyright (C) 2019 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   key=value形式の文字列を取り扱うオブジェクト
/// </summary>
public class ConfDict {

#region プロパティ

    /// <summary>
    ///   項目セパレータ文字列
    /// </summary>
    public string Separator {
        get { return separator[0]; }
        set { separator[0] = value; }
    }

    /// <summary>
    ///   項目セパレータ代替文字列
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ToString()で文字列化する際、valueにSeparatorが含まれているときにこの
    ///     文字列に置換する。
    ///     SeparatorSubstがSeparatorを内包していてはいけない。
    ///   </para>
    /// </remarks>
    public string SeparatorSubst = "\\n";

    /// <summary>
    ///   項目数
    /// </summary>
    public int Count {
        get {
            lock(mutex) {
                return dict.Count;
            }
        }
    }

#endregion

#region コンストラクタ

    /// <summary>
    ///   空のkey-valueデータベースを作る（デフォルトコンストラクタ）
    /// </summary>
    public ConfDict() {
        // Nothing to do.
    }

    /// <summary>
    ///   指定文字列から、key-valueデータベースを作成する。
    /// </summary>
    public ConfDict(string str) {
        append(str);
        dirty = false;
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public ConfDict(ConfDict src) {
        lock(src.mutex) {
            copyFrom(src);
            dirty = false;
        }
    }

#endregion

#region 一括設定など

    /// <summary>
    ///   指定文字列を読み込んでkey-valueデータベースをセットする
    /// </summary>
    public ConfDict Parse(string str) {
        lock(mutex) {
            dict.Clear();
            append(str);
        }
        return this;
    }

    /// <summary>
    ///   指定文字列をkey-valueデータベースに追加取り込みする
    /// </summary>
    public ConfDict Append(string str) {
        lock(mutex) {
            append(str);
        }
        return this;
    }

    /// <summary>
    ///   key-valueデータベースを空にする
    /// </summary>
    public ConfDict Clear() {
        lock(mutex) {
            if(dict.Count > 0) {
                dict.Clear();
                dirty = true;
            }
        }
        return this;
    }

    /// <summary>
    ///   他のConfDictの内容を取り込む
    /// </summary>
    public ConfDict CopyFrom(ConfDict src) {
        lock(mutex) {
            lock(src.mutex) {
                copyFrom(src);
            }
        }
        return this;
    }

    /// <summary>
    ///   コピーインスタンスを作成する
    /// </summary>
    public ConfDict Copy() {
        return new ConfDict(this);
    }

    /// <summary>
    ///   文字列化
    /// </summary>
    public override string ToString() {
        lock(mutex) {
            return toString();
        }
    }

#endregion

#region 項目登録と参照

    /// <summary>
    ///   指定したkeyとvalをデータベースに追加登録する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     valにnullを指定すると、keyの登録が削除される。
    ///   </para>
    /// </remarks>
    public ConfDict Set(string key, string val) {
        lock(mutex) {
            append(key, val);
        }
        return this;
    }

    /// <summary>
    ///   指定したkeyとvalをデータベースに追加登録する。
    /// </summary>
    public ConfDict Set(string key, int val) {
        lock(mutex) {
            append(key, val.ToString());
        }
        return this;
    }

    /// <summary>
    ///   指定したkeyとvalをデータベースに追加登録する。
    /// </summary>
    public ConfDict Set(string key, long val) {
        lock(mutex) {
            append(key, val.ToString());
        }
        return this;
    }

    /// <summary>
    ///   指定したkeyとvalをデータベースに追加登録する。
    /// </summary>
    public ConfDict Set(string key, double val) {
        lock(mutex) {
            append(key, val.ToString());
        }
        return this;
    }

    /// <summary>
    ///   指定したkeyとvalをデータベースに追加登録する。
    /// </summary>
    public ConfDict Set(string key, bool val) {
        lock(mutex) {
            append(key, val?"yes":"no");
        }
        return this;
    }

    /// <summary>
    ///   指定したkeyに対するvalueを返す。
    /// </summary>
    public string Get(string key, string defval=null) {
        lock(mutex) {
            string res;
            if(dict.TryGetValue(key, out res))
                return res;
            return defval;
        }
    }

    /// <summary>
    ///   指定したkeyに対するvalueを返す。
    /// </summary>
    public int Get(string key, int defval) {
        return StringUtil.ToInt(Get(key), defval);
    }

    /// <summary>
    ///   指定したkeyに対するvalueを返す。
    /// </summary>
    public long Get(string key, long defval) {
        return StringUtil.ToLong(Get(key), defval);
    }

    /// <summary>
    ///   指定したkeyに対するvalueを返す。
    /// </summary>
    public double Get(string key, double defval) {
        return StringUtil.ToDouble(Get(key), defval);
    }

    /// <summary>
    ///   指定したkeyに対するvalueを返す。
    /// </summary>
    public bool Get(string key, bool defval) {
        return StringUtil.ToBool(Get(key), defval);
    }

    /// <summary>
    ///   指定keyの登録を削除する
    /// </summary>
    public ConfDict Remove(string key) {
        lock(mutex) {
            append(key, null);
        }
        return this;
    }

    /// <summary>
    ///   インデクサ
    /// </summary>
    public string this[string key] {
        get { return Get(key); }
        set { Set(key, value); }
    }

    /// <summary>
    ///   指定キーが登録されているかどうか
    /// </summary>
    public bool Contains(string key) {
        lock(mutex) {
            return dict.ContainsKey(key);
        }
    }

    /// <summary>
    ///   イテレータ
    /// </summary>
    public IEnumerator<KeyValuePair<string,string>> GetEnumerator() {
        return dict.GetEnumerator();
    }
    
#endregion

#region 変更確認

    /// <summary>
    ///   コンストラクト時または最後にCheckAndClearDirtyを実行した時から内容が変更されたかどうか
    /// </summary>
    public bool Dirty {
        get { return dirty; }
    }

    /// <summary>
    ///   コンストラクト時または最後にCheckAndClearDirtyを実行した時から内容が変更されたかどうかをチェックし、Dirtyフラグをクリアする
    /// </summary>
    public bool CheckAndClearDirty() {
        lock(mutex) {
            if(dirty) {
                dirty = false;
                return true;
            } else {
                return false;
            }
        }
    }

#endregion


#region private部

    private object mutex = new object();
    private Dictionary<string,string> dict = new Dictionary<string,string>();
    private string[] separator = new string[]{"\n"};
    private bool dirty = false;

    private static readonly char[] equalSeparator = "=".ToCharArray();

    private void append(string str) {
        if(String.IsNullOrEmpty(str))
            return;
        foreach(string x in str.Split(separator, StringSplitOptions.RemoveEmptyEntries)) {
            string[] kv = x.Split(equalSeparator, 2);
            if(kv.Length == 1) {
                kv = new string[]{kv[0], "yes"}; // bool型のtrue定義だとして取り扱う
            }
            append(kv[0].Trim(), kv[1].Trim().Replace(SeparatorSubst, Separator));
        }
    }

    private void append(string key, string val) {
        if(String.IsNullOrEmpty(key))
            return;
        if(val == null) {
            if(dict.Remove(key))
                dirty = true;
            return;
        }
        string oldval;
        if(dict.TryGetValue(key, out oldval)) {
            if(oldval != val) {
                dict[key] = val;
                dirty = true;
            }
        } else {
            dict.Add(key, val);
            dirty = true;
        }
    }

    private void copyFrom(ConfDict src) {
        dict.Clear();
        foreach(KeyValuePair<string,string> kv in src.dict) {
            dict.Add(kv.Key, kv.Value);
        }
        dirty = true;
    }

    private string toString() {
        StringBuilder sb = new StringBuilder();
        foreach(KeyValuePair<string,string> kv in dict) {
            if(sb.Length > 0)
                sb.Append(Separator);
            sb.Append(kv.Key);
            sb.Append('=');
            sb.Append(kv.Value.Replace(Separator, SeparatorSubst));
        }
        return sb.ToString();
    }

#endregion

#region SELFTEST
#if SELFTEST
    public static int Main(string[] args) {
        ConfDict dict = new ConfDict("a=hello\nb=my\nc=boy\\ngirl");
        Console.WriteLine("b={0}", dict["b"]);
        Console.WriteLine("c={0}", dict["c"]);
        Console.WriteLine("hoge={0}", dict.Get("hoge", "mogera"));
        dict.Set("hoge", 3.1415);
        dict.Remove("b");
        dict.Remove("x");
        Console.WriteLine("hoge={0}", dict.Get("hoge", "mogera"));
        Console.WriteLine();
        Console.WriteLine(dict.ToString());
        Console.WriteLine();
        ConfDict dict2 = new ConfDict(dict.ToString());
        Console.WriteLine("c={0}", dict2["c"]);
        return 0;
    }
#endif
#endregion

} // End of ConfDict class

} // End of namespace
