/*! @file HttpServer.cs
 * @brief HTTPサーバ
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   HTTPサーバ
/// </summary>
public class HttpServer : Loggable {

    /// <summary>
    ///   サーバ名
    /// </summary>
    public string ServerName {
        get { return m_servername; }
    }

    /// <summary>
    ///   ポート番号
    /// </summary>
    public int PortNo {
        get { return m_port; }
    }

    /// <summary>
    ///   URL
    /// </summary>
    public string URL {
        get { return m_url; }
    }

    /// <summary>
    ///   一時ファイルを格納するディレクトリ
    /// </summary>
    public string TemporaryDirectory = "tmp";

    /// <summary>
    ///   一時ファイルの最大保存時間（秒）
    /// </summary>
    public int TemporaryLifetime = 60*60; // sec.

    /// <summary>
    ///   セッション変数の保持時間（秒）
    /// </summary>
    public int SessionTimeout = 60*60*24; // sec.

    /// <summary>
    ///   HTTPクッキーにexpiresをつけるかどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     HTTPサーバの時計が狂っていてクッキーの有効期限が正しく処理されない
    ///     場合にはfalseにするとよい。
    ///   </para>
    /// </remarks>
    public bool UseCookieExpires = true;

    /// <summary>
    ///   ワーカースレッドに空きが無い場合の、ワーカースレッドの動作終了待ち最大時間（ミリ秒）
    /// </summary>
    public int WorkerThreadWaitTime = 10000; // msec.

    /// <summary>
    ///   ワーカースレッドの応答に時間がかかっている場合時デッドロックと判断してスレッドを終了するまでの時間
    /// </summary>
    public int WorkerTimeout = 0; // sec

    /// <summary>
    ///   セッション変数を保存するファイル名
    /// </summary>
    public string SessionPath = "conf/session.save";

    /// <summary>
    ///   HTTPサーバオブジェクトを作る。
    /// </summary>
    /// <param name="servername">クッキーなどに使われるサーバーのID名。</param>
    /// <param name="domain">Listenするインタフェースを特定するためのドメイン名またはIPアドレス。特定しない場合は、"*"を指定する。</param>
    /// <param name="port">Listenするポート番号。</param>
    public HttpServer(string servername, string domain, int port) {
        m_servername = servername;
        m_port = port;
        m_listener = new HttpListener();
        m_url = string.Format("http://{0}:{1}/", domain, port);
        m_listener.Prefixes.Add(m_url);
        m_pagelist = new Dictionary<string,Type>();
        m_staticpagelist = new Dictionary<string,HttpStaticPage>();
        m_sessiondict = new Dictionary<string,ObjectDictionary>();
        m_sessiontime = new Dictionary<string,DateTime>();
        m_appl = new ObjectDictionary();
        m_allownetaddr = new List<Ipaddr>();
        m_allownetmask = new List<Ipaddr>();
        if(m_rand == null)
            m_rand = new Random(DateTime.Now.Millisecond);
        m_pagecount = 0;
        m_listening = false;
    }

    /// <summary>
    ///   セッション変数、アプリケーション変数をファイルから取り込む。
    /// </summary>
    public void LoadSession() {
        if(SessionPath != null)
            LoadSession(SessionPath);
    }

    /// <summary>
    ///   セッション変数、アプリケーション変数をファイルから取り込む。
    ///   ファイル名を指定するバージョン。
    /// </summary>
    public void LoadSession(string path){
        if(!File.Exists(path))
            return;
        // Not yet implemented.
        m_appl = new ObjectDictionary();
        m_sessiondict = new Dictionary<string,ObjectDictionary>();
        m_sessiontime = new Dictionary<string,DateTime>();
    }

    /// <summary>
    ///   セッション変数、アプリケーション変数をファイルに書き出す
    /// </summary>
    public void SaveSession() {
        if(SessionPath != null)
            SaveSession(SessionPath);
    }
    /// <summary>
    ///   セッション変数、アプリケーション変数をファイルに書き出す
    ///   ファイル名を指定するバージョン。
    /// </summary>
    public void SaveSession(string path) {
        using(StreamWriter sw = FileUtil.Writer(path, Encoding.UTF8)){
            if(m_appl != null)
                ObjectDictionary.Save(sw, "m_appl", m_appl);
            if(m_sessiondict != null)
                ObjectDictionary.Save(sw, "m_sessionlist", m_sessiondict);
            sw.Close();
        }
    }

    /// <summary>
    ///   古いセッション変数を削除する
    /// </summary>
    public void PurgeSession() {
        if((m_sessiondict == null) || (m_sessiontime == null) || (SessionTimeout <= 0))
            return;
        lock(m_sessiondict){
            List<string> expired = new List<string>();
            DateTime dt = DateTime.Now.AddSeconds(-SessionTimeout);
            foreach(KeyValuePair<string,DateTime> kv in m_sessiontime) {
                if(kv.Value < dt)
                    expired.Add(kv.Key);
            }
            foreach(string key in expired){
                m_sessiondict.Remove(key);
                m_sessiontime.Remove(key);
            }
        }
    }

    /// <summary>
    ///   HTTPサーバが動いているかどうか
    /// </summary>
    public bool IsRunning {
        get { return (m_workerlist != null); }
    }

    /// <summary>
    ///   HTTPサーバが接続待ち状態かどうか
    /// </summary>
    public bool IsListening {
        get { return m_listening; }
    }

    /// <summary>
    ///   デフォルトページ
    /// </summary>
    public string DefaultPage {
        get { return m_defaultpage; }
        set { m_defaultpage = value; }
    }


    /// <summary>
    ///   ワーカが例外を発した時にスタックトレースページを表示するかどうか
    /// </summary>
    public bool ShowStackTrace = true;

    /// <summary>
    ///   URLに対応するページレンダラを登録する。
    /// </summary>
    /// <param name="path">URL内のパス名。/から始まる。</param>
    /// <param name="pageClass">pathにアクセスした時にこのクラスのPageLoadメソッドが呼び出される。
    ///   このクラスはHttpPageのサブクラスでなければいけない。</param>
    /// <example>
    ///   \code
    ///   HttpServer sv = new HttpServer("MyServerName", "*", "80");
    ///   sv.AddPage("/hoge", typeof(HogePage));
    ///   ...
    ///   class HogePage : HttpPage {
    ///       public override void PageLoad(string param) {
    ///           ...
    ///       }
    ///   }
    ///   \endcode
    ///   これで、"http://MyServerName/hoge/abcd.html"にアクセスするとHogePageクラスのPageLoadメソッドが呼び出される。
    ///   PageLoadの引数paramには "/abcd.html" がセットされる。
    /// </example>
    public void AddPage(string path, Type pageClass) {
        if(!pageClass.Equals(typeof(HttpPage)) && ! pageClass.IsSubclassOf(typeof(HttpPage)))
            throw new InvalidCastException("Page class must be a subclass of HttpPage.");
        m_pagelist[path] = pageClass;
    }

    /// <summary>
    ///   静的ページのハンドラを登録する。
    /// </summary>
    /// <param name="path">URL内のパス名（ディレクトリ名）。/から始まる。</param>
    /// <param name="dir">静的ページが入っているディレクトリ名。</param>
    /// <example>
    ///   \code
    ///   HttpServer sv = new HttpServer("MyServerName", "*", "80");
    ///   sv.AddStaticPage("/hoge", "foo");
    ///   \endcode
    ///   これで、"http://MyServerName/hoge/abcd.html"にアクセスすると"foo/abcd.html"が返されるようになる。
    /// </example>
    public void AddStaticPage(string path, string dir) {
        m_staticpagelist[path] = new HttpStaticPage(dir);
    }

    /// <summary>
    ///   静的ページのハンドラを登録する。
    /// </summary>
    /// <param name="path">URL内のパス名（ディレクトリ名）。/から始まる。</param>
    /// <param name="page">静的ページを返すハンドラ。</param>
    /// <example>
    ///   \code
    ///   HttpServer sv = new HttpServer("MyServerName", "*", "80");
    ///   sv.AddStaticPage("/hoge", new HttpStaticPage("foo"));
    ///   \endcode
    ///   これで、"http://MyServerName/hoge/abcd.html"にアクセスすると"foo/abcd.html"が返されるようになる。
    /// </example>
    public void AddStaticPage(string path, HttpStaticPage page) {
        m_staticpagelist[path] = page;
    }

    /// <summary>
    ///   接続許可アドレス一覧をクリアする（どこからでも接続できるようにする）
    /// </summary>
    public void ClearAllowAddress() {
        lock(m_allownetaddr) {
            m_allownetaddr.Clear();
            m_allownetmask.Clear();
        }
    }

    /// <summary>
    ///   接続許可ネットワークアドレス/ネットマスクを追加登録する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     現在のところ、IPv4しか対応していない。1つでも接続許可ネットワーク
    ///     アドレスを登録すると、IPv4以外のプロトコルでのアクセスは接続拒否
    ///     するようになる。
    ///
    ///     Loopbackアドレスからのアクセスは常に許可される。
    ///   </para>
    /// </remarks>
    public void AddAllowAddress(Ipaddr netaddr, Ipaddr netmask) {
        lock(m_allownetaddr) {
            m_allownetaddr.Add(netaddr);
            m_allownetmask.Add(netmask);
        }
    }

    /// <summary>
    ///   自分と同じセグメントからのアクセスを接続許可にする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Monoでは自分のIPアドレスの獲得ができないため、本呼び出しは何もしない。
    ///   </para>
    /// </remarks>
    public void AllowLocalSegment() {
        /*
        for(int i = 0; i < IpaddrUtil.MyIpaddr.Length; i++) {
            AddAllowAddress(Ipaddr.GetMasked(IpaddrUtil.MyIpaddr[i], IpaddrUtil.MyMask[i]), IpaddrUtil.MyMask[i]);
        }
        */
    }


    /// <summary>
    ///   HTTPサーバを動かす。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Stop()が（他のスレッドで）呼び出されるまで、本メソッドは戻らない。
    ///   </para>
    /// </remarks>
    /// <param name="nworker">同時接続を許可するHTTPセッション数</param>
    public void Run(int nworker) {
        if(IsRunning){
            LOG_CRIT("Already running.");
            return;
        }
        m_stoprequest = false;
        if(nworker <= 0)
            nworker = 1;

        m_workerlist = new WorkerContext[nworker];
        for(int i = 0; i < m_workerlist.Length; i++){
            m_workerlist[i] = new WorkerContext();
            initContext(m_workerlist[i]);
        }

        try {
            m_listener.Start();
        } catch(Exception e) {

            // Windows 7 でデバッグ実行しているときにここに陥る場合、
            // 管理者権限で 'netsh http add urlacl url=http://+:10080/ user=ユーザー名' を実行してください。

            LOG_CRIT(string.Format("Failed to listen on {0}: {1}", m_url, e.Message));
            for(int i = 0; i < m_workerlist.Length; i++)
                m_workerlist[i].WorkerThread.Abort();
            m_workerlist = null;
            throw e;
        }

        m_deadlockmonitthread = new Thread(monitorWorkerTimeout);
        m_deadlockmonitthread.Start();

        LOG_INFO(string.Format("Accepting {0}, {1} workers.", m_url, nworker));

        while (!m_stoprequest) {
            // コネクトを待つ。
            HttpListenerContext context;
            try{
                m_listening = true;
                context = m_listener.GetContext();
            } catch(Exception e) {
                // Stop()メソッドによってm_listenerがStopさせられたのならOK
                if(m_stoprequest)
                    break;
                throw e;
            } finally {
                m_listening = false;
            }
            // 接続元を確認する
            lock(m_allownetaddr) {
                if(m_allownetaddr.Count > 0) {
                    IPAddress remote = context.Request.RemoteEndPoint.Address;
                    if(!checkRemoteAddress(remote)) {
                        context.Response.Abort();
                        LOG_NOTICE("Reject accress from "+remote.ToString());
                        continue;
                    }
                }
            }
            // 空いているワーカースレッドを探す
            int widx = -1;
            int retry = WorkerThreadWaitTime/10;
            while((widx < 0) && (retry > 0)){
                for(int i = 0; i < m_workerlist.Length; i++){
                    if(m_workerlist[i].Status == WorkerStatus.WAITING){
                        widx = i;
                        break;
                    }
                }
                if(widx < 0)
                    Thread.Sleep(10);
                retry--;
            }
            if(widx < 0){ // All workers are too busy.
                context.Response.Abort();
                LOG_ERR("All workers are busy during "+WorkerThreadWaitTime.ToString()+"msec.");
                continue;
            }
            // ワーカースレッドにcontextを渡して、処理させる。
            m_workerlist[widx].Status = WorkerStatus.CONNECTED;
            m_workerlist[widx].Context = context;
            m_workerlist[widx].WorkerEvent.Set();
            m_pagecount++;
        }
        m_listener.Stop();

        m_deadlockmonitthread.Join();
        for(int i = 0; i < m_workerlist.Length; i++)
            m_workerlist[i].WorkerEvent.Set();
        for(int i = 0; i < m_workerlist.Length; i++)
            m_workerlist[i].WorkerThread.Join();
        m_workerlist = null;
        m_stoprequest = false;

        LOG_INFO(string.Format("Finished accepting {0}, {1} workers.", m_url, nworker));
    }

    /// <summary>
    ///   既に動いているHTTPサーバを停止する。
    /// </summary>
    public void Stop() {
        m_stoprequest = true;
        if(m_listener != null){
            try {
                m_listener.Stop();
            } catch(Exception){
                // just ignore.
            }
        }
        while(IsRunning){
            Thread.Sleep(50);
        }
    }

    /// <summary>
    ///   サーバ稼働状況データ
    /// </summary>
    public class Statistics {
        /// <summary>
        ///   総ページ数
        /// </summary>
        public long PageCount = 0;

        /// <summary>
        ///   総レンダリング時間(msec)
        /// </summary>
        public long TotalTime = 0;

        /// <summary>
        ///   最大レンダリング時間(msec)
        /// </summary>
        public long MaxTime = 0;

        /// <summary>
        ///   平均レンダリング時間(msec)
        /// </summary>
        public long MeanTime {
            get { return (PageCount>0)?(TotalTime/PageCount):0; }
        }

        /// <summary>
        ///   総ワーカ数
        /// </summary>
        public int Workers = 0;

        /// <summary>
        ///   待機中ワーカ数
        /// </summary>
        public int Waiting = 0;

        /// <summary>
        ///   接続中ワーカ数
        /// </summary>
        public int Connected = 0;

        /// <summary>
        ///   処理中ワーカ数
        /// </summary>
        public int Busy = 0;

        /// <summary>
        ///   停止中ワーカ数
        /// </summary>
        public int Stopped = 0;
    }

    /// <summary>
    ///   サーバ稼働状況獲得
    /// </summary>
    public Statistics GetStatistics() {
        Statistics stat = new Statistics();
        if(m_workerlist == null)
            return stat;

        lock(m_workerlist) {
            stat.Workers = m_workerlist.Length;
            for(int i = 0; i < m_workerlist.Length; i++){
                if(m_workerlist[i] == null){
                    stat.Stopped++;
                    continue;
                }
                stat.PageCount += m_workerlist[i].Count;
                stat.TotalTime += m_workerlist[i].TotalElapsed;
                if(stat.MaxTime < m_workerlist[i].MaxElapsed)
                    stat.MaxTime = m_workerlist[i].MaxElapsed;
                switch(m_workerlist[i].Status){
                case WorkerStatus.WAITING:
                    stat.Waiting++;
                    break;
                case WorkerStatus.CONNECTED:
                    stat.Connected++;
                    break;
                case WorkerStatus.BUSY:
                    stat.Busy++;
                    break;
                case WorkerStatus.STOPPED:
                    stat.Stopped++;
                    break;
                }
            }
        }
        return stat;
    }

    /// <summary>
    ///   サーバ稼働状況をログに出力する
    /// </summary>
    public void DebugLog() {
        foreach(string msg in GetDebugMessage())
            LOG_DEBUG(msg);
    }

    /// <summary>
    ///   サーバ稼働状況メッセージを得る
    /// </summary>
    public string[] GetDebugMessage() {
        Statistics stat = GetStatistics();
        return new string[]{
            string.Format("Workers={0}; {1} waiting, {2} connected, {3} busy, {4} stopped", stat.Workers, stat.Waiting, stat.Connected, stat.Busy, stat.Stopped),
            string.Format("Total {0} pages; mean {1}msec, max {2}msec", stat.PageCount, stat.MeanTime, stat.MaxTime)
        };
    }


    /// <summary>
    ///   他ページにリダイレクトする。
    /// </summary>
    /// <param name="pagename">リダイレクト先のURL</param>
    /// <remarks>
    ///   <para>
    ///     ページレンダラ用。本関数の呼び出しは戻らない。
    ///   </para>
    /// </remarks>
    public void Transfer(string pagename){
        throw new PageRedirectException(pagename);
    }


    /// <summary>
    ///   他ページにリダイレクトする際に用いる例外
    /// </summary>
    public class PageRedirectException : Exception {
        public string m_pagename;
        public PageRedirectException(string pagename){
            m_pagename = pagename;
        }
    }

    private enum WorkerStatus {
        WAITING, CONNECTED, BUSY, STOPPED
    }

    /// <summary>
    ///   ワーカー管理データ
    /// </summary>
    private class WorkerContext {
        public HttpServer   Server;
        public WorkerStatus Status;
        public Thread       WorkerThread;
        public HttpListenerContext Context;
        public AutoResetEvent WorkerEvent;
        public long         Count;
        public long         TotalElapsed; // Milliseconds
        public long         MaxElapsed; // Milliseconds
        public Stopwatch    Timer;
    }

    private Thread m_deadlockmonitthread;
    private string m_servername;
    private int m_port;
    private string m_url;
    private HttpListener m_listener;
    private volatile WorkerContext[] m_workerlist;
    private int m_pagecount;
    private string m_defaultpage;
    private Dictionary<string,Type> m_pagelist;
    private Dictionary<string,HttpStaticPage> m_staticpagelist;
    private ObjectDictionary m_appl;
    private Dictionary<string,ObjectDictionary> m_sessiondict;
    private Dictionary<string,DateTime> m_sessiontime;
    private List<Ipaddr> m_allownetaddr;
    private List<Ipaddr> m_allownetmask;
    private bool m_stoprequest;
    private bool m_listening;

    private static Random m_rand;


    private static void DoWork(object param){
        WorkerContext wc = (WorkerContext)param;
        HttpServer sv = wc.Server;
        while(!sv.m_stoprequest){
            wc.WorkerEvent.WaitOne();
            if(sv.m_stoprequest)
                break;
            wc.Status = WorkerStatus.BUSY;
            sv.OnConnect(wc);
            wc.Status = WorkerStatus.WAITING;
        }
        wc.Status = WorkerStatus.STOPPED;
    }

    private void OnConnect(WorkerContext wc) {
        wc.Timer.Reset();
        wc.Timer.Start();
        wc.Count++;
        HttpListenerContext context = wc.Context;
        string requestmemo = string.Format("{0} {1} from {2}",
                                           context.Request.HttpMethod,
                                           context.Request.RawUrl,
                                           context.Request.RemoteEndPoint);
        LOG_DEBUG("Requested "+requestmemo);

        // 古いセッションを破棄
        PurgeSession();

        context.Response.KeepAlive = false;
        string path = context.Request.Url.AbsolutePath;
        int status = 0;
        try {
            // デフォルトページ？
            if((path == "/") && (m_defaultpage != null)){
                using(HttpPage page = new HttpPage()){
                    page.SetLogger(this.Logger);
                    page.SetServerContext(this, context, path);
                    page.Redirect(m_defaultpage);
                    status = 200;
                }
            }

            // 静的ページを探す
            if(status == 0) {
                foreach(KeyValuePair<string,HttpStaticPage> kv in m_staticpagelist){
                    if(path.StartsWith(kv.Key) && (path.Length > kv.Key.Length) && (path[kv.Key.Length] == '/')){
                        if(context.Request.HttpMethod == "GET"){
                            lock(kv.Value){
                                kv.Value.SetLogger(this.Logger);
                                kv.Value.SetServerContext(this, context, path);
                                kv.Value.PageLoad(path.Substring(kv.Key.Length));
                            }
                            status = 200;
                        }else{
                            status = 405;
                        }
                        break;
                    }
                }
            }

            if(status == 0){
                // アプリケーションページを探す
                foreach(KeyValuePair<string,Type> kv in m_pagelist){
                    if(path.StartsWith(kv.Key) && ((path.Length == kv.Key.Length) || (path[kv.Key.Length] == '/'))){
                        if((context.Request.HttpMethod == "GET") || (context.Request.HttpMethod == "POST")){
                            // セッション変数をクッキー管理する。
                            Cookie sessid = context.Request.Cookies[m_servername+"-SID"];
                            ObjectDictionary sess;
                            lock(m_sessiondict){
                                if((sessid != null) && m_sessiondict.ContainsKey(sessid.Value)){
                                    sess = m_sessiondict[sessid.Value];
                                }else{
                                    sessid = new Cookie(m_servername+"-SID", m_rand.Next(9999999).ToString()+m_rand.Next(9999999).ToString());
                                    sess = new ObjectDictionary();
                                    m_sessiondict[sessid.Value] = sess;
                                }
                                m_sessiontime[sessid.Value] = DateTime.Now;
                            }
                            sessid.Version = 1;
                            sessid.Path = "/";
                            if(UseCookieExpires && (SessionTimeout > 0)) {
                                sessid.Expires = DateTime.Now.AddSeconds(SessionTimeout);
                                //context.Response.AppendCookie(sessid);
                                // なぜか正しいSet-Cookieヘッダを生成してくれないので、自力でヘッダを作る。
                                context.Response.AppendHeader("Set-Cookie", string.Format("{0}={1}; expires={2}; path={3}", sessid.Name, sessid.Value, sessid.Expires.ToString("r"), sessid.Path));
                            } else {
                                context.Response.AppendHeader("Set-Cookie", string.Format("{0}={1}; path={2}", sessid.Name, sessid.Value, sessid.Path));
                            }

                            // ページレンダラの呼び出し
                            using(HttpPage page = (HttpPage)(kv.Value.GetConstructor(Type.EmptyTypes).Invoke(null))){
                                page.SetLogger(this.Logger);
                                page.SetServerContext(this, context, kv.Key.Substring(1));
                                // セッション変数アクセスの排他を行なうと、同一端末からのアクセスの処理はシリアライズされる。
                                if(page.UseSession){
                                    lock(sess) {
                                        page.SetSession(sess, m_appl);
                                        page.PageLoad(path.Substring(kv.Key.Length));
                                    }
                                } else {
                                    page.SetSession(sess, m_appl);
                                    page.PageLoad(path.Substring(kv.Key.Length));
                                }
                                status = 200;
                            }
                        } else {
                            status = 405;
                        }
                        break;
                    }
                }
            }

            if(status == 0){
                // ルート静的ページを探す
                if(m_staticpagelist.ContainsKey("/")){
                    HttpStaticPage pg = m_staticpagelist["/"];
                    if(context.Request.HttpMethod == "GET"){
                        lock(pg){
                            pg.SetLogger(this.Logger);
                            pg.SetServerContext(this, context, path);
                            pg.PageLoad(path);
                        }
                        status = 200;
                    }else{
                        status = 405;
                    }
                }
            }

            if(status == 0)
                // 該当ページが無い。
                status = 404;

        } catch(PageRedirectException e) {
            using(HttpPage page = new HttpPage()){
                page.SetLogger(this.Logger);
                page.SetServerContext(this, context, path);
                page.Redirect(e.m_pagename);
                status = 200;
            }
        } catch(HttpListenerException) {
            // コンテンツ送信失敗。きっとクライアント側が切ったのだろう。
            status = 200; // とりあえず、こちら側の処理は成功したことにする。
            LOG_ERR("Client closed socket while sending contents.");
        } catch(IOException e) {
            // コンテンツ送信中にソケットが切れた(？)
            status = 200; // とりあえず、こちら側の処理は成功したことにする。
            LOG_DEBUG("IOError while sending contents: "+e.Message);
        } catch(ThreadAbortException) {
            using(HttpPage page = new HttpPage()){
                page.SetLogger(this.Logger);
                page.SetServerContext(this, context, "error");
                page.PageLoad("500");
            }
        } catch(Exception e) {
            LOG_CRIT(String.Format("{0}: {1} in {2}", e.GetType().Name, e.Message, e.TargetSite));
            if(ShowStackTrace) {
                using(HttpStackTracePage page = new HttpStackTracePage(e)) {
                    try {
                        page.SetLogger(this.Logger);
                        page.SetServerContext(this, context, "StackTrace");
                        page.PageLoad(e.Message);
                    } catch(Exception) {
                        // I can't do any more.
                    }
                }
                status = 200;
            } else {
                status = 500;
            }
        }
        if(status != 200){
            LOG_WARNING(string.Format("Failed to dispatch {0} (code={1})", requestmemo, status));
            using(HttpPage page = new HttpPage()){
                try {
                    page.SetLogger(this.Logger);
                    page.SetServerContext(this, context, "error");
                    page.PageLoad(status.ToString());
                } catch(Exception) {
                    // contextがもう使えない状態にあるらしい。きっとクライアント側が切ったのだろう。
                    // 無視。
                }
            }
        }
        //context.Response.OutputStream.Close(); // 念のため。
        // 上の行は、なぜかDebian版monoでは戻ってこない。
        try {
            context.Response.Close();
        } catch(Exception) {
            // Close失敗は無視する。
        }

        wc.Timer.Stop();
        long elapsed = wc.Timer.ElapsedMilliseconds;
        if(elapsed > wc.MaxElapsed)
            wc.MaxElapsed = elapsed;
        wc.TotalElapsed += elapsed;
        LOG_DEBUG(string.Format("Finished {0} ({1}msec)",requestmemo,elapsed));
    }

    private bool checkRemoteAddress(IPAddress remote) {
        if(IPAddress.IsLoopback(remote))
            return true;
        if(remote.AddressFamily != AddressFamily.InterNetwork)
            return false;
        Ipaddr ip = new Ipaddr(remote.GetAddressBytes(), 0, 4);
        for(int i = 0; i < m_allownetaddr.Count; i++) {
            if(Ipaddr.GetMasked(ip, m_allownetmask[i]) == m_allownetaddr[i])
                return true;
        }
        return false;
    }
    ///
    /// <summary>
    ///   HTTPServerがDeadlockした場合のタイムアウト処理
    /// </summary>
    private void monitorWorkerTimeout(object param){

        while(!m_stoprequest){

            if ( WorkerTimeout != 0 ) {
                for(int i = 0; i < m_workerlist.Length; i++){
                    WorkerContext context = m_workerlist[i];
                    long elapsed = context.Timer.ElapsedMilliseconds;
                    if(elapsed > WorkerTimeout * 1000) {
                        lock(m_workerlist){
                            // 新規のコンテキストと差し替える
                            m_workerlist[i] = new WorkerContext();
                            initContext(m_workerlist[i]);
                        }

                        context.WorkerThread.Abort();
                        LOG_ERR("restart thread for deadlock");
                    }
                }
            }

            Thread.Sleep(1000); // 1秒に１回実行
        }
    }

    private void initContext(WorkerContext ctx){
        ctx.Status = WorkerStatus.WAITING;
        ctx.Server = this;
        ctx.Context = null;
        ctx.WorkerEvent = new AutoResetEvent(false);
        ctx.WorkerThread = new Thread(new ParameterizedThreadStart(DoWork));
        ctx.MaxElapsed = 0;
        ctx.Timer = new Stopwatch();
        ctx.WorkerThread.Start(ctx);
    }

}

} // End of namespace
