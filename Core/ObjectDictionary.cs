/*! @file ObjectDictionary.cs
 * @brief string - object対応ディクショナリ。
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace MACS {

/// <summary>
///   string - object対応ディクショナリ。
/// </summary>
public class ObjectDictionary : SortedDictionary<string,object> {

    /// <summary>
    ///   string - object対応ディクショナリ。
    ///   キーとなる文字列の長さによってソートされる（長いものが先に来る）。
    /// </summary>
    public ObjectDictionary() : base(KeyLengthComparer) {}

    /// <summary>
    ///   string - object対応ディクショナリ。
    ///   キーの並べ替え関数を指定するバージョン。
    /// </summary>
    public ObjectDictionary(IComparer<string> cmp) : base(cmp) {}

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public ObjectDictionary(ObjectDictionary src) : base(src, src.Comparer) {}


    /// <summary>
    ///   キーに対応する要素を取り出す。
    ///   キーが存在しない場合にはnullを返す。
    /// </summary>
    public new object this[string name] {
        get {
            object x;
            if(TryGetValue(name, out x))
                return x;
            return null;
        }
        set {
            base[name] = value;
        }
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public string Get(string key, string defaultValue="") {
        object x = this[key];
        if(x == null)
            return defaultValue;
        return x.ToString();
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（int版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public int Get(string key, int defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is int)
            return (int)x;
        return StringUtil.ToInt(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（uint版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public uint Get(string key, uint defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is uint)
            return (uint)x;
        return StringUtil.ToUInt(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（long版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public long Get(string key, long defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is long)
            return (long)x;
        return StringUtil.ToLong(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（ulong版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public ulong Get(string key, ulong defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is ulong)
            return (ulong)x;
        return StringUtil.ToULong(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（double版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public double Get(string key, double defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is double)
            return (double)x;
        return StringUtil.ToDouble(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（float版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public float Get(string key, float defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is float)
            return (float)x;
        return StringUtil.ToFloat(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対応する要素を取り出す。（bool版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public bool Get(string key, bool defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is bool)
            return (bool)x;
        return StringUtil.ToBool(x.ToString(), defaultValue);
    }

    /// <summary>
    ///   キーに対する要素を更新する
    /// </summary>
    /// <returns>true=更新した, false=もともとその値だった</returns>
    public bool Update(string key, object val) {
        object xval;
        bool ret = (!this.TryGetValue(key, out xval) || (xval.ToString() != val.ToString()));
        this[key] = val;
        return ret;
    }
    
    /// <summary>
    ///   "key=value"形式の定義を読んでObjectDictionaryを作る。
    /// </summary>
    public static ObjectDictionary FromString(string str) {
        return FromString(str, DefaultComparer);
    }
    /// <summary>
    ///   "key=value"形式の定義を読んでObjectDictionaryを作る。
    ///   キー並べ替え関数指定版。
    /// </summary>
    public static ObjectDictionary FromString(string str, IComparer<string> cmp) {
        ObjectDictionary dict = new ObjectDictionary(cmp);
        dict.Set(str);
        return dict;
    }

    /// <summary>
    ///   "key=value"形式の定義を取り込む
    /// </summary>
    public void Set(string str) {
        int count = 0;
        StringBuilder key = new StringBuilder();
        StringBuilder value = new StringBuilder();
        Status st = Status.BEFORE_KEY;
        foreach(char ch in str) {
            switch(st){
            case Status.BEFORE_KEY:
                if(ch != ' '){
                    key.Length = 0;
                    key.Append(ch);
                    st = Status.IN_KEY;
                }
                break;
            case Status.IN_KEY:
                if(ch == ' ')
                    st = Status.BEFORE_EQUAL;
                else if(ch == '=')
                    st = Status.BEFORE_VALUE;
                else
                    key.Append(ch);
                break;
            case Status.BEFORE_EQUAL:
                if(ch == '=')
                    st = Status.BEFORE_VALUE;
                else if(ch != ' '){
                    this[count.ToString()] = ToObject(key.ToString());
                    count++;
                    key.Length = 0;
                    key.Append(ch);
                    st = Status.IN_KEY;
                }
                break;
            case Status.BEFORE_VALUE:
                if(ch == '"'){
                    value.Length = 0;
                    st = Status.IN_QUOTEVALUE;
                }else if(ch != ' '){
                    value.Length = 0;
                    value.Append(ch);
                    st = Status.IN_VALUE;
                }
                break;
            case Status.IN_VALUE:
                if(ch == ' '){
                    this[key.ToString()] = ToObject(value.ToString());
                    st = Status.BEFORE_KEY;
                }else{
                    value.Append(ch);
                }
                break;
            case Status.IN_QUOTEVALUE:
                if(ch == '"'){
                    this[key.ToString()] = value.ToString();
                    st = Status.BEFORE_KEY;
                }else{
                    value.Append(ch);
                }
                break;
            }
        }
        switch(st){
        case Status.IN_KEY:
        case Status.BEFORE_EQUAL:
            this[count.ToString()] = ToObject(key.ToString());
            break;
        case Status.IN_VALUE:
            this[key.ToString()] = ToObject(value.ToString());
            break;
        case Status.IN_QUOTEVALUE:
            this[key.ToString()] = value.ToString();
            break;
        }
    }

    /// <summary>
    ///   オブジェクトのフィールドとその値からObjectDictionaryを作る。
    /// </summary>
    public static ObjectDictionary FromObjectFields(object obj) {
        return FromObjectFields(obj, DefaultComparer);
    }
    /// <summary>
    ///   オブジェクトのフィールドとその値からObjectDictionaryを作る。
    ///   キー並べ替え関数指定版。
    /// </summary>
    public static ObjectDictionary FromObjectFields(object obj, IComparer<string> cmp) {
        ObjectDictionary dict = new ObjectDictionary(cmp);
        if(obj == null)
            return dict;
        dict.SetObjectFields(obj);
        return dict;
    }

    /// <summary>
    ///   オブジェクトのフィールドとその値をセットする
    /// </summary>
    public void SetObjectFields(object obj, string prefix) {
        SetObjectFields(obj, null, prefix);
    }
    
    /// <summary>
    ///   オブジェクトのフィールドとその値をセットする
    /// </summary>
    public void SetObjectFields(object obj, string[] fieldList=null, string prefix="") {
        if(obj == null)
            return;
        List<string> flist = null;
        if(fieldList != null) {
            flist = new List<string>();
            foreach(string i in fieldList)
                flist.Add(i.ToLower());
        }
        foreach(FieldInfo fi in obj.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)) {
            if((flist != null) && !flist.Contains(fi.Name.ToLower()))
                continue;
            this[prefix+fi.Name] = fi.GetValue(obj);
        }
        foreach(PropertyInfo pi in obj.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)) {
            if((flist != null) && !flist.Contains(pi.Name.ToLower()))
                continue;
            try {
                this[prefix+pi.Name] = pi.GetValue(obj, null);
            } catch(ArgumentException) {
                // just ignore
            }
        }
    }

    /// <summary>
    ///   文字列をnull/int/bool/stringのいずれかのオブジェクトに変換する
    /// </summary>
    public static object ToObject(string str) {
        if(str == null)
            return null;
        if(str == "null")
            return null;
        if(str == "true")
            return true;
        if(str == "false")
            return false;
        foreach(char ch in str){
            if(!char.IsDigit(ch))
                return str;
        }
        return StringUtil.ToInt(str);
    }


    /// <summary>
    ///   辞書順のキーソート用コンパレータ
    /// </summary>
    public class DefaultComparerClass : IComparer<string> {
        public int Compare(string a, string b) {
            return a.CompareTo(b);
        }
    }
    public static readonly DefaultComparerClass DefaultComparer = new DefaultComparerClass();

    /// <summary>
    ///   辞書順のキーソート用コンパレータ（大文字小文字区別なし）
    /// </summary>
    public class IgnoreCaseComparerClass : IComparer<string> {
        public int Compare(string a, string b) {
            return String.Compare(a, b, true);
        }
    }
    public static readonly IgnoreCaseComparerClass IgnoreCaseComparer = new IgnoreCaseComparerClass();

    /// <summary>
    ///   文字長さ逆順のキーソート用コンパレータ
    /// </summary>
    public class KeyLengthComparerClass : IComparer<string> {
        public int Compare(string a, string b) {
            int d = b.Length-a.Length;
            if(d == 0)
                return a.CompareTo(b);
            return d;
        }
    }
    public static readonly KeyLengthComparerClass KeyLengthComparer = new KeyLengthComparerClass();


    /// <summary>
    ///   内容をファイルに保存する
    /// </summary>
    public static bool Save(StreamWriter sw, string instancename, object obj) {
        if(obj == null)
            return false;
        Type objtype = obj.GetType();
        sw.Write(objtype.Name);
        sw.Write(" ");
        sw.Write(instancename);
        sw.Write(" ");
        if(objtype == typeof(bool)){
            sw.Write((bool)obj ? "true" : "false");
        }else if(objtype == typeof(string)){
            sw.Write("\"");
            sw.Write(((string)obj).Replace("\n"," "));
            sw.Write("\"");
        }else if(objtype == typeof(ObjectDictionary)){
            sw.WriteLine(" {");
            foreach(KeyValuePair<string, object> kv in (ObjectDictionary)obj){
                Save(sw, kv.Key, kv.Value);
            }
            sw.Write("}");
        }else if(objtype == typeof(Dictionary<string,ObjectDictionary>)){
            sw.WriteLine(" {");
            foreach(KeyValuePair<string,ObjectDictionary> kv in (Dictionary<string,ObjectDictionary>)obj){
                Save(sw, kv.Key, kv.Value);
            }
            sw.Write("}");
        }else{
            sw.Write(obj.ToString().Replace("\n"," "));
        }
        sw.WriteLine();
        return true;
    }


#if false
    /// <summary>
    ///   内容をファイルから読み込む
    /// </summary>
    public static ObjectDictionary Load(StreamWriter sw) {
        sw.Write("ObjectDictionary ");
        sw.Write(instancename);
        sw.WriteLine(" {");
        foreach(KeyValuePair<string, object> kv in this){
            string typename = kv.Value.GetType().Name;
            switch(kv.Value.GetType().Name){
            case "int":
            case "Integer":
                sw.Write(typename);
                sw.Write(" ");
                sw.Write(kv.Key);
                sw.Write(" ");
                sw.Write(kv.Value.ToString());
                break;
            case "bool":
            case "Boolean":
                sw.Write(typename);
                sw.Write(" ");
                sw.Write(kv.Key);
                sw.Write(" ");
                sw.Write((bool)(kv.Value) ? "true" : "false");
                break;
            case "string":
            case "String":
                sw.Write(typename);
                sw.Write(" ");
                sw.Write(kv.Key);
                sw.Write(" \"");
                sw.Write(((string)kv.Value).Replace("\n"," "));
                sw.Write("\"");
                break;
            case "ObjectDictionary":
                ((ObjectDictionary)kv.Value).Save(sw, kv.Key);
                break;
            default:
                Console.WriteLine(string.Format("ObjectDictionary.Save: Unknown type {0} for instance {1}", typename, kv.Key));
                sw.Write("#");
                sw.Write(typename);
                sw.Write(" ");
                sw.Write(kv.Key);
                sw.Write(" \"");
                sw.Write(kv.Value.ToString().Replace("\n"," "));
                sw.Write("\"");
                break;
            }
        }
        sw.WriteLine("}");
        return true;
    }
#endif

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach(KeyValuePair<string,object> kv in this){
            if(sb.Length > 0)
                sb.Append(", ");
            sb.Append("\"");
            sb.Append(kv.Key);
            sb.Append("\":'");
            sb.Append(kv.Value.ToString());
            sb.Append("'");
        }
        return sb.ToString();
    }


    /// <summary>
    ///   本辞書のキーを変数名、値を変数値として、変数展開をする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     "{}"で囲まれた部分が変数展開されます。
    ///   </para>
    /// </remarks>
    public string ExtractVariables(string str, bool keepUnknownVariable=true, char escChar='\0') {
        if(String.IsNullOrEmpty(str))
            return "";
        StringBuilder ret = new StringBuilder();
        StringBuilder v = new StringBuilder();
        bool inEscape = false;
        bool inVar = false;
        foreach(char ch in str) {
            if(!inEscape && (ch == escChar)) {
                inEscape = true;
                continue;
            }
            if(inVar) {
                if(!inEscape && (ch == '}')) {
                    ret.Append(ExtractVariableOne(v.ToString(), keepUnknownVariable));
                    v.Clear();
                    inVar = false;
                } else {
                    v.Append(ch);
                }
            } else {
                if(!inEscape && (ch == '{')) {
                    inVar = true;
                } else {
                    ret.Append(ch);
                }
            }
        }
        if(inVar)
            ret.Append(v.ToString());
        return ret.ToString();
    }

    public string ExtractVariableOne(string str, bool keepUnknownVariable=true) {
        if(String.IsNullOrEmpty(str)) {
            return keepUnknownVariable?"{}":"";
        }
        string[] x = str.Trim().Split(":,".ToCharArray());
        // x[0]が変数名、x[1...]がフォーマットオプション
        string vname = x[0].Trim();
        object val;
        if(!TryGetValue(vname, out val)) {
            return keepUnknownVariable?("{"+str+"}"):"";
        }
        if((val == null) || (val.ToString() == ""))
            return "";
        int len = int.MaxValue;
        string ret = null;
        string unit = "";
        for(int i = 1; i < x.Length; i++) {
            string opt = x[i].Trim();
            if(opt == "")
                continue;
            switch(opt[0]) {
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case '-':
                // 総桁数指定
                len = StringUtil.ToInt(opt);
                break;
            case 'd':
            case 'D':
            case 'x':
            case 'X':
                // 整数フォーマット
                try {
                    ret = StringUtil.ToInt(val.ToString()).ToString(opt);
                } catch(FormatException) {
                    // just ignore
                }
                break;
            case 'f':
            case 'F':
            case 'n':
            case 'N':
            case 'c':
            case 'C':
                // 実数フォーマット
                try {
                    ret = StringUtil.ToDouble(val.ToString()).ToString(opt);
                } catch(FormatException) {
                    // just ignore
                }
                break;
            case '@':
                // 単位
                unit = StringUtil.Substring(opt, 1, 9999);
                break;
            }
        }
        if(ret == null)
            ret = val.ToString();
        ret += unit;
        if(len != int.MaxValue) {
            if(len >= 0) {
                ret = StringUtil.FixLength(ret, len);
            } else {
                ret = StringUtil.FixLengthRight(ret, len);
            }
        }
        return ret;
    }

    /// <summary>
    ///   テキスト内に含まれる変数名の一覧を返す
    /// </summary>
    public static string[] FetchVariables(string str, char escChar='\0') {
        List<string> vars = new List<string>();
        if(String.IsNullOrEmpty(str))
            return vars.ToArray();
        StringBuilder v = new StringBuilder();
        bool inEscape = false;
        bool inVar = false;
        foreach(char ch in str) {
            if(!inEscape && (ch == escChar)) {
                inEscape = true;
                continue;
            }
            if(inVar) {
                if(!inEscape && (ch == '}')) {
                    string vv = v.ToString().Split(":,".ToCharArray())[0].Trim();
                    if(!vars.Contains(vv))
                        vars.Add(vv);
                    v.Clear();
                    inVar = false;
                } else {
                    v.Append(ch);
                }
            } else {
                if(!inEscape && (ch == '{')) {
                    inVar = true;
                }
            }
        }
        return vars.ToArray();
    }



    protected enum Status {BEFORE_KEY, IN_KEY, BEFORE_EQUAL, BEFORE_VALUE, IN_VALUE, IN_QUOTEVALUE};

}

} // End of namespace
