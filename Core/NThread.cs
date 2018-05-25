/*! @file NThread.cs
 * @brief デバッグが容易なThreadを提供する
 *
 * Copyright (C) 2017 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace MACS {

/// <summary>
///   Threadをデバッグしやすくするためのラッピングクラス
/// </summary>
public class NThread : Loggable, IDisposable {

#region 生成と廃棄

    /// <summary>
    ///   スレッドの作成（起動はしない）
    /// </summary>
    /// <param name="name">スレッド名（デバッグ用の任意の名称）</param>
    /// <param name="start">スレッド起動メソッド</param>
    /// <param name="maxStackSize">スタックサイズ。0を指定するとデフォルトサイズを使う</param>
    public NThread(string name, ThreadStart start, int maxStackSize=0) {
        thread = new Thread(start, maxStackSize);
        thread.Name = name;
        lock(threadDict) {
            threadDict[thread] = this;
        }
    }

    /// <summary>
    ///   スレッドの作成（起動はしない）
    /// </summary>
    /// <param name="name">スレッド名（デバッグ用の任意の名称）</param>
    /// <param name="start">スレッド起動メソッド</param>
    /// <param name="maxStackSize">スタックサイズ。0を指定するとデフォルトサイズを使う</param>
    public NThread(string name, ParameterizedThreadStart start, int maxStackSize=0) {
        thread = new Thread(start, maxStackSize);
        thread.Name = name;
        lock(threadDict) {
            threadDict[thread] = this;
        }
    }

    /// <summary>
    ///   既存のスレッドに名前をつけてNThreadとして管理する
    /// </summary>
    public NThread(string name, Thread thread_) {
        thread = thread_;
        thread.Name = name;
        lock(threadDict) {
            threadDict[thread] = this;
        }
    }


    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~NThread() {
        Dispose(false);
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    public void Dispose() {
        Dispose(true);
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    protected virtual void Dispose(bool disposing) {
        if(watchdogThread != null) {
            watchdogThread.Abort();
            watchdogThread = null;
        }
        if(thread != null) {
            // NThreadをDisposeしてもThreadは停止しないことに注意。
            lock(threadDict) {
                threadDict.Remove(thread);
            }
            thread = null;
        }
    }

#endregion

#region キャスト

    /// <summary>
    ///   通常のThreadとして使うためのキャスト定義
    /// </summary>
    public static implicit operator Thread(NThread obj) {
        return obj.thread;
    }

    /// <summary>
    ///   通常のThreadをNThreadとして使うためのキャスト定義
    /// </summary>
    // 意図せずキャストが働いてしまう危険があるため、コメントアウトしました。
    //public static implicit operator NThread(Thread obj) {
    //    return Get(obj);
    //}

    public static NThread Get(Thread obj) {
        if(obj == null)
            return null;
        lock(threadDict) {
            NThread nt;
            if(threadDict.TryGetValue(obj, out nt))
                return nt;
            return null;
        }
    }

#endregion

#region プロパティ

    /// <summary>
    ///   スレッドが生きているかどうか
    /// </summary>
    public bool IsAlive {
        get { return (thread != null) && thread.IsAlive; }
    }

    /// <summary>
    ///   バックグラウンドスレッドかどうか
    /// </summary>
    public bool IsBackground {
        get { return (thread != null) && thread.IsBackground; }
        set {
            if(thread != null)
                thread.IsBackground = value;
        }
    }

    /// <summary>
    ///   スレッドID
    /// </summary>
    public int ManagedThreadId {
        get { return (thread != null)?thread.ManagedThreadId:0; }
    }

    /// <summary>
    ///   スレッド名
    /// </summary>
    public string Name {
        get { return (thread != null)?thread.Name:""; }
        set {
            if(thread != null)
                thread.Name = value;
        }
    }

    /// <summary>
    ///   プライオリティ
    /// </summary>
    public ThreadPriority Priority {
        get {
            if(thread == null)
                throw new ThreadStateException("Thread is disposed.");
            return thread.Priority;
        }
    }

    /// <summary>
    ///   スレッドの状態
    /// </summary>
    public System.Threading.ThreadState ThreadState {
        get {
            return (thread != null)?thread.ThreadState:System.Threading.ThreadState.Stopped;
        }
    }

    /// <summary>
    ///   スレッドがAbort中かどうか
    /// </summary>
    public bool IsAborting {
        get { return isAborting; }
    }

#endregion

#region インスタンスメソッド

    public void Abort(object stateInfo=null) {
        isAborting = true;
        if(watchdogThread != null) {
            watchdogThread.Abort();
            watchdogThread = null;
        }
        if(thread != null) {
            thread.Abort(stateInfo);
        }
    }

    public void Interrupt() {
        if(thread != null)
            thread.Interrupt();
    }

    public void Join() {
        if(thread == null)
            return;
        if(watchdogThread != null) {
            watchdogThread.Abort();
            watchdogThread = null;
        }
        thread.Join();
    }

    public bool Join(int timeout) {
        if(thread == null)
            return true;
        if(watchdogThread != null) {
            watchdogThread.Abort();
            watchdogThread = null;
        }
        return thread.Join(timeout);
    }

    public bool Join(TimeSpan timeout) {
        if(thread == null)
            return true;
        if(watchdogThread != null) {
            watchdogThread.Abort();
            watchdogThread = null;
        }
        return thread.Join(timeout);
    }

    public void Start() {
        if(thread == null)
            throw new ThreadStateException("Thread is disposed.");
        thread.Start();
    }

    public void Start(object obj) {
        if(thread == null)
            throw new ThreadStateException("Thread is disposed.");
        thread.Start(obj);
    }

#endregion

#region グローバルプロパティ

    /// <summary>
    ///   現在のスレッド
    /// </summary>
    public static NThread CurrentThread {
        get { return NThread.Get(Thread.CurrentThread); }
    }

#endregion

#region ウォッチドッグ

    /// <summary>
    ///   ウォッチドッグが発動した時の動作
    /// </summary>
    public enum WatchdogMode {
        LogOnly,          //< ログを記録するだけ
        Interrupt,        //< ThreadInterruptedExceptionを発行する
        Abort,            //< ThreadAbortExceptionを発行する
        DelayedInterrupt, //< 1度目はログを記録するだけ。2度目はInterrupt
        DelayedAbort,     //< 1度目はログを記録するだけ。2度目はAbort
        Keep,             //< 以前の動作指定を継続する
    }

    /// <summary>
    ///   ウォッチドッグを開始する
    /// </summary>
    /// <param name="timeout">ウォッチドッグタイムアウト時間（ミリ秒）。省略または0以下を指定した時は以前の設定を継続する</param>
    /// <param name="mode">ウォッチドッグ発動時の動作。省略時は以前の設定を継続する</param>
    /// <remarks>
    ///   <para>
    ///     このスレッド用のウォッチドッグ用のスレッドが作成されて監視します。
    ///     ただし、すでにウォッチドッグスレッドが作成されている場合は、ウォッチ
    ///     ドッグタイマのリセットだけを行なうことになります。
    ///     同一スレッドで次にWatchdog / StopWatchdogが呼ばれる前にtimeoutミリ秒
    ///     が経過すると、modeで指定した動作が起こります。
    ///     タイムアウトでスレッドをAbortする際には、ExceptionStateに
    ///     TimeoutExceptionオブジェクトがセットされます。
    ///   </para>
    /// </remarks>
    public static void Watchdog(int timeout=0, WatchdogMode mode=WatchdogMode.Keep) {
        NThread nth = CurrentThread;
        if(nth != null) {
            nth.watchdog((long)timeout, mode);
        }
    }

    /// <summary>
    ///   ウォッチドッグを一時停止する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     内部的にはlong.MaxValueミリ秒のウォッチドッグを行なっているだけなの
    ///     で、スレッド監視処理の負荷は減りません。
    ///   </para>
    /// </remarks>
    public static void SuspendWatchdog() {
        NThread nth = CurrentThread;
        if(nth != null) {
            nth.watchdog(long.MaxValue, WatchdogMode.Keep);
        }
    }


    /// <summary>
    ///   ウォッチドッグを止める
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     このスレッド用のウォッチドッグ監視スレッドが破棄されます。
    ///     もしウォッチドッグを開始していない場合には何もしません。
    ///     ウォッチドッグ監視を繰り返し何度も行なう場合に都度StopWatchdogをする
    ///     のは大きなCPU負荷になります。一時的にウォッチドッグがかからないよう
    ///     にしたい場合は、SuspendWatchdog()を呼び出してください。
    ///   </para>
    /// </remarks>
    public static void StopWatchdog() {
        NThread nth = CurrentThread;
        if(nth != null) {
            nth.stopWatchdog();
        }
    }


    private Thread watchdogThread = null;
    private Stopwatch watchdogTimer = null;
    private long watchdogTimeout = 1000;
    private int watchdogCycle = 100;
    private int watchdogCount = 0;
    private WatchdogMode watchdogMode = WatchdogMode.LogOnly;
    private volatile bool isAborting = false;

    private const int MAX_WATCHDOG_CYCLE = 1000;

    private void watchdog(long timeout, WatchdogMode mode) {
        if(watchdogTimer == null) {
            watchdogTimer = new Stopwatch();
        }
        lock(watchdogTimer) {
            if(timeout > 0) {
                watchdogTimeout = timeout;
                // 監視周期はwatchdogTimeout/10。ただし最低1msec、最大1000msec。
                if(watchdogTimeout < 10)
                    watchdogCycle = 1;
                else if(watchdogTimeout < (long)MAX_WATCHDOG_CYCLE*10)
                    watchdogCycle = (int)(watchdogTimeout/10L);
                else
                    watchdogCycle = MAX_WATCHDOG_CYCLE;
            }
            watchdogTimer.Restart();
            if(mode != WatchdogMode.Keep)
                watchdogMode = mode;
            watchdogCount = 0;
        }
        if(watchdogThread == null) {
            watchdogThread = new Thread(doWatchdog);
            watchdogThread.Name = this.Name+"-watchdog";
            watchdogThread.IsBackground = true;
            watchdogThread.Start();
        }
    }

    private void stopWatchdog() {
        if(watchdogThread == null)
            return;
        watchdogThread.Abort();
        watchdogThread = null;
        if(watchdogTimer != null) {
            watchdogTimer = null;
        }
    }

    private void doWatchdog() {
        while((watchdogThread != null) && (watchdogTimer != null)) {
            if(watchdogTimer.ElapsedMilliseconds >= watchdogTimeout) {
                watchdogTimer.Restart();
                watchdogCount++;
                switch(watchdogMode) {
                case WatchdogMode.Interrupt:
                    LOG_NOTICE("Watchdog alert on thread {0} ({1}msec), send interrupt", this.Name, watchdogTimeout);
                    watchdogTimeout = long.MaxValue;
                    thread.Interrupt();
                    break;
                case WatchdogMode.Abort:
                    LOG_NOTICE("Watchdog alert on thread {0} ({1}msec), send abort", this.Name, watchdogTimeout);
                    watchdogTimeout = long.MaxValue;
                    isAborting = true;
                    thread.Abort(new TimeoutException(String.Format("Watchdog alert ({0}msec)", watchdogTimeout)));
                    break;
                case WatchdogMode.DelayedInterrupt:
                    if(watchdogCount > 1) {
                        LOG_NOTICE("Watchdog alert twice on thread {0} ({1}msec), send interrupt", this.Name, watchdogTimeout);
                        watchdogTimeout = long.MaxValue;
                        thread.Interrupt();
                    } else {
                        LOG_NOTICE("Watchdog alert on thread {0} ({1}msec), will send interrupt on next alert", this.Name, watchdogTimeout);
                    }
                    break;
                case WatchdogMode.DelayedAbort:
                    if(watchdogCount > 1) {
                        LOG_NOTICE("Watchdog alert twice on thread {0} ({1}msec), send abort", this.Name, watchdogTimeout);
                        watchdogTimeout = long.MaxValue;
                        isAborting = true;
                        thread.Abort(new TimeoutException(String.Format("Watchdog alert ({0}msec)", watchdogTimeout)));
                    } else {
                        LOG_NOTICE("Watchdog alert on thread {0} ({1}msec), will send abort on next alert", this.Name, watchdogTimeout);
                    }
                    break;
                default:
                    LOG_NOTICE("Watchdog alert on thread {0} ({1}msec)", this.Name, watchdogTimeout);
                    break;
                }
            }
            Thread.Sleep(watchdogCycle);
        }
    }

#endregion

#region 時間計測

    /// <summary>
    ///   実行時間計測用クラス
    /// </summary>
    public class Measure {

        /// <summary>
        ///   実行時間計測の準備をする
        /// </summary>
        /// <param name="logger_">計測結果を出力するときに使うLoggableオブジェクト</param>
        public Measure(Loggable logger_=null) {
            NThread nth = CurrentThread;
            if(nth == null)
                throw new InvalidOperationException("Can't measure time outside NThread's thread");
            threadName = "["+nth.Name+"]";
            logger = (logger_??nth).Logger;
        }

        /// <summary>
        ///   実行時間計測の準備をする
        /// </summary>
        /// <param name="logger_">計測結果を出力するときに使うOpeLog</param>
        public Measure(OpeLog logger_) {
            NThread nth = CurrentThread;
            if(nth == null)
                throw new InvalidOperationException("Can't measure time outside NThread's thread");
            threadName = "["+nth.Name+"]";
            logger = logger_;
        }

        /// <summary>
        ///   実行時間計測の準備をする
        /// </summary>
        /// <param name="writer_">計測結果を出力するときに使うTextWriter</param>
        public Measure(TextWriter writer_) {
            NThread nth = CurrentThread;
            if(nth == null)
                throw new InvalidOperationException("Can't measure time outside NThread's thread");
            threadName = "["+nth.Name+"]";
            writer = writer_;
        }

        /// <summary>
        ///   時間計測を開始する
        /// </summary>
        public void Start(string title_=null) {
            title = title_;
            timer.Restart();
        }

        /// <summary>
        ///   時間計測結果を出力する
        /// </summary>
        /// <returns>計測した実行時間（ミリ秒）</returns>
        public long End() {
            timer.Stop();
            long t = timer.ElapsedMilliseconds;
            show("Time", t);
            title = null;
            return t;
        }

        /// <summary>
        ///   時間計測結果を出力して次の計測を始める
        /// </summary>
        /// <returns>計測した実行時間（ミリ秒）</returns>
        public long Next(string title_=null) {
            long t = timer.ElapsedMilliseconds;
            show("Time", t);
            title = title_;
            timer.Restart();
            return t;
        }

        /// <summary>
        ///   時間計測結果を出力する（測定継続）
        /// </summary>
        /// <returns>計測した実行時間（ミリ秒）</returns>
        public long LapTime() {
            long t = timer.ElapsedMilliseconds;
            show("Lap ", t);
            return t;
        }

        private readonly string threadName;
        private readonly OpeLog logger = null;
        private readonly TextWriter writer = null;
        private string title = null;
        private Stopwatch timer = new Stopwatch();

        private void show(string timeTitle, long t) {
            if(title == null)
                return;
            string msg = String.Format("{1,7:N0}msec {0}", title, t);
            if(logger != null)
                logger.Log(timeTitle+threadName, OpeLog.Level.DEBUG, msg);
            if(writer != null)
                writer.WriteLine("{0}{1}: {2}", timeTitle, threadName, msg);
        }

    }

#endregion


#region スレッドデバッグ

    /// <summary>
    ///   スレッドの状態をデバッグログに書き出す
    /// </summary>
    public void Dump(OpeLog logger) {
        dump(logger, null);
    }

    /// <summary>
    ///   スレッドの状態を書き出す
    /// </summary>
    public void Dump(TextWriter w) {
        dump(null, w);
    }

    private void dump(OpeLog logger, TextWriter w) {
        string msg;
        msg = String.Format("status={0}", (thread != null)?thread.ThreadState.ToString():"Disposed");
        if(logger != null)
            logger.Log("NThread["+this.Name+"]", OpeLog.Level.DEBUG, msg);
        if(w != null)
            w.WriteLine("NThread["+this.Name+"]: "+msg);
        if((watchdogThread != null) && (watchdogTimer != null)) {
            msg = String.Format("watchdog: mode={0}, timeout={1}msec, timer={2}msec", watchdogMode.ToString(), (watchdogTimeout==long.MaxValue)?"inf.":watchdogTimeout.ToString(), watchdogTimer.ElapsedMilliseconds);
            if(logger != null)
                logger.Log("        ", OpeLog.Level.DEBUG, msg);
            if(w != null)
                w.WriteLine("        "+msg);
        }
    }

    /// <summary>
    ///   NThreadの一覧をデバッグログに書き出す
    /// </summary>
    public static void DumpAll(OpeLog logger) {
        lock(threadDict) {
            logger.Log("NThread", OpeLog.Level.DEBUG, "total {0} threads", threadDict.Count);
            foreach(NThread nth in threadDict.Values) {
                nth.Dump(logger);
            }
        }
    }

    /// <summary>
    ///   NThreadの一覧を書き出す
    /// </summary>
    public static void DumpAll(TextWriter w) {
        NThread[] list;
        lock(threadDict) {
            list = new NThread[threadDict.Count];
            threadDict.Values.CopyTo(list, 0);
        }
        Array.Sort(list, delegate(NThread a, NThread b){
                return a.Name.CompareTo(b.Name);
            });
        w.WriteLine("NThread: total {0} threads", list.Length);
        foreach(NThread nth in list) {
            nth.Dump(w);
        }
    }

#endregion

#region private部

    /// <summary>
    ///   スレッドハンドル
    /// </summary>
    private Thread thread;

    private static Dictionary<Thread,NThread> threadDict = new Dictionary<Thread,NThread>();

#endregion

}

} // End of namespace
