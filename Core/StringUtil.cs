/// StringUtil: 文字列操作ユーティリティクラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace MACS {

/// <summary>
///   文字列操作ユーティリティクラス
/// </summary>
public class StringUtil {

    /// <summary>
    ///   intの安全で高速なパース。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    /// <remarks>
    ///   <para>
    ///     Integer.Parseと異なり、パースができない（数字以外の文字が含まれている）
    ///     場合でも例外を発生する事が無い。
    ///     数値区切りの','が入っていても無視する。
    ///   </para>
    /// </remarks>
    public static int ToInt(string str, int def=0){
        int res = 0;
        int sign = 1;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '-'){
                    sign = -1;
                    first = false;
                    continue;
                }
                if(ch == '+'){
                    sign = 1;
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 10;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case ',':
                res = res/10;
                break;
            case '.':
                return (res/10)*sign;
            default:
                return def;
            }
        }
        return res*sign;
    }

    /// <summary>
    ///   16進数値文字列の安全で高速なパース。
    ///   パースできない場合のデフォルト値をdefで与える。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static int ToHexInt(string str, int def=0){
        int res = 0;
        int sign = 1;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '-'){
                    sign = -1;
                    first = false;
                    continue;
                }
                if(ch == '+'){
                    sign = 1;
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 16;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case 'a':
                res += 10;
                break;
            case 'A':
                res += 10;
                break;
            case 'b':
                res += 11;
                break;
            case 'B':
                res += 11;
                break;
            case 'c':
                res += 12;
                break;
            case 'C':
                res += 12;
                break;
            case 'd':
                res += 13;
                break;
            case 'D':
                res += 13;
                break;
            case 'e':
                res += 14;
                break;
            case 'E':
                res += 14;
                break;
            case 'f':
                res += 15;
                break;
            case 'F':
                res += 15;
                break;
            default:
                return def;
            }
        }
        return res*sign;
    }

    /// <summary>
    ///   16進数値文字列の安全で高速なパース。
    ///   パースできない場合のデフォルト値をdefで与える。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static uint ToHexUInt(string str, uint def=0){
        uint res = 0;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '+'){
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 16;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case 'a':
                res += 10;
                break;
            case 'A':
                res += 10;
                break;
            case 'b':
                res += 11;
                break;
            case 'B':
                res += 11;
                break;
            case 'c':
                res += 12;
                break;
            case 'C':
                res += 12;
                break;
            case 'd':
                res += 13;
                break;
            case 'D':
                res += 13;
                break;
            case 'e':
                res += 14;
                break;
            case 'E':
                res += 14;
                break;
            case 'f':
                res += 15;
                break;
            case 'F':
                res += 15;
                break;
            default:
                return def;
            }
        }
        return res;
    }

    /// <summary>
    ///   longの安全で高速なパース。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    /// <remarks>
    ///   <para>
    ///     Long.Parseと異なり、パースができない（数字以外の文字が含まれている）
    ///     場合でも例外を発生する事が無い。
    ///   </para>
    /// </remarks>
    public static long ToLong(string str, long def=0){
        long res = 0;
        long sign = 1;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '-'){
                    sign = -1;
                    first = false;
                    continue;
                }
                if(ch == '+'){
                    sign = 1;
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 10;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case ',':
                res = res/10;
                break;
            case '.':
                return (res/10)*sign;
            default:
                return def;
            }
        }
        return res*sign;
    }

    /// <summary>
    ///   ulongの安全で高速なパース。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    /// <remarks>
    ///   <para>
    ///     Long.Parseと異なり、パースができない（数字以外の文字が含まれている）
    ///     場合でも例外を発生する事が無い。
    ///   </para>
    /// </remarks>
    public static ulong ToULong(string str, ulong def=0){
        ulong res = 0;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '+'){
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 10;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case ',':
                res = res/10;
                break;
            case '.':
                return (res/10);
            default:
                return def;
            }
        }
        return res;
    }

    /// <summary>
    ///   16進数値文字列の安全で高速なパース。
    ///   パースできない場合のデフォルト値をdefで与える。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static long ToHexLong(string str, long def=0){
        long res = 0;
        long sign = 1;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '-'){
                    sign = -1;
                    first = false;
                    continue;
                }
                if(ch == '+'){
                    sign = 1;
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 16;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case 'a':
                res += 10;
                break;
            case 'A':
                res += 10;
                break;
            case 'b':
                res += 11;
                break;
            case 'B':
                res += 11;
                break;
            case 'c':
                res += 12;
                break;
            case 'C':
                res += 12;
                break;
            case 'd':
                res += 13;
                break;
            case 'D':
                res += 13;
                break;
            case 'e':
                res += 14;
                break;
            case 'E':
                res += 14;
                break;
            case 'f':
                res += 15;
                break;
            case 'F':
                res += 15;
                break;
            default:
                return def;
            }
        }
        return res*sign;
    }

    /// <summary>
    ///   16進数値文字列の安全で高速なパース。
    ///   パースできない場合のデフォルト値をdefで与える。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static ulong ToHexULong(string str, ulong def=0){
        ulong res = 0;
        bool first = true;
        if(string.IsNullOrEmpty(str))
            return def;
        foreach(char ch in str){
            if(first){
                if((ch == ' ') || (ch == '\t'))
                    continue;
                if(ch == '+'){
                    first = false;
                    continue;
                }
            }
            first = false;
            res *= 16;
            switch(ch){
            case '0':
                res += 0;
                break;
            case '1':
                res += 1;
                break;
            case '2':
                res += 2;
                break;
            case '3':
                res += 3;
                break;
            case '4':
                res += 4;
                break;
            case '5':
                res += 5;
                break;
            case '6':
                res += 6;
                break;
            case '7':
                res += 7;
                break;
            case '8':
                res += 8;
                break;
            case '9':
                res += 9;
                break;
            case 'a':
                res += 10;
                break;
            case 'A':
                res += 10;
                break;
            case 'b':
                res += 11;
                break;
            case 'B':
                res += 11;
                break;
            case 'c':
                res += 12;
                break;
            case 'C':
                res += 12;
                break;
            case 'd':
                res += 13;
                break;
            case 'D':
                res += 13;
                break;
            case 'e':
                res += 14;
                break;
            case 'E':
                res += 14;
                break;
            case 'f':
                res += 15;
                break;
            case 'F':
                res += 15;
                break;
            default:
                return def;
            }
        }
        return res;
    }

    /// <summary>
    ///   uintの安全で高速なパース。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static uint ToUInt(string str, uint def=0){
        return (uint)ToULong(str, (ulong)def);
    }

    /// <summary>
    ///   floatの安全なパース。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static float ToFloat(string str, float def){
        if(String.IsNullOrEmpty(str))
            return def;
        float res;
        if(!Single.TryParse(str, NumberStyles.Number|NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out res))
            return def;
        return res;
    }

    /// <summary>
    ///   floatの安全なパース。
    ///   パースできない場合のデフォルトは0.0。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    public static float ToFloat(string str){
        return ToFloat(str, (float)0.0);
    }

    /// <summary>
    ///   doubleの安全なパース。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    /// <param name="def">パースできない場合のデフォルト値</param>
    public static double ToDouble(string str, double def){
        if(String.IsNullOrEmpty(str))
            return def;
        double res;
        if(!Double.TryParse(str, NumberStyles.Number|NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out res))
            return def;
        return res;
    }

    /// <summary>
    ///   doubleの安全なパース。
    ///   パースできない場合のデフォルトは0.0。
    /// </summary>
    /// <param name="str">パースする文字列</param>
    public static double ToDouble(string str){
        return ToDouble(str, 0.0);
    }

    /// <summary>
    ///   文字列が"false","no","0",空文字列,null以外の時はtrueを返す。
    ///   文字列が空文字列,nullの時はdefvalを返す。
    /// </summary>
    public static bool ToBool(string str, bool defval=false) {
        if(String.IsNullOrEmpty(str))
            return defval;
        str = str.ToLower();
        if((str == "false") || (str == "no") || (str == "0"))
            return false;
        return true;
    }

    /// <summary>
    ///   末尾のヌル文字を取り除いた文字列を返す。
    /// </summary>
    /// <param name="txt">元の文字列</param>
    public static string TrimNullChar(string txt) {
        int idx = txt.IndexOf('\u0000');
        if(idx >= 0)
            return txt.Substring(0, idx);
        return txt;
    }

    /// <summary>
    ///   16進数文字列をbyte列に変換する。
    /// </summary>
    /// <param name="src">パースする文字列</param>
    /// <param name="bytes">バイト列バッファ</param>
    /// <param name="offset">バッファ内格納先頭位置</param>
    /// <param name="size">バッファ格納サイズ</param>
    /// <remarks>
    ///   <para>
    ///     文字列にsizeバイト分のデータが無い場合、足りない分は0で埋められる。
    ///   </para>
    /// </remarks>
    public static void ToBytes(string src, byte[] bytes, int offset, int size) {
        int len;
        if(src == null){
            len = 0;
        }else{
            len = src.Length/2;
            for(int i = 0; (i < len) && (i < size); i++){
                switch(src[i*2]){
                case '1':
                    bytes[i+offset] = 1;
                    break;
                case '2':
                    bytes[i+offset] = 2;
                    break;
                case '3':
                    bytes[i+offset] = 3;
                    break;
                case '4':
                    bytes[i+offset] = 4;
                    break;
                case '5':
                    bytes[i+offset] = 5;
                    break;
                case '6':
                    bytes[i+offset] = 6;
                    break;
                case '7':
                    bytes[i+offset] = 7;
                    break;
                case '8':
                    bytes[i+offset] = 8;
                    break;
                case '9':
                    bytes[i+offset] = 9;
                    break;
                case 'a':
                case 'A':
                    bytes[i+offset] = 10;
                    break;
                case 'b':
                case 'B':
                    bytes[i+offset] = 11;
                    break;
                case 'c':
                case 'C':
                    bytes[i+offset] = 12;
                    break;
                case 'd':
                case 'D':
                    bytes[i+offset] = 13;
                    break;
                case 'e':
                case 'E':
                    bytes[i+offset] = 14;
                    break;
                case 'f':
                case 'F':
                    bytes[i+offset] = 15;
                    break;
                default:
                    bytes[i+offset] = 0;
                    break;
                }
                bytes[i+offset] *= 16;
                switch(src[i*2+1]){
                case '1':
                    bytes[i+offset] += 1;
                    break;
                case '2':
                    bytes[i+offset] += 2;
                    break;
                case '3':
                    bytes[i+offset] += 3;
                    break;
                case '4':
                    bytes[i+offset] += 4;
                    break;
                case '5':
                    bytes[i+offset] += 5;
                    break;
                case '6':
                    bytes[i+offset] += 6;
                    break;
                case '7':
                    bytes[i+offset] += 7;
                    break;
                case '8':
                    bytes[i+offset] += 8;
                    break;
                case '9':
                    bytes[i+offset] += 9;
                    break;
                case 'a':
                case 'A':
                    bytes[i+offset] += 10;
                    break;
                case 'b':
                case 'B':
                    bytes[i+offset] += 11;
                    break;
                case 'c':
                case 'C':
                    bytes[i+offset] += 12;
                    break;
                case 'd':
                case 'D':
                    bytes[i+offset] += 13;
                    break;
                case 'e':
                case 'E':
                    bytes[i+offset] += 14;
                    break;
                case 'f':
                case 'F':
                    bytes[i+offset] += 15;
                    break;
                default:
                    bytes[i+offset] += 0;
                    break;
                }
            }
        }
        for(int i = len; i < size; i++)
            bytes[i+offset] = 0;
    }

    /// <summary>
    ///   16進数文字列をbyte列に変換する。offsetは0、sizeはbytes.Length
    /// </summary>
    /// <param name="src">パースする文字列</param>
    /// <param name="bytes">バイト列バッファ</param>
    public static void ToBytes(string src, byte[] bytes) {
        ToBytes(src, bytes, 0, bytes.Length);
    }

    /// <summary>
    ///   年月日の文字列をパースしてDateTimeを返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     変換不能な文字列の場合にはDateTime(0)を返します。
    ///     ただし、数値の範囲異常は適宜調整してDateTime化します。
    ///   </para>
    /// </remarks>
    public static DateTime ToDateTime(string str) {
        if(str == null)
            return new DateTime(0);
        try {
            string[] x = str.Split(" /:-,.;".ToCharArray());
            int year,month,day,hour,minute,second;
            switch(x.Length) {
            case 1: // 項目が1つしかなければ、1年1月1日からの日数ということにする
                return (new DateTime(0)).AddDays(ToInt(x[0]));
            case 2: // 項目が2つなら、今年の月日ということにする
                year = DateTime.Now.Year;
                month = ToInt(x[0], Int32.MinValue);
                day = ToInt(x[1], Int32.MinValue);
                hour = 0;
                minute = 0;
                second = 0;
                break;
            case 3: // 項目が3つなら、年月日
                year = ToInt(x[0], Int32.MinValue);
                month = ToInt(x[1], Int32.MinValue);
                day = ToInt(x[2], Int32.MinValue);
                hour = 0;
                minute = 0;
                second = 0;
                break;
            case 4: // 項目が4つなら、年月日時
                year = ToInt(x[0], Int32.MinValue);
                month = ToInt(x[1], Int32.MinValue);
                day = ToInt(x[2], Int32.MinValue);
                hour = ToInt(x[3], Int32.MinValue);
                minute = 0;
                second = 0;
                break;
            case 5: // 項目が5つなら、年月日時分
                year = ToInt(x[0], Int32.MinValue);
                month = ToInt(x[1], Int32.MinValue);
                day = ToInt(x[2], Int32.MinValue);
                hour = ToInt(x[3], Int32.MinValue);
                minute = ToInt(x[4], Int32.MinValue);
                second = 0;
                break;
            default: // 項目が6つ以上なら、年月日時分秒
                year = ToInt(x[0], Int32.MinValue);
                month = ToInt(x[1], Int32.MinValue);
                day = ToInt(x[2], Int32.MinValue);
                hour = ToInt(x[3], Int32.MinValue);
                minute = ToInt(x[4], Int32.MinValue);
                second = ToInt(x[5], Int32.MinValue);
                break;
            }
            int add = 0;
            if(year == Int32.MinValue)
                return new DateTime(0);
            if(year < 1)
                year = 1;
            if(year > 9999)
                year = 9999;
            if(month == Int32.MinValue)
                return new DateTime(0);
            if(month < 1)
                month = 1;
            if(month > 12)
                month = 12;
            if(day == Int32.MinValue)
                return new DateTime(0);
            if(day < 1)
                day = 1;
            if(day > DateTime.DaysInMonth(year,month))
                day = DateTime.DaysInMonth(year,month);
            if(hour == Int32.MinValue)
                return new DateTime(0);
            while(hour < 0){
                add -= 24*60*60;
                hour += 24;
            }
            while(hour >= 24){
                add += 24*60*60;
                hour -= 24;
            }
            if(minute == Int32.MinValue)
                return new DateTime(0);
            while(minute < 0){
                add -= 60*60;
                minute += 60;
            }
            while(minute >= 60){
                add += 60*60;
                minute -= 60;
            }
            if(second == Int32.MinValue)
                return new DateTime(0);
            while(second < 0){
                add -= 60;
                second += 60;
            }
            while(second >= 60){
                add += 60;
                second -= 60;
            }
            DateTime dt = new DateTime(year,month,day,hour,minute,second);
            if(add != 0)
                dt = dt.AddSeconds(add);
            return dt;
        } catch(ArgumentException) {
            return new DateTime(0);
        }
    }


    /// <summary>
    ///   安全な部分文字列取り出し
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字数が足りない場合には足りる分だけ、または空文字列を返す。
    ///   </para>
    /// </remarks>
    public static string Substring(string txt, int pos, int len) {
        if(txt == null)
            return null;
        if(txt.Length < pos)
            return "";
        if(txt.Length < pos+len)
            return txt.Substring(pos);
        return txt.Substring(pos, len);
    }

    /// <summary>
    ///   文字列の末尾の指定文字を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字数が足りない場合には文字列全体を返す
    ///   </para>
    /// </remarks>
    public static string LastString(string txt, int n) {
        if(txt.Length < n)
            return txt;
        return txt.Substring(txt.Length-n, n);
    }

    /// <summary>
    ///   数値文字列を位取り付きでフォーマットする
    /// </summary>
    /// <param name="obj">対象文字列(ToString()で文字列化される)</param>
    /// <param name="fractional">小数点下桁数(default=0)</param>
    /// <param name="forceSign">正負の符号を必ずつけるかどうか(default=false)</param>
    /// <param name="forceFraction">小数点以下を必ずつけるかどうか(default=true)</param>
    /// <remarks>
    ///   <para>
    ///     forceSign==trueの時には、正の数の時には"+"、負の数の時には"-"がつく。
    ///     デフォルトはforceSign=falseで、負の時にのみ"-"がつく。
    ///     forceFraction==trueの時には、指定した小数点下桁数まで必ず出力される。
    ///     forceFraction==falseの時には、小数点下の値が無いときには小数点より下は出力されない。
    ///   </para>
    /// </remarks>
    public static string DecimalString(object obj, int fractional=0, bool forceSign=false, bool forceFraction=true) {
        if(obj == null)
            return "";
        double d = ToDouble(obj.ToString().Replace(",",""));
        if(!forceFraction && (d == Math.Truncate(d)))
            fractional = 0;
        string x = d.ToString(String.Format("N{0}", fractional));
        if(!forceSign || (x[0] == '0') || (x[0] == '-'))
            return x;
        return "+"+x;
    }

    /// <summary>
    ///   数値文字列を位取り付きでフォーマットする
    /// </summary>
    /// <param name="obj">対象文字列(ToString()で文字列化される)</param>
    /// <param name="fractional">小数点下桁数</param>
    /// <param name="length">全文字数</param>
    /// <param name="forceSign">正負の符号を必ずつけるかどうか(default=false)</param>
    /// <param name="forceFraction">小数点以下を必ずつけるかどうか(default=true)</param>
    /// <remarks>
    ///   <para>
    ///     length文字で右詰めになる。
    ///     forceSign==trueの時には、正の数の時には"+"、負の数の時には"-"がつく。
    ///     デフォルトはforceSign=falseで、負の時にのみ"-"がつく。
    ///     forceFraction==trueの時には、指定した小数点下桁数まで必ず出力される。
    ///     forceFraction==falseの時には、小数点下の値が無いときには小数点より下は出力されない。
    ///   </para>
    /// </remarks>
    public static string DecimalString(object obj, int fractional, int length, bool forceSign=false, bool forceFraction=true) {
        return FixLength(DecimalString(obj, fractional, forceSign, forceFraction), length, ' ');
    }

    /// <summary>
    ///   文字列を固定長でフォーマットする
    /// </summary>
    /// <param name="obj">対象文字列(ToString()で文字列化される)</param>
    /// <param name="len">文字数</param>
    /// <param name="fill">指定文字数以下の時に左側に埋められる文字</param>
    /// <remarks>
    ///   <para>
    ///     元の文字列が指定文字数よりも長い場合は、元の文字列の末尾の指定文字数
    ///     が返る。
    ///   </para>
    /// </remarks>
    public static string FixLength(object obj, int len, char fill=' ') {
        char[] f;
        if(obj == null) {
            f = new char[len];
            for(int i = 0; i < len; i++)
                f[i] = fill;
            return new String(f);
        }
        string txt = LastString(obj.ToString(), len);
        if(txt.Length >= len)
            return txt;
        f = new char[len-txt.Length];
        for(int i = 0; i < f.Length; i++)
            f[i] = fill;
        return new String(f)+txt;
    }

    /// <summary>
    ///   文字列を固定長でフォーマットする
    /// </summary>
    /// <param name="obj">対象文字列(ToString()で文字列化される)</param>
    /// <param name="len">文字数</param>
    /// <param name="fill">指定文字数以下の時に右側に埋められる文字</param>
    /// <remarks>
    ///   <para>
    ///     元の文字列が指定文字数よりも長い場合は、元の文字列の先頭の指定文字数
    ///     が返る。
    ///   </para>
    /// </remarks>
    public static string FixLengthRight(object obj, int len, char fill=' ') {
        char[] f;
        if(obj == null) {
            f = new char[len];
            for(int i = 0; i < len; i++)
                f[i] = fill;
            return new String(f);
        }
        string txt = obj.ToString();
        if(txt.Length >= len)
            return txt.Substring(0, len);
        f = new char[len-txt.Length];
        for(int i = 0; i < f.Length; i++)
            f[i] = fill;
        return txt+(new String(f));
    }

    /// <summary>
    ///   UTF8にしたときのバイト数が指定値以下になるような文字列を返す
    /// </summary>
    public static string FixBytesUTF8(object obj, int bytes) {
        Encoding enc = new UTF8Encoding(false);
        byte[] b = enc.GetBytes(obj.ToString());
        if(b.Length <= bytes)
            return enc.GetString(b);
        while(bytes > 0) {
            byte ch = b[bytes];
            if((ch < 0x80) || (ch > 0xbf))
                break;
            bytes--;
        }
        return enc.GetString(b, 0, bytes);
    }

    /// <summary>
    ///   文字列配列の全要素に同じ値をセットする
    /// </summary>
    public static string[] Fill(string[] list, string val) {
        if(list != null) {
            for(int i = 0; i < list.Length; i++)
                list[i] = val;
        }
        return list;
    }

    /// <summary>
    ///   全角文字を半角にする
    /// </summary>
    public static string CompactString(string txt) {
        if(txt == null)
            return null;
        if(zenhanDict == null)
            setupZenhanDict();
        StringBuilder sb = new StringBuilder();
        foreach(char ch in txt) {
            if(zenhanDict.ContainsKey(ch))
                sb.Append(zenhanDict[ch]);
            else
                sb.Append(ch);
        }
        return sb.ToString();
    }

    /// <summary>
    ///   半角カタカナを全角にする
    /// </summary>
    /// <param name="txt">対象文字列</param>
    /// <param name="hiraganaFlag">ひらがなも全角カタカナにするかどうか</param>
    public static string ToZenkana(string txt, bool hiraganaFlag=false) {
        if(txt == null)
            return null;
        if(hankanaDict1 == null)
            setupHankanaDict();
        StringBuilder sb = new StringBuilder();
        Dictionary<char,char> d;
        char c;
        int i = 0;
        while(i < txt.Length) {
            char ch = txt[i];
            if((i < txt.Length-1) && hankanaDict2.TryGetValue(ch, out d) && d.TryGetValue(txt[i+1], out c)) {
                sb.Append(c);
                i++;
            } else if(hankanaDict1.TryGetValue(ch, out c)) {
                sb.Append(c);
            } else if(hiraganaFlag && (ch >= 'ぁ') && (ch <= 'ゖ')) {
                sb.Append(Convert.ToChar(ch-'ぁ'+'ァ'));
            } else {
                sb.Append(ch);
            }
            i++;
        }
        return sb.ToString();
    }
    
    
    /// <summary>
    ///   カンマ区切り文字列をSplitする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カンマの前後の空白とタブは無視される。
    ///     項目がダブルクォートまたはシングルクォートで囲まれている場合はその
    ///     クォート文字は無視される。
    ///     バックスラッシュの次の1文字はカンマやクォート文字であっても特別な
    ///     意味を持たないものとする。
    ///     txtがnullの場合、長さゼロの配列を返す。
    ///     txtがnullでない場合、最低でも長さ1の配列を返す。
    ///   </para>
    /// </remarks>
    public static string[] SplitCSV(string txt) {
        return SplitQuote(txt, ",".ToCharArray(), true, false);
    }

    /// <summary>
    ///   コマンド文字列をSplitする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     文字列を空白でSplitする。連続する空白は一つの区切り文字として扱われる。
    ///     項目がダブルクォートまたはシングルクォートで囲まれている場合はその
    ///     クォート文字は無視される。
    ///     バックスラッシュの次の1文字はカンマやクォート文字であっても特別な
    ///     意味を持たないものとする。
    ///     txtがnullの場合や空文字列または空白だけの文字列の場合、長さゼロの配列を返す。
    ///   </para>
    /// </remarks>
    public static string[] SplitCommand(string txt) {
        return SplitQuote(txt, " \t\r\n".ToCharArray(), true, true);
    }

    /// <summary>
    ///   クォート文字を考慮して文字列をSplitする
    /// </summary>
    /// <param name="txt">Splitする文字列</param>
    /// <param name="separator">区切り文字</param>
    /// <param name="trimElement">各要素の前後の空白文字を削除するかどうか</param>
    /// <param name="ignoreBlank">空要素を無視するかどうか</param>
    /// <remarks>
    ///   <para>
    ///     カンマの前後の空白とタブは無視される。
    ///     項目がダブルクォートまたはシングルクォートで囲まれている場合はその
    ///     クォート文字は無視される。
    ///     バックスラッシュの次の1文字はカンマやクォート文字であっても特別な
    ///     意味を持たないものとする。
    ///     txtがnullの場合、長さゼロの配列を返す。
    ///     txtがnullでない場合、最低でも長さ1の配列を返す。
    ///   </para>
    /// </remarks>
    public static string[] SplitQuote(string txt, char[] separator, bool trimElement=true, bool ignoreBlank=false) {
        List<string> res = new List<string>();
        if(txt == null)
            return res.ToArray();
        StringBuilder sb = new StringBuilder();
        char inQuote = '\0';
        bool inEscape = false;
        foreach(char ch in txt) {
            if(inEscape) {
                sb.Append(ch);
                inEscape = false;
                continue;
            }
            if(inQuote != '\0') {
                if(ch == inQuote) {
                    inQuote = '\0';
                    continue;
                }
                sb.Append(ch);
                continue;
            }
            if(ch == '\\') {
                inEscape = true;
                continue;
            }
            if(In(ch,separator)) {
                string x = sb.ToString();
                if(trimElement)
                    x = x.Trim();
                if(!ignoreBlank || (x != ""))
                    res.Add(x);
                sb.Clear();
                continue;
            }
            if(sb.Length == 0) {
                if((ch == '"')||(ch == '\'')) {
                    inQuote = ch;
                    continue;
                }
            }
            sb.Append(ch);
        }
        string xx = sb.ToString();
        if(trimElement)
            xx = xx.Trim();
        if(!ignoreBlank || (xx != ""))
            res.Add(xx);
        return res.ToArray();
    }

    /// <summary>
    ///   文字列を結合して1つの文字列にする
    /// </summary>
    /// <param name="separator">文字列間に入れる文字列</param>
    /// <param name="strings">文字列の配列</param>
    public static string Join(string separator, params string[] strings) {
        return Join(new StringBuilder(), separator, strings, 0).ToString();
    }

    /// <summary>
    ///   文字列を結合して1つの文字列にする
    /// </summary>
    /// <param name="separator">文字列間に入れる文字列</param>
    /// <param name="strings">文字列の配列</param>
    /// <param name="start">Joinする要素の先頭インデックス</param>
    /// <param name="length">Joinする要素数（負の数を指定すると最終要素までJoinする）</param>
    public static string Join(string separator, string[] strings, int start, int length=-1) {
        return Join(new StringBuilder(), separator, strings, start, length).ToString();
    }

    /// <summary>
    ///   文字列を結合してStringBuilderに追加する
    /// </summary>
    /// <param name="sb">文字列を追加するStringBuilder</param>
    /// <param name="separator">文字列間に入れる文字列</param>
    /// <param name="strings">文字列の配列</param>
    public static StringBuilder Join(StringBuilder sb, string separator, params string[] strings) {
        return Join(sb, separator, strings, 0);
    }
    
    /// <summary>
    ///   文字列を結合してStringBuilderに追加する
    /// </summary>
    /// <param name="sb">文字列を追加するStringBuilder</param>
    /// <param name="separator">文字列間に入れる文字列</param>
    /// <param name="strings">文字列の配列</param>
    /// <param name="start">Joinする要素の先頭インデックス</param>
    /// <param name="length">Joinする要素数（負の数を指定すると最終要素までJoinする）</param>
    public static StringBuilder Join(StringBuilder sb, string separator, string[] strings, int start, int length=-1) {
        if(separator == null)
            separator = "";
        if(start < 0)
            start = 0;
        if(start >= strings.Length)
            return sb;
        int end;
        if(length < 0) {
            end = strings.Length;
        } else {
            end = length+start;
            if(end > strings.Length)
                end = strings.Length;
        }
        bool first = true;
        for(int i = start; i < end; i++) {
            if(first)
                first = false;
            else
                sb.Append(separator);
            sb.Append(strings[i]);
        }
        return sb;
    }

    /// <summary>
    ///   文字列を結合して1つの文字列にする
    /// </summary>
    /// <param name="separator">文字列間に入れる文字列</param>
    /// <param name="strings">文字列の配列</param>
    /// <param name="start">Joinする要素の先頭インデックス</param>
    /// <param name="length">Joinする要素数（負の数を指定すると最終要素までJoinする）</param>
    public static string Join(string separator, List<string> strings, int start=0, int length=-1) {
        return Join(new StringBuilder(), separator, strings, start, length).ToString();
    }

    /// <summary>
    ///   文字列を結合してStringBuilderに追加する
    /// </summary>
    /// <param name="sb">文字列を追加するStringBuilder</param>
    /// <param name="separator">文字列間に入れる文字列</param>
    /// <param name="strings">文字列の配列</param>
    /// <param name="start">Joinする要素の先頭インデックス</param>
    /// <param name="length">Joinする要素数（負の数を指定すると最終要素までJoinする）</param>
    public static StringBuilder Join(StringBuilder sb, string separator, List<string> strings, int start=0, int length=-1) {
        if(separator == null)
            separator = "";
        if(start < 0)
            start = 0;
        if(start >= strings.Count)
            return sb;
        int end;
        if(length < 0) {
            end = strings.Count;
        } else {
            end = length+start;
            if(end > strings.Count)
                end = strings.Count;
        }
        bool first = true;
        for(int i = start; i < end; i++) {
            if(first)
                first = false;
            else
                sb.Append(separator);
            sb.Append(strings[i]);
        }
        return sb;
    }

    /// <summary>
    ///   指定文字が文字配列中にあるかどうか
    /// </summary>
    public static bool In(char ch, char[] array) {
        if(array == null)
            return false;
        foreach(char c in array) {
            if(c == ch)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   指定文字列が文字列配列中にあるかどうか
    /// </summary>
    public static bool In(string str, string[] array) {
        if(array == null)
            return false;
        foreach(string s in array) {
            if(s == str)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   指定文字列をキャメル表記にした文字列を返す
    /// </summary>
    public static string ToCamel(string str) {
        if(str == null)
            return null;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        bool camel = false;
        foreach(char ch in str) {
            if(camel) {
                sb.Append(Char.ToUpper(ch));
                camel = false;
            } else if(ch == '_') {
                camel = true;
            } else if(first) {
                sb.Append(ch);
            } else {
                sb.Append(Char.ToLower(ch));
            }
            first = false;
        }
        return sb.ToString();
    }

    /// <summary>
    ///   先頭の1文字を大文字にした文字列を返す
    /// </summary>
    public static string Capitalize(string str) {
        if(str == null)
            return null;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        foreach(char ch in str) {
            if(first){
                sb.Append(Char.ToUpper(ch));
                first = false;
            } else {
                sb.Append(Char.ToLower(ch));
            }
        }
        return sb.ToString();
    }

    public static readonly string[] SimilarCharList = new string[]{
        "AaＡａ",
        "BbＢｂ",
        "CcＣｃ",
        "DdＤｄ",
        "EeＥｅ",
        "FfＦｆ",
        "GgＧｇ",
        "HhＨｈ",
        "IiＩｉ",
        "JjＪｊ",
        "KkＫｋ",
        "LlＬｌ",
        "MmＭｍ",
        "NnＮｎ",
        "OoＯｏ",
        "PpＰｐ",
        "QqＱｑ",
        "RrＲｒ",
        "SsＳｓ",
        "TtＴｔ",
        "UuＵｕ",
        "VvＶｖ",
        "WwＷｗ",
        "XxＸｘ",
        "YyＹｙ",
        "ZzＺｚ",
        "あぁアァｱｧ",
        "いぃイィｲｨ",
        "うぅウゥｳｩ",
        "えぇエェｴｪ",
        "おぉオォｵｫ",
        "かがカガヵｶ",
        "きぎキギｷ",
        "くぐクグｸ",
        "けげケゲヶｹ",
        "こごコゴｺ",
        "さざサザｻ",
        "しじシジｼ",
        "すずスズｽ",
        "せぜセゼｾ",
        "そぞソゾｿ",
        "ただタダﾀ",
        "ちぢチヂﾁ",
        "つづっツヅッﾂｯ",
        "てでテデﾃ",
        "とどトドﾄ",
        "なナﾅ",
        "にニﾆ",
        "ぬヌﾇ",
        "ねネﾈ",
        "のノﾉ",
        "はばぱハバパﾊ",
        "ひびぴヒビピﾋ",
        "ふぶぷフブプﾌ",
        "へべぺヘベペﾍ",
        "ほぼぽホボポﾎ",
        "まマﾏ",
        "みミﾐ",
        "むムﾑ",
        "めメﾒ",
        "もモﾓ",
        "やゃヤャﾔｬ",
        "ゆゅユュﾕｭ",
        "よょヨョﾖｮ",
        "らラﾗ",
        "りリﾘ",
        "るルﾙ",
        "れレﾚ",
        "ろロﾛ",
        "わワﾜ",
        "ゐヰ",
        "ゑヱ",
        "をヲｦ",
        "んンﾝ",
        "斎斉齋齊",
        "辺邊邉",
        "渋澁",
        "国國",
        "高髙",
    };

    /// <summary>
    ///   指定文字と同じ文字（半角全角の違いやひらがなカタカナの違いなど）の
    ///   リストを返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定文字もリストに含まれます。
    ///   </para>
    /// </remarks>
    public static char[] GetSimilarChars(char ch) {
        foreach(string str in SimilarCharList) {
            foreach(char xch in str) {
                if(xch == ch)
                    return str.ToCharArray();
            }
        }
        return new char[]{ch};
    }

    /// <summary>
    ///   指定文字列を類似文字の代表文字で置き換えたものを返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     IsSimilarToPatternなどの第2引数に用います。
    ///   </para>
    /// </remarks>
    public static string ToSimilarPattern(string str) {
        StringBuilder sb = new StringBuilder();
        foreach(char ch in str) {
            if((ch == 'ﾞ') || (ch == 'ﾟ'))
                continue;
            sb.Append(GetSimilarChars(ch)[0]);
        }
        return sb.ToString();
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して一致判別する
    /// </summary>
    public static bool IsSimilarTo(string str1, string str2) {
        return IsSimilarToPattern(str1, ToSimilarPattern(str2));
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して一致判別する
    /// </summary>
    public static bool IsSimilarToPattern(string str1, string str2) {
        return (ToSimilarPattern(str1) == str2);
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して前方一致判別する
    /// </summary>
    public static bool StartsWithSimilar(string str1, string str2) {
        return StartsWithSimilarPattern(str1, ToSimilarPattern(str2));
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して前方一致判別する
    /// </summary>
    public static bool StartsWithSimilarPattern(string str1, string str2) {
        return (ToSimilarPattern(str1).StartsWith(str2));
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して後方一致判別する
    /// </summary>
    public static bool EndsWithSimilar(string str1, string str2) {
        return EndsWithSimilarPattern(str1, ToSimilarPattern(str2));
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して後方一致判別する
    /// </summary>
    public static bool EndsWithSimilarPattern(string str1, string str2) {
        return (ToSimilarPattern(str1).EndsWith(str2));
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して包含判別する
    /// </summary>
    public static bool ContainsSimilar(string str1, string str2) {
        return ContainsSimilarPattern(str1, ToSimilarPattern(str2));
    }

    /// <summary>
    ///   文字列を全角半角／ひらかなカタカナなどの違いを無視して包含判別する
    /// </summary>
    public static bool ContainsSimilarPattern(string str1, string str2) {
        return (ToSimilarPattern(str1).Contains(str2));
    }

    
    private static Dictionary<char,string> zenhanDict = null;

    private static void setupZenhanDict() {
        zenhanDict = new Dictionary<char,string>(){
            {'　'," "},
            {'Ａ',"A"},
            {'Ｂ',"B"},
            {'Ｃ',"C"},
            {'Ｄ',"D"},
            {'Ｅ',"E"},
            {'Ｆ',"F"},
            {'Ｇ',"G"},
            {'Ｈ',"H"},
            {'Ｉ',"I"},
            {'Ｊ',"J"},
            {'Ｋ',"K"},
            {'Ｌ',"L"},
            {'Ｍ',"M"},
            {'Ｎ',"N"},
            {'Ｏ',"O"},
            {'Ｐ',"P"},
            {'Ｑ',"Q"},
            {'Ｒ',"R"},
            {'Ｓ',"S"},
            {'Ｔ',"T"},
            {'Ｕ',"U"},
            {'Ｖ',"V"},
            {'Ｗ',"W"},
            {'Ｘ',"X"},
            {'Ｙ',"Y"},
            {'Ｚ',"Z"},
            {'ａ',"a"},
            {'ｂ',"b"},
            {'ｃ',"c"},
            {'ｄ',"d"},
            {'ｅ',"e"},
            {'ｆ',"f"},
            {'ｇ',"g"},
            {'ｈ',"h"},
            {'ｉ',"i"},
            {'ｊ',"j"},
            {'ｋ',"k"},
            {'ｌ',"l"},
            {'ｍ',"m"},
            {'ｎ',"n"},
            {'ｏ',"o"},
            {'ｐ',"p"},
            {'ｑ',"q"},
            {'ｒ',"r"},
            {'ｓ',"s"},
            {'ｔ',"t"},
            {'ｕ',"u"},
            {'ｖ',"v"},
            {'ｗ',"w"},
            {'ｘ',"x"},
            {'ｙ',"y"},
            {'ｚ',"z"},
            {'１',"1"},
            {'２',"2"},
            {'３',"3"},
            {'４',"4"},
            {'５',"5"},
            {'６',"6"},
            {'７',"7"},
            {'８',"8"},
            {'９',"9"},
            {'０',"0"},
            {'ア',"ｱ"},
            {'イ',"ｲ"},
            {'ウ',"ｳ"},
            {'エ',"ｴ"},
            {'オ',"ｵ"},
            {'カ',"ｶ"},
            {'キ',"ｷ"},
            {'ク',"ｸ"},
            {'ケ',"ｹ"},
            {'コ',"ｺ"},
            {'サ',"ｻ"},
            {'シ',"ｼ"},
            {'ス',"ｽ"},
            {'セ',"ｾ"},
            {'ソ',"ｿ"},
            {'タ',"ﾀ"},
            {'チ',"ﾁ"},
            {'ツ',"ﾂ"},
            {'テ',"ﾃ"},
            {'ト',"ﾄ"},
            {'ナ',"ﾅ"},
            {'ニ',"ﾆ"},
            {'ヌ',"ﾇ"},
            {'ネ',"ﾈ"},
            {'ノ',"ﾉ"},
            {'ハ',"ﾊ"},
            {'ヒ',"ﾋ"},
            {'フ',"ﾌ"},
            {'ヘ',"ﾍ"},
            {'ホ',"ﾎ"},
            {'マ',"ﾏ"},
            {'ミ',"ﾐ"},
            {'ム',"ﾑ"},
            {'メ',"ﾒ"},
            {'モ',"ﾓ"},
            {'ヤ',"ﾔ"},
            {'ユ',"ﾕ"},
            {'ヨ',"ﾖ"},
            {'ラ',"ﾗ"},
            {'リ',"ﾘ"},
            {'ル',"ﾙ"},
            {'レ',"ﾚ"},
            {'ロ',"ﾛ"},
            {'ワ',"ﾜ"},
            {'ヲ',"ｦ"},
            {'ン',"ﾝ"},
            {'ガ',"ｶﾞ"},
            {'ギ',"ｷﾞ"},
            {'グ',"ｸﾞ"},
            {'ゲ',"ｹﾞ"},
            {'ゴ',"ｺﾞ"},
            {'ザ',"ｻﾞ"},
            {'ジ',"ｼﾞ"},
            {'ズ',"ｽﾞ"},
            {'ゼ',"ｾﾞ"},
            {'ゾ',"ｿﾞ"},
            {'ダ',"ﾀﾞ"},
            {'ヂ',"ﾁﾞ"},
            {'ヅ',"ﾂﾞ"},
            {'デ',"ﾃﾞ"},
            {'ド',"ﾄﾞ"},
            {'バ',"ﾊﾞ"},
            {'ビ',"ﾋﾞ"},
            {'ブ',"ﾌﾞ"},
            {'ベ',"ﾍﾞ"},
            {'ボ',"ﾎﾞ"},
            {'パ',"ﾊﾟ"},
            {'ピ',"ﾋﾟ"},
            {'プ',"ﾌﾟ"},
            {'ペ',"ﾍﾟ"},
            {'ポ',"ﾎﾟ"},
            {'ァ',"ｧ"},
            {'ィ',"ｨ"},
            {'ゥ',"ｩ"},
            {'ェ',"ｪ"},
            {'ォ',"ｫ"},
            {'ッ',"ｯ"},
            {'ャ',"ｬ"},
            {'ュ',"ｭ"},
            {'ョ',"ｮ"},
            {'ー',"ｰ"},
            {'！',"!"},
            {'”',"\""},
            {'＃',"#"},
            {'＄',"$"},
            {'％',"%"},
            {'＆',"&"},
            {'’',"'"},
            {'（',"("},
            {'）',")"},
            {'＝',"="},
            {'〜',"~"},
            {'｜',"|"},
            {'−',"-"},
            {'＾',"^"},
            {'￥',"\\"},
            {'｀',"`"},
            {'｛',"{"},
            {'＠',"@"},
            {'［',"["},
            {'＋',"+"},
            {'＊',"*"},
            {'｝',"}"},
            {'；',";"},
            {'：',":"},
            {'］',"]"},
            {'＜',"<"},
            {'＞',">"},
            {'？',"?"},
            {'＿',"_"},
            {'，',","},
            {'．',"."},
            {'／',"/"},
            {'－',"-"},
        };
    }
    
    private static Dictionary<char,char> hankanaDict1 = null;
    private static Dictionary<char,Dictionary<char,char>> hankanaDict2 = null;

    private static void setupHankanaDict() {
        hankanaDict1 = new Dictionary<char,char>() {
            {'ｱ','ア'},
            {'ｲ','イ'},
            {'ｳ','ウ'},
            {'ｴ','エ'},
            {'ｵ','オ'},
            {'ｶ','カ'},
            {'ｷ','キ'},
            {'ｸ','ク'},
            {'ｹ','ケ'},
            {'ｺ','コ'},
            {'ｻ','サ'},
            {'ｼ','シ'},
            {'ｽ','ス'},
            {'ｾ','セ'},
            {'ｿ','ソ'},
            {'ﾀ','タ'},
            {'ﾁ','チ'},
            {'ﾂ','ツ'},
            {'ﾃ','テ'},
            {'ﾄ','ト'},
            {'ﾅ','ナ'},
            {'ﾆ','ニ'},
            {'ﾇ','ヌ'},
            {'ﾈ','ネ'},
            {'ﾉ','ノ'},
            {'ﾊ','ハ'},
            {'ﾋ','ヒ'},
            {'ﾌ','フ'},
            {'ﾍ','ヘ'},
            {'ﾎ','ホ'},
            {'ﾏ','マ'},
            {'ﾐ','ミ'},
            {'ﾑ','ム'},
            {'ﾒ','メ'},
            {'ﾓ','モ'},
            {'ﾔ','ヤ'},
            {'ﾕ','ユ'},
            {'ﾖ','ヨ'},
            {'ﾗ','ラ'},
            {'ﾘ','リ'},
            {'ﾙ','ル'},
            {'ﾚ','レ'},
            {'ﾛ','ロ'},
            {'ﾜ','ワ'},
            {'ｦ','ヲ'},
            {'ﾝ','ン'},
            {'ｧ','ァ'},
            {'ｨ','ィ'},
            {'ｩ','ゥ'},
            {'ｪ','ェ'},
            {'ｫ','ォ'},
            {'ｯ','ッ'},
            {'ｬ','ャ'},
            {'ｭ','ュ'},
            {'ｮ','ョ'},
            {'ｰ','ー'},
        };
        hankanaDict2 = new Dictionary<char,Dictionary<char,char>>() {
            {'ｶ', new Dictionary<char,char>(){{'ﾞ','ガ'}}},
            {'ｷ', new Dictionary<char,char>(){{'ﾞ','ギ'}}},
            {'ｸ', new Dictionary<char,char>(){{'ﾞ','グ'}}},
            {'ｹ', new Dictionary<char,char>(){{'ﾞ','ゲ'}}},
            {'ｺ', new Dictionary<char,char>(){{'ﾞ','ゴ'}}},
            {'ｻ', new Dictionary<char,char>(){{'ﾞ','ザ'}}},
            {'ｼ', new Dictionary<char,char>(){{'ﾞ','ジ'}}},
            {'ｽ', new Dictionary<char,char>(){{'ﾞ','ズ'}}},
            {'ｾ', new Dictionary<char,char>(){{'ﾞ','ゼ'}}},
            {'ｿ', new Dictionary<char,char>(){{'ﾞ','ゾ'}}},
            {'ﾀ', new Dictionary<char,char>(){{'ﾞ','ダ'}}},
            {'ﾁ', new Dictionary<char,char>(){{'ﾞ','ヂ'}}},
            {'ﾂ', new Dictionary<char,char>(){{'ﾞ','ヅ'}}},
            {'ﾃ', new Dictionary<char,char>(){{'ﾞ','デ'}}},
            {'ﾄ', new Dictionary<char,char>(){{'ﾞ','ド'}}},
            {'ﾊ', new Dictionary<char,char>(){{'ﾞ','バ'},{'ﾟ','パ'}}},
            {'ﾋ', new Dictionary<char,char>(){{'ﾞ','ビ'},{'ﾟ','ピ'}}},
            {'ﾌ', new Dictionary<char,char>(){{'ﾞ','ブ'},{'ﾟ','プ'}}},
            {'ﾍ', new Dictionary<char,char>(){{'ﾞ','ベ'},{'ﾟ','ペ'}}},
            {'ﾎ', new Dictionary<char,char>(){{'ﾞ','ボ'},{'ﾟ','ポ'}}},
        };
    }
    
}

} // End of namespace
