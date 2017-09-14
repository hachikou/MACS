/*! @file HttpStaticPage.cs
 * @brief 静的ページレンダラ
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Net;
using System.Web;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   静的ページレンダラ
/// </summary>
public class HttpStaticPage : HttpNlsSupport {

    /// <summary>
    ///   ファイルを置くディレクトリを指定したコンストラクタ
    /// </summary>
    /// <param name="path">ディレクトリ名</param>
    public HttpStaticPage(string path) : base() {
        SetFilePath(path);
    }

    /// <summary>
    ///   ファイルを置くディレクトリを指定する。
    /// </summary>
    public void SetFilePath(string dir) {
        m_dir = dir;
    }

    /// <summary>
    ///   ファイルのContent-Typeを指定する。
    ///   ファイルの拡張子に関係なく、このContent-Typeを使う。
    /// </summary>
    public void SetContentType(string contenttype) {
        m_contenttype = contenttype;
    }

    /// <summary>
    ///   ファイルのContent-Typeとファイル拡張子の対応表をデフォルト状態にする。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     次の拡張子のContent-Typeが設定される。
    ///     \code
    ///     ".html" = "text/html"
    ///     ".css" = "text/css"
    ///     ".js" = "application/javascript"
    ///     ".xml" = "text/xml"
    ///     ".bin" = "application/octet-stream"
    ///     ".pdf" = "application/pdf"
    ///     ".zip" = "application/zip"
    ///     ".jpeg" = "image/jpeg"
    ///     ".jpg" = "image/jpeg"
    ///     ".gif" = "image/gif"
    ///     ".tiff" = "image/tiff"
    ///     ".png" = "image/png"
    ///     ".mpeg" = "video/mpeg"
    ///     ".mpg" = "video/mpeg"
    ///     ".mov" = "video/quicktime"
    ///     \endcode
    ///   </para>
    /// </remarks>
    public void SetDefaultContentTypeList() {
        m_ext_type = new Dictionary<string,string>();
        m_ext_type[".html"] = "text/html";
        m_ext_type[".css"] = "text/css";
        m_ext_type[".js"] = "application/javascript";
        m_ext_type[".xml"] = "text/xml";
        m_ext_type[".bin"] = "application/octet-stream";
        m_ext_type[".pdf"] = "application/pdf";
        m_ext_type[".zip"] = "application/zip";
        m_ext_type[".jpeg"] = "image/jpeg";
        m_ext_type[".jpg"] = "image/jpeg";
        m_ext_type[".gif"] = "image/gif";
        m_ext_type[".tiff"] = "image/tiff";
        m_ext_type[".png"] = "image/png";
        m_ext_type[".mpeg"] = "video/mpeg";
        m_ext_type[".mpg"] = "video/mpeg";
        m_ext_type[".mov"] = "video/quicktime";
    }

    /// <summary>
    ///   拡張子とContent-Typeの対応を追加する。
    /// </summary>
    /// <param name="ext">拡張子。一般には'.'（ピリオド）で始まる。</param>
    /// <param name="contenttype">コンテントタイプ文字列。"text/html"など。</param>
    /// <remarks>
    ///   <para>
    ///     SetDefaultContentTypeList()がまだ呼ばれていない時には、呼び出されます。
    ///   </para>
    /// </remarks>
    public void SetContentType(string ext, string contenttype) {
        if(m_ext_type == null)
            SetDefaultContentTypeList();
        m_ext_type[ext.ToLower()] = contenttype;
        m_contenttype = null;
    }

    /// <summary>
    ///   ディレクトリが指定された時のデフォルトファイル名
    /// </summary>
    public string IndexFile = "index.html";

    /// <summary>
    ///   Request URLに指定されたファイルを出力する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本メソッドはHttpServerが呼び出します。
    ///   </para>
    /// </remarks>
    public override void PageLoad(string param) {
        // ファイル名の正当性の確認
        string fname = param.Replace('\\','/');
        if(!fname.StartsWith("/") || fname.Contains("/../")){
            SetNoCache();
            Render(403);
            return;
        }

        // ディレクトリが指定されたかどうか
        if(fname.EndsWith("/") || DirectoryExists(m_dir+fname))
            fname = Path.Combine(fname,IndexFile);

        string ext = Path.GetExtension(fname);
        fname = GetNlsFileName(m_dir+fname);

        if(!FileExists(fname)){
            SetNoCache();
            Render(404);
            return;
        }

        // キャッシュコントロール
        DateTime mtime = GetLastWriteTimeUtc(fname);
        Response.AddHeader("Last-Modified", mtime.ToString("r"));
        string etag = "\""+BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(fname+mtime.ToBinary().ToString())))+"\"";
        Response.AddHeader("ETag", etag);
        if(string.Compare(Request.Headers.Get("Cache-Control"), "no-cache", true, CultureInfo.InvariantCulture) != 0){
            try {
                string reqetag = Request.Headers.Get("If-None-Match");
                if(reqetag == null){
                    // ETagが指定されていない場合はIf-Modified-Sinceで確認する。
                    DateTime reqtime = DateTime.ParseExact(Request.Headers.Get("If-Modified-Since"), "r", null);
                    if(mtime <= reqtime){
                        LOG_DEBUG("Cached page is newer");
                        Render(304);
                        return;
                    }
                }else{
                    if(reqetag == etag){
                        LOG_DEBUG("Cached page is unchanged");
                        Render(304);
                        return;
                    }
                }
            } catch(Exception){
                // Just ignore.
            }
        }

        using(Stream f = OpenFile(fname)){
            if(f == null){
                SetNoCache();
                Render(406);
                return;
            }
            Response.ContentEncoding = m_encoding;
            if(m_contenttype == null){
                if(m_ext_type == null)
                    SetDefaultContentTypeList();
                string ext_ = ext.ToLower();
                if(m_ext_type.ContainsKey(ext_))
                    Response.ContentType = m_ext_type[ext_];
                else
                    Response.ContentType = default_contenttype;
            }else{
                Response.ContentType = m_contenttype;
            }
            Int64 len = f.Length;
            Response.ContentLength64 = len;
            byte[] buffer = new byte[4096];
            while(len > 0){
                int xlen = f.Read(buffer, 0, buffer.Length);
                if(xlen <= 0)
                    throw new IOException(string.Format("Read error while reading '{0}'({1} bytes left)", fname, len));
                Response.OutputStream.Write(buffer, 0, xlen);
                len -= xlen;
            }
            f.Close();
            Response.OutputStream.Close();
        }
    }


    private const string default_contenttype = "text/plain";

    private string m_contenttype;
    private Dictionary<string,string> m_ext_type;

} // End of class HttpStaticPage

} // End of namespace
