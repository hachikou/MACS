/// SocStream: Socket handling class with SSL support.
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
///   ソケット接続をバイトストリームとして取り扱うためのクラス
/// </summary>
/// <remarks>
///   <para>
///     本クラスのインスタンスはマルチスレッドセーフではない。
///     必要に応じて SSL接続が透過的に行なわれる。（将来的には）
///   </para>
/// </remarks>

public partial class SocStream: Stream,IDisposable {

    /// <summary>
    ///   書き出しバッファのデフォルトサイズ
    /// </summary>
    public static int DefaultWriteBufferSize = 4096;


#region プロパティ

    /// <summary>
    ///   ソケット
    /// </summary>
    public Socket Soc {
        get; private set;
    }

    /// <summary>
    ///   ソケットが有効かどうか
    /// </summary>
    public bool IsValid {
        get { return (Soc!=null); }
    }

    /// <summary>
    ///   ソケットが接続しているかどうか
    /// </summary>
    public bool Connected {
        get { return ((Soc!=null) && Soc.Connected); }
    }

    /// <summary>
    ///   接続先のIPEndPoint
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     未接続の時はnullを返す。
    ///   </para>
    /// </remarks>
    public IPEndPoint EndPoint {
        get; private set;
    }

    /// <summary>
    ///   接続先のエンドポイントを文字列化したもの
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     未接続の時は空文字列を返す。
    ///   </para>
    /// </remarks>
    public string EndPointString {
        get {
            if(EndPoint == null)
                return "";
            return EndPoint.ToString();
        }
    }

    /// <summary>
    ///   接続先のIPアドレス
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     未接続の時はnullを返す。
    ///   </para>
    /// </remarks>
    public IPAddress Address {
        get {
            if(EndPoint == null)
                return null;
            return EndPoint.Address;
        }
    }

    /// <summary>
    ///   接続先のIPアドレスを文字列化したもの
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     未接続の時は空文字列を返す。
    ///   </para>
    /// </remarks>
    public string AddressString {
        get {
            if(EndPoint == null)
                return "";
            return EndPoint.Address.ToString();
        }
    }

    /// <summary>
    ///   送信バッファサイズ
    /// </summary>
    public int WriteBufferSize {
        get { return writeBuf.Length; }
        set {
            if(writeIndex != 0)
                throw new InvalidOperationException("Can't change WriteBufferSize while using write buffer.");
            writeBuf = new byte[value];
        }
    }

#endregion

#region 接続／切断

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public SocStream() {
        init();
    }

    /// <summary>
    ///   コンストラクタ。指定サーバに接続する
    /// </summary>
    /// <param name="server">接続先サーバ名</param>
    /// <param name="portno">接続先ポート番号</param>
    /// <param name="timeout">接続待ち最大時間（ミリ秒）0以下の場合無限に待つ。</param>
    /// <param name="crtfile">サーバ認証CRTファイル名。nullの場合SSL接続せずに単純なTCPソケット接続を行なう。</param>
    /// <param name="noipv6">false=IPv6プロトコルも使う, true=IPv6プロトコルを使わない。</param>
    public SocStream(string server, int portno, int timeout=0, string crtfile=null, bool noipv6=false) {
        init();
        Connect(server, portno, timeout, crtfile:crtfile, noipv6:noipv6);
    }

