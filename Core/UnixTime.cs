/// UnixTime: UNIX時刻データ操作用クラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;

namespace MACS {

/// <summary>
///   UNIX時刻データ操作用クラス。
/// </summary>
/// <remarks>
///   <para>
///     1970年1月1日0時0分0秒を0とする32ビット符号無し整数で時刻を扱う。
///   </para>
/// </remarks>
public struct UnixTime {
    //public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
    public static readonly DateTime BaseTime = new DateTime(1970, 1, 1, 0, 0, 0);

    private uint tm;

    public UnixTime(UnixTime val) {
        tm = val.tm;
    }
    public UnixTime(uint val) {
        tm = val;
    }
    public UnixTime(string txt) {
        tm = 0;
        Parse(txt);
    }
    public UnixTime(DateTime val) {
        if(val.Ticks == 0)
            tm = 0;
        else
            tm = (uint)((val-BaseTime).TotalSeconds);
    }
    public UnixTime(byte[] buf, int offset) {
        tm = BitConverter.ToUInt32(buf, offset);
    }

    public void Set(UnixTime val){
        tm = val.tm;
    }
    public void Set(uint val){
        tm = val;
    }
    public void Set(string val){
        Parse(val);
    }
    public void Set(DateTime val) {
        if(val.Ticks == 0)
            tm = 0;
        else
            tm = (uint)((val-BaseTime).TotalSeconds);
    }
    public void Set(byte[] buf, int offset){
        tm = BitConverter.ToUInt32(buf, offset);
    }

    public void Clear() {
        tm = 0;
    }

    public bool Parse(string txt){
        tm = 0;
        if(txt == null)
            return false;
        string[] x = txt.Split(" /:-.".ToCharArray());
        if(x.Length != 6)
            return false;
        try {
            int year = StringUtil.ToInt(x[0]);
            int month = StringUtil.ToInt(x[1]);
            int day = StringUtil.ToInt(x[2]);
            int hour = StringUtil.ToInt(x[3]);
            int minute = StringUtil.ToInt(x[4]);
            int second = StringUtil.ToInt(x[5]);
            if((year < 1970)||(year > 2038))
                return false;
            if(year == 2038){
                if(month > 1)
                    return false;
                if(day > 19)
                    return false;
                if(day == 19){
                    if(hour > 3)
                        return false;
                    if(hour == 3){
                        if(minute > 14)
                            return false;
                        if(minute == 14){
                            if(second > 7)
                                return false;
                        }
                    }
                }
            }
            DateTime dt = new DateTime(year, month, day, hour, minute, second);
            tm = (uint)((dt-BaseTime).TotalSeconds);
        } catch(Exception){
            // Just ignore it.
            return false;
        }

        return true;
    }

    public uint ToUint() {
        return tm;
    }
    public static explicit operator uint(UnixTime x) {
        return x.ToUint();
    }

    public override string ToString() {
        if(tm == 0)
            return "";
        DateTime dt = BaseTime.AddSeconds(tm);
        return string.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
    }

    public DateTime ToDateTime() {
        if(tm == 0)
            return new DateTime(0);
        return BaseTime.AddSeconds(tm);
    }

    public byte[] GetBytes(){
        return BitConverter.GetBytes(tm);
    }

    public bool Equals(UnixTime x){
        return tm == x.tm;
    }
    public bool Equals(uint x){
        return tm == x;
    }
    public override bool Equals(object obj){
        if(obj == null)
            return false;
        uint x;
        if(obj.GetType() == typeof(uint))
            x = (uint)obj;
        else if(obj.GetType() == typeof(UnixTime))
            x = ((UnixTime)obj).ToUint();
        else
            return false;
        return tm == x;
    }

    public override int GetHashCode(){
        return (int)tm;
    }

    public int CompareTo(UnixTime x){
        if(tm < x.tm)
            return -1;
        if(x.tm < tm)
            return 1;
        return 0;
    }
    public int CompareTo(uint x){
        if(tm < x)
            return -1;
        if(x < tm)
            return 1;
        return 0;
    }
    public int CompareTo(object obj){
        if(obj == null)
            return -1;
        uint x;
        if(obj.GetType() == typeof(uint))
            x = (uint)obj;
        else if(obj.GetType() == typeof(UnixTime))
            x = ((UnixTime)obj).ToUint();
        else
            return -1;
        return CompareTo(x);
    }

    public bool IsNone() {
        return tm == 0;
    }


    public static bool operator ==(UnixTime x0, UnixTime x1){
        return x0.Equals(x1);
    }
    public static bool operator ==(UnixTime x0, uint x1){
        return x0.tm == x1;
    }
    public static bool operator ==(uint x0, UnixTime x1){
        return x0 == x1.tm;
    }
    public static bool operator !=(UnixTime x0, UnixTime x1){
        return !x0.Equals(x1);
    }
    public static bool operator !=(UnixTime x0, uint x1){
        return x0.tm != x1;
    }
    public static bool operator !=(uint x0, UnixTime x1){
        return x0 != x1.tm;
    }

    public static bool operator <(UnixTime x0, UnixTime x1){
        return x0.tm < x1.tm;
    }
    public static bool operator <(UnixTime x0, uint x1){
        return x0.tm < x1;
    }
    public static bool operator <(uint x0, UnixTime x1){
        return x0 < x1.tm;
    }
    public static bool operator <=(UnixTime x0, UnixTime x1){
        return x0.tm <= x1.tm;
    }
    public static bool operator <=(UnixTime x0, uint x1){
        return x0.tm <= x1;
    }
    public static bool operator <=(uint x0, UnixTime x1){
        return x0 <= x1.tm;
    }

    public static bool operator >(UnixTime x0, UnixTime x1){
        return x0.tm > x1.tm;
    }
    public static bool operator >(UnixTime x0, uint x1){
        return x0.tm > x1;
    }
    public static bool operator >(uint x0, UnixTime x1){
        return x0 > x1.tm;
    }
    public static bool operator >=(UnixTime x0, UnixTime x1){
        return x0.tm >= x1.tm;
    }
    public static bool operator >=(UnixTime x0, uint x1){
        return x0.tm >= x1;
    }
    public static bool operator >=(uint x0, UnixTime x1){
        return x0 >= x1.tm;
    }

    public static UnixTime Now() {
        return new UnixTime((uint)((DateTime.Now-BaseTime).TotalSeconds));
    }

    public static UnixTime None = new UnixTime();


}

} // End of namespace
