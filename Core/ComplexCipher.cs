/// ComplexCipher: 文字列のハッシュ作成.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Security.Cryptography;

namespace MACS {

/// <summary>
///   ハッシュ生成機構
/// </summary>
public class ComplexCipher : IDisposable {

    /// <summary>
    ///   ハッシュ生成器を作る
    /// </summary>
    public ComplexCipher(byte[] seed_) {
        sha = SHA256.Create();
        setSeed(seed_);
    }

    /// <summary>
    ///   ハッシュ生成器を作る
    /// </summary>
    public ComplexCipher(string seed_) {
        sha = SHA256.Create();
        setSeed(Encoding.UTF8.GetBytes(seed_));
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~ComplexCipher() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    public void Dispose() {
        if(sha != null) {
            sha.Dispose();
            sha = null;
        }
    }

    /// <summary>
    ///   ハッシュ化する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     SimpleCipherと同じく使えるようにするため、Encodeと名付けていますが、
    ///     中身はハッシュ化なので、復号はできません。
    ///   </para>
    /// </remarks>
    public string Encode(string src) {
        byte[] data = Encoding.UTF8.GetBytes(src);
        int ptr = 0;
        for(int i = 0; i < data.Length; i++) {
            data[i] ^= seed[ptr];
            if(++ptr >= seed.Length)
                ptr = 0;
        }
        byte[] hash = sha.ComputeHash(data);
        StringBuilder sb = new StringBuilder();
        foreach(byte ch in hash) {
            sb.AppendFormat("{0:X2}", ch);
        }
        return sb.ToString();
    }

    private SHA256 sha;
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

}

} // End of namespace
