/**
 * NDJson: Non-Dynamic JSON
 * $Id:$
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace MACS {


/// <summary>
///   dynamicを使わないJSON
/// </summary>
/// <remarks>
///   <para>
///     DynamicJsonを使うとコンパイル時の型チェックが弱くなって実行時の致命的
///     エラーが多発したため、腹が立ってこのクラスを自作しました。
///     本クラスはマルチスレッドセーフではありません。
///     使い方の例は、SELFTESTのregionをご覧ください。
///   </para>
/// </remarks>
public class NDJson {

#region 値の型

    /// <summary>
    ///   値の型
    /// </summary>
    public NDJsonType Type
    { get; private set; }

    /// <summary>
    ///   値がNULL型かどうか
    /// </summary>
    public bool IsNull {
        get { return Type==NDJsonType.NULL; }
    }

    /// <summary>
    ///   値がSTRING型かどうか
    /// </summary>
    public bool IsString {
        get { return Type==NDJsonType.STRING; }
    }

    /// <summary>
    ///   値がNUMBER型かどうか
    /// </summary>
    public bool IsNumber {
        get { return Type==NDJsonType.NUMBER; }
    }

    /// <summary>
    ///   値がBOOL型かどうか
    /// </summary>
    public bool IsBool {
        get { return Type==NDJsonType.BOOL; }
    }

    /// <summary>
    ///   値がOBJECT型かどうか
    /// </summary>
    public bool IsObject {
        get { return Type==NDJsonType.OBJECT; }
    }

    /// <summary>
    ///   値がARRAY型かどうか
    /// </summary>
    public bool IsArray {
        get { return Type==NDJsonType.ARRAY; }
    }

#endregion

#region NDJsonの生成

    /// <summary>
    ///   値が空のコンストラクタ
    /// </summary>
    public NDJson() {
        Type = NDJsonType.NULL;
    }

    /// <summary>
    ///   値が文字列のコンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字列をJSONパースするわけではないことに注意してください。
    ///   </para>
    /// </remarks>
    public NDJson(string val)
    :this() {
        Set(val);
    }

    /// <summary>
    ///   値がintのコンストラクタ
    /// </summary>
    public NDJson(int val)
    :this() {
        Set(val);
    }

    /// <summary>
    ///   値がdoubleのコンストラクタ
    /// </summary>
    public NDJson(double val)
    :this() {
        Set(val);
    }

    /// <summary>
    ///   値がboolのコンストラクタ
    /// </summary>
    public NDJson(bool val)
    :this() {
        Set(val);
    }

    /// <summary>
    ///   値がオブジェクトのコンストラクタ
    /// </summary>
    public NDJson(string key, object value, params object[] args)
    :this() {
        Set(key,value,args);
    }

    /// <summary>
    ///   値がオブジェクトのコンストラクタ
    /// </summary>
    public NDJson(string key, string value)
    :this() {
        Set(key,value);
    }

    /// <summary>
    ///   値がオブジェクトのコンストラクタ
    /// </summary>
    public NDJson(string key, int value)
    :this() {
        Set(key,value);
    }

    /// <summary>
    ///   値がオブジェクトのコンストラクタ
    /// </summary>
    public NDJson(string key, double value)
    :this() {
        Set(key,value);
    }

    /// <summary>
    ///   値が文字列の配列であるコンストラクタ
    /// </summary>
    public NDJson(string[] values)
    :this() {
        Set(values);
    }

    /// <summary>
    ///   値がintの配列であるコンストラクタ
    /// </summary>
    public NDJson(int[] values)
    :this() {
        Set(values);
    }

    /// <summary>
    ///   値がdoubleの配列であるコンストラクタ
    /// </summary>
    public NDJson(double[] values)
    :this() {
        Set(values);
    }

    /// <summary>
    ///   値がboolの配列であるコンストラクタ
    /// </summary>
    public NDJson(bool[] values)
    :this() {
        Set(values);
    }

    /// <summary>
    ///   値がNDJsonの配列であるコンストラクタ
    /// </summary>
    public NDJson(NDJson[] values)
    :this() {
        Set(values);
    }

    /// <summary>
    ///   値がobjectの配列であるコンストラクタ
    /// </summary>
    public NDJson(object[] values)
    :this() {
        Set(values);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     コピー元がOBJECT型、ARRAY型でNDJsonを内包している場合、内包するもの
    ///     はコピーされません。（shallow copy）
    ///     ディープコピーが必要な場合、DeepCopy()メソッドを使ってください。
    ///   </para>
    /// </remarks>
    public NDJson(NDJson val)
    :this() {
        CopyFrom(val);
    }

    /// <summary>
    ///   コピーを作成する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     コピー元がOBJECT型、ARRAY型でNDJsonを内包している場合、内包するもの
    ///     はコピーされません。（shallow copy）
    ///     ディープコピーが必要な場合、DeepCopy()メソッドを使ってください。
    ///   </para>
    /// </remarks>
    public static NDJson Copy(NDJson src) {
        NDJson json = new NDJson();
        json.CopyFrom(src);
        return json;
    }

    /// <summary>
    ///   ディープコピーを作成する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     コピー元がOBJECT型、ARRAY型でNDJsonを内包している場合、内包するもの
    ///     もコピーが作成されます。（deep copy）
    ///   </para>
    /// </remarks>
    public static NDJson DeepCopy(NDJson src) {
        NDJson json = new NDJson();
        json.DeepCopyFrom(src);
        return json;
    }

    /// <summary>
    ///   OBJECT型の空のJSONを作成する
    /// </summary>
    public static NDJson NewObject() {
        NDJson json = new NDJson();
        json.objectValue = new Dictionary<string,NDJson>();
        json.Type = NDJsonType.OBJECT;
        return json;
    }

    /// <summary>
    ///   ARRAY型の空のJSONを作成する
    /// </summary>
    public static NDJson NewArray() {
        NDJson json = new NDJson();
        json.arrayValue = new List<NDJson>();
        json.Type = NDJsonType.ARRAY;
        return json;
    }

#endregion

#region 値の読み書き

    /// <summary>
    ///   値をセットする。
    /// </summary>
    /// <param name="obj">セットする値</param>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     objがint型の時はSet(int), double型の時はSet(double), それ以外の時は
    ///     Set(string)が使われます。
    ///   </para>
    /// </remarks>
    public NDJson Set(object obj) {
        if(obj is int)
            return Set((int)obj);
        if(obj is double)
            return Set((double)obj);
        return Set(obj.ToString());
    }

    /// <summary>
    ///   値を文字列としてアクセスする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     getアクセスするとき、値がOBJECT型、ARRAY型の場合はJSON文字列を返します。
    ///     setアクセスすると、値の型はSTRING型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public string StringValue {
        get {
            switch(Type){
            case NDJsonType.NULL:
                return "";
            case NDJsonType.STRING:
                return stringValue;
            case NDJsonType.NUMBER:
                return numberValue.ToString();
            case NDJsonType.BOOL:
                return (numberValue!=0)?"true":"false";
            default:
                return ToString();
            }
        }
        set { Set(value); }
    }

    /// <summary>
    ///   文字列を値にする。
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     値の型はSTRING型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string value) {
        if((Type != NDJsonType.STRING) && (Type != NDJsonType.NULL))
            clear();
        if(value == null)
            stringValue = "";
        else
            stringValue = value;
        Type = NDJsonType.STRING;
        return this;
    }

    /// <summary>
    ///   値をintとしてアクセスする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     getアクセスするとき、値が整数に変換できない場合は0を返します。
    ///     setアクセスすると、値の型はNUMBER型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public int IntValue {
        get {
            switch(Type){
            case NDJsonType.NULL:
                return 0;
            case NDJsonType.STRING:
                return StringUtil.ToInt(stringValue);
            case NDJsonType.NUMBER:
                return (int)numberValue;
            case NDJsonType.BOOL:
                return (numberValue!=0)?1:0;
            default:
                return 0;
            }
        }
        set { Set(value); }
    }

    /// <summary>
    ///   intを値にする。
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     値の型はNUMBER型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int value) {
        if((Type != NDJsonType.NUMBER) && (Type != NDJsonType.NULL))
            clear();
        numberValue = (double)value;
        Type = NDJsonType.NUMBER;
        return this;
    }

    /// <summary>
    ///   値をdoubleとしてアクセスする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     getアクセスするとき、値が浮動小数点数に変換できない場合は0を返します。
    ///     setアクセスすると、値の型はNUMBER型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public double DoubleValue {
        get {
            switch(Type){
            case NDJsonType.NULL:
                return 0;
            case NDJsonType.STRING:
                return StringUtil.ToDouble(stringValue);
            case NDJsonType.NUMBER:
                return numberValue;
            case NDJsonType.BOOL:
                return (numberValue!=0)?1:0;
            default:
                return 0;
            }
        }
        set { Set(value); }
    }

    /// <summary>
    ///   doubleを値にする。
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     値の型はNUMBER型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(double value) {
        if((Type != NDJsonType.NUMBER) && (Type != NDJsonType.NULL))
            clear();
        numberValue = value;
        Type = NDJsonType.NUMBER;
        return this;
    }

    /// <summary>
    ///   値をboolとしてアクセスする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     getアクセスするとき、NUMBER型は0以外のときにtrue、STRING型は"false",
    ///     "no",空文字列以外の文字列の時にtrue、ARRAY型とOBJECT型はLengthが1以
    ///     上の時にtrueを返します。
    ///     setアクセスすると、値の型はBOOL型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public bool BoolValue {
        get {
            switch(Type){
            case NDJsonType.NULL:
                return false;
            case NDJsonType.STRING:
                return StringUtil.ToBool(stringValue);
            case NDJsonType.NUMBER:
                return (numberValue!=0);
            case NDJsonType.BOOL:
                return (numberValue!=0);
            case NDJsonType.ARRAY:
                return (arrayValue.Count>0);
            case NDJsonType.OBJECT:
                return (objectValue.Count>0);
            default:
                return false;
            }
        }
        set { Set(value); }
    }

    /// <summary>
    ///   boolを値にする。
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     値の型はBOOL型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(bool value) {
        if((Type != NDJsonType.BOOL) && (Type != NDJsonType.NULL))
            clear();
        numberValue = value?1.0:0;
        Type = NDJsonType.BOOL;
        return this;
    }

#endregion

#region OBJECT型

    /// <summary>
    ///   キーに対応する値を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定したキーが存在しない場合や値がOBJECT型でないときはダミーの空NDJsonを返します。
    ///   </para>
    /// </remarks>
    public NDJson Get(string key) {
        if(Type != NDJsonType.OBJECT)
            return new NDJson();
        NDJson v;
        if(objectValue.TryGetValue(key, out v))
            return v;
        return new NDJson();
    }

    /// <summary>
    ///   キーに対応する値を返す
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="defval">指定したキーが存在しない場合や値がOBJECT型でないときに返す値</param>
    public NDJson Get(string key, NDJson defval) {
        if(Type != NDJsonType.OBJECT)
            return defval;
        NDJson v;
        if(objectValue.TryGetValue(key, out v))
            return v;
        return defval;
    }

    /// <summary>
    ///   キーに対応する値を返す
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="defval">指定したキーが存在しない場合や値がOBJECT型でないときに返す値</param>
    public string Get(string key, string defval) {
        if(Type != NDJsonType.OBJECT)
            return defval;
        NDJson v;
        if(objectValue.TryGetValue(key, out v))
            return v.StringValue;
        return defval;
    }

    /// <summary>
    ///   キーに対応する値を返す
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="defval">指定したキーが存在しない場合や値がOBJECT型でないときに返す値</param>
    public int Get(string key, int defval) {
        if(Type != NDJsonType.OBJECT)
            return defval;
        NDJson v;
        if(objectValue.TryGetValue(key, out v))
            return v.IntValue;
        return defval;
    }

    /// <summary>
    ///   キーに対応する値を返す
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="defval">指定したキーが存在しない場合や値がOBJECT型でないときに返す値</param>
    public double Get(string key, double defval) {
        if(Type != NDJsonType.OBJECT)
            return defval;
        NDJson v;
        if(objectValue.TryGetValue(key, out v))
            return v.DoubleValue;
        return defval;
    }

    /// <summary>
    ///   キーに対応する値を返す
    /// </summary>
    /// <param name="key">キー</param>
    /// <param name="defval">指定したキーが存在しない場合や値がOBJECT型でないときに返す値</param>
    public bool Get(string key, bool defval) {
        if(Type != NDJsonType.OBJECT)
            return defval;
        NDJson v;
        if(objectValue.TryGetValue(key, out v))
            return v.BoolValue;
        return defval;
    }

    /// <summary>
    ///   キーに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     valがint型の時はSet(string,int), double型の時はSet(string,double),
    ///     NDJsonの時はSet(string,NDJson), それ以外の時は Set(string,string)が
    ///     使われます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string key, object val, params object[] args) {
        setOne(key,val);
        if(args.Length%2 != 0)
            throw new ArgumentException("Number of arguments for Set must be even.");
        for(int i = 0; i < args.Length; i += 2) {
            if(!(args[i] is string))
                throw new ArgumentException("Key for Set operation must be a string");
            setOne((string)args[i], args[i+1]);
        }
        return this;
    }


    /// <summary>
    ///   キーに対応する値をセットする
    /// </summary>
    /// <param name="key">キー名</param>
    /// <param name="val">値</param>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はOBJECT型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string key, NDJson val) {
        if(val == null)
            val = new NDJson();
        if((Type != NDJsonType.OBJECT) && (Type != NDJsonType.NULL))
            clear();
        if(objectValue == null)
            objectValue = new Dictionary<string,NDJson>();
        objectValue[key] = val;
        Type = NDJsonType.OBJECT;
        return this;
    }

    /// <summary>
    ///   キーに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はOBJECT型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string key, string val) {
        return Set(key, new NDJson(val));
    }

    /// <summary>
    ///   キーに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はOBJECT型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string key, int val) {
        return Set(key, new NDJson(val));
    }

    /// <summary>
    ///   キーに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はOBJECT型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string key, double val) {
        return Set(key, new NDJson(val));
    }

    /// <summary>
    ///   キーに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はOBJECT型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(string key, bool val) {
        return Set(key, new NDJson(val));
    }

    /// <summary>
    ///   インデクサ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     getアクセスの場合、Get(string key)が実行されます。
    ///     setアクセスの場合、Set(string key, NDJson val)が実行されます。
    ///   </para>
    /// </remarks>
    public NDJson this[string key] {
        get { return Get(key); }
        set { Set(key, value); }
    }

    /// <summary>
    ///   指定キーが存在するかどうかを返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     OBJECT型以外のときは常にfalseを返します。
    ///   </para>
    /// </remarks>
    public bool IsDefined(string key) {
        if((key == null) || (Type != NDJsonType.OBJECT))
            return false;
        return objectValue.ContainsKey(key);
    }

    /// <summary>
    ///   指定キーを削除する
    /// </summary>
    /// <returns>true=削除した, false=削除しなかった</returns>
    /// <remarks>
    ///   <para>
    ///     指定キーが存在しない場合や、OBJECT型ではない場合は、何もしません。
    ///   </para>
    /// </remarks>
    public bool Delete(string key) {
        if((key == null) || (Type != NDJsonType.OBJECT))
            return false;
        return objectValue.Remove(key);
    }

    /// <summary>
    ///   ディクショナリとして値を獲得する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     OBJECT型の時はキーと値の辞書を作成して返します。
    ///     ARRAY型の時はインデックスを文字列化したものがキーになります。
    ///     STRING型,NUMBER型,BOOL型の時は空文字列がキーとなる1要素の辞書を作成して返します。
    ///     NULL型の時は空辞書を返します。
    ///   </para>
    /// </remarks>
    public Dictionary<string,NDJson> GetDictionary() {
        Dictionary<string,NDJson> ret = new Dictionary<string,NDJson>();
        switch(Type){
        case NDJsonType.STRING:
        case NDJsonType.NUMBER:
        case NDJsonType.BOOL:
            ret[""] = this;
            break;
        case NDJsonType.OBJECT:
            foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                ret[kv.Key] = kv.Value;
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                ret[i.ToString()] = arrayValue[i];
            }
            break;
        }
        return ret;
    }

    /// <summary>
    ///   イテレータ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     OBJECT型の時はKeyValuePair<string,NDJson>のイテレータを返します。
    ///     ARRAY型の時はインデックスを文字列化したものがキーになります。
    ///     STRING型,NUMBER型,BOOL型の時は空文字列がキーとなる1要素だけのイテレータを作成して返します。
    ///     NULL型の時は空イテレータを返します。
    ///   </para>
    /// </remarks>
    public IEnumerator<KeyValuePair<string,NDJson>> GetEnumerator() {
        switch(Type){
        case NDJsonType.STRING:
        case NDJsonType.NUMBER:
        case NDJsonType.BOOL:
            for(int i = 0; i < 1; i++)
                yield return new KeyValuePair<string,NDJson>("",this);
            break;
        case NDJsonType.OBJECT:
            foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                yield return kv;
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                yield return new KeyValuePair<string,NDJson>(i.ToString(),arrayValue[i]);
            }
            break;
        }
    }

#endregion

#region ARRAY型

    /// <summary>
    ///   インデックスに対応する値を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を返します。）
    ///     指定したインデックスが存在しない場合や値がARRAY型でないときはダミーの空NDJsonを返します。
    ///   </para>
    /// </remarks>
    public NDJson Get(int index) {
        if(Type != NDJsonType.ARRAY)
            return new NDJson();
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return new NDJson();
        return arrayValue[index];
    }

    /// <summary>
    ///   インデックスに対応する値を返す
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <param name="defval">指定したインデックスが存在しない場合や値がARRAY型でないときに返す値</param>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を返します。）
    ///   </para>
    /// </remarks>
    public NDJson Get(int index, NDJson defval) {
        if(Type != NDJsonType.ARRAY)
            return defval;
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return defval;
        return arrayValue[index];
    }

    /// <summary>
    ///   インデックスに対応する値を返す
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <param name="defval">指定したインデックスが存在しない場合や値がARRAY型でないときに返す値</param>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を返します。）
    ///   </para>
    /// </remarks>
    public string Get(int index, string defval) {
        if(Type != NDJsonType.ARRAY)
            return defval;
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return defval;
        return arrayValue[index].StringValue;
    }

    /// <summary>
    ///   インデックスに対応する値を返す
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <param name="defval">指定したインデックスが存在しない場合や値がARRAY型でないときに返す値</param>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を返します。）
    ///   </para>
    /// </remarks>
    public int Get(int index, int defval) {
        if(Type != NDJsonType.ARRAY)
            return defval;
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return defval;
        return arrayValue[index].IntValue;
    }

    /// <summary>
    ///   インデックスに対応する値を返す
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <param name="defval">指定したインデックスが存在しない場合や値がARRAY型でないときに返す値</param>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を返します。）
    ///   </para>
    /// </remarks>
    public double Get(int index, double defval) {
        if(Type != NDJsonType.ARRAY)
            return defval;
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return defval;
        return arrayValue[index].DoubleValue;
    }

    /// <summary>
    ///   インデックスに対応する値を返す
    /// </summary>
    /// <param name="index">インデックス</param>
    /// <param name="defval">指定したインデックスが存在しない場合や値がARRAY型でないときに返す値</param>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を返します。）
    ///   </para>
    /// </remarks>
    public bool Get(int index, bool defval) {
        if(Type != NDJsonType.ARRAY)
            return defval;
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return defval;
        return arrayValue[index].BoolValue;
    }

    /// <summary>
    ///   インデックスに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     valがint型の時はSet(int,int), double型の時はSet(int,double), NDJson
    ///     の時はSet(int,NDJson), それ以外の時は Set(int,string)が使われます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int index, object val, params object[] args) {
        setOne(index, val);
        if(args.Length%2 != 0)
            throw new ArgumentException("Number of arguments for Set must be even.");
        for(int i = 0; i < args.Length; i += 2) {
            if(!(args[i] is int))
                throw new ArgumentException("Index for Set operation must be a int");
            setOne((int)args[i],args[i+1]);
        }
        return this;
    }

    /// <summary>
    ///   インデックスに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を変更します。）
    ///     指定インデックスが存在しない場合、指定インデックスまで空のNDJson
    ///     オブジェクトが自動的に挿入されます。
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int index, NDJson val) {
        if(val == null)
            val = new NDJson();
        if((Type != NDJsonType.ARRAY) && (Type != NDJsonType.NULL))
            clear();
        if(arrayValue == null)
            arrayValue = new List<NDJson>();
        if(index < 0) {
            index = arrayValue.Count+index;
            if(index < 0)
                index = 0;
        }
        while(index >= arrayValue.Count) {
            arrayValue.Add(new NDJson());
        }
        arrayValue[index] = val;
        Type = NDJsonType.ARRAY;
        return this;
    }

    /// <summary>
    ///   インデックスに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を変更します。）
    ///     指定インデックスが存在しない場合、指定インデックスまで空のNDJson
    ///     オブジェクトが自動的に挿入されます。
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int index, string val) {
        return Set(index, new NDJson(val));
    }

    /// <summary>
    ///   インデックスに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を変更します。）
    ///     指定インデックスが存在しない場合、指定インデックスまで空のNDJson
    ///     オブジェクトが自動的に挿入されます。
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int index, int val) {
        return Set(index, new NDJson(val));
    }

    /// <summary>
    ///   インデックスに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を変更します。）
    ///     指定インデックスが存在しない場合、指定インデックスまで空のNDJson
    ///     オブジェクトが自動的に挿入されます。
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int index, double val) {
        return Set(index, new NDJson(val));
    }

    /// <summary>
    ///   インデックスに対応する値をセットする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     indexが負の時は末尾からの指定になります。（Get(-1)は末尾の要素を変更します。）
    ///     指定インデックスが存在しない場合、指定インデックスまで空のNDJson
    ///     オブジェクトが自動的に挿入されます。
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Set(int index, bool val) {
        return Set(index, new NDJson(val));
    }

    /// <summary>
    ///   インデクサ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     getアクセスの場合、Get(int index)が実行されます。
    ///     setアクセスの場合、Set(int index, NDJson val)が実行されます。
    ///   </para>
    /// </remarks>
    public NDJson this[int index] {
        get { return Get(index); }
        set { Set(index, value); }
    }

    /// <summary>
    ///   配列に値を追加する
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Add(object val) {
        if(Type == NDJsonType.ARRAY)
            return Set(arrayValue.Count,val);
        return Set(0,val);
    }

    /// <summary>
    ///   配列に値を追加する
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Add(NDJson val) {
        if(Type == NDJsonType.ARRAY)
            return Set(arrayValue.Count,val);
        return Set(0,val);
    }

    /// <summary>
    ///   配列に値を追加する
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Add(string val) {
        if(Type == NDJsonType.ARRAY)
            return Set(arrayValue.Count,val);
        return Set(0,val);
    }

    /// <summary>
    ///   配列に値を追加する
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Add(int val) {
        if(Type == NDJsonType.ARRAY)
            return Set(arrayValue.Count,val);
        return Set(0,val);
    }

    /// <summary>
    ///   配列に値を追加する
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Add(double val) {
        if(Type == NDJsonType.ARRAY)
            return Set(arrayValue.Count,val);
        return Set(0,val);
    }

    /// <summary>
    ///   配列に値を追加する
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     本NDJsonの値の型はARRAY型になり、他の型の値は消えます。
    ///   </para>
    /// </remarks>
    public NDJson Add(bool val) {
        if(Type == NDJsonType.ARRAY)
            return Set(arrayValue.Count,val);
        return Set(0,val);
    }

    /// <summary>
    ///   指定インデックスを削除する
    /// </summary>
    /// <returns>true=削除した, false=削除しなかった</returns>
    /// <remarks>
    ///   <para>
    ///     指定インデックスが存在しない場合や、ARRAY型ではない場合は、何もしません。
    ///     インデックスに負の値を指定すると、末尾から数えたインデックス位置が
    ///     指定されたものとみなします。（-1は最後の要素を、-2は最後から2つめの
    ///     要素を削除します。）
    ///   </para>
    /// </remarks>
    public bool Delete(int index) {
        if(Type != NDJsonType.ARRAY)
            return false;
        if(index < 0)
            index = arrayValue.Count+index;
        if((index < 0) || (index >= arrayValue.Count))
            return false;
        arrayValue.RemoveAt(index);
        return true;
    }

    /// <summary>
    ///   文字列配列をいっぺんにセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Set(string[] values) {
        clear();
        if(values == null)
            return this;
        Type = NDJsonType.ARRAY;
        arrayValue = new List<NDJson>();
        foreach(string x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   文字列配列をいっぺんに追加する
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Add(string[] values) {
        if(values == null)
            return this;
        foreach(string x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   int配列をいっぺんにセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Set(int[] values) {
        clear();
        if(values == null)
            return this;
        Type = NDJsonType.ARRAY;
        arrayValue = new List<NDJson>();
        foreach(int x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   int配列をいっぺんに追加する
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Add(int[] values) {
        if(values == null)
            return this;
        foreach(int x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   double配列をいっぺんにセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Set(double[] values) {
        clear();
        if(values == null)
            return this;
        Type = NDJsonType.ARRAY;
        arrayValue = new List<NDJson>();
        foreach(double x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   double配列をいっぺんに追加する
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Add(double[] values) {
        if(values == null)
            return this;
        foreach(double x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   bool配列をいっぺんにセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Set(bool[] values) {
        clear();
        if(values == null)
            return this;
        Type = NDJsonType.ARRAY;
        arrayValue = new List<NDJson>();
        foreach(bool x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   bool配列をいっぺんに追加する
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Add(bool[] values) {
        if(values == null)
            return this;
        foreach(bool x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   NDJson配列をいっぺんにセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Set(NDJson[] values) {
        clear();
        if(values == null)
            return this;
        Type = NDJsonType.ARRAY;
        arrayValue = new List<NDJson>();
        foreach(NDJson x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   NDJson配列をいっぺんに追加する
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Add(NDJson[] values) {
        if(values == null)
            return this;
        foreach(NDJson x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   object配列をいっぺんにセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Set(object[] values) {
        clear();
        if(values == null)
            return this;
        Type = NDJsonType.ARRAY;
        arrayValue = new List<NDJson>();
        foreach(object x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   object配列をいっぺんに追加する
    /// </summary>
    /// <returns>自分自身</returns>
    public NDJson Add(object[] values) {
        if(values == null)
            return this;
        foreach(object x in values) {
            Add(x);
        }
        return this;
    }

    /// <summary>
    ///   値をNDJsonの配列として取り出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ARRAY型の時は各要素を配列化して返します。
    ///     OBJECT型の時は各要素の値を配列化して返します。
    ///     STRING型,NUMBER型,BOOL型の時は要素数1の配列を返します。
    ///     NULL型の時は空配列を返します。
    ///   </para>
    /// </remarks>
    public NDJson[] GetArray() {
        NDJson[] ret = new NDJson[Length];
        switch(Type) {
        case NDJsonType.STRING:
        case NDJsonType.NUMBER:
        case NDJsonType.BOOL:
            ret[0] = this;
            break;
        case NDJsonType.OBJECT:
            {
                int i = 0;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    ret[i++] = kv.Value;
                }
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                ret[i] = arrayValue[i];
            }
            break;
        }
        return ret;
    }

    /// <summary>
    ///   値を文字列の配列として取り出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ARRAY型の時は各要素の値を文字列化した配列を返します。
    ///     OBJECT型の時は各要素のキー名を配列化して返します。
    ///     STRING型,NUMBER型,BOOL型の時は要素数1の配列を返します。
    ///     NULL型の時は空配列を返します。
    ///   </para>
    /// </remarks>
    public string[] GetStringArray() {
        string[] ret = new string[Length];
        switch(Type) {
        case NDJsonType.STRING:
            ret[0] = stringValue;
            break;
        case NDJsonType.NUMBER:
            ret[0] = numberValue.ToString();
            break;
        case NDJsonType.BOOL:
            ret[0] = (numberValue!=0)?"true":"false";
            break;
        case NDJsonType.OBJECT:
            {
                int i = 0;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    ret[i++] = kv.Key;
                }
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                ret[i] = arrayValue[i].StringValue;
            }
            break;
        }
        return ret;
    }

    /// <summary>
    ///   値をintの配列として取り出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ARRAY型の時は各要素の値を整数化した配列を返します。
    ///     OBJECT型の時は各要素の値を整数化したものを配列化して返します。
    ///     STRING型,NUMBER型,BOOL型の時は要素数1の配列を返します。
    ///     NULL型の時は空配列を返します。
    ///   </para>
    /// </remarks>
    public int[] GetIntArray() {
        int[] ret = new int[Length];
        switch(Type) {
        case NDJsonType.STRING:
            ret[0] = StringUtil.ToInt(stringValue);
            break;
        case NDJsonType.NUMBER:
            ret[0] = (int)numberValue;
            break;
        case NDJsonType.BOOL:
            ret[0] = (numberValue!=0)?1:0;
            break;
        case NDJsonType.OBJECT:
            {
                int i = 0;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    ret[i++] = kv.Value.IntValue;
                }
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                ret[i] = arrayValue[i].IntValue;
            }
            break;
        }
        return ret;
    }

    /// <summary>
    ///   値をdoubleの配列として取り出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ARRAY型の時は各要素の値をdouble化した配列を返します。
    ///     OBJECT型の時は各要素の値をdouble化したものを配列化して返します。
    ///     STRING型,NUMBER型,BOOL型の時は要素数1の配列を返します。
    ///     NULL型の時は空配列を返します。
    ///   </para>
    /// </remarks>
    public double[] GetDoubleArray() {
        double[] ret = new double[Length];
        switch(Type) {
        case NDJsonType.STRING:
            ret[0] = StringUtil.ToDouble(stringValue);
            break;
        case NDJsonType.NUMBER:
            ret[0] = numberValue;
            break;
        case NDJsonType.BOOL:
            ret[0] = (numberValue!=0)?1.0:0;
            break;
        case NDJsonType.OBJECT:
            {
                int i = 0;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    ret[i++] = kv.Value.DoubleValue;
                }
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                ret[i] = arrayValue[i].DoubleValue;
            }
            break;
        }
        return ret;
    }

    /// <summary>
    ///   値をboolの配列として取り出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ARRAY型の時は各要素のBoolValueを配列化して返します。
    ///     OBJECT型の時は各要素のBoolValueを配列化して返します。
    ///     STRING型,NUMBER型,BOOL型の時は要素数1の配列を返します。
    ///     NULL型の時は空配列を返します。
    ///   </para>
    /// </remarks>
    public bool[] GetBoolArray() {
        bool[] ret = new bool[Length];
        switch(Type) {
        case NDJsonType.STRING:
            ret[0] = StringUtil.ToBool(stringValue);
            break;
        case NDJsonType.NUMBER:
        case NDJsonType.BOOL:
            ret[0] = (numberValue!=0);
            break;
        case NDJsonType.OBJECT:
            {
                int i = 0;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    ret[i++] = kv.Value.BoolValue;
                }
            }
            break;
        case NDJsonType.ARRAY:
            for(int i = 0; i < arrayValue.Count; i++) {
                ret[i] = arrayValue[i].BoolValue;
            }
            break;
        }
        return ret;
    }

    /// <summary>
    ///   要素数を返す
    /// </summary>
    /// <returns>要素数</returns>
    public int Length {
        get {
            switch(Type) {
            case NDJsonType.STRING:
            case NDJsonType.NUMBER:
            case NDJsonType.BOOL:
                return 1;
            case NDJsonType.OBJECT:
                return objectValue.Count;
            case NDJsonType.ARRAY:
                return arrayValue.Count;
            default:
                return 0;
            }
        }
    }

    /// <summary>
    ///   要素数を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Lengthと全く同じ。
    ///   </para>
    /// </remarks>
    /// <returns>要素数</returns>
    public int Count {
        get { return Length; }
    }

#endregion

#region コピー

    /// <summary>
    ///   値をコピーする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     コピー元がOBJECT型、ARRAY型でNDJsonを内包している場合、内包するもの
    ///     はコピーされません。（shallow copy）
    ///     ディープコピーが必要な場合、DeepCopyFrom()メソッドを使ってください。
    ///   </para>
    /// </remarks>
    public NDJson CopyFrom(NDJson src) {
        clear();
        if(src == null) {
            Type = NDJsonType.NULL;
            return this;
        }
        Type = src.Type;
        switch(Type) {
        case NDJsonType.NULL:
            // nothing to do.
            break;
        case NDJsonType.STRING:
            stringValue = src.stringValue;
            break;
        case NDJsonType.NUMBER:
        case NDJsonType.BOOL:
            numberValue = src.numberValue;
            break;
        case NDJsonType.OBJECT:
            objectValue = new Dictionary<string,NDJson>();
            foreach(KeyValuePair<string,NDJson> kv in src.objectValue) {
                objectValue.Add(kv.Key,kv.Value);
            }
            break;
        case NDJsonType.ARRAY:
            arrayValue = new List<NDJson>();
            foreach(NDJson v in src.arrayValue) {
                arrayValue.Add(v);
            }
            break;
        }
        return this;
    }

    /// <summary>
    ///   値をディープコピーする
    /// </summary>
    /// <returns>自分自身</returns>
    /// <remarks>
    ///   <para>
    ///     コピー元がOBJECT型、ARRAY型でNDJsonを内包している場合、内包するもの
    ///     もコピーが作成されます。（deep copy）
    ///   </para>
    /// </remarks>
    public NDJson DeepCopyFrom(NDJson src) {
        clear();
        if(src == null) {
            Type = NDJsonType.NULL;
            return this;
        }
        Type = src.Type;
        switch(Type) {
        case NDJsonType.NULL:
            // nothing to do.
            break;
        case NDJsonType.STRING:
            stringValue = src.stringValue;
            break;
        case NDJsonType.NUMBER:
        case NDJsonType.BOOL:
            numberValue = src.numberValue;
            break;
        case NDJsonType.OBJECT:
            objectValue = new Dictionary<string,NDJson>();
            foreach(KeyValuePair<string,NDJson> kv in src.objectValue) {
                objectValue.Add(kv.Key,DeepCopy(kv.Value));
            }
            break;
        case NDJsonType.ARRAY:
            arrayValue = new List<NDJson>();
            foreach(NDJson v in src.arrayValue) {
                arrayValue.Add(DeepCopy(v));
            }
            break;
        }
        return this;
    }

#endregion

#region その他

    /// <summary>
    ///   値を空の状態にする
    /// </summary>
    public NDJson Clear() {
        clear();
        Type = NDJsonType.NULL;
        return this;
    }

    /// <summary>
    ///   JSON文字列にする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字列は"で囲まれ、エスケープ処理もされます。
    ///     単に値の文字列を得たい場合は、StringValueを使ってください。
    ///   </para>
    /// </remarks>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        toString(sb);
        return sb.ToString();
    }

    /// <summary>
    ///   JSON文字列を出力する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字列は"で囲まれ、エスケープ処理もされます。
    ///   </para>
    /// </remarks>
    public void WriteTo(TextWriter writer) {
        writeTo(writer);
    }

    /// <summary>
    ///   JSONで使えない文字をエスケープした文字列を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Unicode文字のエスケープは行なわれません。
    ///   </para>
    /// </remarks>
    public static string Escape(string txt) {
        StringBuilder sb = new StringBuilder();
        foreach(char ch in txt) {
            switch(ch) {
            case '\"':
                sb.Append("\\\"");
                break;
            case '\\':
                sb.Append("\\\\");
                break;
            case '/':
                sb.Append("\\/");
                break;
            case '\b':
                sb.Append("\\b");
                break;
            case '\n':
                sb.Append("\\n");
                break;
            case '\r':
                sb.Append("\\r");
                break;
            case '\t':
                sb.Append("\\t");
                break;
            default:
                sb.Append(ch);
                break;
            }
        }
        return sb.ToString();
    }

#endregion

#region パーサ

    /// <summary>
    ///   JSON文字列をパースして新しいNDJsonを作る
    /// </summary>
    /// <param name="txt">パースする文字列</param>
    /// <remarks>
    ///   <para>
    ///     JSONパース後に余分な文字があってもそれは無視されます。
    ///     JSONフォーマット違反がある場合は ArgumentExceptionが発生します。
    ///   </para>
    /// </remarks>
    public static NDJson Parse(string txt) {
        if(txt == null)
            txt = "";
        NDJson json = new NDJson();
        json.StartParse();
        foreach(char ch in txt) {
            if(json.ParseChar(ch))
                break;
        }
        json.FinishParse();
        return json;
    }

    /// <summary>
    ///   JSON文字列をパースして新しいNDJsonを作る
    /// </summary>
    /// <param name="txt">パースする文字列</param>
    /// <param name="rest">パース後に残った文字列</param>
    /// <remarks>
    ///   <para>
    ///     JSONフォーマット違反がある場合は ArgumentExceptionが発生します。
    ///   </para>
    /// </remarks>
    public static NDJson Parse(string txt, out string rest) {
        if(txt == null)
            txt = "";
        NDJson json = new NDJson();
        json.StartParse();
        int c = 0;
        foreach(char ch in txt) {
            c++;
            if(json.ParseChar(ch))
                break;
        }
        rest = txt.Substring(c);
        json.FinishParse();
        return json;
    }

    /// <summary>
    ///   JSON文字列をパースして新しいNDJsonを作る
    /// </summary>
    /// <param name="textReader">パースする文字列を供給するTextReader</param>
    /// <remarks>
    ///   <para>
    ///     JSONフォーマット違反がある場合は ArgumentExceptionが発生します。
    ///   </para>
    /// </remarks>
    public static NDJson Parse(TextReader textReader) {
        NDJson json = new NDJson();
        json.StartParse();
        while(true) {
            int ch = textReader.Read();
            if(ch < 0)
                break;
            if(json.ParseChar((char)ch))
                break;
        }
        json.FinishParse();
        return json;
    }

    /// <summary>
    ///   JSON文字列のパースを開始する
    /// </summary>
    public void StartParse() {
        clear();
        parseContext = new ParseContext();
    }

    /// <summary>
    ///   JSON文字列のパースを終了する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     パースが完了していない状態で本メソッドが呼ばれると、ArgumentException
    ///     が発生します。
    ///   </para>
    /// </remarks>
    public void FinishParse() {
        if(parseContext == null)
            throw new ApplicationException("FinishParse is called before StartParse");
        try {
            switch(parseContext.status) {
            case ParseStatus.Finished:
                return;
            case ParseStatus.InString:
                throw new ArgumentException("NDJson: String end is not found.");
            case ParseStatus.InNull:
            case ParseStatus.InNumber:
            case ParseStatus.InBool:
                // 読み取りを確定させるためにダミーの1文字をパースする
                ParseChar(' ');
                return;
            case ParseStatus.InObject:
            case ParseStatus.InObjectKey:
            case ParseStatus.InObjectValue:
                throw new ArgumentException("NDJson: Parsing object is not completed.");
            case ParseStatus.InArray:
                throw new ArgumentException("NDJson: Parsing array is not completed.");
            default:
                throw new ArgumentException("NDJson: Failed to parse ({0}).", parseContext.status.ToString());
            }
        } finally {
            parseContext = null;
        }
    }

    /// <summary>
    ///   1文字をパーサに供給する
    /// </summary>
    /// <param name="ch">パースさせる文字</param>
    /// <returns>true=パース完了、false=パース未完了
    /// <remarks>
    ///   <para>
    ///     本メソッドはStartParse呼び出し後に呼び出すこと。
    ///   </para>
    /// </remarks>
    public bool ParseChar(char ch) {
        if(parseContext == null)
            throw new ApplicationException("ParseChar is called before StartParse");
        bool dummy;
        return parseChar(ch, out dummy);
    }

#endregion

#region private部

    private string stringValue;
    private double numberValue;
    private Dictionary<string,NDJson> objectValue;
    private List<NDJson> arrayValue;

    private void clear() {
        if(stringValue != null)
            stringValue = null;
        if(objectValue != null){
            objectValue.Clear();
            objectValue = null;
        }
        if(arrayValue != null) {
            arrayValue.Clear();
            arrayValue = null;
        }
        Type = NDJsonType.NULL;
    }

    private void setOne(string key, object val) {
        if(val is int)
            Set(key,(int)val);
        else if(val is double)
            Set(key,(double)val);
        else if(val is bool)
            Set(key,(bool)val);
        else if(val is NDJson)
            Set(key,(NDJson)val);
        else if(val is string[])
            Set(key, new NDJson(val as string[]));
        else if(val is int[])
            Set(key, new NDJson(val as int[]));
        else if(val is double[])
            Set(key, new NDJson(val as double[]));
        else
            Set(key,val.ToString());
    }

    private void setOne(int index, object val) {
        if(val is int)
            Set(index,(int)val);
        else if(val is double)
            Set(index,(double)val);
        else if(val is bool)
            Set(index,(bool)val);
        else if(val is NDJson)
            Set(index,(NDJson)val);
        else if(val is string[])
            Set(index, new NDJson(val as string[]));
        else if(val is int[])
            Set(index, new NDJson(val as int[]));
        else if(val is double[])
            Set(index, new NDJson(val as double[]));
        else
            Set(index,val.ToString());
    }

    private void toString(StringBuilder sb) {
        switch(Type) {
        case NDJsonType.NULL:
            sb.Append("null");
            break;
        case NDJsonType.STRING:
            sb.Append('\"');
            sb.Append(Escape(stringValue));
            sb.Append('\"');
            break;
        case NDJsonType.NUMBER:
            sb.Append(numberValue.ToString());
            break;
        case NDJsonType.BOOL:
            sb.Append((numberValue!=0)?"true":"false");
            break;
        case NDJsonType.OBJECT:
            {
                sb.Append('{');
                bool first = true;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    if(first)
                        first = false;
                    else
                        sb.Append(',');
                    sb.Append('\"');
                    sb.Append(kv.Key);
                    sb.Append("\":");
                    kv.Value.toString(sb);
                }
                sb.Append('}');
            }
            break;
        case NDJsonType.ARRAY:
            {
                sb.Append('[');
                bool first = true;
                foreach(NDJson v in arrayValue) {
                    if(first)
                        first = false;
                    else
                        sb.Append(',');
                    v.toString(sb);
                }
                sb.Append(']');
            }
            break;
        }
    }

    private void writeTo(TextWriter writer) {
        switch(Type) {
        case NDJsonType.NULL:
            writer.Write("null");
            break;
        case NDJsonType.STRING:
            writer.Write('\"');
            writer.Write(Escape(stringValue));
            writer.Write('\"');
            break;
        case NDJsonType.NUMBER:
            writer.Write(numberValue.ToString());
            break;
        case NDJsonType.BOOL:
            writer.Write((numberValue!=0)?"true":"false");
            break;
        case NDJsonType.OBJECT:
            {
                writer.Write('{');
                bool first = true;
                foreach(KeyValuePair<string,NDJson> kv in objectValue) {
                    if(first)
                        first = false;
                    else
                        writer.Write(',');
                    writer.Write('\"');
                    writer.Write(kv.Key);
                    writer.Write("\":");
                    kv.Value.writeTo(writer);
                }
                writer.Write('}');
            }
            break;
        case NDJsonType.ARRAY:
            {
                writer.Write('[');
                bool first = true;
                foreach(NDJson v in arrayValue) {
                    if(first)
                        first = false;
                    else
                        writer.Write(',');
                    v.writeTo(writer);
                }
                writer.Write(']');
            }
            break;
        }
    }

    /// <summary>
    ///   JSON文字列パースの状態
    /// </summary>
    private enum ParseStatus {
        Finished,  // パース完了（パース前も含む）
        InNull,    // null読み取り中
        InString,  // 文字列読み取り中
        InNumber,  // 数値読み取り中
        InBool,    // 真偽値読み取り中
        InObject,  // オブジェクト読み取り中
        InObjectKey, // オプジェクトのキー文字列読み取り中
        InObjectValue, // オブジェクトの値読み取り中
        InArray,   // 配列読み取り中
    }

    /// <summary>
    ///   パース中の状態保持用クラス
    /// </summary>
    private class ParseContext {
        public ParseStatus status = ParseStatus.Finished;
        public StringBuilder text = new StringBuilder();
        public bool textEscape = false;
        public NDJson subJson = null;
    }

    private ParseContext parseContext = null;

    private static readonly char[] numberChars = "0123456789+-.".ToCharArray();

    private static bool isNumberChar(char ch) {
        foreach(char i in numberChars)
            if(ch == i)
                return true;
        return false;
    }

    private static readonly char[] spaceChars = " \t\r\n\f".ToCharArray();

    private static bool isSpaceChar(char ch) {
        foreach(char i in spaceChars)
            if(ch == i)
                return true;
        return false;
    }

    private bool parseChar(char ch, out bool consumed) {
        bool completed = false;
        consumed = false;
        switch(parseContext.status) {
        case ParseStatus.Finished:
            // パース前
            if(ch == 'n') {
                // nullの開始
                parseContext.text.Clear();
                parseContext.text.Append(ch);
                parseContext.status = ParseStatus.InNull;
                consumed = true;
            } else if(ch == '"') {
                // 文字列の開始
                parseContext.status = ParseStatus.InString;
                parseContext.text.Clear();
                parseContext.textEscape = false;
                consumed = true;
            } else if(isNumberChar(ch)) {
                // 数値の開始
                parseContext.text.Clear();
                parseContext.text.Append(ch);
                parseContext.status = ParseStatus.InNumber;
                consumed = true;
            } else if((ch == 't')||(ch == 'f')) {
                // true/falseの開始
                parseContext.text.Clear();
                parseContext.text.Append(ch);
                parseContext.status = ParseStatus.InBool;
                consumed = true;
            } else if(ch == '{') {
                // オブジェクトの開始
                objectValue = new Dictionary<string,NDJson>();
                parseContext.status = ParseStatus.InObject;
                consumed = true;
            } else if(ch == '[') {
                // 配列の開始
                arrayValue = new List<NDJson>();
                parseContext.status = ParseStatus.InArray;
                consumed = true;
            } else if(isSpaceChar(ch)) {
                // 空白は読み飛ばす
                consumed = true;
            } else {
                throw new ArgumentException(String.Format("Invalid char ({0}) while waiting for start JSON string", ch));
            }
            break;

        case ParseStatus.InNull:
            // null読み取り中
            if(Char.IsLetter(ch)) {
                // 英文字
                parseContext.text.Append(ch);
                consumed = true;
            } else {
                // 英文字以外。null読み取り終了
                string keyword = parseContext.text.ToString();
                if(keyword != "null") {
                    throw new ArgumentException(String.Format("Invalid keyword ({0})", keyword));
                }
                parseContext.text.Clear();
                parseContext.status = ParseStatus.Finished;
                Type = NDJsonType.NULL;
                consumed = false; // 終了を認識した文字は、オブジェクトや配列の区切り文字の可能性がある
                completed = true;
            }
            break;

        case ParseStatus.InString:
            // 文字列読み取り中
            if(parseContext.textEscape) {
                // エスケープ文字のすぐ後
                switch(ch) {
                case '"':
                case '\\':
                case '/':
                    parseContext.text.Append(ch);
                    break;
                case 'b':
                    parseContext.text.Append('\b');
                    break;
                case 'f':
                    parseContext.text.Append('\f');
                    break;
                case 'n':
                    parseContext.text.Append('\n');
                    break;
                case 'r':
                    parseContext.text.Append('\r');
                    break;
                case 't':
                    parseContext.text.Append('\t');
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown escape char ({0})", ch));
                }
                parseContext.textEscape = false;
                consumed = true;
            } else if(ch == '\\') {
                // エスケープ文字
                parseContext.textEscape = true;
                consumed = true;
            } else if(ch == '"') {
                // 文字列終了
                stringValue = parseContext.text.ToString();
                parseContext.text.Clear();
                parseContext.status = ParseStatus.Finished;
                Type = NDJsonType.STRING;
                consumed = true;
                completed = true;
            } else {
                parseContext.text.Append(ch);
                consumed = true;
            }
            break;

        case ParseStatus.InNumber:
            // 数値読み取り中
            if(isNumberChar(ch)) {
                // 数値文字
                parseContext.text.Append(ch);
                consumed = true;
            } else {
                // 数値文字以外。数値読み取り終了
                numberValue = StringUtil.ToDouble(parseContext.text.ToString());
                parseContext.text.Clear();
                parseContext.status = ParseStatus.Finished;
                Type = NDJsonType.NUMBER;
                consumed = false; // 数値終了を認識した文字は、オブジェクトや配列の区切り文字の可能性がある
                completed = true;
            }
            break;

        case ParseStatus.InBool:
            // true/false読み取り中
            if(Char.IsLetter(ch)) {
                // 英文字
                parseContext.text.Append(ch);
                consumed = true;
            } else {
                // 英文字以外。読み取り終了
                string keyword = parseContext.text.ToString();
                if(keyword == "true") {
                    numberValue = 1.0;
                } else if(keyword == "false") {
                    numberValue = 0;
                } else {
                    throw new ArgumentException(String.Format("Invalid keyword ({0})", keyword));
                }
                parseContext.text.Clear();
                parseContext.status = ParseStatus.Finished;
                Type = NDJsonType.BOOL;
                consumed = false; // 終了を認識した文字は、オブジェクトや配列の区切り文字の可能性がある
                completed = true;
            }
            break;

        case ParseStatus.InObject:
            // オブジェクト読み取り中
            if(ch == '"') {
                // キー文字列開始
                parseContext.status = ParseStatus.InObjectKey;
                parseContext.text.Clear();
                parseContext.textEscape = false;
                consumed = true;
            } else if(ch == ':') {
                // 値開始
                parseContext.status = ParseStatus.InObjectValue;
                parseContext.subJson = new NDJson();
                parseContext.subJson.StartParse();
                consumed = true;
            } else if(ch == '}') {
                // オブジェクト終了
                parseContext.status = ParseStatus.Finished;
                Type = NDJsonType.OBJECT;
                consumed = true;
                completed = true;
            } else if((ch == ',') || isSpaceChar(ch)) {
                // カンマと空白は読み飛ばす
                consumed = true;
            } else {
                throw new ArgumentException(String.Format("Invalid char ({0}) while waiting for JSON object separator", ch));
            }
            break;

        case ParseStatus.InObjectKey:
            // オブジェクトのキー文字列読み取り中
            if(parseContext.textEscape) {
                // エスケープ文字のすぐ後
                switch(ch) {
                case '"':
                case '\\':
                case '/':
                    parseContext.text.Append(ch);
                    break;
                case 'b':
                    parseContext.text.Append('\b');
                    break;
                case 'f':
                    parseContext.text.Append('\f');
                    break;
                case 'n':
                    parseContext.text.Append('\n');
                    break;
                case 'r':
                    parseContext.text.Append('\r');
                    break;
                case 't':
                    parseContext.text.Append('\t');
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown escape char ({0})", ch));
                }
                parseContext.textEscape = false;
                consumed = true;
            } else if(ch == '\\') {
                // エスケープ文字
                parseContext.textEscape = true;
                consumed = true;
            } else if(ch == '"') {
                // 文字列終了
                parseContext.status = ParseStatus.InObject;
                consumed = true;
            } else {
                parseContext.text.Append(ch);
                consumed = true;
            }
            break;

        case ParseStatus.InObjectValue:
            // オブジェクトの値読み取り中
            if(parseContext.subJson.parseChar(ch, out consumed)) {
                // サブオブジェクト読み取り完了
                parseContext.subJson.FinishParse();
                objectValue.Add(parseContext.text.ToString(),parseContext.subJson);
                parseContext.status = ParseStatus.InObject;
                if(!consumed) {
                    // サブオブジェクトの読み取りを完了させた文字は、オブジェクトの区切り文字かもしれないので、処理をする。
                    completed = parseChar(ch, out consumed);
                }
            }
            break;

        case ParseStatus.InArray:
            // 配列読み取り中
            if(parseContext.subJson != null) {
                // 配列要素読み取り中
                if(parseContext.subJson.parseChar(ch, out consumed)) {
                    // 配列要素読み取り完了
                    parseContext.subJson.FinishParse();
                    arrayValue.Add(parseContext.subJson);
                    parseContext.subJson = null;
                    if(!consumed) {
                        // 配列要素の読み取りを完了させた文字は、配列の区切り文字かもしれないので、処理をする。
                        completed = parseChar(ch, out consumed);
                    }
                }
            } else if(ch == ',') {
                // 次の要素の開始
                parseContext.subJson = new NDJson();
                parseContext.subJson.StartParse();
                consumed = true;
            } else if(ch == ']') {
                // 配列完了
                parseContext.status = ParseStatus.Finished;
                Type = NDJsonType.ARRAY;
                consumed = true;
                completed = true;
            } else if(isSpaceChar(ch)) {
                // 空白は読み飛ばす
                consumed = true;
            } else {
                parseContext.subJson = new NDJson();
                parseContext.subJson.StartParse();
                parseContext.subJson.parseChar(ch, out consumed);
            }
            break;
        }
        return completed;
    }

#endregion

#region SELFTEST
#if SELFTEST
    public static int Main(string[] args) {
        // 空のJSONを作る
        NDJson json = new NDJson();
        // キーバリューを追加する
        json.Set("alpha", "あるふぁ");
        // キーバリューを指定した新しいJSONを作る
        NDJson json1 = new NDJson("year", 2015);
        // キーバリューを追加する際に、他のJSONをバリューにすることもできる
        json.Set("beta", json1);
        // いっぺんに複数のキーバリューを追加する
        json.Set("x1", 100, "x2", "boy", "x3", new NDJson("name","Jose","age",32));
        // JSON文字列を作る
        Console.WriteLine(json.ToString());
        //     -> {"alpha":"あるふぁ","beta":{"year":2015},"x1":100,"x2":"boy","x3":{"name":"Jose","age":32}}
        // キーを指定して要素を取り出す
        Console.WriteLine("{0}, {1}", json["alpha"].StringValue, json["beta"]["year"].IntValue);
        //     -> あるふぁ, 2015
        // キーが無い時のデフォルト値を指定して要素を取り出す
        Console.WriteLine("{0}, {1}", json.Get("alpha","ありません"), json.Get("gamma","ありません"));
        //     -> あるふぁ, ありません
        // 配列として使う
        NDJson json2 = new NDJson();
        json2.Add("alpha");
        json2.Add("beta");
        json2.Add("gamma");
        Console.WriteLine("{0},{1}",json2.ToString(),json2[1].StringValue);
        //     -> ["alpha","beta","gamma"],beta
        // 負のインデックスは末尾からの指定
        Console.WriteLine(json2[-1].StringValue);
        //     -> gamma
        // 文字列の配列として値を取り出す
        foreach(string x in json2.GetStringArray()) {
            Console.Write("{0} ", x);
        }
        Console.WriteLine();
        //     -> alpha beta gamma
        // オブジェクトに対して文字列の配列を取り出すと、キー名の一覧になる
        foreach(string x in json.GetStringArray()) {
            Console.Write("{0}->{1}, ", x, json[x].StringValue);
        }
        Console.WriteLine();
        //     -> alpha->あるふぁ, beta->{"year":2015}, x1->100, x2->boy, x3->{"name":"Jose","age":32},
        // 配列をいっぺんにJSON化する
        json2 = new NDJson(new string[]{"あ","い","う","え","お"});
        Console.WriteLine(json2.ToString());
        //     -> ["あ","い","う","え","お"]
        json2 = new NDJson("columns", new string[]{"name","age","gender"});
        Console.WriteLine(json2.ToString());
        //     -> {"columns":["name","age","gender"]}
        json2 = new NDJson(new int[]{3,1,4,1,5,9,2});
        Console.WriteLine(json2.ToString());
        //     -> [3,1,4,1,5,9,2]

        // JSON文字列を読み込む
        NDJson json3 = NDJson.Parse(@"{""one"":""uno"",""two"":""dos"",""numbers"":[1,2],""booleans"":[false,true],""object"":{""name"":""ジェイソン"",""weekday"":""金曜""}}");
        Console.WriteLine(json3.ToString());
        //     -> {"one":"uno","two":"dos","numbers":[1,2],"booleans":[false,true],"object":{"name":"ジェイソン","weekday":"金曜"}}

        // ファイルを読み込む例
        string f = "NDJsonTest.json";
        try {
            using(StreamReader sr = new StreamReader(f, Encoding.UTF8)) {
                json = NDJson.Parse(sr);
                sr.Close();
            }
            Console.WriteLine(json.ToString());
        } catch(Exception ex) {
            Console.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
        }

        // ファイルを書き出す例
        using(StreamWriter sw = new StreamWriter("NDJsonTest2.json", false, Encoding.UTF8)) {
            json.WriteTo(sw);
            // sw.Write(json.ToString()); でも結果は同じだが、JSONストリングを
            // すべて構築するためのメモリが必要になり、効率が著しく悪くなる。
            sw.Close();
        }

        return 0;
    }
#endif
#endregion

}

/// <summary>
///   値の型
/// </summary>
public enum NDJsonType {
    NULL,   // nullまたは無効
    STRING, // 文字列
    NUMBER, // 数値
    BOOL,   // 真偽値
    OBJECT, // JSONオブジェクト（連想配列）
    ARRAY,  // 配列
}


} // End of namespace
