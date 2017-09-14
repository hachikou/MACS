/*! @file ByteUtil.cs
 * @brief 各種の値をバイト列からまたはバイト列に変換するユーティリティ。
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
///   各種の値をバイト列からまたはバイト列に変換するユーティリティ。
///   BitConverterでは使いづらいパターンの変換に対応。
/// </summary>
public class ByteUtil {

    /// <summary>
    ///   64bit符号無し整数を読み取る
    /// </summary>
    public static ulong ToULong(byte[] buf) {
        return BitConverter.ToUInt64(buf, 0);
    }
    /// <summary>
    ///   64bit符号無し整数を読み取る
    /// </summary>
    public static ulong ToULong(byte[] buf, int offset) {
        return BitConverter.ToUInt64(buf, offset);
    }
    /// <summary>
    ///   64bit符号無し整数を読み取る
    /// </summary>
    public static ulong ToULong(byte[] buf, ref int offset) {
        ulong val = BitConverter.ToUInt64(buf, offset);
        offset += 8;
        return val;
    }

    /// <summary>
    ///   64bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ulong val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   64bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ulong val, byte[] buf, int offset) {
        buf[offset] = (byte)(val&0xff);
        val /= 256;
        buf[offset+1] = (byte)(val&0xff);
        val /= 256;
        buf[offset+2] = (byte)(val&0xff);
        val /= 256;
        buf[offset+3] = (byte)(val&0xff);
        val /= 256;
        buf[offset+4] = (byte)(val&0xff);
        val /= 256;
        buf[offset+5] = (byte)(val&0xff);
        val /= 256;
        buf[offset+6] = (byte)(val&0xff);
        val /= 256;
        buf[offset+7] = (byte)(val&0xff);
    }
    /// <summary>
    ///   64bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ulong val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 8;
    }

    /// <summary>
    ///   64bit符号付き整数を読み取る
    /// </summary>
    public static long ToLong(byte[] buf) {
        return BitConverter.ToInt64(buf, 0);
    }
    /// <summary>
    ///   64bit符号付き整数を読み取る
    /// </summary>
    public static long ToLong(byte[] buf, int offset) {
        return BitConverter.ToInt64(buf, offset);
    }
    /// <summary>
    ///   64bit符号付き整数を読み取る
    /// </summary>
    public static long ToLong(byte[] buf, ref int offset) {
        long val = BitConverter.ToInt64(buf, offset);
        offset += 8;
        return val;
    }

    /// <summary>
    ///   64bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(long val, byte[] buf) {
        ToBytes((ulong)val, buf, 0);
    }
    /// <summary>
    ///   64bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(long val, byte[] buf, int offset) {
        ToBytes((ulong)val, buf, offset);
    }
    /// <summary>
    ///   64bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(long val, byte[] buf, ref int offset) {
        ToBytes((ulong)val, buf, offset);
        offset += 8;
    }

    /// <summary>
    ///   32bit符号無し整数を読み取る
    /// </summary>
    public static uint ToUInt(byte[] buf) {
        return BitConverter.ToUInt32(buf, 0);
    }
    /// <summary>
    ///   32bit符号無し整数を読み取る
    /// </summary>
    public static uint ToUInt(byte[] buf, int offset) {
        return BitConverter.ToUInt32(buf, offset);
    }
    /// <summary>
    ///   32bit符号無し整数を読み取る
    /// </summary>
    public static uint ToUInt(byte[] buf, ref int offset) {
        uint val = BitConverter.ToUInt32(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   32bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(uint val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   32bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(uint val, byte[] buf, int offset) {
        buf[offset] = (byte)(val&0xff);
        val /= 256;
        buf[offset+1] = (byte)(val&0xff);
        val /= 256;
        buf[offset+2] = (byte)(val&0xff);
        val /= 256;
        buf[offset+3] = (byte)(val&0xff);
    }
    /// <summary>
    ///   32bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(uint val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 4;
    }

    /// <summary>
    ///   32bit符号付き整数を読み取る
    /// </summary>
    public static int ToInt(byte[] buf) {
        return BitConverter.ToInt32(buf, 0);
    }
    /// <summary>
    ///   32bit符号付き整数を読み取る
    /// </summary>
    public static int ToInt(byte[] buf, int offset) {
        return BitConverter.ToInt32(buf, offset);
    }
    /// <summary>
    ///   32bit符号付き整数を読み取る
    /// </summary>
    public static int ToInt(byte[] buf, ref int offset) {
        int val = BitConverter.ToInt32(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   32bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(int val, byte[] buf) {
        ToBytes((uint)val, buf, 0);
    }
    /// <summary>
    ///   32bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(int val, byte[] buf, int offset) {
        ToBytes((uint)val, buf, offset);
    }
    /// <summary>
    ///   32bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(int val, byte[] buf, ref int offset) {
        ToBytes((uint)val, buf, offset);
        offset += 4;
    }

    /// <summary>
    ///   16bit符号無し整数を読み取る
    /// </summary>
    public static ushort ToUShort(byte[] buf) {
        return BitConverter.ToUInt16(buf, 0);
    }
    /// <summary>
    ///   16bit符号無し整数を読み取る
    /// </summary>
    public static ushort ToUShort(byte[] buf, int offset) {
        return BitConverter.ToUInt16(buf, offset);
    }
    /// <summary>
    ///   16bit符号無し整数を読み取る
    /// </summary>
    public static ushort ToUShort(byte[] buf, ref int offset) {
        ushort val = BitConverter.ToUInt16(buf, offset);
        offset += 2;
        return val;
    }

    /// <summary>
    ///   16bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ushort val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   16bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ushort val, byte[] buf, int offset) {
        buf[offset] = (byte)(val&0xff);
        val /= 256;
        buf[offset+1] = (byte)(val&0xff);
    }
    /// <summary>
    ///   16bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(ushort val, byte[] buf, ref int offset) {
        ToBytes(val, buf, offset);
        offset += 2;
    }

    /// <summary>
    ///   16bit符号付き整数を読み取る
    /// </summary>
    public static short ToShort(byte[] buf) {
        return BitConverter.ToInt16(buf, 0);
    }
    /// <summary>
    ///   16bit符号付き整数を読み取る
    /// </summary>
    public static short ToShort(byte[] buf, int offset) {
        return BitConverter.ToInt16(buf, offset);
    }
    /// <summary>
    ///   16bit符号付き整数を読み取る
    /// </summary>
    public static short ToShort(byte[] buf, ref int offset) {
        short val = BitConverter.ToInt16(buf, offset);
        offset += 2;
        return val;
    }

    /// <summary>
    ///   16bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(short val, byte[] buf) {
        ToBytes((ushort)val, buf, 0);
    }
    /// <summary>
    ///   16bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(short val, byte[] buf, int offset) {
        ToBytes((ushort)val, buf, offset);
    }
    /// <summary>
    ///   16bit符号付き整数を書き込む
    /// </summary>
    public static void ToBytes(short val, byte[] buf, ref int offset) {
        ToBytes((ushort)val, buf, offset);
        offset += 2;
    }

    /// <summary>
    ///   8bit符号無し整数を読み取る
    /// </summary>
    public static byte ToByte(byte[] buf) {
        return buf[0];
    }
    /// <summary>
    ///   8bit符号無し整数を読み取る
    /// </summary>
    public static byte ToByte(byte[] buf, int offset) {
        return buf[offset];
    }
    /// <summary>
    ///   8bit符号無し整数を読み取る
    /// </summary>
    public static byte ToByte(byte[] buf, ref int offset) {
        byte val = buf[offset];
        offset++;
        return val;
    }

    /// <summary>
    ///   8bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(byte val, byte[] buf) {
        buf[0] = val;
    }
    /// <summary>
    ///   8bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(byte val, byte[] buf, int offset) {
        buf[offset] = val;
    }
    /// <summary>
    ///   8bit符号無し整数を書き込む
    /// </summary>
    public static void ToBytes(byte val, byte[] buf, ref int offset) {
        buf[offset] = val;
        offset++;
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
    ///   float値を読み取る
    /// </summary>
    public static float ToFloat(byte[] buf) {
        return BitConverter.ToSingle(buf, 0);
    }
    /// <summary>
    ///   float値を読み取る
    /// </summary>
    public static float ToFloat(byte[] buf, int offset) {
        return BitConverter.ToSingle(buf, offset);
    }
    /// <summary>
    ///   float値を読み取る
    /// </summary>
    public static float ToFloat(byte[] buf, ref int offset) {
        float val = BitConverter.ToSingle(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   float値を書き込む
    /// </summary>
    public static void ToBytes(float val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   float値を書き込む
    /// </summary>
    public static void ToBytes(float val, byte[] buf, int offset) {
        Buffer.BlockCopy(BitConverter.GetBytes(val), 0, buf, offset, 4);
    }
    /// <summary>
    ///   float値を書き込む
    /// </summary>
    public static void ToBytes(float val, byte[] buf, ref int offset) {
        Buffer.BlockCopy(BitConverter.GetBytes(val), 0, buf, offset, 4);
        offset += 4;
    }


    /// <summary>
    ///   double値を読み取る
    /// </summary>
    public static double ToDouble(byte[] buf) {
        return BitConverter.ToDouble(buf, 0);
    }
    /// <summary>
    ///   double値を読み取る
    /// </summary>
    public static double ToDouble(byte[] buf, int offset) {
        return BitConverter.ToDouble(buf, offset);
    }
    /// <summary>
    ///   double値を読み取る
    /// </summary>
    public static double ToDouble(byte[] buf, ref int offset) {
        double val = BitConverter.ToDouble(buf, offset);
        offset += 4;
        return val;
    }

    /// <summary>
    ///   double値を書き込む
    /// </summary>
    public static void ToBytes(double val, byte[] buf) {
        ToBytes(val, buf, 0);
    }
    /// <summary>
    ///   double値を書き込む
    /// </summary>
    public static void ToBytes(double val, byte[] buf, int offset) {
        Buffer.BlockCopy(BitConverter.GetBytes(val), 0, buf, offset, 8);
    }
    /// <summary>
    ///   double値を書き込む
    /// </summary>
    public static void ToBytes(double val, byte[] buf, ref int offset) {
        Buffer.BlockCopy(BitConverter.GetBytes(val), 0, buf, offset, 8);
        offset += 8;
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
        ToBytes(val, buf, offset, len, 0x00, enc);
    }
    /// <summary>
    ///   文字列を書き込む
    /// </summary>
    public static void ToBytes(string val, byte[] buf, ref int offset, int len, Encoding enc) {
        ToBytes(val, buf, ref offset, len, 0x00, enc);
    }

    /// <summary>
    ///   文字列を書き込む
    /// </summary>
    public static void ToBytes(string val, byte[] buf, int offset, int len, byte fill, Encoding enc) {
        if(val == null) {
            if(fill == 0)
                Array.Clear(buf, offset, len);
            else
                for(int i = 0; i < len; i++)
                    buf[offset+i] = fill;
        } else {
            ToBytes(enc.GetBytes(val), buf, offset, len, fill);
        }
    }
    /// <summary>
    ///   文字列を書き込む
    /// </summary>
    public static void ToBytes(string val, byte[] buf, ref int offset, int len, byte fill, Encoding enc) {
        if(val == null){
            if(fill == 0)
                Array.Clear(buf, offset, len);
            else
                for(int i = 0; i < len; i++)
                    buf[offset+i] = fill;
            offset += len;
        } else {
            ToBytes(enc.GetBytes(val), buf, ref offset, len, fill);
        }
    }

    /// <summary>
    ///   バイト列を書き込む。
    ///   コピー元バイト列の長さが必要サイズ未満の場合には、fillで埋められる。
    /// </summary>
    public static void ToBytes(byte[] val, byte[] buf, int offset, int len, byte fill=0) {
        if(val == null) {
            if(fill == 0)
                Array.Clear(buf, offset, len);
            else
                for(int i = 0; i < len; i++)
                    buf[offset+i] = fill;
            return;
        }
        if(val.Length < len){
            Buffer.BlockCopy(val, 0, buf, offset, val.Length);
            if(fill == 0)
                Array.Clear(buf, offset+val.Length, len-val.Length);
            else
                for(int i = 0; i < len-val.Length; i++)
                    buf[offset+val.Length+i] = fill;
        } else {
            Buffer.BlockCopy(val, 0, buf, offset, len);
        }
    }
    /// <summary>
    ///   バイト列を書き込む。
    ///   コピー元バイト列の長さが必要サイズ未満の場合には、fillで埋められる。
    /// </summary>
    public static void ToBytes(byte[] val, byte[] buf, ref int offset, int len, byte fill=0) {
        ToBytes(val, buf, offset, len, fill);
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

        lvalue = ToULong(buf, 0);
        ivalue = ToUInt(buf, 0);
        svalue = ToUShort(buf, 0);

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
        ToBytes(-10L, buf);
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

        buf[0] = 0x00;
        buf[1] = 0x00;
        buf[2] = 0x00;
        buf[3] = 0x00;
        buf[4] = 0x00;
        buf[5] = 0x00;
        buf[6] = 0x1c;
        buf[7] = 0x40;
        Console.WriteLine("{0}", ToDouble(buf, 0));

        return 0;
    }
#endif
}

} // End of namespace
