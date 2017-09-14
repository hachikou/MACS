/**
 * MACアドレスを管理するクラス
 * $Id: $
 *
 * Copyright (C) 2008-2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using System.Web;
using System.Net;

namespace MACS {

/// <summary>
///   MACアドレスを管理するクラス
/// </summary>
public class Hwaddr:IComparable {

    private byte[] vals;

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public Hwaddr() {
        vals = null;
    }

    /// <summary>
    ///   文字列表現からのコンストラクタ
    /// </summary>
    public Hwaddr(string hwaddr) {
        Set(hwaddr);
    }

    /// <summary>
    ///   ulong値からのコンストラクタ
    /// </summary>
    public Hwaddr(ulong addr) {
        Set(addr);
    }

    /// <summary>
    ///   バイト列からのコンストラクタ
    /// </summary>
    /// <param name="addr">バイト列先頭アドレス</param>
    /// <param name="offset">読み取りオフセットバイト数</param>
    /// <param name="len">読み取りバイト数</param>
    public Hwaddr(byte[] addr, int offset, int len) {
        vals = new byte[len];
        for(int i = 0; i < len; i++)
            vals[i] = addr[offset + i];
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public Hwaddr(Hwaddr src) {
        Set(src);
    }

    /// <summary>
    ///   文字列からMACアドレスをセットする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     HEX値の区切り文字は':'、'-'または' 'のいずれでもよい。
    ///     6バイト以下のデータも読み取る。
    ///   </para>
    /// </remarks>
    /// <returns>自分自身</returns>
    public Hwaddr Set(string hwaddr) {
        if((hwaddr == null) || (hwaddr == "")){
            vals = new byte[0];
            return this;
        }
        vals = null;
        byte[] tmp = new byte[6];
        int count = 0;
        int digit = 0;
        tmp[count] = 0;
        foreach(char ch in hwaddr.ToCharArray()){
            if((ch == ':') || (ch == '-') || (ch == ' ')){ // delimiter
                if(++count >= tmp.Length)
                    return this; // Format error
                tmp[count] = 0;
                digit = 0;
            }else{
                if(digit >= 2){ // delimiter無しで書き連ねた場合
                    if(++count >= tmp.Length)
                        return this; // Too many digits
                    digit = 0;
                }
                tmp[count] *= 16;
                if((ch >= '0') && (ch <= '9'))
                    tmp[count] += (byte)(Convert.ToByte(ch)-Convert.ToByte('0'));
                else if((ch >= 'a') && (ch <= 'f'))
                    tmp[count] += (byte)(Convert.ToByte(ch)-Convert.ToByte('a')+10);
                else if((ch >= 'A') && (ch <= 'F'))
                    tmp[count] += (byte)(Convert.ToByte(ch)-Convert.ToByte('A')+10);
                else
                    return this; // Invalid char for hexadecimal
                digit++;
            }
        }
        if(digit > 0)
            count++;
        vals = new byte[count];
        Array.Copy(tmp, 0, vals, 0, count);
        return this;

        /* OLD CODE - ちょっと動作が遅いかもしれない
        string[] x = hwaddr.Split(":-".ToCharArray());
        vals = new byte[x.Length];
        try {
            for(int i = 0; i < x.Length; i++)
                vals[i] = Byte.Parse(x[i], System.Globalization.NumberStyles.HexNumber);
        } catch(OverflowException) {
            vals = null;
        } catch(FormatException) {
            vals = null;
        }
        return this;
        */
    }

    /// <summary>
    ///   ulong値からMACアドレスをセットする
    /// </summary>
    /// <returns>自分自身</returns>
    public Hwaddr Set(ulong hwaddr) {
        if((vals == null) || (vals.Length != 6))
            vals = new byte[6];
        for(int i = 5; i >= 0; i--) {
            vals[i] = (byte)(hwaddr % 256);
            hwaddr /= 256;
        }
        return this;
    }

    public Hwaddr Set(Hwaddr src) {
        if(src.IsValid()) {
            vals = new byte[src.vals.Length];
            for(int i = 0; i < vals.Length; i++)
                vals[i] = src.vals[i];
        } else {
            vals = null;
        }
        return this;
    }

    public Hwaddr Set(byte[] addr, int offset, int len) {
        vals = new byte[len];
        for(int i = 0; i < len; i++)
            vals[i] = addr[offset + i];
        return this;
    }

    /// <summary>
    ///   インスタンスが有効かどうかを返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     6バイト以下の状態でもtrueを返す。
    ///     6バイトそろっているかどうかは、IsComplete()を使って確認すること。
    ///   </para>
    /// </remarks>
    public bool IsValid() {
        if((vals == null) || (vals.Length == 0))
            return false;
        return true;
    }

    /// <summary>
    ///   インスタンスが6バイトのアドレスを持っているかどうかを返す
    /// </summary>
    public bool IsComplete() {
        if((vals == null) || (vals.Length != 6))
            return false;
        return true;
    }

    /// <summary>
    ///   インスタンスが6バイトのアドレスを持っているかまたはまったく設定されていないときにtrueを返す
    /// </summary>
    public bool IsCompleteOrNull() {
        if(vals == null)
            return false;
        if((vals.Length != 0) && (vals.Length != 6))
            return false;
        return true;
    }

    /// <summary>
    ///   文字列表現を返す
    /// </summary>
    override public string ToString() {
        if(!IsValid())
            return "INVALID";
        return BitConverter.ToString(vals).Replace("-", ":");
    }

    /// <summary>
    ///   URLエンコーディングされた文字列表現を返す
    /// </summary>
    public string ToUrlString() {
        return HttpUtility.UrlEncode(ToString(), Encoding.GetEncoding("us-ascii"));
    }

    /// <summary>
    ///   アドレスをulong型の値として返す。
    /// </summary>
    public ulong ToUlong() {
        if(vals == null)
            return 0;
        ulong x = 0;
        for(int i = 0; i < 6; i++) {
            x *= 256;
            if(i < vals.Length)
                x += vals[i];
        }
        return x;
    }

    /// <summary>
    ///   MACアドレスのバイト列を返す。
    /// </summary>
    public byte[] GetBytes() {
        return vals;
    }

    /// <summary>
    ///   MACアドレスのバイト列を獲得する。
    /// </summary>
    /// <param name="dst">格納先のバイト列</param>
    /// <param name="index">dstの何バイト目から格納するか</param>
    public void GetBytes(byte[] dst, int index) {
        if(vals != null)
            Array.Copy(vals, 0, dst, index, vals.Length);
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレスパターンを内包しているかどうかをチェックする。
    /// </summary>
    public bool Contains(Hwaddr src) {
        return ToString().Contains(src.ToString());
    }
    /// <summary>
    ///   インスタンスがsrcで示されるアドレスパターンを内包しているかどうかをチェックする。
    ///   文字列と比較するバージョン。
    /// </summary>
    public bool Contains(string src) {
        return ToString().Contains(src.ToUpper());
    }

    /// <summary>
    ///   OUI(ベンダーコード)を獲得する。
    /// </summary>
    /// <returns>OUIの3バイトだけが定義されたHwaddrインスタンス</returns>
    public Hwaddr GetOui() {
        if((vals == null) || (vals.Length < 3))
            return new Hwaddr();
        Hwaddr res = new Hwaddr();
        res.vals = new byte[3];
        for(int i = 0; i < 3; i++)
            res.vals[i] = vals[i];
        return res;
    }

    /// <summary>
    ///   インスタンスがsrcで示されるアドレスと同じかどうかを返す
    /// </summary>
    public bool Equals(Hwaddr src) {
        if((vals == null) || (src == null) || (src.vals == null))
            return false;
        if(vals.Length != src.vals.Length)
            return false;
        for(int i = 0; i < vals.Length; i++) {
            if(vals[i] != src.vals[i])
                return false;
        }
        return true;
    }
    /// <summary>
    ///   インスタンスがsrcで示されるアドレスと同じかどうかを返す
    ///   一般オブジェクトとの比較バージョン。
    /// </summary>
    public override bool Equals(Object obj) {
        if(obj == null)
            return false;
        Hwaddr src = obj as Hwaddr;
        return Equals(src);
    }

    /// <summary>
    ///   インスタンスのハッシュコードを返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     全バイトを足し合わせたものをハッシュ値としている。
    ///   </para>
    /// </remarks>
    public override int GetHashCode() {
        if(vals == null)
            return 0;
        int res = 0;
        for(int i = 0; i < vals.Length; i++)
            res += (int)vals[i];
        return res;
    }

    public static bool operator ==(Hwaddr a, Hwaddr b) {
        if(ReferenceEquals(a, b))
            return true;
        if(ReferenceEquals(a, null))
            return false;
        return a.Equals(b);
    }

    public static bool operator !=(Hwaddr a, Hwaddr b) {
        return !(a == b);
    }

    /// <summary>
    ///   アドレス値の大小を比較する。
    /// </summary>
    public int CompareTo(Hwaddr src) {
        if(vals == null)
            return -1;
        if((src == null) || (src.vals == null))
            return 1;
        if(vals.Length < src.vals.Length)
            return -1;
        if(vals.Length > src.vals.Length)
            return 1;
        for(int i = 0; i < vals.Length; i++) {
            if(vals[i] < src.vals[i])
                return -1;
            if(vals[i] > src.vals[i])
                return 1;
        }
        return 0;
    }

    /// <summary>
    ///   アドレス値の大小を比較する。
    ///   一般オブジェクトとの比較バージョン。
    /// </summary>
    public int CompareTo(Object obj) {
        if(obj == null)
            return 1;
        return CompareTo(obj as Hwaddr);
    }

    public static bool operator <(Hwaddr a, Hwaddr b) {
        return (a.CompareTo(b) < 0);
    }

    public static bool operator <=(Hwaddr a, Hwaddr b) {
        return (a.CompareTo(b) <= 0);
    }

    public static bool operator >(Hwaddr a, Hwaddr b) {
        return (a.CompareTo(b) > 0);
    }

    public static bool operator >=(Hwaddr a, Hwaddr b) {
        return (a.CompareTo(b) >= 0);
    }

    /// <summary>
    ///   アドレス値の大小を比較する。
    /// </summary>
    public static int Compare(Hwaddr a, Hwaddr b) {
        return a.CompareTo(b);
    }

    /// <summary>
    ///   アドレスを一つ進める
    /// </summary>
    public void Incr() {
        if(vals == null)
            return;
        int idx = vals.Length-1;
        while(idx >= 0) {
            if(vals[idx] >= 255) {
                vals[idx] = 0;
                idx--;
            } else {
                vals[idx]++;
                return;
            }
        }
    }

    /// <summary>
    ///   文字列がMACアドレスの形式になっているかどうかを確認する
    /// </summary>
    public static bool IsHwaddrString(string str) {
        if(String.IsNullOrEmpty(str))
            return false;
        string[] x = str.Trim().Split(":- ".ToCharArray());
        if(x.Length != 6)
            return false;
        foreach(string i in x) {
            if(i.Length != 2)
                return false;
            if(!isHexDigit(i[0]) || !isHexDigit(i[1]))
                return false;
        }
        return true;
    }

    /// <summary>
    ///   文字列表現にした時に指定文字列から始まっているかどうか
    /// </summary>
    public bool StartsWith(string str) {
        return this.ToString().StartsWith(str.ToUpper());
    }

    /// <summary>
    ///   文字列表現にした時に指定文字列で終わっているかどうか
    /// </summary>
    public bool EndsWith(string str) {
        return this.ToString().EndsWith(str.ToUpper());
    }


    private static bool isHexDigit(char x) {
        return (('0'<=x)&&(x<='9'))||(('a'<=x)&&(x<='f'))||(('A'<=x)&&(x<='F'));
    }

}

} // End of namespace
