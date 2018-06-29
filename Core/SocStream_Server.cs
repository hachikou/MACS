/// SocStream_Server: Socket handling class with SSL support. - Server service routines.
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
///   サーバー動作用拡張部
/// </summary>

public partial class SocStream {

    /// <summary>
    ///   接続待ち受け用ソケットを作成する
    /// </summary>
    /// <param name="portno">ポート番号</param>
    /// <param name="backlog">接続待ち行列最大数</param>
    /// <returns>待ち受けソケット</returns>
    public static SocStream CreateListener(int portno, int backlog=1) {
        return CreateListener(IPAddress.Any, portno, backlog);
    }

    /// <summary>
    ///   接続待ち受け用ソケットを作成する
    /// </summary>
    /// <param name="addr">待ち受けIPアドレス</param>
    /// <param name="portno">ポート番号</param>
    /// <param name="backlog">接続待ち行列最大数</param>
    /// <returns>待ち受けソケット</returns>
    public static SocStream CreateListener(IPAddress addr, int portno, int backlog=1) {
        if((portno <= 0) || (portno >= 65535))
            throw new ArgumentOutOfRangeException("Invalid port number.");
        if(backlog < 1)
            throw new ArgumentOutOfRangeException("Invalid backlog parameter.");

        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ep = new IPEndPoint(addr, portno);
        s.Bind(ep);
        s.Listen(backlog);
        return new SocStream(s);
    }

    /// <summary>
    ///   クライアントからの接続を待ち、通信用ソケットを新たに作成する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     タイムアウト以外の接続失敗が発生すると、SocketExceptionが発生する。
    ///   </para>
    /// </remarks>
    /// <param name="timeout">最大待ち時間（ミリ秒）負の値を指定すると無限に待つ。</param>
    /// <param name="crtfile">サーバ認証CRTファイル名。nullの場合SSL接続せずに単純なTCPソケット接続を行なう。</param>
    /// <returns>新たな通信用ソケット。タイムアウト時はnull</returns>
    public SocStream Accept(int timeout=-1, string crtfile=null) {
        if(Soc == null)
            throw new SocketException(SocError.ENOTCONN);
        if(!Soc.Poll(timeout*1000, SelectMode.SelectRead))
            return null;
        Socket s = Soc.Accept();
        return new SocStream(s, crtfile);
    }

}

} // End of namespace
