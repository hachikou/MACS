/*! @file SendMail.cs
 * @brief メール送信用クラス。
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace MACS {

/// <summary>
///   メール送信用クラス。
/// </summary>
/// <remarks>
///   <para>
///     .NETの SmtpClientがいささか使いづらいので、ラッピングした。
///     POP before SMTPもサポート。
///   </para>
/// </remarks>
public class SendMail : Loggable {

    /// <summary>
    ///   認証タイプ
    /// </summary>
    public enum AuthType {
        NONE, ///< 認証無し
        POP,  ///< POP before SMTP
        SMTP, ///< SMTP Auth
    };

    /// <summary>
    ///   送信者
    /// </summary>
    public MailAddress Sender;

    /// <summary>
    ///   受信者（リスト）
    /// </summary>
    public MailAddressCollection Receivers = new MailAddressCollection();

    /// <summary>
    ///   メールサーバ（ホスト名）
    /// </summary>
    public string Server;

    /// <summary>
    ///   メールサーバのポート番号
    /// </summary>
    public int    Port = 25;

    /// <summary>
    ///   認証方式
    /// </summary>
    public AuthType Auth = AuthType.NONE;

    /// <summary>
    ///   認証ユーザ名
    /// </summary>
    public string AuthUser;

    /// <summary>
    ///   認証パスワード
    /// </summary>
    public string AuthPass;

    /// <summary>
    ///   SSL利用
    /// </summary>
    public bool EnableSsl = false;

    /// <summary>
    ///   証明書を無視するかどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     SSL利用時にサーバの証明書を無視する（検証しない）かどうか。
    ///     EnableSsl = falseの時には意味なし。
    ///   </para>
    /// </remarks>
    public bool IgnoreCert = false;

    /// <summary>
    ///   サブジェクト
    /// </summary>
    public string Subject;

    /// <summary>
    ///   本文
    /// </summary>
    public string Body;

    /// <summary>
    ///   送信タイムアウト
    /// </summary>
    public int    Timeout = 100000;

    /// <summary>
    ///   POPサーバ（ホスト名）
    /// </summary>
    public string PopServer;

    /// <summary>
    ///   POPポート番号
    /// </summary>
    public int    PopPort = 110;

    /// <summary>
    ///   POP認証後待機時間
    /// </summary>
    public int    PopWait = 300;

    /// <summary>
    ///   メール文字エンコーディング
    /// </summary>
    public Encoding MailEncoding = Encoding.GetEncoding("iso-2022-jp");


    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public SendMail() {}

    /// <summary>
    ///   サーバを指定したコンストラクタ
    /// </summary>
    public SendMail(string server) {
        Server = server;
    }

    /// <summary>
    ///   サーバとポートを指定したコンストラクタ
    /// </summary>
    public SendMail(string server, int port) {
        Server = server;
        Port = port;
    }

    /// <summary>
    ///   送信に必要なパラメータがセットされているかどうかを確認する。
    /// </summary>
    public bool IsReady {
        get {
            return (Sender != null) && (!string.IsNullOrEmpty(Sender.Address))
                && (Receivers != null) && (Receivers.Count > 0)
                && (Receivers[0] != null) && (!string.IsNullOrEmpty(Receivers[0].Address))
                && (!string.IsNullOrEmpty(Server)) && (Port != 0);
        }
    }

    /// <summary>
    ///   メールを送信する。Subjectと本文を指定するバージョン。
    /// </summary>
    public bool Send(string subject, string body, bool exceptionFlag=false) {
        Subject = subject;
        Body = body;
        return Send(exceptionFlag);
    }

    /// <summary>
    ///   メールを送信する。
    /// </summary>
    public bool Send(bool exceptionFlag=false) {
        lock(mutex) {
            RemoteCertificateValidationCallback orgCertificationCallback = ServicePointManager.ServerCertificateValidationCallback;
            try {
                using(SmtpClient smtp = new SmtpClient(Server, Port)) {
                    smtp.Timeout = Timeout;
                    string authstring;
                    switch(Auth){
                    case AuthType.NONE:
                        authstring = "No authentification";
                        break;
                    case AuthType.POP:
                        authstring = "POP before SMTP:"+AuthUser;
                        if(!PopBeforeSmtp())
                            return false;
                        Thread.Sleep(PopWait);
                        break;
                    case AuthType.SMTP:
                        authstring = "SMTP-AUTH:"+AuthUser;
                        smtp.Credentials = new NetworkCredential(AuthUser, AuthPass);
                        break;
                    default:
                        authstring = "Unknown";
                        break;
                    }
                    smtp.EnableSsl = EnableSsl;
                    if(smtp.EnableSsl) {
                        authstring += ",STARTTLS";
                        if(IgnoreCert) {
                            ServicePointManager.ServerCertificateValidationCallback = ignoreCertificateValidation;
                        }
                    }
                    LOG_INFO(string.Format("Sending mail from {0} to {1} server={2}:{3} ({4}) timeout={5}msec", Sender.ToString(), Receivers.ToString(), Server, Port, authstring, Timeout));
                    using(MailMessage msg = new MailMessage()){
                        msg.From = Sender;
                        foreach(MailAddress a in Receivers)
                            msg.To.Add(a);
                        if(MailEncoding == Encoding.GetEncoding("iso-2022-jp")) {
                            // .Net4.5の実装異常に対応するため、次のように二重にBエンコーディングをかける
                            msg.Subject = EncodeIso2022jpBase64(EncodeIso2022jpBase64(Subject));
                        } else {
                            msg.Subject = Subject;
                            msg.SubjectEncoding = MailEncoding;
                        }
                        msg.Body =  Body;
                        msg.BodyEncoding = MailEncoding;
                        smtp.Send(msg);
                    }
                }
            } catch(SmtpException e){
                LOG_ERR(string.Format("Failed to send mail ({0})", e.Message));
                if(exceptionFlag)
                    throw e;
                return false;
            } finally {
                ServicePointManager.ServerCertificateValidationCallback = orgCertificationCallback;
            }
        }
        return true;
    }

    /// <summary>
    ///   POPアクセスを試みる。
    /// </summary>
    protected bool PopBeforeSmtp() {
        if(string.IsNullOrEmpty(PopServer)){
            LOG_ERR("POP server is not specified.");
            return false;
        }
        if((PopPort <= 0) || (PopPort >= 65535)){
            LOG_ERR("Invalid POP port number.");
            return false;
        }
        if(string.IsNullOrEmpty(AuthUser)){
            LOG_ERR("POP user is not specified.");
            return false;
        }
        if(string.IsNullOrEmpty(AuthPass)){
            LOG_ERR("POP password is not specified.");
            return false;
        }
        TcpClient client = new TcpClient();
        client.ReceiveTimeout = Timeout;
        client.SendTimeout = Timeout;
        try {
            client.Connect(PopServer, PopPort);
            //LOG_DEBUG("POP connected");
            using(NetworkStream ns = client.GetStream()){
                ns.ReadTimeout = Timeout;
                ns.WriteTimeout = Timeout;
                string msg = Recv(ns);
                //LOG_DEBUG("POP Got "+msg);
                if(!msg.StartsWith("+OK")){
                    LOG_ERR(string.Format("Invalid responce from POP connection ({0})", msg));
                    return false;
                }
                Send(ns, "USER "+AuthUser);
                msg = Recv(ns);
                //LOG_DEBUG("POP Got "+msg);
                if(!msg.StartsWith("+OK")){
                    LOG_ERR(string.Format("Invalid responce from POP USER command ({0})", msg));
                    return false;
                }
                Send(ns, "PASS "+AuthPass);
                msg = Recv(ns);
                //LOG_DEBUG("POP Got "+msg);
                if(!msg.StartsWith("+OK")){
                    LOG_ERR(string.Format("POP Authentification failed ({0})", msg));
                    return false;
                }
                Send(ns, "QUIT");
                ns.Close();
                //LOG_DEBUG("POP Fin.");
            }
        } catch(Exception e){
            LOG_ERR(string.Format("POP session is aborted. ({0})", e.Message));
            return false;
        } finally {
            client.Close();
        }
        return true;
    }

    /// <summary>
    ///   メールサーバに1行送信する。
    /// </summary>
    protected void Send(NetworkStream ns, string msg) {
        byte[] buf = Encoding.ASCII.GetBytes(msg+"\r\n");
        ns.Write(buf, 0, buf.Length);
    }

    /// <summary>
    ///   メールサーバから1行受信する。
    /// </summary>
    protected string Recv(NetworkStream ns) {
        byte[] buf = new byte[256];
        for(int i = 0; i < buf.Length; i++){
            int ch = ns.ReadByte();
            if((ch < 0) || (ch == 0x0a)){
                if((i > 0) && (buf[i-1] == 0x0d))
                    return Encoding.ASCII.GetString(buf, 0, i-1);
                else
                    return Encoding.ASCII.GetString(buf, 0, i);
            }
            buf[i] = (byte)ch;
        }
        return Encoding.ASCII.GetString(buf);
    }

    /// <summary>
    ///   ISO2022JP Base64エンコーディング
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     .NetフレームワークがISO2022JPのエンコーディングをきちんとできない対策
    ///   </para>
    /// </remarks>
    private static string EncodeIso2022jpBase64(string txt) {
        string strBase64 = Convert.ToBase64String(Encoding.GetEncoding("iso-2022-jp").GetBytes(txt));
        return string.Format("=?{0}?B?{1}?=", "iso-2022-jp", strBase64);
    }


    /// <summary>
    ///   メール送信処理を排他するためのmutex
    /// </summary>
    private static object mutex = new object();

    /// <summary>
    ///   証明書のチェックを無視するダミー関数
    /// </summary>
    private static bool ignoreCertificateValidation(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        return true;  // 常に証明書を有効とみなす
    }

}

} // End of namespace
