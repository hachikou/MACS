/// NumberList: 重複しない整数列
///
/// Copyright (C) 2019 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;

namespace MACS {

/// <summary>
///   番号列管理クラス
/// </summary>

public class NumberList : IComparable<NumberList>, IEquatable<NumberList> {

    /// <summary>
    ///   空の番号列を作る
    /// </summary>
    public NumberList() {
        // nothing to do.
    }

    /// <summary>
    ///   番号列文字列からのコンストラクタ
    /// </summary>
    public NumberList(string txt) {
        Parse(txt);
    }

    /// <summary>
    ///   int配列からのコンストラクタ
    /// </summary>
    public NumberList(int[] src) {
        Set(src);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public NumberList(NumberList src) {
        MinValue = src.MinValue;
        MaxValue = src.MaxValue;
        CopyFrom(src);
    }

    /// <summary>
    ///   空の番号列を作る（値域指定）
    /// </summary>
    public NumberList(int min, int max) {
        MinValue = min;
        MaxValue = max;
    }

    /// <summary>
    ///   番号列文字列からのコンストラクタ（値域指定）
    /// </summary>
    public NumberList(int min, int max, string txt) {
        MinValue = min;
        MaxValue = max;
        Parse(txt);
    }

    /// <summary>
    ///   int配列からのコンストラクタ（値域指定）
    /// </summary>
    public NumberList(int min, int max, int[] src) {
        MinValue = min;
        MaxValue = max;
        Set(src);
    }

    /// <summary>
    ///   値域（最小）
    /// </summary>
    public int MinValue = Int32.MinValue;

    /// <summary>
    ///   値域（最大）
    /// </summary>
    public int MaxValue = Int32.MaxValue;

    /// <summary>
    ///   要素数
    /// </summary>
    public int Count {
        get { return list.Count; }
    }

    /// <summary>
    ///   要素数
    /// </summary>
    public int Length {
        get { return list.Count; }
    }

    /// <summary>
    ///   int配列を得る
    /// </summary>
    public int[] ToIntArray() {
        return list.ToArray();
    }

    /// <summary>
    ///   n番目の番号（nは0からCount）
    /// </summary>
    public int Get(int n) {
        if((n < 0) || (n >= list.Count))
            return 0;
        return list[n];
    }

    /// <summary>
    ///   インデクサ
    /// </summary>
    public int this[int n] {
        get {
            if((n < 0) || (n >= list.Count))
                throw new IndexOutOfRangeException();
            return list[n];
        }
    }
    
    /// <summary>
    ///   int配列をセットする
    /// </summary>
    public NumberList Set(params int[] src) {
        list.Clear();
        Add(src);
        return this;
    }

    /// <summary>
    ///   番号を追加する
    /// </summary>
    public NumberList Add(params int[] src) {
        foreach(int n in src) {
            if((n >= MinValue) && (n <= MaxValue) && !list.Contains(n))
                list.Add(n);
        }
        list.Sort();
        return this;
    }

    /// <summary>
    ///   List<int>をセットする
    /// </summary>
    public NumberList Set(List<int> src) {
        list.Clear();
        Add(src);
        return this;
    }

    /// <summary>
    ///   List<int>を追加する
    /// </summary>
    public NumberList Add(List<int> src) {
        foreach(int n in src) {
            if((n >= MinValue) && (n <= MaxValue) && !list.Contains(n))
                list.Add(n);
        }
        list.Sort();
        return this;
    }

    /// <summary>
    ///   番号を追加する（値域チェックなし、重複チェックなし、並べなおしなし）
    /// </summary>
    public NumberList QuickAdd(int n) {
        list.Add(n);
        return this;
    }

    /// <summary>
    ///   番号列文字列を解釈し、読み込む
    /// </summary>
    public NumberList Parse(string txt) {
        list.Clear();
        Add(txt);
        return this;
    }

    /// <summary>
    ///   番号列文字列を解釈し、追加する
    /// </summary>
    public NumberList Add(string txt) {
        foreach(string x in txt.Split(',')) {
            int n = StringUtil.ToInt(x);
            if((n >= MinValue) && (n <= MaxValue) && !list.Contains(n))
                list.Add(n);
        }
        list.Sort();
        return this;
    }

    /// <summary>
    ///   番号列を空にする
    /// </summary>
    public NumberList Clear() {
        list.Clear();
        return this;
    }

    /// <summary>
    ///   文字列化する
    /// </summary>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach(int n in list) {
            if(sb.Length > 0)
                sb.Append(',');
            sb.Append(n.ToString());
        }
        return sb.ToString();
    }

    /// <summary>
    ///   内容のコピー
    /// </summary>
    public NumberList CopyFrom(NumberList src) {
        Set(src.list);
        return this;
    }

    /// <summary>
    ///   コピーを作る
    /// </summary>
    public NumberList Copy() {
        return new NumberList(this);
    }

    /// <summary>
    ///   イテレータ
    /// </summary>
    public IEnumerator<int> GetEnumerator() {
        return list.GetEnumerator();
    }

    /// <summary>
    ///   指定の番号を含むかどうか
    /// </summary>
    public bool Contains(int n) {
        return list.Contains(n);
    }

    /// <summary>
    ///   等値比較
    /// </summary>
    public bool Equals(NumberList src) {
        if(src == null)
            return false;
        if(this.Count != src.Count)
            return false;
        for(int i = 0; i < this.Count; i++) {
            if(list[i] != src.list[i])
                return false;
        }
        return true;
    }

    public override bool Equals(object obj) {
        return Equals(obj as NumberList);
    }

    public static bool operator ==(NumberList a, NumberList b) {
        if((object)a == null)
            return ((object)b == null);
        return a.Equals(b);
    }
    
    public static bool operator !=(NumberList a, NumberList b) {
        return !(a == b);
    }

    public override int GetHashCode() {
        int h = 0;
        foreach(int n in list)
            h ^= n.GetHashCode();
        return h;
    }

    /// <summary>
    ///   List<NumberList>をソートするときの比較演算
    /// </summary>
    public int CompareTo(NumberList dst) {
        int i = 0;
        while((i < this.Count) && (i < dst.Count)) {
            if(this.list[i] < dst.list[i])
                return -1;
            if(this.list[i] > dst.list[i])
                return 1;
            i++;
        }
        return this.Count.CompareTo(dst.Count);
    }


    private List<int> list = new List<int>();
}

} // End of namespace
