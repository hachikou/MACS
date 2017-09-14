/*
  * 文字列の簡易暗号化
  * $Id: $
  *
  * Copyright (C) 2013 Microbrains Inc. All rights reserved.
  * This code was designed and coded by SHIBUYA K.
  */

using System;
using System.Text;

namespace MACS {

/// <summary>
///   簡易暗号化機構
/// </summary>
public class SimpleCipher {

    /// <summary>
    ///   簡易暗号化器を作る
    /// </summary>
    public SimpleCipher(byte[] seed_) {
        setSeed(seed_);
    }

    /// <summary>
    ///   簡易暗号化器を作る
    /// </summary>
    public SimpleCipher(string seed_) {
        setSeed(Encoding.UTF8.GetBytes(seed_));
    }

    /// <summary>
    ///   暗号化する
    /// </summary>
    public string Encode(string src) {
        StringBuilder sb = new StringBuilder();
        int ptr = 0;
        foreach(byte ch in Encoding.UTF8.GetBytes(src)) {
            sb.AppendFormat("{0:X2}", ch^seed[ptr]);
            if(++ptr >= seed.Length)
                ptr = 0;
        }
        return sb.ToString();
    }

    /// <summary>
    ///   復号化する
    /// </summary>
    public string Decode(string src) {
        if(String.IsNullOrEmpty(src) || (src.Length%2 != 0))
            return "";
        byte[] buf = new byte[src.Length/2];
        int ptr = 0;
        for(int i = 0; i < src.Length/2; i++) {
            buf[i] = (byte)((hexValue(src[i*2])*16+hexValue(src[i*2+1]))^seed[ptr]);
            if(++ptr >= seed.Length)
                ptr = 0;
        }
        try {
            return Encoding.UTF8.GetString(buf);
        } catch(Exception) {
            return "";
        }
    }


    private byte[] seed;

    private void setSeed(byte[] seed_) {
        if((seed_ == null) || (seed_.Length == 0)) {
            seed = new byte[2]{0xaa,0x55};
            return;
        }
        seed = new byte[seed_.Length];
        for(int i = 0; i < seed_.Length; i++)
            seed[i] = (byte)(seed_[i]^0xff);
    }

    private static int hexValue(char ch) {
        if((ch >= '0') && (ch <= '9'))
            return ch-'0';
        if((ch >= 'a') && (ch <= 'f'))
            return ch-'a'+10;
        if((ch >= 'A') && (ch <= 'F'))
            return ch-'A'+10;
        return 0;
    }

}

} // End of namespace
