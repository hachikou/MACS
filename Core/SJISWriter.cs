/*! @file SJISWriter.cs
 * @brief Shift_JISによる固定バイト数出力をサポートするクラス
 * $Id: $
 *
 * Copyright (C) 2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MACS {

/// <summary>
///   Shift_JISによる固定バイト数出力をサポートするクラス
/// </summary>
public class SJISWriter : IDisposable {

    /// <summary>
    ///   Shift_JISによるファイル出力ストリームを開く
    /// </summary>
    public SJISWriter(string filename) {
        fs = FileUtil.BinaryWriter(filename);
        if(fs == null)
            throw new IOException(String.Format("Can't open '{0}' for writing.", filename));
        internalfs = true;
    }

    /// <summary>
    ///   指定ストリームをShift_JIS書き出し用ストリームとして使う
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本クラスでClose()しても、指定ストリームはClose()されない。
    ///   </para>
    /// </remarks>
    public SJISWriter(Stream fs_) {
        if(fs_ == null)
            throw new IOException("Stream is null");
        fs = fs_;
        internalfs = false;
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~SJISWriter() {
        Dispose();
    }

    /// <summary>
    ///   使用リソースの解放
    /// </summary>
    public void Dispose() {
        Close();
    }

    /// <summary>
    ///   ファイル書き込みを終了する。
    /// </summary>
    public void Close() {
        if(internalfs && (fs != null)) {
            fs.Close();
            fs = null;
        }
    }

    /// <summary>
    ///   指定文字列を書き出す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     txtがnullの場合は何も書き出さない。
    ///   </para>
    /// </remarks>
    public void Write(string txt) {
        if(txt == null)
            return;
        byte[] b = SJISDictionary.GetBytes(txt);
        fs.Write(b, 0, b.Length);
    }

    /// <summary>
    ///   フォーマット付き文字列書き出し
    /// </summary>
    public void Write(string fmt, params object[] objs) {
        Write(String.Format(fmt, objs));
    }

    /// <summary>
    ///   指定文字列を書き出し、改行を書き出す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     txtがnullの場合は改行のみ書き出す。
    ///     改行は "\r\n"。
    ///   </para>
    /// </remarks>
    public void WriteLine(string txt=null) {
        Write(txt);
        Write("\r\n");
    }

    /// <summary>
    ///   フォーマット付き文字列書き出し。改行付き。
    /// </summary>
    public void WriteLine(string fmt, params object[] objs) {
        Write(String.Format(fmt, objs));
        Write("\r\n");
    }

    /// <summary>
    ///   出力バイト数指定付き文字列書き出し
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="rightAlign">右寄せにするかどうか</param>
    /// <param name="txt">出力文字列</param>
    /// <param name="fillbyte">埋め文字コード</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合、rightAlignフラグに応じて左また
    ///     は右にfillbyteで指定したコードが挿入される。
    ///     なお、fillbyteのデフォルトは 0x20 (' ')である。
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、先頭のlenバイトだけが出力され
    ///     る。この際、文字コードの区切りが考慮される。
    ///   </para>
    ///   <para>
    ///     txtがnullの時にはlen個のfillbyteで指定したコードが出力される。
    ///   </para>
    /// </remarks>
    public void Write(int len, bool rightAlign, string txt, byte fillbyte=0x20) {
        write(len, rightAlign, txt, fillbyte, true, true);
    }


    /// <summary>
    ///   出力バイト数指定付き文字列書き出し（左寄せ）
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="txt">出力文字列</param>
    /// <param name="fillbyte">埋め文字コード</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合、右にfillbyteで指定するコードが挿入される。
    ///     なお、fillbyteのデフォルトは 0x20 (' ')である。
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、先頭のlenバイトだけが出力され
    ///     る。この際、文字コードの区切りが考慮される。
    ///   </para>
    ///   <para>
    ///     txtがnullの時にはlen個のfillbyteで指定するコードが出力される。
    ///   </para>
    /// </remarks>
    public void Write(int len, string txt, byte fillbyte=0x20) {
        write(len, false, txt, fillbyte, true, true);
    }

    /// <summary>
    ///   出力バイト数指定付き文字列書き出し（右寄せ）
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="txt">出力文字列</param>
    /// <param name="fillbyte">埋め文字コード</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合、左にfillbyteで指定するコードが挿入される。
    ///     なお、fillbyteのデフォルトは 0x20 (' ')である。
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、先頭のlenバイトだけが出力され
    ///     る。この際、文字コードの区切りが考慮される。
    ///   </para>
    ///   <para>
    ///     txtがnullの時にはlen個のfillbyteで指定するコードが出力される。
    ///   </para>
    /// </remarks>
    public void WriteRight(int len, string txt, byte fillbyte=0x20) {
        write(len, true, txt, fillbyte, true, true);
    }

    /// <summary>
    ///   出力バイト数指定付き文字列書き出し（最大バイト数指定）
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="txt">出力文字列</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合指定文字列だけが出力される。
    ///     （lenより少ないバイト数が出力される。）
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、先頭のlenバイトだけが出力され
    ///     る。この際、文字コードの区切りが考慮される。
    ///   </para>
    ///   <para>
    ///     txtがnullの時には何も出力されない。
    ///   </para>
    /// </remarks>
    public void WriteLimitLength(int len, string txt) {
        write(len, false, txt, 0x20, false, true);
    }

    /// <summary>
    ///   出力バイト数指定付き文字列書き出し（切り捨て無し）
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="rightAlign">右寄せにするかどうか</param>
    /// <param name="txt">出力文字列</param>
    /// <param name="fillbyte">埋め文字コード</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合、rightAlignフラグに応じて左また
    ///     は右にfillbyteで指定したコードが挿入される。
    ///     なお、fillbyteのデフォルトは 0x20 (' ')である。
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、lenに関係なくすべての文字が
    ///     出力される。この点がWrite(len,rightAlign,txt,fillbyte)と異なる点で
    ///     ある。
    ///   </para>
    ///   <para>
    ///     txtがnullの時にはlen個のfillbyteで指定したコードが出力される。
    ///   </para>
    /// </remarks>
    public void WriteAll(int len, bool rightAlign, string txt, byte fillbyte=0x20) {
        write(len, rightAlign, txt, fillbyte, true, false);
    }


    /// <summary>
    ///   出力バイト数指定付き文字列書き出し（左寄せ、切り捨て無し）
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="txt">出力文字列</param>
    /// <param name="fillbyte">埋め文字コード</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合、右にfillbyteで指定するコードが挿入される。
    ///     なお、fillbyteのデフォルトは 0x20 (' ')である。
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、lenに関係なくすべての文字が
    ///     出力される。この点がWrite(len,txt,fillbyte)と異なる点である。
    ///   </para>
    ///   <para>
    ///     txtがnullの時にはlen個のfillbyteで指定するコードが出力される。
    ///   </para>
    /// </remarks>
    public void WriteAll(int len, string txt, byte fillbyte=0x20) {
        write(len, false, txt, fillbyte, true, false);
    }

    /// <summary>
    ///   出力バイト数指定付き文字列書き出し（右寄せ、切り捨て無し）
    /// </summary>
    /// <param name="len">出力バイト数</param>
    /// <param name="txt">出力文字列</param>
    /// <param name="fillbyte">埋め文字コード</param>
    /// <remarks>
    ///   <para>
    ///     指定文字列がlenバイトに満たない場合、左にfillbyteで指定するコードが挿入される。
    ///     なお、fillbyteのデフォルトは 0x20 (' ')である。
    ///   </para>
    ///   <para>
    ///     指定文字列がlenバイトよりも大きい場合、lenに関係なくすべての文字が
    ///     出力される。この点がWriteRight(len,txt,fillbyte)と異なる点である。
    ///   </para>
    ///   <para>
    ///     txtがnullの時にはlen個のfillbyteで指定するコードが出力される。
    ///   </para>
    /// </remarks>
    public void WriteRightAll(int len, string txt, byte fillbyte=0x20) {
        write(len, true, txt, fillbyte, true, false);
    }


    /// <summary>
    ///   内部ストリーム
    /// </summary>
    private Stream fs;

    private bool internalfs;

    private void write(int len, bool rightAlign, string txt, byte fillbyte, bool fillFlag, bool cutFlag) {
        if(len <= 0)
            return;
        if(txt == null)
            txt = "";
        byte[] b = SJISDictionary.GetBytes(txt);
        if(b.Length >= len) {
            if(cutFlag) {
                while(b.Length > len) {
                    txt = txt.Substring(0, txt.Length-1);
                    b = SJISDictionary.GetBytes(txt);
                }
            }
            fs.Write(b, 0, b.Length);
	    int blen = b.Length;
            b = new byte[1];
            b[0] = 0x20;
            for(int i = 0; i < len-blen; i++)
                fs.Write(b, 0, 1);
            return;
        }
        if(fillFlag) {
            byte[] fill = new byte[len-b.Length];
            for(int i = 0; i < fill.Length; i++)
                fill[i] = 0x20;
            if(rightAlign) {
                fs.Write(fill, 0, fill.Length);
                fs.Write(b, 0, b.Length);
            } else {
                fs.Write(b, 0, b.Length);
                fs.Write(fill, 0, fill.Length);
            }
        } else {
            fs.Write(b, 0, b.Length);
        }
    }

}

} // End of namespace
