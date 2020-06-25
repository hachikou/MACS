/// ObjectDictionary: string-object対応ディクショナリ.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    ///   Dictionary<string,string>からのコンストラクタ
    /// </summary>
    public ObjectDictionary(Dictionary<string,string> src) {
        if(src == null)
            return;
        foreach(KeyValuePair<string,string> kv in src) {
            this[kv.Key] = kv.Value;
        }
    }

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
    ///   キーに対応する要素を取り出す。（byte版）
    ///   キーが存在しない場合にはdefaultValueを返す。
    /// </summary>
    public byte Get(string key, byte defaultValue) {
        object x = this[key];
        if(x == null)
            return defaultValue;
        if(x is byte)
            return (byte)x;
        return (byte)StringUtil.ToInt(x.ToString(), (int)defaultValue);
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
    ///   キーが存在しないときに値をセットする
    /// </summary>
    public void XSet(string key, object val) {
        if(!base.ContainsKey(key))
            base[key] = val;
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

    /// <summary>
    ///   指定オブジェクトが真の値とみなされる場合はtrueを返す。
    /// </summary>
    public static bool IsTrue(object obj) {
        if(obj == null)
            return false;
        if((obj is string) && ((string)obj == ""))
            return false;
        if((obj is byte) && ((byte)obj == 0))
            return false;
        if((obj is int) && ((int)obj == 0))
            return false;
        if((obj is uint) && ((uint)obj == 0))
            return false;
        if((obj is long) && ((long)obj == 0))
            return false;
        if((obj is ulong) && ((ulong)obj == 0))
            return false;
        if((obj is double) && ((double)obj == 0))
            return false;
        if((obj is float) && ((float)obj == 0))
            return false;
        if(obj is bool)
            return (bool)obj;
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

    /// <summary>
    ///   オブジェクトメンバを抽出する
    /// </summary>
    public object GetObject(string fieldname) {
        return getObject(fieldname, null);
    }

    private static Regex pat_float = new Regex(@"^[+-]?\d+\.\d*$");
    private static Regex pat_int = new Regex(@"^[+-]?\d+$");
    private static Regex pat_hex = new Regex(@"^[+-]?0x([0-9a-fA-F]+)$");
    
    private object getObject(string fieldname, object baseobj) {
        if(String.IsNullOrEmpty(fieldname))
            return baseobj;
        Match m;
        // 定数のチェック
        if((m = pat_int.Match(fieldname)).Success) {
            return StringUtil.ToInt(fieldname);
        } else if((m = pat_float.Match(fieldname)).Success) {
            return StringUtil.ToDouble(fieldname);
        } else if((m = pat_hex.Match(fieldname)).Success) {
            return StringUtil.ToHexInt(m.Groups[1].Value);
        } else if(fieldname == "true") {
            return true;
        } else if(fieldname == "false") {
            return false;
        } else if((fieldname.Length > 1) && (((fieldname[0] == '"')&&(fieldname[fieldname.Length-1] == '"')) || ((fieldname[0] == '\'')&&(fieldname[fieldname.Length-1] == '\'')))) {
            return fieldname.Substring(1, fieldname.Length-2);
        }
        // .で分割し、最初のフィールド指定をチェック
        string varname, indexname;
        splitField(fieldname, out varname, out fieldname);
        splitIndex(varname, out varname, out indexname);
        object index = getObject(indexname, null);
        object obj = null;
        if(baseobj == null) {
            // baseobjがnullの場合、辞書を探す
            if(!this.ContainsKey(varname))
                return null;
            obj = this[varname];
        } else if(baseobj is DataArray) {
            // baseobjがDataArrayの場合、カラム名を探す
            DataArray da = baseobj as DataArray;
            int idx = da.ColumnNum(varname);
            if(idx >= 0)
                return da[idx];
        } else {
            // baseobjのメンバを探す
            foreach(FieldInfo fi in baseobj.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)) {
                if(fi.Name == varname) {
                    obj = fi.GetValue(baseobj);
                    break;
                }
            }
            if(obj == null) {
                foreach(PropertyInfo pi in baseobj.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)) {
                    if(pi.Name == varname) {
                        try {
                            obj = pi.GetValue(baseobj, null);
                        } catch(ArgumentException) {
                            obj = null;
                        }
                        break;
                    }
                }
            }
        }
        // 添字を展開
        if(index is int) {
            if(obj is Array) {
                try {
                    obj = (obj as Array).GetValue((int)index);
                } catch(ArgumentException) {
                    //LOG_ERR("{0} has invalid array type", varname);
                    return null;
                } catch(IndexOutOfRangeException) {
                    //LOG_ERR("index out of range for {0}", varname);
                    return null;
                }
            } else if(obj is IEnumerable<object>) {
                bool found = false;
                int i = 0;
                foreach(object o in (IEnumerable<object>)obj) {
                    if(i == (int)index) {
                        obj = o;
                        found = true;
                        break;
                    }
                    i++;
                }
                if(!found) {
                    //LOG_ERR("out of range for {0}", varname);
                    return null;
                }
            } else {
                //LOG_ERR("{0} is not an Array", varname);
                return null;
            }
        } else if(index is string) {
            if(obj is ObjectDictionary) {
                obj = (obj as ObjectDictionary).Get((string)index);
            } else if(obj is DataArray) {
                obj = (obj as DataArray).Get((string)index);
            }
        }
        if(obj == null)
            return null;
        // サブフィールド指定を展開して返す
        return getObject(fieldname, obj);
    }

    /// <summary>
    ///   文字列を'.'の前と後に分割する
    /// </summary>
    private static void splitField(string str, out string varname, out string fieldname) {
        if(String.IsNullOrEmpty(str)) {
            varname = "";
            fieldname = "";
        }
        bool inEscape = false;
        bool inQuote = false;
        bool inDQuote = false;
        int blockLevel = 0;
        int i = 0;
        while(i < str.Length) {
            char ch = str[i];
            if(inEscape) {
                inEscape = false;
            } else if(inQuote) {
                if(ch == '\'')
                    inQuote = false;
            } else if(inDQuote) {
                if(ch == '"')
                    inDQuote = false;
            } else if(ch == '\\') {
                inEscape = true;
            } else if(ch == '\'') {
                inQuote = true;
            } else if(ch == '"') {
                inDQuote = true;
            } else if(ch == '[') {
                blockLevel++;
            } else if(blockLevel > 0) {
                if(ch == ']')
                    blockLevel--;
            } else if(ch == '.') {
                varname = str.Substring(0,i);
                fieldname = str.Substring(i+1);
                return;
            }
            i++;
        }
        varname = str;
        fieldname = "";
    }

    /// <summary>
    ///   変数名[インデックス] の形の文字列を変数名とインデックスに分解する
    /// </summary>
    private static void splitIndex(string str, out string varname, out string index) {
        if(String.IsNullOrEmpty(str)) {
            varname = "";
            index = null;
            return;
        }
        int idx = str.IndexOf('[');
        if((idx >= 0) && (str[str.Length-1] == ']')) {
            varname = str.Substring(0, idx);
            index = str.Substring(idx+1, str.Length-idx-2);
        } else {
            varname = str;
            index = null;
        }
    }
    
    protected enum Status {BEFORE_KEY, IN_KEY, BEFORE_EQUAL, BEFORE_VALUE, IN_VALUE, IN_QUOTEVALUE};

}

