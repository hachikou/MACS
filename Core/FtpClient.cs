/*! @file FtpClient.cs
 * @brief FTPクライアントクラス
 * $Id: $
 *
 * Copyright (C) 2012-2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Net;

namespace MACS {

/// <summary>
///   FTPクライアント動作のためのクラス。
/// </summary>
/// <remarks>
///   <para>
///     .NetのFtpWebRequestクラスなどがあまりにも使いづらいので、ラッピングクラスを用意した。
///   </para>
/// </remarks>
public class FtpClient : Loggable {

    /// <summary>
    ///   FTPクライアントを作成
    /// </summary>
    public FtpClient(string host_) {
        host = host_;
        user = passwd = null;
        isPassive = false;
    }

    /// <summary>
    ///   ユーザとパスワードを指定する
    /// </summary>
    public void SetUserAndPassword(string user_, string passwd_) {
        user = user_;
        passwd = passwd_;
    }

    /// <summary>
    ///   ユーザ指定
    /// </summary>
    public string User {
        get { return user; }
        set { user = value; }
    }

    /// <summary>
    ///   パスワード指定
    /// </summary>
    public string Password {
        get { return passwd; }
        set { passwd = value; }
    }

    /// <summary>
    ///   PASSIVEモードかどうか
    /// </summary>
    public bool IsPassive {
        get { return isPassive; }
        set { isPassive = value; }
    }

    /// <summary>
    ///   ファイルをダウンロードする
    /// </summary>
    public void Download(string filename, string outputfilename) {
        using(FileStream fs = FileUtil.BinaryWriter(outputfilename)) {
            if(fs == null)
                throw new IOException(String.Format("Can't open {0} for writing", outputfilename));
            Download(filename, fs);
            fs.Close();
        }
    }

    /// <summary>
    ///   ファイルをダウンロードする
    /// </summary>
    public void Download(string filename, FileStream fs) {
        Uri u = new Uri("ftp://"+host+"/"+filename);
        //Console.WriteLine("URI={0}", u.ToString());
        FtpWebRequest req = (FtpWebRequest)WebRequest.Create(u);
        if((user != null) && (passwd != null))
            req.Credentials = new NetworkCredential(user, passwd);
        req.Method = WebRequestMethods.Ftp.DownloadFile;
        req.KeepAlive = false;
        req.UseBinary = true;
        req.UsePassive = isPassive;
        //Console.WriteLine("req ok");
        using(FtpWebResponse res = (FtpWebResponse)(req.GetResponse())) {
            //Console.WriteLine("res ok");
            using(Stream sr = res.GetResponseStream()) {
                //Console.WriteLine("stream ok");
                byte[] buf = new byte[1024];
                while(true) {
                    int len = sr.Read(buf, 0, buf.Length);
                    if(len == 0)
                        break;
                    fs.Write(buf, 0, len);
                }
                sr.Close();
                //Console.WriteLine("stream closed");
            }
            res.Close();
            //Console.WriteLine("res closed");
        }
    }

    /// <summary>
    ///   ファイルをアップロードする
    /// </summary>
    public void Upload(string filename, string outputfilename) {
        using(FileStream fs = FileUtil.BinaryReader(filename)) {
            if(fs == null)
                throw new IOException(String.Format("Can't open {0} for reading", filename));
            Upload(fs, outputfilename);
            fs.Close();
        }
    }

    /// <summary>
    ///   ファイルをアップロードする
    /// </summary>
    public bool Upload(Stream fs, string outputfilename) {
        Uri u = new Uri("ftp://"+host+"/"+outputfilename);
        //Console.WriteLine("URI={0}", u.ToString());
        FtpWebRequest req = (FtpWebRequest)WebRequest.Create(u);
        if((user != null) && (passwd != null))
            req.Credentials = new NetworkCredential(user, passwd);
        req.Method = WebRequestMethods.Ftp.UploadFile;
        req.KeepAlive = false;
        req.UseBinary = true;
        req.UsePassive = isPassive;
        //Console.WriteLine("req ok");
        using(Stream reqStream = req.GetRequestStream()) {
            byte[] buf = new byte[4096];
            while(true) {
                int len = fs.Read(buf, 0, buf.Length);
                if(len == 0)
                    break;
                reqStream.Write(buf, 0, len);
            }
            reqStream.Close();
        }

        using(FtpWebResponse res = (FtpWebResponse)(req.GetResponse())) {
            if(res.StatusCode != FtpStatusCode.CommandOK)
                return false;
            res.Close();
            //Console.WriteLine("res closed");
        }
        return true;
    }


    private string host;
    private string user;
    private string passwd;
    private bool isPassive;


#if SELFTEST
    public static void Main(string[] args) {
        if(args[0] == "get") {
            FtpClient ftp = new FtpClient(args[1]);
            ftp.User = args[2];
            ftp.Password = args[3];
            //ftp.IsPassive = true;
            string localname = Path.GetFileName(args[4]);
            ftp.Download(args[4], localname);
        } else {
            Console.WriteLine("Command must be 'get' or 'put'.");
        }
    }
#endif

}

} // End of namespace
