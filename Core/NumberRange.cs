/// NumberRange: 整数範囲
///
/// Copyright (C) 2019 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;

namespace MACS {

/// <summary>
///   整数範囲クラス
/// </summary>

public class NumberRange : IComparable<NumberRange>, IEquatable<NumberRange> {

    /// <summary>
    ///   空の番号範囲をつくる
    /// </summary>
    public NumberRange() {
        Start = Int32.MinValue;
        End = Int32.MinValue;
    }

    /// <summary>
    ///   番号範囲文字列からのコンストラクタ
    /// </summary>
    public NumberRange(string txt) {
        Parse(txt);
    }

    /// <summary>
    ///   下限値、上限値からのコンストラクタ
    /// </summary>
    public NumberRange(int start, int end) {
        Set(start, end);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public NumberRange(NumberRange src) {
        CopyFrom(src);
    }

    /// <summary>
    ///   値域（最小）
    /// </summary>
    public int Start;

    /// <summary>
    ///   値域（最大）
    /// </summary>
    public int End;

    /// <summary>
    ///   要素数（最大値-最小値+1）
    /// </summary>
    public int Count {
        get {
            if(!IsValid)
                return 0;
            if(IsFullRange)
                return Int32.MaxValue;
            else if(Start <= End)
                return End-Start+1;
            else
                return Start-End+1;
        }
    }

    /// <summary>
    ///   要素数（最大値-最小値+1）
    /// </summary>
    public int Length {
        get { return Count; }
    }

    /// <summary>
    ///   有効な番号範囲になっているかどうか
    /// </summary>
    public bool IsValid {
        get { return ((Start != Int32.MinValue) || (End != Int32.MinValue)); }
    }

    /// <summary>
    ///   全整数範囲になっているかどうか
    /// </summary>
    public bool IsFullRange {
        get { return ((Start == Int32.MinValue) && (End == Int32.MaxValue)); }
    }

    /// <summary>
    ///   Start<=Endかどうか
    /// </summary>
    public bool IsAscend {
        get { return (Start <= End); }
    }
    
    /// <summary>
    ///   Start>=Endかどうか
    /// </summary>
    public bool IsDescend {
        get { return (Start >= End); }
    }

    /// <summary>
    ///   int配列を得る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     実際に配列を生成するので、巨大な範囲の場合は使用しないこと。
    ///   </para>
    /// </remarks>
    public int[] ToIntArray() {
        int[] list;
        if(!IsValid) {
            list = new int[0];
        } else if(IsAscend) {
            list = new int[End-Start+1];
            for(int i = 0; i < list.Length; i++) {
                list[i] = Start+i;
            }
        } else {
            list = new int[Start-End+1];
            for(int i = 0; i < list.Length; i++) {
                list[i] = Start-i;
            }
        }
        return list;
    }

    /// <summary>
    ///   n番目の番号（nは0からCount）
    /// </summary>
    public int Get(int n) {
        if(IsAscend)
            return Start+n;
        else
            return Start-n;
    }

    /// <summary>
    ///   インデクサ
    /// </summary>
    public int this[int n] {
        get { return Get(n); }
    }

    /// <summary>
    ///   番号範囲をセットする
    /// </summary>
    public NumberRange Set(int start, int end) {
        Start = start;
        End = end;
        return this;
    }

    /// <summary>
    ///   番号範囲文字列を解釈し、読み込む
    /// </summary>
    public NumberRange Parse(string txt) {
        if(String.IsNullOrEmpty(txt)) {
            Start = End = Int32.MinValue;
            return this;
        }
        Start = Int32.MinValue;
        StringBuilder sb = new StringBuilder();
        foreach(char ch in txt) {
            if((ch >= '0') && (ch <= '9')) {
                sb.Append(ch);
            } else if((ch == '-') || (ch == ',')) {
                if(sb.Length == 0) {
                    sb.Append(ch);
                } else {
                    string s = sb.ToString();
                    if(s == "*")
                        Start = Int32.MinValue;
                    else
                        Start = StringUtil.ToInt(s);
                    sb.Clear();
                }
            } else if(ch == '*') {
                sb.Clear();
                sb.Append(ch);
            }
        }
        if(sb.Length > 0) {
            string s = sb.ToString();
            if(s == "*")
                End = Int32.MaxValue;
            else
                End = StringUtil.ToInt(s);
        } else {
            End = Start;
        }
        return this;
    }

    /// <summary>
    ///   番号範囲を空にする
    /// </summary>
    public NumberRange Clear() {
        Start = End = Int32.MinValue;
        return this;
    }

    /// <summary>
    ///   文字列化する
    /// </summary>
    public override string ToString() {
        if(!IsValid)
            return "";
        if(IsFullRange)
            return "*";
        return String.Format("{0}-{1}", (Start==Int32.MinValue)?"*":Start.ToString(), (End==Int32.MaxValue)?"*":End.ToString());
    }

    /// <summary>
    ///   内容のコピー
    /// </summary>
    public NumberRange CopyFrom(NumberRange src) {
        Start = src.Start;
        End = src.End;
        return this;
    }

    /// <summary>
    ///   コピーを作る
    /// </summary>
    public NumberRange Copy() {
        return new NumberRange(this);
    }

    /// <summary>
    ///   イテレータ
    /// </summary>
    public IEnumerator<int> GetEnumerator() {
        if(!IsValid)
            yield break;
        if(IsAscend) {
            for(int i = Start; i <= End; i++)
                yield return i;
        } else {
            for(int i = Start; i >= End; i--)
                yield return i;
        }
    }

    /// <summary>
    ///   指定の番号を含むかどうか
    /// </summary>
    public bool Contains(int n) {
        if(!IsValid)
            return false;
        if(IsAscend)
            return ((Start <= n) && (n <= End));
        else
            return ((Start >= n) && (n >= End));
    }

    /// <summary>
    ///   等値比較
    /// </summary>
    public bool Equals(NumberRange src) {
        if(src == null)
            return false;
        return ((Start == src.Start) && (End == src.End));
    }

    public override bool Equals(object obj) {
        return Equals(obj as NumberRange);
    }

    public static bool operator ==(NumberRange a, NumberRange b) {
        if((object)a == null)
            return ((object)b == null);
        return a.Equals(b);
    }
    
    public static bool operator !=(NumberRange a, NumberRange b) {
        return !(a == b);
    }

    public override int GetHashCode() {
        return Start.GetHashCode()^End.GetHashCode();
    }

    /// <summary>
    ///   List<NumberRange>をソートするときの比較演算
    /// </summary>
    public int CompareTo(NumberRange dst) {
        if(Start < dst.Start)
            return -1;
        if(Start > dst.Start)
            return 1;
        if(End < dst.End)
            return -1;
        if(End > dst.End)
            return 1;
        return 0;
    }


#region SELFTEST
#if SELFTEST
    public static int Main(string[] args) {
        NumberRange nl = new NumberRange();
        foreach(string x in args) {
            Console.Write(x);
            Console.Write("=>");
            nl.Parse(x);
            Console.Write("{0}-{1}", nl.Start, nl.End);
            Console.Write("=>");
            Console.Write(nl.ToString());
            if(nl.Count < 100) {
                Console.Write("=>");
                foreach(int i in nl) {
                    Console.Write("{0},", i);
                }
            }
            Console.WriteLine();
        }
        return 0;
    }
#endif
#endregion

}

} // End of namespace