#if SELFTEST

    public class TestClass {

        public int IntNumber;
        public double DoubleNumber;
        public string StringField;

        public int IntNumber2 {
            get { return IntNumber*2; }
        }

        public string[] StringList;
    
        public static int Main(string[] args) {
            ObjectDictionary dict = new ObjectDictionary();
            dict["abc"] = "hello";
            dict["def"] = 3;
            TestClass tc = new TestClass();
            tc.IntNumber = 2;
            tc.DoubleNumber = 3.14;
            tc.StringField = "a long time ago";
            tc.StringList = new string[]{"uno", "dos", "tres"};
            dict["testclass"] = tc;
            List<TestClass> list = new List<TestClass>();
            for(int i = 0; i < 10; i++) {
                tc = new TestClass();
                tc.IntNumber = i;
                tc.DoubleNumber = (double)i/3.0;
                tc.StringField = String.Format("Number#{0}", i);
                tc.StringList = new string[8];
                for(int j = 0; j < tc.StringList.Length; j++) {
                    tc.StringList[j] = String.Format("Number#{0}-{1}", i, j);
                }
                list.Add(tc);
            }
            dict["list"] = list;

            Console.WriteLine("abc={0}", dict.Get("abc"));
            Console.WriteLine("def={0}", dict["def"]);
            tc = (TestClass)dict["testclass"];
            Console.WriteLine("tc: {0}, {1}, {2}, {3}", tc.IntNumber, tc.DoubleNumber, tc.StringField, tc.IntNumber2);
            tc = (TestClass)dict.GetObject("testclass");
            Console.WriteLine("tc: {0}, {1}, {2}, {3}", tc.IntNumber, tc.DoubleNumber, tc.StringField, tc.IntNumber2);
            Console.WriteLine("testclass.StringList[1] = {0}", dict.GetObject("testclass.StringList[1]"));
            Console.WriteLine("list[3].StringList[5] = {0}", dict.GetObject("list[3].StringList[5]"));
            Console.WriteLine("list[def].StringList[5] = {0}", dict.GetObject("list[def].StringList[5]"));
            Console.WriteLine("list[testclass.IntNumber2].StringList[5] = {0}", dict.GetObject("list[testclass.IntNumber2].StringList[5]"));
            Console.WriteLine("list[list[6].IntNumber].StringList[5] = {0}", dict.GetObject("list[list[6].IntNumber].StringList[5]"));
            Console.WriteLine("'hogehoge' = {0}", dict.GetObject("'hogehoge'"));
            Console.WriteLine("3.14 = {0}", dict.GetObject("3.14"));
            return 0;
        }
    }
#endif
    
} // End of namespace
