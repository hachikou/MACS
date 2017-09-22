/*! @file Syslog.cs
 * @brief syslogを書き出すロガー
 *
 * Copyright (C) 2017 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MACS {

/// <summary>
///   syslogを書き出すロガー
/// </summary>
public class Syslog : IDisposable {

#region 定数定義

    /// <summary>
    ///   デフォルトポート番号
    /// </summary>
    public const int DefaultPort = 514;

    /// <summary>
    ///   ログレベル
    /// </summary>
    public enum Level {
        EMERG = 0,
        ALERT = 1,
        CRIT = 2,
        ERR = 3,
        WARNING = 4,
        NOTICE = 5,
        INFO = 6,
        DEBUG = 7,
    };

    public enum FacilityEnum {
        KERN        = (0<<3),  /* kernel messages */
        USER        = (1<<3),  /* random user-level messages */
        MAIL        = (2<<3),  /* mail system */
        DAEMON      = (3<<3),  /* system daemons */
        AUTH        = (4<<3),  /* security/authorization messages */
        SYSLOG      = (5<<3),  /* messages generated internally by syslogd */
        LPR         = (6<<3),  /* line printer subsystem */
        NEWS        = (7<<3),  /* network news subsystem */
        UUCP        = (8<<3),  /* UUCP subsystem */
        CRON        = (9<<3),  /* clock daemon */
        AUTHPRIV    = (10<<3), /* security/authorization messages (private) */
        FTP         = (11<<3), /* ftp daemon */
        /* other codes through 15 reserved for system use */
        LOCAL0      = (16<<3), /* reserved for local use */
        LOCAL1      = (17<<3), /* reserved for local use */
        LOCAL2      = (18<<3), /* reserved for local use */
        LOCAL3      = (19<<3), /* reserved for local use */
        LOCAL4      = (20<<3), /* reserved for local use */
        LOCAL5      = (21<<3), /* reserved for local use */
        LOCAL6      = (22<<3), /* reserved for local use */
        LOCAL7      = (23<<3), /* reserved for local use */
    };

    /// <summary>
    ///   文字列をログレベルに変換する
    /// </summary>
    public static Level ToLevel(string str) {
        if(str == null)
            return Level.DEBUG;
        switch(str.ToUpper()) {
        case "DEBUG":
            return Level.DEBUG;
        case "INFO":
            return Level.INFO;
        case "NOTICE":
            return Level.NOTICE;
        case "WARNING":
        case "WARN":
            return Level.WARNING;
        case "ERR":
        case "ERROR":
            return Level.ERR;
        case "CRIT":
            return Level.CRIT;
        case "ALERT":
            return Level.ALERT;
        case "EMERG":
            return Level.EMERG;
        default:
            return Level.DEBUG;
        }
    }

    /// <summary>
    ///   文字列をファシリティに変換する
    /// </summary>
    public static FacilityEnum ToFacility(string str) {
        if(str == null)
            return FacilityEnum.USER;
        FacilityEnum e;
        if(Enum.TryParse<FacilityEnum>(str.ToUpper(), out e))
            return e;
        return FacilityEnum.USER;
    }

#endregion