    /// <summary>
    ///   コンストラクタ。Socket指定版。
    /// </summary>
    public SocStream(Socket s, string crtfile=null) {
        init();
        Soc = s;
        if(s == null) {
            EndPoint = null;
        } else {
            try {
                EndPoint = s.RemoteEndPoint as IPEndPoint;
            } catch(SocketException) {
                // Listener利用時にはRemoteEndPointが無くてエラーになるが、
                // 問題はない。
            }
            if(!String.IsNullOrEmpty(crtfile))
                startSSL(crtfile);
        }
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~SocStream() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    protected override void Dispose(bool disposing) {
        Close();
    }

    public override bool CanRead {
        get { return true; }
    }

    public override bool CanSeek {
        get { return false; }
    }

    public override bool CanWrite {
        get { return true; }
    }

    public override bool CanTimeout {
        get { return true; }
    }

    public override long Length {
        get { throw new InvalidOperationException(); }
    }

    public override long Position {
        get { throw new InvalidOperationException(); }
        set { throw new InvalidOperationException(); }
    }

    public override int ReadTimeout {
        get; set;
    }

    public override int WriteTimeout {
        get; set;
    }

    /// <summary>
    ///   指定サーバに接続する
    /// </summary>
    /// <param name="server">接続先サーバ名</param>
    /// <param name="portno">接続先ポート番号</param>
    /// <param name="timeout">接続待ち最大時間（ミリ秒）0以下の場合無限に待つ。</param>
    /// <param name="crtfile">サーバ認証CRTファイル名。nullの場合SSL接続せずに単純なTCPソケット接続を行なう。</param>
    /// <param name="noipv6">false=IPv6プロトコルも使う, true=IPv6プロトコルを使わない。</param>
    /// <returns>true=接続成功, false=接続失敗</returns>
    public bool Connect(string server, int portno, int timeout=-1, string crtfile=null, bool noipv6=false) {
        if(timeout < 0)
            timeout = -1;
        Close();
        if(String.IsNullOrEmpty(server))
            throw new ArgumentNullException("Server is not specified.");
        if((portno <= 0) || (portno >= 65535))
            throw new ArgumentOutOfRangeException("Invalid port number.");
        timer.Reset();
        timer.Start();
        IPHostEntry host;
        try {
            host = Dns.GetHostEntry(server);
        } catch(SocketException ex) {
            if(ex.ErrorCode == SocError.HOST_NOT_FOUND)
                return false;
            throw ex;
        }
        foreach(IPAddress addr in host.AddressList) {
            if(noipv6 && (addr.AddressFamily == AddressFamily.InterNetworkV6))
                continue;
            IPEndPoint ipe = new IPEndPoint(addr, portno);
            Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try {
                IAsyncResult ar = s.BeginConnect(ipe, null, null);
                if(ar.AsyncWaitHandle.WaitOne(timeout,true)) {
                    s.EndConnect(ar);
                    if(s.Connected) {
                        Soc = s;
                        EndPoint = ipe;
                        break;
                    }
                } else {
                    throw new SocketException(SocError.ETIMEDOUT);
                }
            } catch(SocketException) {
                s.Close();
            }
            if((timeout >= 0) && (timer.ElapsedMilliseconds > timeout))
                break;
        }
        timer.Stop();
        if(Soc == null)
            return false;

        if(!String.IsNullOrEmpty(crtfile))
            startSSL(crtfile);
        return true;
    }

    /// <summary>
    ///   接続を切る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     すでに切断されている場合は何もしない。
    ///   </para>
    /// </remarks>
    public override void Close() {
        if(writeBuf != null) {
            try {
                if(writeIndex > 0)
                    flush(WriteTimeout);
            } catch(Exception) {
                // just ignore.
            }
            writeBuf = null;
        }
        if(Soc != null) {
            Soc.Close();
            init();
        }
    }

#endregion

#region バイト列の読み書き

    /// <summary>
    ///   ソケットから1バイト読む
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ソケットの切断などのエラーが発生した場合は、SocketExceptionがthrow
    ///     される。
    ///   </para>
    /// </remarks>
    /// <param name="res">読み取ったデータを格納する変数</param>
    /// <param name="timeout">タイムアウト時間（ミリ秒）</param>
    /// <returns>true=読み取り成功, false=タイムアウト</returns>
    public bool ReadOne(out byte res, int timeout) {
        if(Soc == null)
            throw new SocketException(SocError.ENOTCONN);
        if(!Soc.Poll(timeout*1000, SelectMode.SelectRead)) {
            res = 0;
            return false;
        }
        if(Soc.Available <= 0) {
            // 最後まで読み切った時
            res = 0;
            return false;
        }
        byte[] buf = new byte[1];
        if(Soc.Receive(buf, 0, 1, SocketFlags.None) != 1)
            throw new SocketException(SocError.EWOULDBLOCK);
        res = buf[0];
        return true;
    }

    /// <summary>
    ///   ソケットから1バイト読む（Stream互換用）
    /// </summary>
    public override int ReadByte() {
        byte b;
        if(ReadOne(out b,ReadTimeout))
            return (int)b;
        return -1;
    }

    /// <summary>
    ///   ソケットから複数バイトを読む
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ソケットの切断などのエラーが発生した場合は、SocketExceptionがthrow
    ///     される。
    ///   </para>
    /// </remarks>
    /// <param name="buf">読み取ったデータを格納する配列</param>
    /// <param name="offset">読み取ったデータを格納する先頭位置</param>
    /// <param name="size">読み取りバイト数</param>
    /// <param name="timeout">最大待ち時間（ミリ秒）</param>
    /// <returns>読みとったバイト数。sizeと等しくない場合はタイムアウトが発生した。</returns>
    public int Read(byte[] buf, int offset, int size, int timeout) {
        if(Soc == null)
            throw new SocketException(SocError.ENOTCONN);
        if(timeout == 0) {
            // すぐに読める分だけ読む
            int sz = Soc.Available;
            if(size < sz)
                sz = size;
            if(sz > 0)
                return Soc.Receive(buf, offset, sz, SocketFlags.None);
            else
                return 0;
        }
        timer.Reset();
        timer.Start();
        int len = 0;
        while((len < size) && ((timeout < 0) || (timer.ElapsedMilliseconds < timeout))) {
            int t = -1;
            if(timeout >= 0) {
                t = timeout-(int)timer.ElapsedMilliseconds;
                if(t < 0)
                    t = 0;
            }
            if(Soc.Poll(t*1000, SelectMode.SelectRead)) {
                int sz = Soc.Available;
                if(sz == 0)
                    break;
                if(sz < 0)
                    throw new SocketException(SocError.EWOULDBLOCK);
                if(size-len < sz)
                    sz = size-len;
                int l = Soc.Receive(buf, offset+len, sz, SocketFlags.None);
                if(l <= 0)
                    throw new SocketException(SocError.EWOULDBLOCK);
                len += l;
            }
        }
        timer.Stop();
        return len;
    }

    /// <summary>
    ///   ソケットから複数バイトを読む（Stream互換用）
    /// </summary>
    public override int Read(byte[] buf, int offset, int size) {
        return Read(buf,offset,size,ReadTimeout);
    }

    /// <summary>
    ///   ソケットに1バイト書き出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ソケットの切断などのエラーが発生した場合は、SocketExceptionがthrow
    ///     される。
    ///   </para>
    /// </remarks>
    /// <param name="val">書き出すデータ</param>
    /// <param name="timeout">最大待ち時間（ミリ秒）</param>
    public void WriteOne(byte val, int timeout) {
        byte[] b = new byte[1];
        b[0] = val;
        Write(b,0,1,timeout);
    }

    /// <summary>
    ///   ソケットに1バイト書き出す（Stream互換用）
    /// </summary>
    public override void WriteByte(byte val) {
        WriteOne(val,WriteTimeout);
    }

    /// <summary>
    ///   ソケットに複数バイトを書き出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     実際のソケットへの送信は内部バッファがたまる時点まで遅延する。
    ///     すぐに送信をしたい場合にはFlushを呼び出すこと。
    ///     ソケットの切断などのエラーが発生した場合は、SocketExceptionがthrow
    ///     される。
    ///   </para>
    /// </remarks>
    /// <param name="buf">書き出すデータ</param>
    /// <param name="offset">書き出すデータの先頭位置</param>
    /// <param name="size">書き出しバイト数</param>
    /// <param name="timeout">最大待ち時間（ミリ秒）</param>
    public void Write(byte[] buf, int offset, int size, int timeout) {
        if(Soc == null)
            throw new SocketException(SocError.ENOTCONN);
        if(writeBuf == null) {
            writeBuf = new byte[DefaultWriteBufferSize];
            writeIndex = 0;
        }

        timer.Reset();
        timer.Start();
        int len = 0;
        while((len < size) && ((timeout < 0) || (timer.ElapsedMilliseconds < timeout))) {
            // writeBufに詰め込めるだけ詰め込む。
            int l = size-len;
            if(l > writeBuf.Length-writeIndex)
                l = writeBuf.Length-writeIndex;
            Buffer.BlockCopy(buf, offset+len, writeBuf, writeIndex, l);
            writeIndex += l;
            // writeBufが満タンになったら送信。
            if(writeIndex >= writeBuf.Length)
                flush(timeout);
            len += l;
        }
        timer.Stop();
    }

    /// <summary>
    ///   ソケットに複数バイトを書き出す（Stream互換用）
    /// </summary>
    public override void Write(byte[] buf, int offset, int size) {
        Write(buf,offset,size,WriteTimeout);
    }

    /// <summary>
    ///   書き出しバッファに溜まっているデータを送信する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本ルーチンはデバイスドライバにデータを渡せば完了する。デバイスドライ
    ///     バの送信完了を待つわけではない。
    ///     ソケットの切断などのエラーが発生した場合は、SocketExceptionがthrow
    ///     される。
    ///   </para>
    /// </remarks>
    /// <param name="timeout">最大待ち時間（ミリ秒）負の値を指定すると無限に待つ。</param>
    /// <returns>true=成功,false=タイムアウト</returns>
    public bool Flush(int timeout) {
        if(Soc == null)
            throw new SocketException(SocError.ENOTCONN);
        if((writeBuf == null) || (writeIndex <= 0))
            return true;
        timer.Reset();
        timer.Start();
        bool res = flush(timeout);
        timer.Stop();
        return res;
    }

    /// <summary>
    ///   書き出しバッファに溜まっているデータを送信する（Stream互換用）
    /// </summary>
    public override void Flush() {
        Flush(WriteTimeout);
    }

    public override void SetLength(long len) {
        throw new InvalidOperationException();
    }

    public override long Seek(long pos, SeekOrigin o) {
        throw new InvalidOperationException();
    }

#endregion

#region private部

    private byte[] writeBuf = null;
    private int writeIndex = 0;
    private Stopwatch timer = new Stopwatch();

    private void init() {
        Soc = null;
        EndPoint = null;
        ReadTimeout = WriteTimeout = -1;
    }

    private bool flush(int timeout) {
        int len = 0;
        while((len < writeIndex) && ((timeout < 0) || (timer.ElapsedMilliseconds < timeout))) {
            int t = -1;
            if(timeout >= 0) {
                t = timeout-(int)timer.ElapsedMilliseconds;
                if(t < 0)
                    t = 0;
            }
            if(Soc.Poll(t*1000, SelectMode.SelectWrite)) {
                int l = Soc.Send(writeBuf, len, writeIndex-len, SocketFlags.None);
                if(l <= 0)
                    throw new SocketException(SocError.EWOULDBLOCK);
                len += l;
            }
        }
        writeIndex -= len;
        if(writeIndex < 0)
            writeIndex = 0; // Fail safe.
        return (writeIndex == 0);
    }

    private void startSSL(string crtfile) {
        throw new InvalidOperationException("Sorry, SSH connection is not supported yet.");
    }

#endregion

}

} // End of namespace
