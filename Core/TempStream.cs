/*! @file TempStream.cs
 * @brief 一時ファイルを読み書き可能なストリームとして扱うクラス。
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Collections;

namespace MACS {


/// <summary>
///   一時ファイルを読み書き可能なストリームとして扱うクラス。
/// </summary>
/// <remarks>
///   <para>
///     このストリームに書き込みを行なうと、まずMemoryStreamが作成され、そこに書
///     き込まれる。MemoryStreamのサイズが一定バイト数以上（デフォルト4096）に
///     なると、FileStreamが作成され、一時ファイルとして書き出される。
///     一度でも読み出しが行われると、それ以上の書き込みができなくなる。
///     MemoryStreamやFileStream、作成した一時ファイルは、Closeと共に消去され
///     る。
///     本クラスはマルチスレッドセーフではない。
///   </para>
/// </remarks>
public class TempStream : Stream, IDisposable {

    public static string TempPath = ".";   ///< 一時ファイルを作成するディレクトリ名
    public static int DefaultLimitSize = 4096; ///< FileStreamを使うように切り替えるサイズ（デフォルト値）

    /// <summary>
    ///   一時ファイルストリームを作成する。
    /// </summary>
    public TempStream() {
        init(DefaultLimitSize);
    }
    /// <summary>
    ///   一時ファイルストリームを作成する。FileStreamを使うように切り替えるサイ
    ///   ズを指定する。
    /// </summary>
    public TempStream(int limitsize) {
        init(limitsize);
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~TempStream() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    protected override void Dispose(bool disposing) {
        if(m_memstream != null) {
            m_memstream.Dispose();
            m_memstream = null;
        }
        if(m_filestream != null) {
            try {
                m_filestream.Close();
                File.Delete(m_filestream.Name);
            } catch(Exception) {
                // just ignore.
            }
            m_filestream.Dispose();
            m_filestream = null;
        }
    }

    public override bool CanRead {
        get { return true; }
    }

    public override bool CanSeek {
        get { return false; }
    }

    public override bool CanWrite {
        get { return !m_reading; }
    }

    public override long Length {
        get {
            if(m_filestream != null)
                return m_filestream.Length;
            if(m_memstream != null)
                return m_memstream.Length;
            return 0;
        }
    }

    public override long Position {
        get { throw new InvalidOperationException(); }
        set { throw new InvalidOperationException(); }
    }

    public override void Close() {
        if(m_filestream != null){
            m_filestream.Close();
            try {
                File.Delete(m_filestream.Name);
            } catch(Exception) {
                // just ignore.
            }
            m_filestream = null;
        }
        if(m_memstream != null){
            m_memstream.Close();
            m_memstream = null;
        }
    }

    public override void Flush() {
        if(m_filestream != null)
            m_filestream.Flush();
        if(m_memstream != null)
            m_memstream.Flush();
    }

    public override void SetLength(long len) {
        throw new InvalidOperationException();
    }

    public override long Seek(long pos, SeekOrigin o) {
        throw new InvalidOperationException();
    }

    public override int Read(byte[] buf, int start, int length) {
        if(!m_reading){
            if(m_filestream != null){
                string name = m_filestream.Name;
                m_filestream.Close();
                m_filestream = new FileStream(name, FileMode.Open, FileAccess.Read);
            }
            if(m_memstream != null){
                m_memstream.Seek(0, SeekOrigin.Begin);
            }
            m_reading = true;
        }
        if(m_filestream != null)
            return m_filestream.Read(buf, start, length);
        if(m_memstream != null)
            return m_memstream.Read(buf, start, length);
        return 0;
    }

    public override void Write(byte[] buf, int start, int length) {
        if(m_reading)
            throw new InvalidOperationException();
        if(m_filestream != null){
            m_filestream.Write(buf, start, length);
            return;
        }
        if(m_memstream == null)
            m_memstream = new MemoryStream();
        m_memstream.Write(buf, start, length);
        if(m_memstream.Length >= m_limitsize){
            string tmpname = Path.Combine(TempPath, string.Format("tmp.{0}", TempRandom.Next(1000000000)));
            m_filestream = new FileStream(tmpname, FileMode.Create, FileAccess.Write);
            m_memstream.Flush();
            m_memstream.WriteTo(m_filestream);
            m_memstream.SetLength(0);
        }
    }

    public override void WriteByte(byte data) {
        if(m_reading)
            throw new InvalidOperationException();
        if(m_filestream != null){
            m_filestream.WriteByte(data);
            return;
        }
        if(m_memstream == null)
            m_memstream = new MemoryStream();
        m_memstream.WriteByte(data);
        if(m_memstream.Length >= m_limitsize){
            string tmpname = Path.Combine(TempPath, string.Format("tmp.{0}", TempRandom.Next(1000000000)));
            m_filestream = new FileStream(tmpname, FileMode.Create, FileAccess.Write);
            m_memstream.Flush();
            m_memstream.WriteTo(m_filestream);
            m_memstream.SetLength(0);
        }
    }

    public void Reset() {
        if(m_filestream != null) {
            try {
                m_filestream.Close();
                File.Delete(m_filestream.Name);
            } catch(Exception) {
                // just ignore.
            }
            m_filestream = null;
        }
        if(m_memstream != null) {
            m_memstream.SetLength(0);
        }
        m_reading = false;
    }

    private static Random TempRandom = new Random();  // ユニークなファイル名を得るための乱数ジェネレータ

    private MemoryStream m_memstream;
    private FileStream m_filestream;
    private bool m_reading;
    private int m_limitsize;

    private void init(int limitsize) {
        m_limitsize = limitsize;
        m_memstream = null;
        m_filestream = null;
        m_reading = false;
    }

}

} // End of namespace
