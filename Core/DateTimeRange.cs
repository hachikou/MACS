/// DateTimeRange: 日時範囲
///
/// Copyright (C) 2020 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;

namespace MACS {

/// <summary>
///   日時範囲クラス
/// </summary>

public class DateTimeRange : IComparable<DateTimeRange>, IEquatable<DateTimeRange> {

    /// <summary>
    ///   空の日時範囲をつくる
    /// </summary>
    public DateTimeRange() {
        Start = DateTime.MinValue;
        End = DateTime.MinValue;
    }

    /// <summary>
    ///   日時範囲文字列からのコンストラクタ
    /// </summary>
    public DateTimeRange(string txt) {
        Parse(txt);
    }

    /// <summary>
    ///   下限値、上限値からのコンストラクタ
    /// </summary>
    public DateTimeRange(DateTime start, DateTime end) {
        Set(start, end);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public DateTimeRange(DateTimeRange src) {
        CopyFrom(src);
    }

    /// <summary>
    ///   開始日時
    /// </summary>
    public DateTime Start;

    /// <summary>
    ///   終了日時
    /// </summary>
    public DateTime End;

    /// <summary>
    ///   時間
    /// </summary>
    public TimeSpan Span {
        get {
            if(!IsValid)
                return new TimeSpan(0);
            if(IsFullRange)
                return TimeSpan.MaxValue;
            else if(Start <= End)
                return End-Start;
            else
                return Start-End;
        }
    }

    /// <summary>
    ///   有効な日時範囲になっているかどうか
    /// </summary>
    public bool IsValid {
        get { return ((Start != DateTime.MinValue) || (End != DateTime.MinValue)); }
    }

    /// <summary>
    ///   全日時範囲になっているかどうか
    /// </summary>
    public bool IsFullRange {
        get { return ((Start == DateTime.MinValue) && (End == DateTime.MaxValue)); }
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
    ///   日時範囲をセットする
    /// </summary>
    public DateTimeRange Set(DateTime start, DateTime end) {
        Start = start;
        End = end;
        return this;
    }

    /// <summary>
    ///   日時範囲文字列を解釈し、読み込む
    /// </summary>
    public DateTimeRange Parse(string txt) {
        if(String.IsNullOrEmpty(txt)) {
            Start = End = DateTime.MinValue;
            return this;
        }
        string[] x = txt.Split(',');
        if(x.Length == 1) {
            string xx = x[0].Trim();
            if(xx == "*") {
                Start = DateTime.MinValue;
                End = DateTime.MaxValue;
            } else {
                Start = End = StringUtil.ToDateTime(x[0]);
            }
        } else {
            string xx = x[0].Trim();
            if(xx == "*") {
                Start = DateTime.MinValue;
            } else {
                Start = StringUtil.ToDateTime(xx);
            }
            xx = x[1].Trim();
            if(xx == "*") {
                End = DateTime.MaxValue;
            } else {
                End = StringUtil.ToDateTime(xx);
            }
        }
        return this;
    }

    /// <summary>
    ///   日時範囲を空にする
    /// </summary>
    public DateTimeRange Clear() {
        Start = End = DateTime.MinValue;
        return this;
    }

    /// <summary>
    ///   文字列化する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ミリ秒単位までしか保持しない
    ///   </para>
    /// </remarks>
    public override string ToString() {
        if(!IsValid)
            return "";
        if(IsFullRange)
            return "*";
        return String.Format("{0},{1}", (Start==DateTime.MinValue)?"*":StringUtil.PrettyDateTime(Start), (End==DateTime.MaxValue)?"*":StringUtil.PrettyDateTime(End));
    }

    /// <summary>
    ///   内容のコピー
    /// </summary>
    public DateTimeRange CopyFrom(DateTimeRange src) {
        Start = src.Start;
        End = src.End;
        return this;
    }

    /// <summary>
    ///   コピーを作る
    /// </summary>
    public DateTimeRange Copy() {
        return new DateTimeRange(this);
    }

    /// <summary>
    ///   指定の日時を含むかどうか
    /// </summary>
    public bool Contains(DateTime dt) {
        if(!IsValid)
            return false;
        if(IsAscend)
            return ((Start <= dt) && (dt <= End));
        else
            return ((Start >= dt) && (dt >= End));
    }

    /// <summary>
    ///   指定の日時を含むかどうか（境界日時を含まない）
    /// </summary>
    public bool ContainsExceptLimit(DateTime dt) {
        if(!IsValid)
            return false;
        if(IsAscend)
            return ((Start < dt) && (dt < End));
        else
            return ((Start > dt) && (dt > End));
    }

    /// <summary>
    ///   日時範囲が重なっているかどうか
    /// </summary>
    /// <param name="dst">対象日時範囲</param>
    /// <param name="continuation">隣接する日時範囲を許容するかどうか</param>
    public bool Overlaps(DateTimeRange dst, bool continuation=false) {
        if(continuation) {
            return this.ContainsExceptLimit(dst.Start) || this.ContainsExceptLimit(dst.End)
                || dst.ContainsExceptLimit(this.Start) || (this == dst);
        } else {
            return this.Contains(dst.Start) || this.Contains(dst.End)
                || dst.Contains(this.Start);
        }
    }

    /// <summary>
    ///   等値比較
    /// </summary>
    public bool Equals(DateTimeRange src) {
        if(src == null)
            return false;
        return ((Start == src.Start) && (End == src.End));
    }

    public override bool Equals(object obj) {
        return Equals(obj as DateTimeRange);
    }

    public static bool operator ==(DateTimeRange a, DateTimeRange b) {
        if((object)a == null)
            return ((object)b == null);
        return a.Equals(b);
    }
    
    public static bool operator !=(DateTimeRange a, DateTimeRange b) {
        return !(a == b);
    }

    public override int GetHashCode() {
        return Start.GetHashCode()^End.GetHashCode();
    }

    /// <summary>
    ///   List<DateTimeRange>をソートするときの比較演算
    /// </summary>
    public int CompareTo(DateTimeRange dst) {
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
        DateTimeRange nl = new DateTimeRange();
        foreach(string x in args) {
            Console.Write(x);
            Console.Write("=>");
            nl.Parse(x);
            Console.Write("{0}-{1}", nl.Start, nl.End);
            Console.Write("=>");
            Console.Write(nl.ToString());
            Console.WriteLine();
        }
        return 0;
    }
#endif
#endregion

}

} // End of namespace
