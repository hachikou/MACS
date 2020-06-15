/// TinyChunkStream: Tiny simple chunked stream filter.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace MACS {


/// <summary>
///   入出力バイト列を255バイト以下のデータブロック列として扱うストリーム
/// </summary>
/// <remarks>
///   <para>
///     本クラスはマルチスレッドセーフではない。
///   </para>
/// </remarks>
public class TinyChunkStream: Stream,IDisposable {

    /// <summary>
    ///   デフォルトチャンクサイズ
    /// </summary>
    public static byte DefaultChunkSize = 255;


    /// <summary>
    ///   チャンクサイズ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     書き出し時のチャンクサイズ。読み込み時のサイズは受信データから読み取られます。
    ///   </para>
    /// </remarks>
    public byte ChunkSize {
        get { return chunkSize; }
        set {
            if(count > 0)
                throw new InvalidOperationException("Can't change ChunkSize while the buffer is not empty.");
            chunkSize = value;
            if(buffer != null)
                buffer = null;
        }
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    /// <param name="baseStream_">チャンク化されたバイト列の書き出し先／読み込み元</param>
    /// <param name="chunkSize_">チャンクサイズ。1〜255。省略時は DefaultChunkSize</param>
    /// <param name="leaveOpen_">Closeする際にbaseStreamをCloseしない場合にはtrue。デフォルトはfalse（Closeの際にbaseStreamもCloseする）</param>
    public TinyChunkStream(Stream baseStream_, byte chunkSize_ = 0, bool leaveOpen_=false) {
        baseStream = baseStream_;
        if(chunkSize_ == 0)
            chunkSize = DefaultChunkSize;
        else
            chunkSize = chunkSize_;
        leaveOpen = leaveOpen_;
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~TinyChunkStream() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    protected override void Dispose(bool disposing) {
        Close();
    }

    public override bool CanRead {
        get { return (!writing && baseStream.CanRead); }
    }

    public override bool CanSeek {
        get { return false; }
    }

    public override bool CanWrite {
        get { return baseStream.CanWrite; }
    }

    public override bool CanTimeout {
        get { return baseStream.CanTimeout; }
    }

    public override long Length {
        get { throw new InvalidOperationException("Length is not supported"); }
    }

    public override long Position {
        get { throw new InvalidOperationException("Position is not supported"); }
        set { throw new InvalidOperationException("Position(set) is not supported"); }
    }

    /// <summary>
    ///   ストリームを閉じる
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Finishされていない出力はFinishされる。
    ///   </para>
    /// </remarks>
    public override void Close() {
        if(baseStream != null) {
            if(writing) {
                try {
                    Finish(true);
                } catch(Exception) {
                    // just ignore.
                }
            }
            if(!leaveOpen) {
                baseStream.Close();
            }
            baseStream = null;
        }
    }

    public override int ReadByte() {
        byte[] b = new byte[1];
        if(Read(b, 0, 1) != 1)
            return -1;
        return (int)b[0];
    }

    public override int Read(byte[] buf, int offset, int size) {
        if(writing) {
            Finish(true);
        }
        if(rChunkSize == 0) {
            rChunkSize = DefaultChunkSize; // 次のReadで新しいデータを読み取るように
            return 0;
        }
        if(rBuffer == null) {
            rBuffer = new byte[255];
            rCount = 0;
        }
        int len = 0;
        while(size > 0) {
            if(rCount == 0) {
                if(baseStream.Read(rBuffer,0,1) != 1)
                    break;
                rChunkSize = rCount = rBuffer[0];
                if(rChunkSize == 0)
                    break;
                rCount = (byte)baseStream.Read(rBuffer, 0, rChunkSize);
            }
            int sz = (int)rCount;
            if(size < sz)
                sz = size;
            Buffer.BlockCopy(rBuffer,rChunkSize-rCount,buf,offset,sz);
            size -= sz;
            offset += sz;
            rCount -= (byte)sz;
            len += sz;
        }
        return len;
    }

    public override void WriteByte(byte val) {
        byte[] b = new byte[1];
        b[0] = val;
        Write(b,0,1);
    }

    public override void Write(byte[] buf, int offset, int size) {
        if(!writing || (buffer == null)) {
            buffer = new byte[chunkSize];
            count = 0;
            writing = true;
        }
        while(size > 0) {
            int sz = (int)(chunkSize-count);
            if(size < sz)
                sz = size;
            Buffer.BlockCopy(buf,offset,buffer,count,sz);
            size -= sz;
            offset += sz;
            count += (byte)sz;
            if(count >= chunkSize){
                Flush(false);
            }
        }
    }

    public override void Flush() {
        Flush(true);
    }

    public void Flush(bool baseFlush) {
        if(writing && (count > 0) && (buffer != null)) {
            byte[] b = new byte[1];
            b[0] = count;
            baseStream.Write(b,0,1);
            baseStream.Write(buffer,0,count);
            count = 0;
        }
        if(baseFlush)
            baseStream.Flush();
    }

    public void Finish(bool baseFlush=false) {
        if(writing && (buffer != null)) {
            Flush(false);
            buffer[0] = 0;
            baseStream.Write(buffer,0,1);
            buffer = null;
            writing = false;
        }
        if(baseFlush)
            baseStream.Flush();
    }

    public override void SetLength(long len) {
        throw new InvalidOperationException();
    }

    public override long Seek(long pos, SeekOrigin o) {
        throw new InvalidOperationException();
    }


    private Stream baseStream;
    private bool leaveOpen;
    private byte[] buffer = null;
    private byte[] rBuffer = null;
    private byte chunkSize;
    private byte rChunkSize = DefaultChunkSize;
    private byte count = 0;
    private byte rCount = 0;
    private bool writing = false;
}

} // End of namespace
