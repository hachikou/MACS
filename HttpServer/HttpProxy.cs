/*! @file HttpProxy.cs
 * @brief HTTPプロキシ機構
 * $Id: $
 *
 * Copyright (C) 2017 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Net;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   HTTPプロキシ機構
/// </summary>
public class HttpProxy : Loggable {

    /// <summary>
    ///   リモートサーバ名
    /// </summary>
    public string ServerName;

    /// <summary>
    ///   リモートポート番号
    /// </summary>
    public int PortNo;

    /// <summary>
    ///   リモートURLプレフィックス
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     URLの先頭のSourceUrlPrefixがRemoteUrlPrefixに変換されます。
    ///   </para>
    /// </remarks>
    public string RemoteUrlPrefix {
        get { return remoteUrlPrefix; }
        set { remoteUrlPrefix = cleanUrlPrefix(value); }
    }

    /// <summary>
    ///   ソースURLプレフィックス
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     URLの先頭のSourceUrlPrefixがRemoteUrlPrefixに変換されます。
    ///   </para>
    /// </remarks>
    public string SourceUrlPrefix {
        get { return sourceUrlPrefix; }
        set { sourceUrlPrefix = cleanUrlPrefix(value); }
    }

    /// <summary>
    ///   接続待ち時間（ミリ秒）
    /// </summary>
    public int Timeout = 10000;

    /// <summary>
    ///   詳細デバッグメッセージを出すかどうか
    /// </summary>
    public bool FullDebug = false;


    /// <summary>
    ///   HTTPプロキシオブジェクトを作る。
    /// </summary>
    /// <param name="servername">リモートサーバ名</param>
    /// <param name="port">リモートポート番号</param>
    public HttpProxy(string servername, int port=80) {
        ServerName = servername;
        PortNo = port;
    }

    /// <summary>
    ///   HTTPのプロキシを行なう
    /// </summary>
    public bool Proxy(HttpListenerRequest request, HttpListenerResponse response, string url=null) {
        Uri orgUrl = request.Url;
        if(url == null) {
            url = orgUrl.PathAndQuery;
        }
        string remoteUrl = String.Format("http://{0}:{1}{2}", ServerName, PortNo, rewriteUrl(url, sourceUrlPrefix, remoteUrlPrefix));
        LOG_DEBUG("Remote URL={0}", remoteUrl);
        try{
            HttpWebRequest remoteRequest = WebRequest.CreateHttp(remoteUrl);
            foreach(string key in request.Headers.Keys) {
                try {
                    remoteRequest.Headers[key] = request.Headers[key];
                    DETAIL("Set request header {0}={1}", key, request.Headers[key]);
                } catch(ArgumentException) {
                    DETAIL("Failed to set request header {0}={1}", key, request.Headers[key]);
                }
            }
            remoteRequest.Timeout = Timeout;
            remoteRequest.AllowAutoRedirect = false;
            if(FullDebug) {
                foreach(Cookie cookie in request.Cookies) {
                    DETAIL("Request cookie '{0}'", cookie.ToString());
                }
            }
            remoteRequest.Method = request.HttpMethod.ToUpper();
            if(FullDebug) {
                foreach(string key in remoteRequest.Headers.Keys) {
                    DETAIL("Request header: {0}={1}", key, remoteRequest.Headers[key]);
                }
            }
            if(request.UrlReferrer != null && !String.IsNullOrEmpty(request.UrlReferrer.AbsoluteUri))
                remoteRequest.Referer = request.UrlReferrer.AbsoluteUri;
            remoteRequest.UserAgent = request.UserAgent;
            
            byte[] buf = new byte[1024*4];
            int len, totallen;

            // POSTデータを転送する
            if(remoteRequest.Method == "POST") {
                //remoteRequest.ContentLength = request.ContentLength64;
                remoteRequest.ContentType = request.ContentType;
                using(Stream remoteStreamIn = remoteRequest.GetRequestStream()) {
                    totallen = 0;
                    while((len = request.InputStream.Read(buf, 0, buf.Length)) > 0) {
                        remoteStreamIn.Write(buf, 0, len);
                        totallen += len;
                    }
                    remoteStreamIn.Close();
                    DETAIL("Written {0}bytes to request stream", totallen);
                }
            }

            // 応答を受信してクライアントに返す
            using(HttpWebResponse remoteResponse = (HttpWebResponse)remoteRequest.GetResponse()) {
                string[] setCookieList = null;
                string[] delimiter = {","};
                response.Cookies = remoteResponse.Cookies;
                foreach(string key in remoteResponse.Headers.Keys) {
                    try {
                        if (key == "Set-Cookie") {
                            setCookieList = remoteResponse.Headers[key].Replace(", ","[HTTPPROXY_COMMA_SPACE]").Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                        } else {
                            response.Headers[key] = remoteResponse.Headers[key];
                        }
                        DETAIL("Set response header {0}={1}", key, remoteResponse.Headers[key]);
                    } catch(ArgumentException) {
                        DETAIL("Failed to set response header {0}={1}", key, remoteResponse.Headers[key]);
                    }
                }
                if (setCookieList != null) {
                    foreach(string cookie in setCookieList) {
                        response.AppendHeader("Set-Cookie", cookie.Replace("[HTTPPROXY_COMMA_SPACE]", ", "));
                    }
                }
                response.ProtocolVersion = remoteResponse.ProtocolVersion;
                response.StatusCode = (int)remoteResponse.StatusCode;
                if(isRedirect(response.StatusCode)) {
                    string location = remoteResponse.Headers.Get("Location");
                    if(location == null) {
                        LOG_NOTICE("{0} returned Http-Status#{1}, but no Location", remoteUrl, response.StatusCode);
                    } else {
                        LOG_DEBUG("Redirect to {0}", location);
                        string redirectPath;
                        try {
                            Uri redirectUrl = new Uri(location);
                            redirectPath = redirectUrl.PathAndQuery;
                        } catch(UriFormatException) {
                            redirectPath = location;
                        }
                        response.RedirectLocation = rewriteUrl(redirectPath, remoteUrlPrefix, sourceUrlPrefix);
                    }
                }
                response.StatusDescription = remoteResponse.StatusDescription;
                response.KeepAlive = false;
                if(FullDebug) {
                    foreach(string key in response.Headers.Keys) {
                        DETAIL("Response header: {0}={1}", key, response.Headers[key]);
                    }
                }
                Stream remoteStream = remoteResponse.GetResponseStream();
                buf = new byte[1024*4];
                totallen = 0;
                while((len = remoteStream.Read(buf, 0, buf.Length)) > 0) {
                    response.OutputStream.Write(buf, 0, len);
                    totallen += len;
                }
                DETAIL("Written {0}bytes to response stream", totallen);
            }
        } catch(ProtocolViolationException ex) {
            LOG_ERR("Proxy error: {0}", ex.Message);
            response.StatusCode = 500; // Internal Server Error
            response.StatusDescription = ex.Message;
            return false;
        } catch(WebException ex) {
            LOG_NOTICE("WebException: {0}: {1}", ex.Status.ToString(), ex.Message);
            if(ex.Status == WebExceptionStatus.ProtocolError) {
                response.StatusCode = (int)((HttpWebResponse)ex.Response).StatusCode;
                response.StatusDescription = ((HttpWebResponse)ex.Response).StatusDescription;
            } else {
                response.StatusCode = 500; // Internal Server Error
                response.StatusDescription = ex.Message;
            }
            return true;
        }
        return true;
    }

    /// <summary>
    ///   WEBサーバに接続できるかどうか確認する
    /// </summary>
    public bool CheckConnection() {
        bool ok = false;
        string remoteUrl = String.Format("http://{0}:{1}{2}", ServerName, PortNo, remoteUrlPrefix);
        DETAIL("Check access to {0}", remoteUrl);
        try {
            HttpWebRequest remoteRequest = WebRequest.CreateHttp(remoteUrl);
            remoteRequest.KeepAlive = false;
            remoteRequest.Method = "HEAD";
            remoteRequest.Timeout = Timeout;
            using(HttpWebResponse remoteResponse = (HttpWebResponse)remoteRequest.GetResponse()) {
                int status = (int)remoteResponse.StatusCode;
                DETAIL("Got {0} {1}", status, remoteResponse.StatusDescription);
                if((status >= 100) && (status < 500))
                    ok = true; // とりあえずサーバエラー以外のなんか応答があればOK
            }
        } catch(ProtocolViolationException ex) {
            DETAIL("Connection error: {0}", ex.Message);
            ok = false;
        } catch(WebException ex) {
            DETAIL("WebException: {0}: {1}", ex.Status.ToString(), ex.Message);
            ok = false;
        } catch(Exception ex) {
            LOG_ERR("Fatal error while checking connection: {0}: {1}", ex.GetType().Name, ex.Message);
            ok = false;
        }
        return ok;
    }

    private string remoteUrlPrefix = "/";
    private string sourceUrlPrefix = "/";

    private static string cleanUrlPrefix(string urlPrefix) {
        if(String.IsNullOrEmpty(urlPrefix))
            return "/";
        if(!urlPrefix.StartsWith("/"))
            urlPrefix = "/"+urlPrefix;
        if(!urlPrefix.EndsWith("/"))
            urlPrefix = urlPrefix+"/";
        return urlPrefix;
    }

    private static string rewriteUrl(string url, string srcPrefix, string dstPrefix) {
        if(url == null)
            return null;
        if(url.StartsWith(srcPrefix))
            url = dstPrefix+url.Substring(srcPrefix.Length);
        return url;
    }

    private static bool isRedirect(int code) {
        switch(code){
        case 301: // Moved Permanently
        case 302: // Found
        case 303: // See Other
        case 307: // Temporary Redirect
        case 308: // Permanent Redirect
            return true;
        default:
            return false;
        }
    }

    private void DETAIL(string msg, params object[] args) {
        if(!FullDebug)
            return;
        LOG_DEBUG(msg, args);
    }

}


} // End of namespace
