/// ThreadBase: バックグラウンド動作スレッドを持つクラスの基底.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Threading;

namespace MACS {


/// <summary>
///   バックグラウンド動作スレッドを持つクラス
/// </summary>
/// <remarks>
///   <para>
///     派生クラスではRun()メソッドを定義すること。
///     Runメソッド内では、StopRequestを定期的にチェックし、trueならばリソースを
///     開放するなどの処置をしてメソッドからreturnすること。
///   </para>
/// </remarks>
public abstract class ThreadBase : Loggable,IDisposable {

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ThreadBase(string name_) {
        name = name_;
        threadMutex = new object();
        mythread = null;
    }

    /// <summary>
    /// デストラクタ
    /// </summary>
    ~ThreadBase() {
        Dispose();
    }

    /// <summary>
    ///   リソース解放
    /// </summary>
    public virtual void Dispose() {
        lock(threadMutex) {
            _waitForStop(1000);
        }
    }

    /// <summary>
    ///   サービス名（デバッグメッセージ用）
    /// </summary>
    public virtual string ThreadName {
        get { return (mythread!=null)?mythread.Name:name; }
    }
    
    /// <summary>
    ///   スレッド動作を開始する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     既に開始している場合は何もしない。
    ///   </para>
    /// </remarks>
    public void Start() {
        lock(threadMutex) {
            if(mythread != null)
                return;
            StopRequest = false;
            mythread = new NThread(name, Run);
            mythread.Start();
        }
    }

    /// <summary>
    ///   スレッド動作の停止を要求する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     サブスレッドに停止要求を出すだけで、停止は待たない。
    ///   </para>
    /// </remarks>
    public void Stop() {
        lock(threadMutex) {
            StopRequest = true;
        }
    }

    /// <summary>
    ///   スレッド動作の停止を待つ
    /// </summary>
    /// <param name="timelimit">最大待ち時間。ミリ秒</param>
    public void WaitForStop(int timelimit) {
        lock(threadMutex) {
            _waitForStop(timelimit);
        }
    }


    /// <summary>
    ///   スレッド停止要求フラグ
    /// </summary>
    protected bool StopRequest
    { get; private set; }

    /// <summary>
    ///   スレッド実行メソッド
    /// </summary>
    protected abstract void Run();


    private object threadMutex;
    private string name;
    private NThread mythread;

    private void _waitForStop(int timelimit) {
        if(mythread == null)
            return;
        StopRequest = true;
        mythread.Join(timelimit);
        mythread = null;
    }

}

} // End of namespace