#region ロガーの作成と属性

    /// <summary>
    ///   syslogのデータロガーを作成する
    /// </summary>
    public Syslog() {
        server = "localhost";
        port = DefaultPort;
    }

    /// <summary>
    ///   syslogのデータロガーを作成する（サーバとポートを指定する版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     サーバー名に":ポート番号"がついていれば、port_引数は無視されます。
    ///   </para>
    /// </remarks>
    public Syslog(string server_, int port_=DefaultPort) {
        port = port_;
        setServer(server_);
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~Syslog() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    public void Dispose() {
        lock(mutex) {
            close();
        }
    }

    /// <summary>
    ///   ソケットを廃棄する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     一旦クローズしても、再びログを出力することができます。
    ///   </para>
    /// </remarks>
    public void Close() {
        lock(mutex) {
            close();
        }
    }

    /// <summary>
    ///   サーバ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     末尾に":ポート番号"がついているとポート番号も同時に指定できます。
    ///   </para>
    /// </remarks>
    public string Server {
        get {
            if(port == DefaultPort)
                return server;
            return server+":"+port.ToString();
        }
        set {
            lock(mutex){
                close();
                setServer(value);
            }
        }
    }

    /// <summary>
    ///   ポート番号
    /// </summary>
    public int Port {
        get { return port; }
        set {
            lock(mutex){
                close();
                port = value;
            }
        }
    }

    /// <summary>
    ///   ファシリティ
    /// </summary>
    public FacilityEnum Facility = FacilityEnum.LOCAL0;

    /// <summary>
    ///   送信元ホスト名
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     特に指定しない場合、Dns.GetHostName()が用いられます。
    ///   </para>
    /// </remarks>
    public string HostName = null;

    /// <summary>
    ///   モジュール名
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     プログラム名など。メッセージの先頭に付加される。
    ///   </para>
    /// </remarks>
    public string ModuleName = "";

    /// <summary>
    ///   送信するログレベル
    /// </summary>
    public Level SendLevel = Level.DEBUG;

#endregion

#region グローバルロガー

    /// <summary>
    ///   グローバルロガー
    /// </summary>
    public static readonly Syslog Logger = new Syslog();

    /// <summary>
    ///   グローバルロガーでsyslogを送る
    /// </summary>
    public static void Log(Level level, string msg, params object[] args) {
        Logger.LogAny(level, msg, args);
    }

    /// <summary>
    ///   グローバルロガーでDEBUGログを送る
    /// </summary>
    public static void Debug(string msg, params object[] args) {
        Logger.LogDebug(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでINFOログを送る
    /// </summary>
    public static void Info(string msg, params object[] args) {
        Logger.LogInfo(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでNOTICEログを送る
    /// </summary>
    public static void Notice(string msg, params object[] args) {
        Logger.LogNotice(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでWARNINGログを送る
    /// </summary>
    public static void Warning(string msg, params object[] args) {
        Logger.LogWarning(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでWARNINGログを送る
    /// </summary>
    public static void Warn(string msg, params object[] args) {
        Logger.LogWarning(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでERRログを送る
    /// </summary>
    public static void Err(string msg, params object[] args) {
        Logger.LogErr(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでERRログを送る
    /// </summary>
    public static void Error(string msg, params object[] args) {
        Logger.LogErr(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでCRITログを送る
    /// </summary>
    public static void Crit(string msg, params object[] args) {
        Logger.LogCrit(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでALERTログを送る
    /// </summary>
    public static void Alert(string msg, params object[] args) {
        Logger.LogAlert(msg, args);
    }

    /// <summary>
    ///   グローバルロガーでEMERGログを送る
    /// </summary>
    public static void Emerg(string msg, params object[] args) {
        Logger.LogEmerg(msg, args);
    }

#endregion

#region ログ送信

    /// <summary>
    ///   ログを送信する
    /// </summary>
    public void LogAny(Level level, string msg, params object[] args) {
        lock(mutex) {
            log(level, msg, args);
        }
    }

    /// <summary>
    ///   DEBUGログを送る
    /// </summary>
    public void LogDebug(string msg, params object[] args) {
        LogAny(Level.DEBUG, msg, args);
    }

    /// <summary>
    ///   INFOログを送る
    /// </summary>
    public void LogInfo(string msg, params object[] args) {
        LogAny(Level.INFO, msg, args);
    }

    /// <summary>
    ///   NOTICEログを送る
    /// </summary>
    public void LogNotice(string msg, params object[] args) {
        LogAny(Level.NOTICE, msg, args);
    }

    /// <summary>
    ///   WARNINGログを送る
    /// </summary>
    public void LogWarning(string msg, params object[] args) {
        LogAny(Level.WARNING, msg, args);
    }

    /// <summary>
    ///   WARNINGログを送る
    /// </summary>
    public void LogWarn(string msg, params object[] args) {
        LogAny(Level.WARNING, msg, args);
    }

    /// <summary>
    ///   ERRログを送る
    /// </summary>
    public void LogErr(string msg, params object[] args) {
        LogAny(Level.ERR, msg, args);
    }

    /// <summary>
    ///   ERRログを送る
    /// </summary>
    public void LogError(string msg, params object[] args) {
        LogAny(Level.ERR, msg, args);
    }

    /// <summary>
    ///   CRITログを送る
    /// </summary>
    public void LogCrit(string msg, params object[] args) {
        LogAny(Level.CRIT, msg, args);
    }

    /// <summary>
    ///   ALERTログを送る
    /// </summary>
    public void LogAlert(string msg, params object[] args) {
        LogAny(Level.ALERT, msg, args);
    }

    /// <summary>
    ///   EMERGログを送る
    /// </summary>
    public void LogEmerg(string msg, params object[] args) {
        LogAny(Level.EMERG, msg, args);
    }

#endregion

#region private部

    private string server;
    private int port;
    private UdpClient udp = null;
    private Encoding enc = new UTF8Encoding(false);
    private object mutex = new object();

    private void setServer(string name) {
        if(String.IsNullOrEmpty(name)) {
            server = "localhost";
            return;
        }
        string[] x = name.Split(":".ToCharArray(),2);
        if(x.Length > 1) {
            server = x[0];
            port = StringUtil.ToInt(x[1]);
        } else {
            server = name;
        }
    }

    private void open() {
        try {
            udp = new UdpClient(server, port);
        } catch(SocketException) {
            // just ignore.
            udp = null;
        }
    }

    private void close() {
        if(udp != null) {
            ((IDisposable)udp).Dispose();
            udp = null;
        }
    }

    private void log(Level level, string msg, params object[] args) {
        if(level > SendLevel)
            return;
        // RFC3164に従ったメッセージを作る
        if(HostName == null)
            HostName = Dns.GetHostName().Split(".".ToCharArray())[0];
        DateTime dt = DateTime.Now;
        string formatted;
        if(String.IsNullOrEmpty(ModuleName))
            formatted = "";
        else
            formatted = ModuleName+" ";
        try {
            formatted += String.Format(msg, args);
        } catch(FormatException) {
            formatted += msg;
        }
        string txt = String.Format("<{0}>{1} {2,2} {3:D2}:{4:D2}:{5:D2} {6} {7}",
                                   (int)Facility+(int)level,
                                   monthName[dt.Month-1], dt.Day, dt.Hour, dt.Minute, dt.Second,
                                   HostName,
                                   formatted);
        byte[] buf = enc.GetBytes(txt);
        if(udp == null)
            open();
        if(udp == null)
            return;
        udp.Send(buf, buf.Length);
    }

    private static readonly string[] monthName = {"Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"};

#endregion

}

} // End of namespace
