/*! @file HttpPage.cs
 * @brief ページレンダラ基底クラス
 * $Id: $
 *
 * Copyright (C) 2008-2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

//#define FORMDEBUG

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Globalization;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   ページレンダラ基底クラス
/// </summary>
public class HttpPage : HtmlTool, IDisposable {

    /// <summary>
    ///   デフォルトコンストラクタ。
    /// </summary>
    public HttpPage() {
        if(m_rand == null)
            m_rand = new Random(DateTime.Now.Millisecond);
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~HttpPage() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     フォームで送信されたファイルを受信するための一時ファイルなどを片付ける。
    ///   </para>
    /// </remarks>
    public void Dispose() {
        m_form = null;
        if(m_fileform != null){
            foreach(KeyValuePair<string,HttpPostedFile> kv in m_fileform){
                if(kv.Value != null)
                    kv.Value.Dispose();
            }
            m_fileform = null;
        }
        /*
        if(m_context != null){
            if(m_context.Request != null){
                if(m_context.Request.InputStream != null){
                    m_context.Request.InputStream.Close();
                }
            }
            if(m_context.Response != null){
                if(m_context.Response.OutputStream != null){
                    m_context.Response.OutputStream.Close();
                }
            }
        }
        */
    }

    /// <summary>
    ///   ページレンダリングタイムアウト（ミリ秒）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     この時間経過してもPageLoadが完了しない場合、InterruptedExceptionが
    ///     発生する。
    ///     0を設定するとタイムアウト処理なしになる。
    ///     負の数を設定すると、HttpServer.DefaultTimeout が用いられる。
    ///   </para>
    /// </remarks>
    public virtual int Timeout {
        get { return -1; }
    }
    
    /// <summary>
    ///   キャッシュ無効用コントロールヘッダをセットする。
    /// </summary>
    public void SetNoCache() {
        Response.AppendHeader("Pragma", "no-cache");
        Response.AddHeader("Cache-Control", "no-cache");
        Response.AddHeader("Expires", "Thu, 01 Dec 1994 16:00:00 GMT");
    }

    /// <summary>
    ///   ページリダイレクトを出力する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     ページリダイレクトのHTTPレスポンスをブラウザに送信後、本メソッドは
    ///     リターンします。
    ///     リターンせずにそのまま例外で処理を打ち切りたい場合は、Transfer()を
    ///     読んでください。
    ///   </para>
    /// </remarks>
    /// <param name="url">リダイレクト先URL</param>
    public void Redirect(string url) {
        LOG_INFO("Redirect to "+url);
        Response.Redirect(url);
        Response.OutputStream.Close();
        m_rendered = true;
    }

    /// <summary>
    ///   ページリダイレクト例外を発生させる
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本メソッドを呼び出すと、HttpServer.PageRedirect例外を発生させて処理
    ///     を打ち切ります。この例外は原則として HttpServerクラスのスレッドで
    ///     キャッチされ、ページリダイレクトのHTTPレスポンスがブラウザに送られ
    ///     ます。
    ///   </para>
    /// </remarks>
    /// <param name="url">リダイレクト先URL</param>
    public void Transfer(string url) {
        Server.Transfer(url);
        // Never comes here.
    }


    /// <summary>
    ///   デフォルトページを出力する。
    /// </summary>
    /// <param name="param">HTTPステータスを示す文字列</param>
    /// <remarks>
    ///   <para>
    ///     HttpPageの派生ページでオーバーライドして下さい。
    ///     オーバーライドせずに本メソッドが呼び出されると、デフォルトのステータス名表示が行なわれます。
    ///
    ///     HttpServerが呼び出すのでpublicになっているが、それ以外のルーチンでは
    ///     呼び出さないで下さい。
    ///
    ///     AddPageで登録されたページハンドラでは、引数paramにはURLのパス部分が渡されます。
    ///   </para>
    /// </remarks>
    public virtual void PageLoad(string param) {
        SetNoCache();
        Render(StringUtil.ToInt(param));
    }


    /// <summary>
    ///   セッション管理をするかどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     デフォルトはセッション管理をします。セッション管理をしたくない場合は
    ///     派生クラスでオーバーライドして falseを返すようにしてください。
    ///     セッション管理されている場合、同一セッションのアクセスはシリアライズ
    ///     されて処理されます。
    ///   </para>
    /// </remarks>
    public virtual bool UseSession {
        get { return true; }
    }

    /// <summary>
    ///   クライアントのIPアドレス
    /// </summary>
    public string ClientAddress {
        get {
            if((Request == null) || (Request.RemoteEndPoint == null) || (Request.RemoteEndPoint.Address == null))
                return "";
            return Request.RemoteEndPoint.Address.ToString();
        }
    }

    /// <summary>
    ///   コンテンツのエンコーディング。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     デフォルトはUTF8。
    ///   </para>
    /// </remarks>
    protected Encoding m_encoding = Encoding.UTF8;

    /// <summary>
    ///   ページ名
    /// </summary>
    protected string m_pagename;

    /// <summary>
    ///   ページタイトル
    /// </summary>
    protected string m_title;

    /// <summary>
    ///   HTTPサーバオブジェクト
    /// </summary>
    protected HttpServer Server {
        get { return m_server;}
    }

    /// <summary>
    ///   HTTPリクエストオブジェクト
    /// </summary>
    protected HttpListenerRequest Request {
        get { return m_context.Request; }
    }

    /// <summary>
    ///   HTTPレスポンスオブジェクト
    /// </summary>
    protected HttpListenerResponse Response {
        get { return m_context.Response; }
    }

    /// <summary>
    ///   レンダー済みかどうか
    /// </summary>
    protected bool IsRendered {
        get { return m_rendered; }
        set { m_rendered = value; }
    }

    /// <summary>
    ///   HTMLページを出力する。
    /// </summary>
    /// <param name="html">HTMLコンテンツとなる文字列。</param>
    /// <remarks>
    ///   <para>
    ///     現在のところ、UTF8以外のエンコーディングを使うとContentTypeの charsetがセットされない。
    ///   </para>
    /// </remarks>
    protected void Render(string html) {
        HttpListenerResponse response = m_context.Response;
        response.ContentEncoding = m_encoding;
        if(m_encoding == Encoding.UTF8)
            response.ContentType = "text/html; charset=utf-8";
        else
            response.ContentType = "text/html";
        byte[] buffer = m_encoding.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
        m_rendered = true;
    }

    /// <summary>
    ///   HTMLページを出力する。ページタイトルとボディー内容だけを指定するバージョン。
    /// </summary>
    /// <param name="title">ページタイトルとなる文字列。</param>
    /// <param name="html">HTMLのBODYタグ内のコンテンツとなる文字列。</param>
    protected void RenderBody(string title, string html) {
        m_title = title;
        if(m_title == null)
            m_title = m_context.Response.StatusDescription;
        StringBuilder res = new StringBuilder("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\n");
        res.Append("<html>\n");
        res.Append("<head>\n");
        res.Append("<title>");
        res.Append(m_title);
        res.Append("</title>\n");
        res.Append("</head>");
        res.Append("<body>\n");
        res.Append(html);
        res.Append("</body>\n");
        res.Append("</html>\n");
        Render(res.ToString());
    }

    /// <summary>
    ///   HTMLページを出力する。ボディー内容だけを指定するバージョン。
    /// </summary>
    /// <param name="html">HTMLのBODYタグ内のコンテンツとなる文字列。</param>
    protected void RenderBody(string html) {
        RenderBody(m_title, html);
    }

    /// <summary>
    ///   HTTPステータスに応じたデフォルトページを出力する。
    /// </summary>
    /// <param name="statuscode">HTTPステータスコード</param>
    protected void Render(int statuscode) {
        HttpListenerRequest req = m_context.Request;
        Response.StatusCode = statuscode;
        string body = null;
        switch(statuscode){
        case 100:
            Response.StatusDescription = "Continue";
            break;
        case 200:
            Response.StatusDescription = "Ok";
            break;
        case 201:
            Response.StatusDescription = "Created";
            break;
        case 202:
            Response.StatusDescription = "Accepted";
            break;
        case 203:
            Response.StatusDescription = "Non-Authoritative Information";
            break;
        case 204:
            Response.StatusDescription = "No Content";
            break;
        case 205:
            Response.StatusDescription = "Reset Content";
            break;
        case 206:
            Response.StatusDescription = "Partial Content";
            break;
        case 300:
            Response.StatusDescription = "Multiple Choices";
            break;
        case 301:
            Response.StatusDescription = "Moved Permanently";
            break;
        case 302:
            Response.StatusDescription = "Moved Temporarily";
            break;
        case 303:
            Response.StatusDescription = "See Other";
            break;
        case 304:
            Response.StatusDescription = "Not Modified";
            break;
        case 305:
            Response.StatusDescription = "Use Proxy";
            break;
        case 400:
            Response.StatusDescription = "Bad Request";
            body = string.Format("Your request ({0} {1}) is invalid.", req.HttpMethod, req.RawUrl);
            break;
        case 401:
            Response.StatusDescription = "Unauthorized";
            body = "Your request is not authorized.";
            break;
        case 403:
            Response.StatusDescription = "Forbidden";
            body = string.Format("Your request ({0} {1}) is forbidden.", req.HttpMethod, req.RawUrl);
            break;
        case 404:
            Response.StatusDescription = "Not Found";
            body = string.Format("Requested page ({0}) does not exist.", req.RawUrl);
            break;
        case 405:
            Response.StatusDescription = "Method Not Allowed";
            body = string.Format("Your request ({0} {1}) is forbidden.", req.HttpMethod, req.RawUrl);
            break;
        case 406:
            Response.StatusDescription = "Not Acceptable";
            body = string.Format("Your request ({0} {1}) is forbidden.", req.HttpMethod, req.RawUrl);
            break;
        case 407:
            Response.StatusDescription = "Proxy Authentication Required";
            break;
        case 408:
            Response.StatusDescription = "Request Time-out";
            body = "Request is timed out.";
            break;
        case 409:
            Response.StatusDescription = "Conflict";
            break;
        case 410:
            Response.StatusDescription = "Gone";
            break;
        case 411:
            Response.StatusDescription = "Length Required";
            break;
        case 412:
            Response.StatusDescription = "Precondition Failed";
            break;
        case 413:
            Response.StatusDescription = "Request Entity Too Large";
            break;
        case 414:
            Response.StatusDescription = "Request-URI Too Large";
            break;
        case 415:
            Response.StatusDescription = "Unsupported Media Type";
            break;
        case 500:
            Response.StatusDescription = "Internal Server Error";
            body = "Sorry, internal server error has been occured.";
            break;
        case 501:
            Response.StatusDescription = "Not Implemented";
            body = string.Format("Sorry, this server does not have implementation for your request ({0} {1}).", req.HttpMethod, req.RawUrl);
            break;
        case 502:
            Response.StatusDescription = "Bad Gateway";
            break;
        case 503:
            Response.StatusDescription = "Server Unavailable";
            body = "Sorry, this server is temporary unavailable.";
            break;
        case 504:
            Response.StatusDescription = "Gateway Time-out";
            break;
        case 505:
            Response.StatusDescription = "HTTP Version not supported";
            break;
        default:
            Response.StatusDescription = "";
            break;
        }
        if(body == null)
            Render("");
        else
            RenderBody(body);
    }

    /// <summary>
    ///   クライアントにファイルを送信する
    /// </summary>
    /// <param name="filename">ファイル名（フルパス）</param>
    /// <param name="mime">MIMEタイプ名(Content-type)</param>
    /// <remarks>
    ///   <para>
    ///     クライアントブラウザにはfilenameのbasenameだけがファイル名として
    ///     通知される。
    ///   </para>
    /// </remarks>
    protected void SendFile(string filename, string mime) {
        SendFile(filename, Path.GetFileName(filename), mime);
    }

    /// <summary>
    ///   クライアントにファイルを送信する
    /// </summary>
    /// <param name="realfilename">ファイル名（フルパス）</param>
    /// <param name="filename">クライアントに通知するファイル名</param>
    /// <param name="mime">MIMEタイプ名(Content-type)</param>
    protected void SendFile(string realfilename, string filename, string mime) {
        if(String.IsNullOrEmpty(realfilename))
            throw new ArgumentNullException("Real file name is not specified.");
        using(FileStream fs = FileUtil.BinaryReader(realfilename)) {
            if(fs == null)
                throw new IOException(String.Format("Can't open {0} for reading", realfilename));
            SendFile(fs, filename, mime);
        }
    }

    /// <summary>
    ///   クライアントにファイルを送信する
    /// </summary>
    /// <param name="fs">送信するファイルのファイルストリーム</param>
    /// <param name="filename">クライアントに通知するファイル名</param>
    /// <param name="mime">MIMEタイプ名(Content-Type)</param>
    protected void SendFile(FileStream fs, string filename, string mime) {
        if(fs == null)
            throw new ArgumentNullException("No file stream to send.");
        if(m_rendered)
            throw new InvalidOperationException("Content is already rendered.");
        HttpListenerResponse response = m_context.Response;
        if(!String.IsNullOrEmpty(mime))
            response.ContentType = mime;
        else
            response.ContentType = "application/octet-stream";
        string disposition = "attachment";
        if(!String.IsNullOrEmpty(filename))
            disposition += "; filename=\""+filename+"\"";
        response.AddHeader("Content-disposition", disposition);
        response.ContentLength64 = fs.Length;
        byte[] buf = new byte[4096];
        int len;
        while((len = fs.Read(buf, 0, buf.Length)) > 0)
            response.OutputStream.Write(buf, 0, len);
        response.OutputStream.Close();
        m_rendered = true;
    }

    /// <summary>
    ///   Requestからnameで指定されるパラメータを獲得する。
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    /// <param name="defaultValue">指定フォーム要素が見つからない時に返す値</param>
    /// <remarks>
    ///   <para>
    ///   まずFormを探し、なければQueryStringを探す。
    ///   指定した名前の値が複数存在する場合は、","で区切ってつなげた文字列を返す。
    ///   </para>
    /// </remarks>
    public string Fetch(string name, string defaultValue=null) {
        if(m_form == null)
            FetchForm();
        string res = m_form.Get(name);
        if(res != null)
            return res;
        if(m_querystring == null)
            FetchQueryString();
        res = m_querystring.Get(name);
        if(res != null)
            return res;
        return defaultValue;
    }

    /// <summary>
    ///   Requestからnameで指定されるパラメータを獲得する。（整数版）
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    /// <param name="defaultValue">指定フォーム要素が見つからない時に返す値</param>
    /// <remarks>
    ///   <para>
    ///   まずFormを探し、なければQueryStringを探す。
    ///   フォーム要素に記入された文字列が数値でない場合はdefaultValueを返す。
    ///   指定した名前の値が複数存在する場合は先頭の要素の値を返す。
    ///   </para>
    /// </remarks>
    public int Fetch(string name, int defaultValue) {
        return StringUtil.ToInt(Fetch(name, defaultValue.ToString()).Split(",".ToCharArray())[0], defaultValue);
    }

    /// <summary>
    ///   Requestからnameで指定されるパラメータを獲得する。（浮動小数点数版）
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    /// <param name="defaultValue">指定フォーム要素が見つからない時に返す値</param>
    /// <remarks>
    ///   <para>
    ///   まずFormを探し、なければQueryStringを探す。
    ///   フォーム要素に記入された文字列が数値でない場合はdefaultValueを返す。
    ///   指定した名前の値が複数存在する場合は先頭の要素の値を返す。
    ///   </para>
    /// </remarks>
    public double Fetch(string name, double defaultValue) {
        return StringUtil.ToDouble(Fetch(name, defaultValue.ToString()).Split(",".ToCharArray())[0], defaultValue);
    }

    /// <summary>
    ///   Requestからnameで指定されるパラメータを獲得する。（bool版）
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    /// <param name="defaultValue">指定フォーム要素が見つからない時に返す値</param>
    /// <remarks>
    ///   <para>
    ///   まずFormを探し、なければQueryStringを探す。
    ///   フォーム要素に記入された文字列が空文字列でない時にtrueを返す。
    ///   このため、input[type="check"]要素の場合、チェックマークが付いていない
    ///   時には falseではなく defaultValue を返すことに注意。（ブラウザはチェッ
    ///   クマークが付いていない要素のデータを送信しないため。）
    ///   指定した名前の値が複数存在する場合は先頭の要素の値を返す。
    ///   </para>
    /// </remarks>
    public bool Fetch(string name, bool defaultValue) {
        string res = Fetch(name, defaultValue?"yes":"").Split(",".ToCharArray())[0];
        return !String.IsNullOrEmpty(res);
    }

    /// <summary>
    ///   Requestからnameで指定されるパラメータを獲得する。（enum版）
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    /// <param name="defaultValue">指定フォーム要素が見つからない時に返す値</param>
    /// <remarks>
    ///   <para>
    ///   まずFormを探し、なければQueryStringを探す。
    ///   </para>
    /// </remarks>
    public T Fetch<T>(string name, T defaultValue)
        where T : struct {
        T res;
            if(Enum.TryParse(Fetch(name,""), out res))
                return res;
            return defaultValue;
    }

    /// <summary>
    ///   Requestからnameで指定されるパラメータを獲得する。（複数値）
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    /// <remarks>
    ///   <para>
    ///   まずFormを探し、なければQueryStringを探す。
    ///   指定した名前の値を文字列の配列で返す。
    ///   指定した名前の値が1つしかない場合も、配列で返す。
    ///   指定した名前の値が全くない場合は、空配列を返す。
    ///   注意: 値の中に　','(カンマ)が含まれていると、カンマで区切られた複数の
    ///   値として処理される。
    ///   </para>
    /// </remarks>
    public string[] FetchMulti(string name) {
        string v = Fetch(name);
        if(v == null)
            return new string[0];
        return v.Split(",".ToCharArray());
    }

    /// <summary>
    ///   Requestからnameで指定されるファイルアップロードパラメータを獲得する。
    /// </summary>
    /// <param name="name">フォーム要素の名前</param>
    public HttpPostedFile FetchFile(string name) {
        if(m_fileform == null)
            FetchForm();
        if(m_fileform.ContainsKey(name))
            return m_fileform[name];
        return null;
    }

    /// <summary>
    ///   セッション変数
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     セッション変数は辞書になっているので、Session["変数名"] ですぐに値を読み書きすることができます。
    ///   </para>
    /// </remarks>
    protected ObjectDictionary Session {
        get {
            if(m_session == null)
                return new ObjectDictionary(); // return dummy.
            return m_session;
        }
    }

    /// <summary>
    ///   アプリケーション変数
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     アプリケーション変数は辞書になっているので、Application["変数名"] ですぐに値を読み書きすることができます。
    ///   </para>
    /// </remarks>
    protected ObjectDictionary Application {
        get {
            if(m_application == null)
                m_application = new ObjectDictionary();
            return m_application;
        }
    }

    /// <summary>
    ///   動作ページ名を獲得する。
    ///   Loggable用。
    /// </summary>
    protected override string GetPageName() {
        return m_pagename;
    }


    /// <summary>
    ///   HttpServer, HttpListenerContextをセットする。
    ///   HttpServer用。
    /// </summary>
    internal void SetServerContext(HttpServer server, HttpListenerContext context, string pagename) {
        m_server = server;
        m_context = context;
        m_pagename = pagename;
        m_rendered = false;
    }

    /// <summary>
    ///   セッション変数、アプリケーション変数をセットする。
    ///   HttpServer用。
    /// </summary>
    internal void SetSession(ObjectDictionary session, ObjectDictionary application) {
        m_session = session;
        m_application = application;
    }



    private HttpServer m_server;
    private HttpListenerContext m_context;
    private bool m_rendered;

    private NameValueCollection m_form;
    private Dictionary<string,HttpPostedFile> m_fileform;
    private NameValueCollection m_querystring;
    private ObjectDictionary m_session;
    private ObjectDictionary m_application;

    private static Random m_rand;

    private const string TemporaryPrefix = "upload-";


    private void FetchForm() {
        // 古い一時ファイルを（念のため）消しておく
        FileUtil.Purge(m_server.TemporaryDirectory, TemporaryPrefix+"*", m_server.TemporaryLifetime);
        HttpListenerRequest req = m_context.Request;
        m_form = new NameValueCollection();
        m_fileform = new Dictionary<string,HttpPostedFile>();
        if((req == null) || (req.ContentType == null))
            return;
        if(req.ContentType.StartsWith("application/x-www-form-urlencoded")){
            using(StreamReader sr = new StreamReader(req.InputStream, req.ContentEncoding)){
                AppendQueryString(m_form, sr.ReadToEnd());
                sr.Close();
            }
            req.InputStream.Close();
        }else if(req.ContentType.StartsWith("multipart/form-data")){
            FetchMultipartForm(req);
            req.InputStream.Close();
        }
    }

    private void FetchQueryString() {
        m_querystring = new NameValueCollection();
        if((Request == null) || (Request.Url == null) || (Request.Url.Query == null))
            return;
        // QUERY_STRING を読み取る。
        string qstr = Request.Url.Query;
        if(qstr.StartsWith("?"))
            AppendQueryString(m_querystring, qstr.Substring(1));
    }

    private static void AppendQueryString(NameValueCollection dict, string qstr) {
        foreach(string str in qstr.Split("&;".ToCharArray())) {
            string[] kv = str.Split("=".ToCharArray(),2);
            if(kv.Length == 1)
                dict.Add(Uri.UnescapeDataString(kv[0].Replace('+',' ')),"");
            else if(kv.Length == 2)
                dict.Add(Uri.UnescapeDataString(kv[0].Replace('+',' ')), Uri.UnescapeDataString(kv[1].Replace('+', ' ')));
        }
    }

    private void FetchMultipartForm(HttpListenerRequest req) {
        #if FORMDEBUG
        Console.WriteLine("Entering FetchMultipartForm");
        #endif
        // まずboundary文字列を獲得する
        string boundary = null;
        foreach(string i in req.ContentType.Split(";".ToCharArray())){
            string[] kv = i.Split("=".ToCharArray(), 2);
            if(kv.Length != 2)
                continue;
            if(kv[0].Trim().ToLower() == "boundary")
                boundary = kv[1].Trim().Trim("\"".ToCharArray());
        }
        if((boundary == null) || (boundary == "")){
            LOG_ERR("No boundary for multipart/form-data");
            return;
        }
        #if FORMDEBUG
        Console.WriteLine("boundary="+boundary);
        #endif
        boundary = "--"+boundary;
        string finalboundary = boundary+"--";

        //Encoding enc = req.ContentEncoding; //クライアントは正しくContentEncodingを渡さない。
        Encoding enc = m_encoding;
        Decoder dec = enc.GetDecoder();
        byte[] buf = new byte[512];
        int buflen = 0;
        bool inhead = false;
        string name = null;
        StringBuilder tmpvalue = null;
        HttpPostedFile tmpfile = null;
        #if FORMDEBUG
        int n = 0;
        #endif
        int ch;
        try {
            // 入力をスキャン
            using(Stream sr = new BufferedStream(req.InputStream)) {
            while((ch = sr.ReadByte()) >= 0){
                #if FORMDEBUG
                if(n >= 16){
                    Console.WriteLine();
                    n = 0;
                }
                Console.Write("[{0:X2}]", ch);
                if((ch >= 0x20) && (ch <= 0xfe))
                    Console.Write(Char.ConvertFromUtf32(ch));
                else
                    Console.Write(' ');
                n++;
                #endif

                // ラインバッファに溜め込む
                buf[buflen++] = (byte)ch;
                if(buflen >= buf.Length){
                    // ラインバッファがいっぱい。
                    if(tmpvalue != null){
                        // 文字列受信中は部分デコードする。
                        char[] tmpchars = new char[buflen];
                        int tmpcharslen = dec.GetChars(buf, 0, buflen, tmpchars, 0, false);
                        tmpvalue.Append(tmpchars, 0, tmpcharslen);
                        #if FORMDEBUG
                        Console.WriteLine();
                        Console.WriteLine("Append {0}chars", tmpcharslen);
                        #endif
                        // TODO: あまりにも巨大なデータを受けた場合は入力スキャンを途中で止める事
                    } else if(tmpfile != null){
                        // ファイル受信中は書き出す。
                        tmpfile.m_stream.Write(buf, 0, buflen);
                        #if FORMDEBUG
                        Console.WriteLine();
                        Console.WriteLine("Write {0}bytes to {1}", buflen, tmpfile.m_innerfilename);
                        #endif
                        // TODO: あまりにも巨大なファイルを受けた場合は入力スキャンを途中で止める事
                    }
                    buflen = 0;
                    continue;
                }

                if(ch == 0x0a){ // ライン終端
                    try {
                        char[] tmpchars = new char[buflen];
                        int tmpcharslen;
                        if((buflen > 1) && (buf[buflen-2] == 0x0d)) // LFの前にCRがあってもなくてもよいように。
                            tmpcharslen = dec.GetChars(buf, 0, buflen-2, tmpchars, 0, true);
                        else
                            tmpcharslen = dec.GetChars(buf, 0, buflen-1, tmpchars, 0, true);
                        dec = enc.GetDecoder(); // デコーダをリセットしておく
                        string line = new string(tmpchars, 0, tmpcharslen);
                        #if FORMDEBUG
                        Console.WriteLine();
                        Console.WriteLine("Line '"+line+"'");
                        n = 0;
                        #endif
                        if(inhead){
                            // パートヘッダ中
                            if(line == ""){
                                #if FORMDEBUG
                                Console.WriteLine("Begin part body");
                                #endif
                                inhead = false;
                            }else if(line.StartsWith("content-disposition:", true, CultureInfo.InvariantCulture)){
                                // 属性値取り出し
                                foreach(string i in line.Split(";".ToCharArray())){
                                    string[] kv = i.Split("=".ToCharArray(), 2);
                                    if(kv.Length != 2)
                                        continue;
                                    string k = kv[0].Trim().ToLower();
                                    switch(k){
                                    case "name":
                                        name = kv[1].Trim().Trim("\"".ToCharArray());
                                        break;
                                    case "filename":
                                        tmpfile = new HttpPostedFile();
                                        tmpfile.m_filename = kv[1].Trim().Trim("\"".ToCharArray());
                                        if(!Directory.Exists(m_server.TemporaryDirectory))
                                            Directory.CreateDirectory(m_server.TemporaryDirectory);
                                        tmpfile.m_innerfilename = Path.Combine(m_server.TemporaryDirectory, TemporaryPrefix+m_rand.Next(0x7fffffff).ToString("X8"));
                                        tmpfile.m_stream = FileUtil.BinaryWriter(tmpfile.m_innerfilename);
                                        if(tmpfile.m_stream == null){
                                            LOG_ERR(string.Format("Can't create '{0}'", tmpfile.m_innerfilename));
                                            tmpfile = null;
                                        }
                                        break;
                                    }
                                }
                                if(tmpfile == null)
                                    tmpvalue = new StringBuilder();
                                #if FORMDEBUG
                                Console.WriteLine("Fetch '{0}' part", name);
                                if(tmpfile != null)
                                    Console.WriteLine("Inner filename={0}", tmpfile.m_innerfilename);
                                #endif
                            }
                            buflen = 0;
                        } else if(tmpvalue != null) {
                            // フォーム要素値獲得中
                            if((line == boundary) || (line == finalboundary)){
                                if(tmpvalue.Length > 0)
                                    tmpvalue.Length--;
                                if(name == null)
                                    name = "";
                                m_form.Add(name, tmpvalue.ToString());
                                #if FORMDEBUG
                                Console.WriteLine("{0}='{1}'", name, tmpvalue.ToString());
                                #endif
                                tmpvalue = null;
                                buflen = 0;
                                inhead = true;
                            }else{
                                tmpvalue.Append(line);
                                tmpvalue.Append('\n');
                                // TODO: あまりにも巨大なデータを受けた場合は入力スキャンを途中で止める事
                            }
                        } else if(tmpfile != null){
                            // ファイル受信中
                            if((line == boundary) || (line == finalboundary)){
                                if(tmpfile.m_stream.Length >= 2)
                                    tmpfile.m_stream.SetLength(tmpfile.m_stream.Length-2);
                                tmpfile.m_stream.Close();
                                tmpfile.m_stream = null;
                                if(name == null)
                                    name = "";
                                m_fileform[name] = tmpfile;
                                #if FORMDEBUG
                                Console.WriteLine("{0}='{1}', {2}bytes", name, tmpfile.FileName, new FileInfo(tmpfile.m_innerfilename).Length);
                                #endif
                                tmpfile = null;
                                buflen = 0;
                                inhead = true;
                            }
                        } else {
                            if((line == boundary) || (line == finalboundary)){
                                #if FORMDEBUG
                                Console.WriteLine("Got boundary");
                                #endif
                                buflen = 0;
                                inhead = true;
                            }
                        }
                    } catch(ArgumentException e){
                        #if FORMDEBUG
                        Console.WriteLine(e.Message);
                        #endif
                        e.GetType();
                        // just ignore
                    }
                    // ファイル受信中は書き出す
                    if((tmpfile != null) && (buflen > 0)){
                        #if FORMDEBUG
                        Console.WriteLine("Write {0}bytes to {1}", buflen, tmpfile.m_innerfilename);
                        #endif
                        tmpfile.m_stream.Write(buf, 0, buflen);
                        // TODO: あまりにも巨大なデータを受けた場合は入力スキャンを途中で止める事
                    }
                    buflen = 0;
                }
            }
            }

            #if FORMDEBUG
            Console.WriteLine();
            #endif
        } finally {
            #if FORMDEBUG
            Console.WriteLine("Leaving FetchMultipartForm");
            #endif
            if(tmpvalue != null)
                tmpvalue = null;
            if(tmpfile != null){
                tmpfile.Dispose();
                tmpfile = null;
            }
        }
    }

} // End of class HttpPage

} // End of namespace
