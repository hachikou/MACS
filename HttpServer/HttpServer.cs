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
    ///   最大ワーカースレッド数
    /// </summary>
    public int MaxWorkers {
        get { return m_maxworkers; }
        set {
            if(value < 1)
                value = 1;
            m_maxactivepages = value-(m_maxworkers-m_maxactivepages);
            if(m_maxactivepages < 1)
                m_maxactivepages = 1;
            m_maxworkers = value;
        }
    }

    /// <summary>
    ///   WEBアプリ用最大ワーカースレッド数
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     動的ページ処理をするワーカースレッドの最大同時実行数。
    ///     この個数を超える動的ページのHttpセッションが要求された時には、
    ///     EmergencyPageクラスで処理が行なわれるようになる。
    ///   </para>
    /// </remarks>
    public int MaxActivePages {
        get { return m_maxactivepages; }
        set {
            if(value < 1)
                m_maxactivepages = 1;
            else if(value > m_maxworkers)
                m_maxactivepages = m_maxworkers;
            else
                m_maxactivepages = value;
        }
    }

    /// <summary>
    ///   緊急時用ワーカスレッド数
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     この数は、MaxWorkersに含まれる。
    ///     MaxActivePagesを超えた数の動的ページ生成セッションが張られた時に緊急
    ///     ページ表示に使用するためのワーカスレッド数。
    ///   </para>
    /// </remarks>
    public int ReservedWorkers {
        get { return m_maxworkers-m_maxactivepages; }
        set {
            if(value < 0)
                value = 0;
            m_maxactivepages = m_maxworkers-value;
            if(m_maxactivepages < 1)
                m_maxactivepages = 1;
        }
    }
    
    /// <summary>
    ///   ページレンダリングタイムアウトのデフォルト値（ミリ秒）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     この時間経過してもPageLoadが完了しない場合、AbortExceptionが
    ///     発生する。
    ///     0を設定するとタイムアウト処理なしになる。
    ///   </para>
    /// </remarks>
    public int DefaultTimeout = 0;

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
        m_emergencypage = typeof(HttpEmergencyPage);
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
        if(!pageClass.Equals(typeof(HttpPage)) && !pageClass.IsSubclassOf(typeof(HttpPage)))
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
    ///   緊急ページレンダラを登録する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     動的ページを処理中のワーカー数がMaxActivePagesを超えた場合には、ここ
    ///     で指定されたページレンダラが呼び出される。
    ///     一般的には、緊急ページレンダラは、HTTPアクセスが過負荷状態になって
    ///     いることを表示して終了する。
    ///   </para>
    /// </remarks>
    public void SetEmergencyPage(Type pageClass) {
        if(!pageClass.Equals(typeof(HttpPage)) && !pageClass.IsSubclassOf(typeof(HttpPage)))
            throw new InvalidCastException("Page class must be a subclass of HttpPage.");
        m_emergencypage = pageClass;
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
    /// <param name="initworkers">初期ワーカースレッド数</param>
    public void Run(int initworkers) {
        if(IsRunning){
            LOG_CRIT("Already running.");
            return;
        }
        m_stoprequest = false;
        if(initworkers <= 0)
            initworkers = 1;

        m_workerlist = new List<WorkerContext>();
        for(int i = 0; i < initworkers; i++){
            m_workerlist.Add(new WorkerContext(this, i));
        }

        try {
            m_listener.Start();
        } catch(Exception e) {

            // Windows 7 でデバッグ実行しているときにここに陥る場合、
            // 管理者権限で 'netsh http add urlacl url=http://+:10080/ user=ユーザー名' を実行してください。

            LOG_CRIT(string.Format("Failed to listen on {0}: {1}", m_url, e.Message));
            foreach(WorkerContext wc in m_workerlist) {
                wc.Dispose();
            }
            m_workerlist = null;
            throw e;
        }

        LOG_INFO(string.Format("Accepting {0}, {1} workers.", m_url, m_workerlist.Count));

        m_activepages = 0;
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
            lock(m_workerlist) {
                for(int i = 0; i < m_workerlist.Count; i++){
                    if(m_workerlist[i].Status == WorkerStatus.WAITING){
                        if(widx < 0)
                            widx = i;
                    } else if(m_workerlist[i].Status == WorkerStatus.STOPPED) {
                        // 止まってしまったスレッドを回収する
                        LOG_NOTICE("Disposing worker#{0}", i);
                        m_workerlist[i].Dispose();
                        m_workerlist[i] = new WorkerContext(this, i);
                        if(widx < 0)
                            widx = i;
                    }
                }
                if(widx < 0){ // All workers are busy.
                    if(m_workerlist.Count >= m_maxworkers) {
                        context.Response.Abort();
                        LOG_ERR("No more workers (limit={0}). Connection from client is aborted.", m_maxworkers);
                        continue;
                    }
                    // ワーカーを増やして対応する
                    widx = m_workerlist.Count;
                    m_workerlist.Add(new WorkerContext(this, widx));
                }
            } // end of lock(m_workerlist)
            // ワーカースレッドにcontextを渡して、処理させる。
            m_workerlist[widx].Status = WorkerStatus.CONNECTED;
            m_workerlist[widx].Context = context;
            m_workerlist[widx].WorkerEvent.Set();
            m_pagecount++;
        }
        try {
            m_listener.Stop();
        } catch(Exception) {
            // just ignore.
        }

        foreach(WorkerContext wc in m_workerlist)
            wc.WorkerEvent.Set();
        foreach(WorkerContext wc in m_workerlist) {
            wc.WorkerThread.Join();
            wc.Dispose();
        }
        m_workerlist = null;
        m_stoprequest = false;

        LOG_INFO("Finished accepting {0}", m_url);
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
            stat.Workers = m_workerlist.Count;
            foreach(WorkerContext wc in m_workerlist) {
                if(wc == null){
                    stat.Stopped++;
                    continue;
                }
                stat.PageCount += wc.Count;
                stat.TotalTime += wc.TotalElapsed;
                if(stat.MaxTime < wc.MaxElapsed)
                    stat.MaxTime = wc.MaxElapsed;
                switch(wc.Status){
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
    public void DebugLog(bool detail=false) {
        foreach(string msg in GetDebugMessage(detail))
            LOG_DEBUG(msg);
    }

    /// <summary>
    ///   サーバ稼働状況メッセージを得る
    /// </summary>
    public string[] GetDebugMessage(bool detail=false) {
        List<string> res = new List<string>();
        Statistics stat = GetStatistics();
        res.Add(String.Format("Workers={0}; {1} waiting, {2} connected, {3} busy, {4} stopped", stat.Workers, stat.Waiting, stat.Connected, stat.Busy, stat.Stopped));
        res.Add(String.Format("Total {0} pages; mean {1}msec, max {2}msec", stat.PageCount, stat.MeanTime, stat.MaxTime));
        if(detail && (stat.Workers > 0)) {
            if(m_workerlist == null) {
                res.Add("Corrupted (workerlist is gone away)");
            } else {
                lock(m_workerlist) {
                    foreach(WorkerContext wc in m_workerlist) {
                        string title = String.Format("Worker#{0}: ", wc.WorkerNumber);
                        if(wc == null) {
                            res.Add(title+"DISPOSED");
                            continue;
                        }
                        res.Add(String.Format("{0}{1} pages={2}, totalTime={3}msec, maxTime={4}msec", title, wc.Status.ToString(), wc.Count, wc.TotalElapsed, wc.MaxElapsed));
                    }
                }
            }
        }
        return res.ToArray();
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
    private class WorkerContext : IDisposable {
        
        public readonly HttpServer   Server;
        public readonly int          WorkerNumber;
        public volatile WorkerStatus Status;
        public NThread       WorkerThread;
        public HttpListenerContext Context;
        public AutoResetEvent WorkerEvent;
        public long         Count;
        public long         TotalElapsed; // Milliseconds
        public long         MaxElapsed; // Milliseconds
        public Stopwatch    Timer;

        public WorkerContext(HttpServer server, int workerNumber) {
            server.LOG_INFO("Creating worker#{0}", workerNumber);
            this.WorkerNumber = workerNumber;
            this.Status = WorkerStatus.WAITING;
            this.Server = server;
            this.Context = null;
            this.WorkerEvent = new AutoResetEvent(false);
            this.WorkerThread = new NThread(String.Format("HttpServer({0}):Worker#{1}", this.Server.ServerName, this.WorkerNumber), doWork);
            this.MaxElapsed = 0;
            this.Timer = new Stopwatch();
            this.WorkerThread.Start();
        }

        ~WorkerContext() {
           Dispose();
        }

        public void Dispose() {
            if(this.WorkerEvent != null) {
                this.WorkerEvent.Dispose();
                this.WorkerEvent = null;
            }
            if(this.WorkerThread != null) {
                this.WorkerThread.Abort();
                this.WorkerThread.Dispose();
                this.WorkerThread = null;
            }
        }
        
        private void doWork(){
            HttpServer sv = this.Server;
            try {
                sv.LOG_DEBUG("Start worker#{0}", this.WorkerNumber);
                while(!sv.m_stoprequest){
                    this.WorkerEvent.WaitOne();
                    if(sv.m_stoprequest)
                        break;
                    this.Status = WorkerStatus.BUSY;
                    sv.OnConnect(this);
                    this.Status = WorkerStatus.WAITING;
                }
                sv.LOG_DEBUG("Stop worker#{0}", this.WorkerNumber);
            } catch(ThreadAbortException) {
                // OnConnect内でログを吐いているので、ここでは何もしなくてよい
            } catch(Exception ex) {
                sv.LOG_ERR("Worker#{0} caught exception {1}", this.WorkerNumber, ex.GetType().Name);
                sv.LOG_EXCEPTION(ex);
            } finally {
                this.Status = WorkerStatus.STOPPED;
            }
        }
    }

    private string m_servername;
    private int m_port;
    private string m_url;
    private HttpListener m_listener;
    private List<WorkerContext> m_workerlist;
    private int m_pagecount;
    private string m_defaultpage;
    private Dictionary<string,Type> m_pagelist;
    private Dictionary<string,HttpStaticPage> m_staticpagelist;
    private Type m_emergencypage;
    private ObjectDictionary m_appl;
    private Dictionary<string,ObjectDictionary> m_sessiondict;
    private Dictionary<string,DateTime> m_sessiontime;
    private List<Ipaddr> m_allownetaddr;
    private List<Ipaddr> m_allownetmask;
    private bool m_stoprequest;
    private bool m_listening;
    private int m_maxworkers = 64;
    private int m_maxactivepages = 48;
    private int m_activepages;
    private object m_activepages_mutex = new object();

    private static Random m_rand;


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
        context.Response.AddHeader("Server", "MACS HttpServer");
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
                        // 動的ページにマッチした
                        Type pageClass = kv.Value;
                        // 動的ページは同時にMaxActivePages個しかレンダリングしない
                        lock(m_activepages_mutex) {
                            if(++m_activepages > m_maxactivepages) {
                                // オーバーした時は強制的にEmergencyPageでレンダリングする
                                LOG_NOTICE("Number of active pages exceeds MaxActivePages({0}), use {1}", m_maxactivepages, m_emergencypage.Name);
                                pageClass = m_emergencypage;
                            }
                        }
                        int timeout = 0; // ページレンダリングタイムアウト
                        try {
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
                                using(HttpPage page = (HttpPage)(pageClass.GetConstructor(Type.EmptyTypes).Invoke(null))){
                                    page.SetLogger(this.Logger);
                                    page.SetServerContext(this, context, kv.Key.Substring(1));
                                    // ページレンダリングタイムアウトの設定
                                    timeout = page.Timeout;
                                    if(timeout < 0)
                                        timeout = DefaultTimeout;
                                    if(timeout > 0)
                                        NThread.Watchdog(timeout, NThread.WatchdogMode.Abort);
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
                        } finally {
                            if(timeout > 0)
                                NThread.SuspendWatchdog();
                            lock(m_activepages_mutex) {
                                if(m_activepages > 0)
                                    m_activepages--;
                            }
                        }
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
        } catch(ThreadInterruptedException) {
            using(HttpPage page = new HttpPage()){
                page.SetLogger(this.Logger);
                page.SetServerContext(this, context, "error");
                page.PageLoad("503");
            }
        } catch(ThreadAbortException ex) {
            bool isTimeout = (ex.ExceptionState != null)&&(ex.ExceptionState is TimeoutException);
            if(isTimeout) {
                LOG_ERR("{0} is aborted due to timeout", path);
            } else {
                LOG_ERR("{0} is aborted", path);
            }
            LOG_EXCEPTION(ex);
            using(HttpPage page = new HttpPage()){
                page.SetLogger(this.Logger);
                page.SetServerContext(this, context, "error");
                page.PageLoad(isTimeout?"408":"500");
            }
        } catch(Exception e) {
            LOG_CRIT(String.Format("{0}: {1} in {2}", e.GetType().Name, e.Message, e.TargetSite));
            LOG_EXCEPTION(e);
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

}

} // End of namespace
