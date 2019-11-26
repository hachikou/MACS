/// ExTimeSpan: 年・月を指定できる時間間隔
///
/// Copyright (C) 2019 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;

namespace MACS {

/// <summary>
///   年・月を指定できる時間間隔
/// </summary>

public class ExTimeSpan : IComparable<ExTimeSpan>, IEquatable<ExTimeSpan> {

    // ExTimeSpanの文字列表現について
    // ExTimeSpanを文字列でセットするときには、次のフォーマットに従います
    // （以降、nは整数（負を含む）を表します）
    // nY または ny : n年間
    // nM           : nヶ月
    // nD または nd : n日間
    // nW または nw : n週間
    // nh または nH : n時間
    // nm           : n分間
    // ns または nS : n秒間
    // nv または nV : nミリ秒間
    // nt または nT : nTick間
    //
    // なお、ToString()をした際には、左側の表記を用います。ただし、nWは用いられず、nD表記になります
    
    /// <summary>
    ///   年
    /// </summary>
    public int Year;

    /// <summary>
    ///   月
    /// </summary>
    public int Month;

    /// <summary>
    ///   日
    /// </summary>
    public int Day;

    /// <summary>
    ///   時
    /// </summary>
    public long Hour;

    /// <summary>
    ///   分
    /// </summary>
    public long Minute;

    /// <summary>
    ///   秒
    /// </summary>
    public long Second;

    /// <summary>
    ///   ミリ秒
    /// </summary>
    public long Millisecond;

    /// <summary>
    ///   Tick
    /// </summary>
    public long Tick;


    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public ExTimeSpan() {
        // nothing to do.
    }

    /// <summary>
    ///   年月日時分秒ミリ秒Tickを指定したコンストラクタ
    /// </summary>
    public ExTimeSpan(int year, int month, int day, long hour=0, long minute=0, long second=0, long millisecond=0, long tick=0) {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
        Second = second;
        Millisecond = millisecond;
        Tick = tick;
    }

    /// <summary>
    ///   文字列からのコンストラクタ
    /// </summary>
    public ExTimeSpan(string txt) {
        Set(txt);
    }

    /// <summary>
    ///   TimeSpanからのコンストラクタ
    /// </summary>
    public ExTimeSpan(TimeSpan src) {
        Set(src);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public ExTimeSpan(ExTimeSpan src) {
        Set(src);
    }

    /// <summary>
    ///   年月日時分秒ミリ秒Tickを設定する
    /// </summary>
    public ExTimeSpan Set(int year, int month, int day, long hour=0, long minute=0, long second=0, long millisecond=0, long tick=0) {
        Year = year;
        Month = month;
        Day = day;
        Hour = hour;
        Minute = minute;
        Second = second;
        Millisecond = millisecond;
        Tick = tick;
        return this;
    }

    /// <summary>
    ///   空の設定にする
    /// </summary>
    public ExTimeSpan Clear() {
        Year = 0;
        Month = 0;
        Day = 0;
        Hour = 0;
        Minute = 0;
        Second = 0;
        Millisecond = 0;
        Tick = 0;
        return this;
    }

    /// <summary>
    ///   文字列から設定する
    /// </summary>
    public ExTimeSpan Set(string txt) {
        Clear();
        if(String.IsNullOrEmpty(txt))
            return this;
        long val = 0;
        int sign = 1;
        foreach(char ch in txt) {
            switch(ch) {
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
                val = val*10+(ch-'0');
                break;
            case '-':
                sign = -1;
                break;
            case 'Y':
            case 'y':
                Year = (int)val*sign;
                val = 0;
                sign = 1;
                break;
            case 'M':
                Month = (int)val*sign;
                val = 0;
                sign = 1;
                break;
            case 'D':
            case 'd':
                Day = (int)val*sign;
                val = 0;
                sign = 1;
                break;
            case 'W':
            case 'w':
                Day = (int)val*sign*7;
                val = 0;
                sign = 1;
                break;
            case 'h':
            case 'H':
                Hour = val*sign;
                val = 0;
                sign = 1;
                break;
            case 'm':
                Minute = val*sign;
                val = 0;
                sign = 1;
                break;
            case 's':
            case 'S':
                Second = val*sign;
                val = 0;
                sign = 1;
                break;
            case 'v':
            case 'V':
                Millisecond = val*sign;
                val = 0;
                sign = 1;
                break;
            case 't':
            case 'T':
                Tick = val*sign;
                val = 0;
                sign = 1;
                break;
            }
        }
        if(val > 0) {
            Day = (int)val*sign;
        }
        return this;
    }    

    /// <summary>
    ///   TimeSpanから設定する
    /// </summary>
    public ExTimeSpan Set(TimeSpan src) {
        Year = 0;
        Month = 0;
        Day = src.Days;
        Hour = src.Hours;
        Minute = src.Minutes;
        Second = src.Seconds;
        Millisecond = src.Milliseconds;
        Tick = src.Ticks%TimeSpan.TicksPerMillisecond;
        return this;
    }

    /// <summary>
    ///   設定をコピーする
    /// </summary>
    public ExTimeSpan Set(ExTimeSpan src) {
        Year = src.Year;
        Month = src.Month;
        Day = src.Day;
        Hour = src.Hour;
        Minute = src.Minute;
        Second = src.Second;
        Millisecond = src.Millisecond;
        Tick = src.Tick;
        return this;
    }


    /// <summary>
    ///   TimeSpanを得る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     注意：年、月の値は失われます
    ///   </para>
    /// </remarks>
    public TimeSpan ToTimeSpan() {
        return new TimeSpan((((((long)Day*24+Hour)*60+Minute)*60+Second)*1000+Millisecond)*TimeSpan.TicksPerMillisecond+Tick);
    }

    /// <summary>
    ///   文字列化する
    /// </summary>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        if(Year != 0)
            sb.AppendFormat("{0}Y", Year);
        if(Month != 0)
            sb.AppendFormat("{0}M", Month);
        if(Day != 0)
            sb.AppendFormat("{0}D", Day);
        if(Hour != 0)
            sb.AppendFormat("{0}h", Hour);
        if(Minute != 0)
            sb.AppendFormat("{0}m", Minute);
        if(Second != 0)
            sb.AppendFormat("{0}s", Second);
        if(Millisecond != 0)
            sb.AppendFormat("{0}v", Millisecond);
        if(Tick != 0)
            sb.AppendFormat("{0}t", Tick);
        return sb.ToString();
    }

