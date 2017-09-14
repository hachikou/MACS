/*! @file NetworkByteOrder.cs
 * @brief ネットワークバイトオーダ操作ユーティリティクラス
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;

namespace MACS {

/// <summary>
///   ネットワークバイトオーダ操作ユーティリティクラス
/// </summary>
public class NetworkByteOrder {

    /// <summary>
    ///   ネットワークバイトオーダで64bit符号無し整数を読み取る
    /// </summary>
    public static ulong ToULong(byte[] buf) {
        return ToULong(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダで64bit符号無し整数を読み取る
    /// </summary>
    public static ulong ToULong(byte[] buf, int offset) {
        return (ulong)(buf[offset])*256*256*256*256*256*256*256+(ulong)(buf[offset+1])*256*256*256*256*256*256+(ulong)(buf[offset+2])*256*256*256*256*256+(ulong)(buf[offset+3])*256*256*256*256+(ulong)(buf[offset+4])*256*256*256+(ulong)(buf[offset+5])*256*256+(ulong)(buf[offset+6])*256+(ulong)(buf[offset+7]);
    }
    /// <summary>
    ///   ネットワークバイトオーダで64bit符号無し整数を読み取る
    /// </summary>
    public static ulong ToULong(byte[] buf, ref int offset) {
        ulong val = ToULong(buf, offset);
        offset += 8;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ulong val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ulong val, byte[] buf, int offset) {
        buf[offset+7] = (byte)(val&0xff);
        val /= 256;
        buf[offset+6] = (byte)(val&0xff);
        val /= 256;
        buf[offset+5] = (byte)(val&0xff);
        val /= 256;
        buf[offset+4] = (byte)(val&0xff);
        val /= 256;
        buf[offset+3] = (byte)(val&0xff);
        val /= 256;
        buf[offset+2] = (byte)(val&0xff);
        val /= 256;
        buf[offset+1] = (byte)(val&0xff);
        val /= 256;
        buf[offset] = (byte)(val&0xff);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ulong val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 8;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号付き整数を読み取る
    /// </summary>
    public static long ToLong(byte[] buf) {
        return (long)ToULong(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号付き整数を読み取る
    /// </summary>
    public static long ToLong(byte[] buf, int offset) {
        return (long)ToULong(buf, offset);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号付き整数を読み取る
    /// </summary>
    public static long ToLong(byte[] buf, ref int offset) {
        long val = (long)ToULong(buf, offset);
        offset += 8;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(long val, byte[] buf) {
        ToBytes((ulong)val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(long val, byte[] buf, int offset) {
        ToBytes((ulong)val, buf, offset);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで64bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(long val, byte[] buf, ref int offset) {
        ToBytes((ulong)val, buf, offset);
        offset += 8;
    }

    /// <summary>
    ///   ネットワークバイトオーダで32bit符号無し整数を読み取る
    /// </summary>
    public static uint ToUInt(byte[] buf) {
        return ToUInt(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダで32bit符号無し整数を読み取る
    /// </summary>
    public static uint ToUInt(byte[] buf, int offset) {
        return (uint)(buf[offset])*256*256*256+(uint)(buf[offset+1])*256*256+(uint)(buf[offset+2])*256+(uint)(buf[offset+3]);
    }
    /// <summary>
    ///   ネットワークバイトオーダで32bit符号無し整数を読み取る
    /// </summary>
    public static uint ToUInt(byte[] buf, ref int offset) {
        uint val = ToUInt(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで32bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(uint val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで32bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(uint val, byte[] buf, int offset) {
        buf[offset+3] = (byte)(val&0xff);
        val /= 256;
        buf[offset+2] = (byte)(val&0xff);
        val /= 256;
        buf[offset+1] = (byte)(val&0xff);
        val /= 256;
        buf[offset] = (byte)(val&0xff);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで32bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(uint val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 4;
    }

    /// <summary>
    ///   ネットワークバイトオーダで32bit符号付き整数を読み取る
    /// </summary>
    public static int ToInt(byte[] buf) {
        return (int)ToUInt(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダで32bit符号付き整数を読み取る
    /// </summary>
    public static int ToInt(byte[] buf, int offset) {
        return (int)ToUInt(buf, offset);
    }
    /// <summary>
    ///   ネットワークバイトオーダで32bit符号付き整数を読み取る
    /// </summary>
    public static int ToInt(byte[] buf, ref int offset) {
        int val = (int)ToUInt(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダで32bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(int val, byte[] buf) {
        ToBytes((uint)val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダで32bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(int val, byte[] buf, int offset) {
        ToBytes((uint)val, buf, offset);
    }
    /// <summary>
    ///   ネットワークバイトオーダで32bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(int val, byte[] buf, ref int offset) {
        ToBytes((uint)val, buf, offset);
        offset += 4;
    }

    /// <summary>
    ///   ネットワークバイトオーダで16bit符号無し整数を読み取る
    /// </summary>
    public static ushort ToUShort(byte[] buf) {
        return ToUShort(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダで16bit符号無し整数を読み取る
    /// </summary>
    public static ushort ToUShort(byte[] buf, int offset) {
        return (ushort)((uint)(buf[offset])*256+(uint)(buf[offset+1]));
    }
    /// <summary>
    ///   ネットワークバイトオーダで16bit符号無し整数を読み取る
    /// </summary>
    public static ushort ToUShort(byte[] buf, ref int offset) {
        ushort val = ToUShort(buf, offset);
        offset += 2;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ushort val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ushort val, byte[] buf, int offset) {
        buf[offset+1] = (byte)(val&0xff);
        val /= 256;
        buf[offset] = (byte)(val&0xff);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ushort val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 2;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号付き整数を読み取る
    /// </summary>
    public static short ToShort(byte[] buf) {
        return (short)ToUShort(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号付き整数を読み取る
    /// </summary>
    public static short ToShort(byte[] buf, int offset) {
        return (short)ToUShort(buf, offset);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号付き整数を読み取る
    /// </summary>
    public static short ToShort(byte[] buf, ref int offset) {
        short val = (short)ToUShort(buf, offset);
        offset += 2;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(short val, byte[] buf) {
        ToBytes((ushort)val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(short val, byte[] buf, int offset) {
        ToBytes((ushort)val, buf, offset);
    }
    /// <summary>
    ///   ネットワークバイトオーダーで16bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(short val, byte[] buf, ref int offset) {
        ToBytes((ushort)val, buf, offset);
        offset += 2;
    }

    /// <summary>
    ///   bool値を読み取る
    /// </summary>
    public static bool ToBool(byte[] buf) {
        return BitConverter.ToBoolean(buf, 0);
    }
    /// <summary>
    ///   bool値を読み取る
    /// </summary>
    public static bool ToBool(byte[] buf, int offset) {
        return BitConverter.ToBoolean(buf, offset);
    }
    /// <summary>
    ///   bool値を読み取る
    /// </summary>
    public static bool ToBool(byte[] buf, ref int offset) {
        bool val = BitConverter.ToBoolean(buf, offset);
        offset += 1;
        return val;
    }

    /// <summary>
    ///   bool値を書き込む
    /// </summary>
    public static void ToBytes(bool val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   bool値を書き込む
    /// </summary>
    public static void ToBytes(bool val, byte[] buf, int offset) {
        buf[offset] = val?(byte)1:(byte)0;
    }
    /// <summary>
    ///   bool値を書き込む
    /// </summary>
    public static void ToBytes(bool val, byte[] buf, ref int offset) {
        buf[offset] = val?(byte)1:(byte)0;
        offset++;
    }

    /// <summary>
    ///   ネットワークバイトオーダーでfloat値を読み取る
    /// </summary>
    public static float ToFloat(byte[] buf) {
        return ToFloat(buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーでfloat値を読み取る
    /// </summary>
    public static float ToFloat(byte[] buf, int offset) {
        byte[] xbuf = new byte[4];
        xbuf[0] = buf[offset+3];
        xbuf[1] = buf[offset+2];
        xbuf[2] = buf[offset+1];
        xbuf[3] = buf[offset];
        return BitConverter.ToSingle(xbuf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーでfloat値を読み取る
    /// </summary>
    public static float ToFloat(byte[] buf, ref int offset) {
        float val = ToFloat(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   ネットワークバイトオーダーでfloat値を書き込む
    /// </summary>
    public static void ToBytes(float val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   ネットワークバイトオーダーでfloat値を書き込む
    /// </summary>
    public static void ToBytes(float val, byte[] buf, int offset) {
        byte[] xbuf = BitConverter.GetBytes(val);
        buf[offset] = xbuf[3];
        buf[offset+1] = xbuf[2];
        buf[offset+2] = xbuf[1];
        buf[offset+3] = xbuf[0];
    }
    /// <summary>
    ///   ネットワークバイトオーダーでfloat値を書き込む
    /// </summary>
    public static void ToBytes(float val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 4;
    }


    /// <summary>
    ///   文字列を読み取る
    /// </summary>
    public static string ToString(byte[] buf, int offset, int len, Encoding enc) {
        return StringUtil.TrimNullChar(enc.GetString(buf, offset, len));
    }
    /// <summary>
    ///   文字列を読み取る
    /// </summary>
    public static string ToString(byte[] buf, ref int offset, int len, Encoding enc) {
        string val = StringUtil.TrimNullChar(enc.GetString(buf, offset, len));
        offset += len;
        return val;
    }

    /// <summary>
    ///   文字列を書き込む
    /// </summary>
    public static void ToBytes(string val, byte[] buf, int offset, int len, Encoding enc) {
        if(val == null)
            Array.Clear(buf, offset, len);
        else
            ToBytes(enc.GetBytes(val), buf, offset, len);
    }
    /// <summary>
    ///   文字列を書き込む
    /// </summary>
    public static void ToBytes(string val, byte[] buf, ref int offset, int len, Encoding enc) {
        if(val == null){
            Array.Clear(buf, offset, len);
            offset += len;
        } else {
            ToBytes(enc.GetBytes(val), buf, ref offset, len);
        }
    }

    /// <summary>
    ///   バイト列を書き込む。
    ///   コピー元バイト列の長さが必要サイズ未満の場合には、ゼロで埋められる。
    /// </summary>
    public static void ToBytes(byte[] val, byte[] buf, int offset, int len) {
        if(val.Length < len){
            Buffer.BlockCopy(val, 0, buf, offset, val.Length);
            Array.Clear(buf, offset+val.Length, len-val.Length);
        } else {
            Buffer.BlockCopy(val, 0, buf, offset, len);
        }
    }
    /// <summary>
    ///   バイト列を書き込む。
    ///   コピー元バイト列の長さが必要サイズ未満の場合には、ゼロで埋められる。
    /// </summary>
    public static void ToBytes(byte[] val, byte[] buf, ref int offset, int len) {
        ToBytes(val, buf, offset, len);
        offset += len;
    }


#if SELFTEST
    public static int Main(string[] args) {
        ulong lvalue;
        uint ivalue;
        ushort svalue;
        byte[] buf = new byte[8];

        for(int i = 0; i < 8; i++){
            if(i < args.Length)
                buf[i] = (byte)StringUtil.ToHexInt(args[i], 0);
            else
                buf[i] = 0;
        }

        lvalue = ToULong(buf);
        ivalue = ToUInt(buf);
        svalue = ToUShort(buf);

        Console.Write("ULong: {0}", lvalue.ToString("X"));
        for(int i = 0; i < 8; i++)
            buf[i] = 0;
        ToBytes(lvalue, buf);
        for(int i = 0; i < 8; i++)
            Console.Write(" {0}", buf[i].ToString("X"));
        Console.WriteLine();
        Console.Write("UInt:  {0}", ivalue.ToString("X"));
        for(int i = 0; i < 8; i++)
            buf[i] = 0;
        ToBytes(ivalue, buf);
        for(int i = 0; i < 8; i++)
            Console.Write(" {0}", buf[i].ToString("X"));
        Console.WriteLine();
        Console.Write("UShort:{0}", svalue.ToString("X"));
        for(int i = 0; i < 8; i++)
            buf[i] = 0;
        ToBytes(svalue, buf);
        for(int i = 0; i < 8; i++)
            Console.Write(" {0}", buf[i].ToString("X"));
        Console.WriteLine();

        for(int i = 0; i < 8; i++)
            buf[i] = 0;
        ToBytes(-128L, buf);
        for(int i = 0; i < 8; i++)
            Console.Write(" {0}", buf[i].ToString("X"));
        Console.WriteLine();
        Console.WriteLine("{0}",ToLong(buf, 0));

        float f = 3.14159265358979f;
        for(int i = 0; i < 8; i++)
            buf[i] = 0;
        ToBytes(f, buf);
        for(int i = 0; i < 8; i++)
            Console.Write(" {0}", buf[i].ToString("X"));
        Console.WriteLine();
        Console.WriteLine("{0}", ToFloat(buf, 0));

        return 0;
    }
#endif
}

} // End of namespace
