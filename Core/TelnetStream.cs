/**
 * TelnetStream: Telnetプロトコルを処理するストリーム
 * $Id:$
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

//#define FULLDEBUG

using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace MACS {


/// <summary>
///   Telnetプロトコルを処理するストリーム
/// </summary>
/// <remarks>
///   <para>
///     本ストリームはバイト列を取り扱います。文字エンコードが必要な場合は、
///     StreamReaderなどを使ってラッピングしてください。
///   </para>
/// </remarks>
public class TelnetStream: Stream,IDisposable {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    /// <param name="baseStream_"></param>
    /// <param name="leaveOpen_">Closeする際にbaseStreamをCloseしない場合にはtrue。デフォルトはfalse（Closeの際にbaseStreamもCloseする）</param>
    public TelnetStream(Stream baseStream_, bool leaveOpen_=false) {
        baseStream = baseStream_;
        leaveOpen = leaveOpen_;
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~TelnetStream() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    protected override void Dispose(bool disposing) {
        Close();
    }

    public override bool CanRead {
        get { return baseStream.CanRead; }
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
        get { throw new InvalidOperationException(); }
    }

    public override long Position {
        get { throw new InvalidOperationException(); }
        set { throw new InvalidOperationException(); }
    }

    /// <summary>
    ///   ストリームを閉じる
    /// </summary>
    public override void Close() {
        if(baseStream != null) {
            if(!leaveOpen) {
                baseStream.Close();
            }
            baseStream = null;
        }
    }

    public override int ReadByte() {
        int ch = 0;
        while(true) {
            ch = baseStream.ReadByte();
            if(ch < 0) // エラー発生
                return ch;
#if FULLDEBUG
            Console.WriteLine("RECV: 0x{0:X2}", ch);
#endif
            if(ch < 0xf0) // 通常の文字
                break;
            if(ch != IAC) // IAC以外のTelnet特殊コードは無視
                continue;
            // Telnetエスケープ開始
            int cmd = baseStream.ReadByte();
            if(cmd < 0) {
#if FULLDEBUG
                Console.WriteLine("No cmd.");
#endif
                return cmd;
            }
            int opt = baseStream.ReadByte();
            if(opt < 0) {
#if FULLDEBUG
                Console.WriteLine("No opt.");
#endif
                return opt;
            }
#if FULLDEBUG
            Console.WriteLine("IAC {0} {1}", codeName(cmd), codeName(opt));
#endif
            switch(cmd) {
            case DO: // 相手からオプションの使用を要求されている
                switch(opt) {
                case BinaryTransmission:
                    if(!binaryTransmission) {
                        binaryTransmission = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WILL BinaryTransmission");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WILL,(byte)BinaryTransmission}, 0, 3);
                    }
                    break;
                case Echo:
                    if(!myEcho) {
                        myEcho = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WILL Echo");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WILL,(byte)Echo}, 0, 3);
                    }
                    break;
                case SupressGoAhead:
                    if(!supressGoAhead) {
                        supressGoAhead = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WILL SupressGoAhead");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WILL,(byte)SupressGoAhead}, 0, 3);
                    }
                    break;
                case LineMode:
                    if(!myLineMode) {
                        myLineMode = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WILL LineMode");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WILL,(byte)LineMode}, 0, 3);
                    }
                    break;
                default:
                    // それ以外のオプションは無視する。
                    break;
                }
                break;
            case WILL: // 相手がオプションの使用を宣言した
                switch(opt) {
                case BinaryTransmission:
                    if(!binaryTransmission) {
                        binaryTransmission = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WILL BinaryTransmission");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WILL,(byte)BinaryTransmission}, 0, 3);
                    }
                    break;
                case Echo:
                    if(!herEcho) {
                        herEcho = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC DO Echo");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)DO,(byte)Echo}, 0, 3);
                    }
                    break;
                case SupressGoAhead:
                    if(!supressGoAhead) {
                        supressGoAhead = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WILL SupressGoAhead");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WILL,(byte)SupressGoAhead}, 0, 3);
                    }
                    break;
                case LineMode:
                    if(!herLineMode) {
                        herLineMode = true;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC DO LineMode");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)DO,(byte)LineMode}, 0, 3);
                    }
                    break;
                default:
                    // それ以外のオプションは無視する。
                    break;
                }
                break;
            case DONT: // 相手からオプションを使用しないことを要求されている
                switch(opt) {
                case BinaryTransmission:
                    if(binaryTransmission) {
                        binaryTransmission = false;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WONT BinaryTransmission");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WONT,(byte)BinaryTransmission}, 0, 3);
                    }
                    break;
                case Echo:
                    if(myEcho) {
                        myEcho = false;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WONT Echo");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WONT,(byte)Echo}, 0, 3);
                    }
                    break;
                case LineMode:
                    if(myLineMode) {
                        myLineMode = false;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WONT LineMode");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WONT,(byte)LineMode}, 0, 3);
                    }
                    break;
                default:
                    // それ以外のオプションは無視する。
                    break;
                }
                break;
            case WONT: // 相手がオプションを使用しないことを宣言した
                switch(opt) {
                case BinaryTransmission:
                    if(binaryTransmission) {
                        binaryTransmission = false;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC WONT BinaryTransmission");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)WONT,(byte)BinaryTransmission}, 0, 3);
                    }
                    break;
                case Echo:
                    if(herEcho) {
                        herEcho = false;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC DONT Echo");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)DONT,(byte)Echo}, 0, 3);
                    }
                    break;
                case LineMode:
                    if(herLineMode) {
                        herLineMode = false;
#if FULLDEBUG
                        Console.WriteLine("Sending IAC DONT LineMode");
#endif
                        baseStream.Write(new byte[]{(byte)IAC,(byte)DONT,(byte)LineMode}, 0, 3);
                    }
                    break;
                default:
                    // それ以外のオプションは無視する。
                    break;
                }
                break;
            case SB:
                // IAC-SEが来るまでスキップ。
                if(!skipSBParams())
                    return -1;
                break;
            default:
                // それ以外のコマンドは無視。
                break;
            }
        }
        if(myEcho && !SuspendEcho) {
#if FULLDEBUG
            Console.WriteLine("ECHO 0x{0:X2}", ch);
#endif
            baseWriteByte((byte)ch);
        } else if(CookedMode && (ch == 0x0a)) {
            baseStream.WriteByte(0x0d);
        }
        return ch;
    }

    public override int Read(byte[] buf, int offset, int size) {
        int len = 0;
        while(size > 0) {
            int ch = ReadByte();
            if(ch < 0) {
                //if(len == 0)
                //    return ch;
                return len;
            }
            buf[offset] = (byte)ch;
            offset++;
            size--;
            len++;
        }
        return len;
    }

    public override void WriteByte(byte val) {
        baseWriteByte(val);
        if(ForceFlush)
            baseStream.Flush();
    }

    public override void Write(byte[] buf, int offset, int size) {
        baseWrite(buf, offset, size);
        if(ForceFlush)
            baseStream.Flush();
    }

    public override void Flush() {
        baseStream.Flush();
    }

    public override void SetLength(long len) {
        throw new InvalidOperationException();
    }

    public override long Seek(long pos, SeekOrigin o) {
        throw new InvalidOperationException();
    }

    /// <summary>
    ///   WriteのたびにFlushするフラグ
    /// </summary>
    public bool ForceFlush = false;

    /// <summary>
    ///   特殊コードを加工して送信するフラグ
    /// </summary>
    public bool CookedMode = true;

    /// <summary>
    ///   エコーバックを一時的に止めるフラグ
    /// </summary>
    public bool SuspendEcho = false;

    /// <summary>
    ///   相手にBinaryTransmissionオプションを要求する
    /// </summary>
    public void RequestBinaryTransmission(bool yesno=true) {
#if FULLDEBUG
        Console.WriteLine("Request IAC {0} BinaryTransmission",yesno?"DO":"DONT");
#endif
        baseStream.Write(new byte[]{(byte)IAC,yesno?(byte)DO:(byte)DONT,(byte)BinaryTransmission}, 0, 3);
        binaryTransmission = yesno;
    }

    /// <summary>
    ///   自分がBinaryTransmissionオプションを宣言する
    /// </summary>
    public void DeclareBinaryTransmission(bool yesno=true) {
#if FULLDEBUG
        Console.WriteLine("Declare IAC {0} BinaryTransmission",yesno?"WILL":"WONT");
#endif
        baseStream.Write(new byte[]{(byte)IAC,yesno?(byte)WILL:(byte)WONT,(byte)BinaryTransmission}, 0, 3);
        binaryTransmission = yesno;
    }

    /// <summary>
    ///   相手にEchoオプションを要求する
    /// </summary>
    public void RequestEcho(bool yesno=true) {
#if FULLDEBUG
        Console.WriteLine("Request IAC {0} Echo",yesno?"DO":"DONT");
#endif
        baseStream.Write(new byte[]{(byte)IAC,yesno?(byte)DO:(byte)DONT,(byte)Echo}, 0, 3);
        herEcho = yesno;
    }

    /// <summary>
    ///   自分がEchoオプションを宣言する
    /// </summary>
    public void DeclareEcho(bool yesno=true) {
#if FULLDEBUG
        Console.WriteLine("Declare IAC {0} Echo",yesno?"WILL":"WONT");
#endif
        baseStream.Write(new byte[]{(byte)IAC,yesno?(byte)WILL:(byte)WONT,(byte)Echo}, 0, 3);
        myEcho = yesno;
    }

    /// <summary>
    ///   相手にLineModeオプションを要求する
    /// </summary>
    public void RequestLineMode(bool yesno=true) {
#if FULLDEBUG
        Console.WriteLine("Request IAC {0} LineMode",yesno?"DO":"DONT");
#endif
        baseStream.Write(new byte[]{(byte)IAC,yesno?(byte)DO:(byte)DONT,(byte)LineMode}, 0, 3);
        herLineMode = yesno;
    }

    /// <summary>
    ///   自分がLineModeオプションを宣言する
    /// </summary>
    public void DeclareLineMode(bool yesno=true) {
#if FULLDEBUG
        Console.WriteLine("Declare IAC {0} LineMode",yesno?"WILL":"WONT");
#endif
        baseStream.Write(new byte[]{(byte)IAC,yesno?(byte)WILL:(byte)WONT,(byte)LineMode}, 0, 3);
        myLineMode = yesno;
    }

    private const int SE   = 0xf0;
    private const int SB   = 0xfa;
    private const int WILL = 0xfb;
    private const int WONT = 0xfc;
    private const int DO   = 0xfd;
    private const int DONT = 0xfe;
    private const int IAC  = 0xff;
    private const int BinaryTransmission = 0x00;
    private const int Echo = 0x01;
    private const int SupressGoAhead = 0x03;
    private const int LineMode = 0x22;

    private Stream baseStream;
    private bool leaveOpen;
    private bool binaryTransmission = true;
    private bool myEcho = false;
    private bool herEcho = false;
    private bool supressGoAhead = true;
    private bool myLineMode = true;
    private bool herLineMode = true;

    private void baseWrite(byte[] vals, int offset, int size) {
        if(CookedMode) {
            for(int i = 0; i < size; i++) {
                baseWriteOne(vals[offset+i]);
            }
        } else {
            baseStream.Write(vals, offset, size);
        }
        baseStream.Flush();
    }

    private void baseWriteByte(byte val) {
        if(CookedMode) {
            baseWriteOne(val);
        } else {
            baseStream.WriteByte(val);
        }
        baseStream.Flush();
    }

    private void baseWriteOne(byte val) {
        switch(val) {
        case 0x0d: // CR
            break;
        case 0x0a: // LF
            baseStream.Write(new byte[]{0x0d,0x0a}, 0, 2);
            break;
        case 0x02: // STX (local echo on)
        case 0x03: // ETX (local echo off)
            SuspendEcho = (val == 0x03);
#if FULLDEBUG
            Console.WriteLine("SuspendEcho={0}",SuspendEcho);
#endif
            DeclareEcho(SuspendEcho);
            break;
        default:
            baseStream.WriteByte(val);
            break;
        }
    }

    private bool skipSBParams() {
        int phase = 0;
        while(true) {
            int ch = baseStream.ReadByte();
            if(ch < 0)
                break;
            switch(phase) {
            case 0: // IAC待ち
                if(ch == IAC)
                    phase++;
                break;
            case 1: // SE待ち
                if(ch == SE)
                    return true;
                if(ch != IAC)
                    phase = 0;
                break;
            }
        }
        return false;
    }

    private static string codeName(int x) {
        switch(x) {
        case SE:
            return "SE";
        case SB:
            return "SB";
        case WILL:
            return "WILL";
        case WONT:
            return "WONT";
        case DO:
            return "DO";
        case DONT:
            return "DONT";
        case IAC:
            return "IAC";
        case BinaryTransmission:
            return "BinaryTransmission";
        case Echo:
            return "Echo";
        case SupressGoAhead:
            return "SupressGoAhead";
        case LineMode:
            return "LineMode";
        default:
            return String.Format("0x{0:X2}",x);
        }
    }
}

} // End of namespace