    /// <summary>
    ///   コピーを作る
    /// </summary>
    public ExTimeSpan Copy() {
        return new ExTimeSpan(this);
    }

    /// <summary>
    ///   指定日時のこの期間前の日時を得る
    /// </summary>
    public DateTime Before(DateTime dt) {
        return dt.AddYears(-Year).AddMonths(-Month).AddDays(-Day)
            .AddHours(-Hour).AddMinutes(-Minute).AddSeconds(-Second)
            .AddMilliseconds(-Millisecond).AddTicks(-Tick);
    }

    /// <summary>
    ///   指定日時のこの期間前の日時を得る（下位データの丸め付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     日以下のデータが連続して0になっている場合、その部分の値を0にする。
    ///     例：
    ///     (new TimeSpan("4D")).Before(new DateTime(2019,10,20,4,51,30)) -> 2019/10/14 04:51:30
    ///     (new TimeSpan("4D")).BeforeTruncate(new DateTime(2019,10,20,4,51,30)) -> 2019/10/14 00:00:00
    ///     (new TimeSpan("1D8h")).BeforeTruncate(new DateTime(2019,10,20,4,51,30)) -> 2019/10/18 20:00:00
    ///   </para>
    /// </remarks>
    public DateTime BeforeTruncate(DateTime dt) {
        long t = 0;
        bool trunc = true;
        if(Tick != 0) {
            t = (dt.Ticks%TimeSpan.TicksPerMillisecond)-Tick;
            trunc = false;
        }
        if(!trunc || (Millisecond != 0)) {
            t += (dt.Millisecond-Millisecond)*TimeSpan.TicksPerMillisecond;
            trunc = false;
        }
        if(!trunc || (Second != 0)) {
            t += (dt.Second-Second)*1000*TimeSpan.TicksPerMillisecond;
            trunc = false;
        }
        if(!trunc || (Minute != 0)) {
            t += (dt.Minute-Minute)*60*1000*TimeSpan.TicksPerMillisecond;
            trunc = false;
        }
        if(!trunc || (Hour != 0)) {
            t += (dt.Hour-Hour)*60*60*1000*TimeSpan.TicksPerMillisecond;
            trunc = false;
        }
        return (new DateTime(dt.Year, dt.Month, dt.Day)).AddYears(-Year).AddMonths(-Month).AddDays(-Day).AddTicks(t);
    }

    /// <summary>
    ///   指定日時のこの期間後の日時を得る
    /// </summary>
    public DateTime After(DateTime dt) {
        return dt.AddYears(Year).AddMonths(Month).AddDays(Day)
            .AddHours(Hour).AddMinutes(Minute).AddSeconds(Second)
            .AddMilliseconds(Millisecond).AddTicks(Tick);
    }

    /// <summary>
    ///   指定日時のこの期間後の日時を得る（下位データの切り上げ付き）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     日以下のデータが連続して0になっている場合、その部分の値を最大値にする
    ///     例：
    ///     (new TimeSpan("4D")).After(new DateTime(2019,10,20,4,51,30)) -> 2019/10/24 04:51:30
    ///     (new TimeSpan("4D")).AfterCeiling(new DateTime(2019,10,20,4,51,30)) -> 2019/10/24 23:59:59.9999
    ///     (new TimeSpan("1D8h")).AfterCeiling(new DateTime(2019,10,20,4,51,30)) -> 2019/10/24 12:59:59.9999
    ///   </para>
    /// </remarks>
    public DateTime AfterCeiling(DateTime dt) {
        long t;
        bool ceiling = true;
        if(Tick != 0) {
            t = (dt.Ticks%TimeSpan.TicksPerMillisecond)+Tick;
            ceiling = false;
        } else {
            t = TimeSpan.TicksPerMillisecond-1;
        }
        if(!ceiling || (Millisecond != 0)) {
            t += (dt.Millisecond+Millisecond)*TimeSpan.TicksPerMillisecond;
            ceiling = false;
        } else {
            t = 1000*TimeSpan.TicksPerMillisecond-1;
        }
        if(!ceiling || (Second != 0)) {
            t += (dt.Second+Second)*1000*TimeSpan.TicksPerMillisecond;
            ceiling = false;
        } else {
            t = 60*1000*TimeSpan.TicksPerMillisecond-1;
        }
        if(!ceiling || (Minute != 0)) {
            t += (dt.Minute+Minute)*60*1000*TimeSpan.TicksPerMillisecond;
            ceiling = false;
        } else {
            t = 60*60*1000*TimeSpan.TicksPerMillisecond-1;
        }
        if(!ceiling || (Hour != 0)) {
            t += (dt.Hour+Hour)*60*60*1000*TimeSpan.TicksPerMillisecond;
            ceiling = false;
        } else {
            t = 24*60*60*1000*TimeSpan.TicksPerMillisecond-1;
        }
        return (new DateTime(dt.Year, dt.Month, dt.Day)).AddYears(Year).AddMonths(Month).AddDays(Day).AddTicks(t);
    }


    /// <summary>
    ///   等値比較
    /// </summary>
    public bool Equals(ExTimeSpan src) {
        if(src == null)
            return false;
        return (Year == src.Year) && (Month == src.Month) && (Day == src.Day)
            && (Hour == src.Hour) && (Minute == src.Minute) && (Second == src.Second)
            && (Millisecond == src.Millisecond) && (Tick == src.Tick);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ExTimeSpan);
    }

    public static bool operator ==(ExTimeSpan a, ExTimeSpan b) {
        if((object)a == null)
            return ((object)b == null);
        return a.Equals(b);
    }
    
    public static bool operator !=(ExTimeSpan a, ExTimeSpan b) {
        return !(a == b);
    }

    public override int GetHashCode() {
        return Year.GetHashCode()^Month.GetHashCode()^Day.GetHashCode()
            ^Hour.GetHashCode()^Minute.GetHashCode()^Second.GetHashCode()
            ^Millisecond.GetHashCode()^Tick.GetHashCode();
    }
    
    /// <summary>
    ///   大小比較
    /// </summary>
    public int CompareTo(ExTimeSpan dst) {
        if(Year < dst.Year)
            return -1;
        if(Year > dst.Year)
            return 1;
        if(Month < dst.Month)
            return -1;
        if(Month > dst.Month)
            return 1;
        if(Day < dst.Day)
            return -1;
        if(Day > dst.Day)
            return 1;
        if(Hour < dst.Hour)
            return -1;
        if(Hour > dst.Hour)
            return 1;
        if(Minute < dst.Minute)
            return -1;
        if(Minute > dst.Minute)
            return 1;
        if(Second < dst.Second)
            return -1;
        if(Second > dst.Second)
            return 1;
        if(Millisecond < dst.Millisecond)
            return -1;
        if(Millisecond > dst.Millisecond)
            return 1;
        if(Tick < dst.Tick)
            return -1;
        if(Tick > dst.Tick)
            return 1;
        return 0;
    }


#region SELFTEST
#if SELFTEST
    public static int Main(string[] args) {
        ExTimeSpan ts = new ExTimeSpan();
        DateTime now = DateTime.Now;
        foreach(string x in args) {
            Console.Write(x);
            Console.Write("->");
            ts.Set(x);
            Console.WriteLine(ts.ToString());
            Console.WriteLine("ExTimeSpan(\"{0}\").Before({1}) -> {2}", x, now, ts.Before(now));
            Console.WriteLine("ExTimeSpan(\"{0}\").BeforeTruncate({1}) -> {2}", x, now, ts.BeforeTruncate(now));
            Console.WriteLine("ExTimeSpan(\"{0}\").After({1}) -> {2}", x, now, ts.After(now));
            Console.WriteLine("ExTimeSpan(\"{0}\").AfterCeiling({1}) -> {2}", x, now, ts.AfterCeiling(now));
        }
        return 0;
    }
#endif
#endregion

}

} // End of namespace
