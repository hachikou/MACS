/*! @file RWLock.cs
 * @brief 安全に使え、デバッグが容易なReadLock/WriteLock/UpgradableLockを提供する
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

namespace MACS {

/// <summary>
///   ReaderWriterLockSlimをデバッグしやすくするためのラッピングクラス
/// </summary>
/// <remarks>
///   <para>
///     lock(mutex){...} と同じような感覚でRead/Writeロックを使えるようにしたクラスです。
///     RWLockオブジェクトは、上記の'mutex'にあたる、ロック対象オブジェクトで、
///     その実態は C#標準の ReaderWriterLockSlimです。
///     従来lock()を使って記述していたコードを、次のように記述することでRead/Write
///     ロック化することができます。
///     
///     object mutex = new object();
///     .....
///         lock(mutex) {
///             .....
///         }
///
///     ↓↓↓
///     RWLock rwlock = new RWLock("ロック名称");
///     .....
///         // Read lockをする場合
///         using(var lockhandle = new ReadLock(rwlock)) {
///             .....
///         }
///         // Write lockをする場合
///         using(var lockhandle = new WriteLock(rwlock)) {
///             .....
///         }
///
///     "ロック名称"は、デバッグを容易にするための任意の文字列で、ロック動作には
///     関係ありません。
///     ReadLockまたはWriteLockインスタンスをnewするところでロックがかかり（必要
///     に応じて待ちが発生し）、using句を抜けてインスタンスがDisposeされる際に
///     ロックが解除されます。
///     using句を使って確実にロックを解除できるので、ReaderWriterLockSlimクラス
///     をそのまま使う時に起こりがちな、ロック解除コードの書き忘れを防ぐことが
///     できます。
///
///     RWLock.DumpAll(OpeLog logger) メソッドを使うと、現在使われている全ての
///     RWLockの状態をログに書き出すことができます。
///   </para>
/// </remarks>
public class RWLock : ReaderWriterLockSlim {

    /// <summary>
    ///   ロック名称
    /// </summary>
    public string Name;

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    /// <param name="name">ロック名称（デバッグ用）</param>
    /// <param name="allowRecursiveLock">再帰的なロックを許可するかどうか</param>
    /// <param name="debugThread">ロックしているスレッドのデバッグ情報を記録するかどうか</param>
    public RWLock(string name, bool allowRecursiveLock=true, bool debugThread=false)
        : base(allowRecursiveLock?LockRecursionPolicy.SupportsRecursion:LockRecursionPolicy.NoRecursion) {
        Name = name;
        if(debugThread) {
            readingThread = new List<Thread>();
            writingThread = new List<Thread>();
            upgradeableReadingThread = new List<Thread>();
            waitingReadThread = new List<Thread>();
            waitingWriteThread = new List<Thread>();
            waitingUpgradeableReadThread = new List<Thread>();
        }
        lock(globalRWLockList) {
            globalRWLockList.Add(this);
        }
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~RWLock() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    public new void Dispose() {
        lock(globalRWLockList) {
            globalRWLockList.Remove(this);
        }
        base.Dispose();
    }

    /// <summary>
    ///   再帰ロックを許可するかどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     再帰ロックを許可する場合、次のロックが許可されます。
    ///     - WriteロックをかけているスレッドがWriteロック、Readロック、UpgradeableReadLockをする。
    ///     - ReadロックをかけているスレッドがReadロックをする。
    ///     - UpgradeableReadロックをかけているスレッドがWriteロック、Readロック、UpgradeableReadLockをする。
    ///   </para>
    /// </remarks>
    public bool AllowRecursiveLock {
        get { return (this.RecursionPolicy == LockRecursionPolicy.SupportsRecursion); }
    }

    /// <summary>
    ///   ロック状態のデバッグダンプ
    /// </summary>
    public void Dump(OpeLog logger, bool full=false) {
        dump(logger, null, full);
    }

    /// <summary>
    ///   ロック状態の出力
    /// </summary>
    public void Dump(TextWriter w, bool full=false) {
        dump(null, w, full);
    }

    private void dump(OpeLog logger, TextWriter w, bool full) {
        if(!full && (this.CurrentReadCount == 0) && !this.IsWriteLockHeld && !this.IsUpgradeableReadLockHeld)
            return;
        if(logger != null) {
            logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG,
                       "reading {0}, writing {1}, u-reading {2}",
                       this.CurrentReadCount, this.IsWriteLockHeld?1:0, this.IsUpgradeableReadLockHeld?1:0);
        }
        if(w != null) {
            w.WriteLine("RWLock[{0}]: reading {1}, writing {2}, u-reading {3}",
                        this.Name, this.CurrentReadCount, this.IsWriteLockHeld?1:0, this.IsUpgradeableReadLockHeld?1:0);
        }
        if((readingThread != null) && (readingThread.Count > 0)) {
            StringBuilder sb = new StringBuilder("reading:");
            foreach(Thread t in readingThread) {
                sb.Append(' ');
                sb.Append(String.IsNullOrEmpty(t.Name)?t.ManagedThreadId.ToString():t.Name);
            }
            if(logger != null)
                logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG, sb.ToString());
            if(w != null)
                w.WriteLine("RWLock[{0}]: {1}", this.Name, sb.ToString());
        }
        if((writingThread != null) && (writingThread.Count > 0)) {
            StringBuilder sb = new StringBuilder("writing:");
            foreach(Thread t in writingThread) {
                sb.Append(' ');
                sb.Append(String.IsNullOrEmpty(t.Name)?t.ManagedThreadId.ToString():t.Name);
            }
            if(logger != null)
                logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG, sb.ToString());
            if(w != null)
                w.WriteLine("RWLock[{0}]: {1}", this.Name, sb.ToString());
        }
        if((upgradeableReadingThread != null) && (upgradeableReadingThread.Count > 0)) {
            StringBuilder sb = new StringBuilder("upgradeableReading:");
            foreach(Thread t in upgradeableReadingThread) {
                sb.Append(' ');
                sb.Append(String.IsNullOrEmpty(t.Name)?t.ManagedThreadId.ToString():t.Name);
            }
            if(logger != null)
                logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG, sb.ToString());
            if(w != null)
                w.WriteLine("RWLock[{0}]: {1}", this.Name, sb.ToString());
        }
        if((waitingReadThread != null) && (waitingReadThread.Count > 0)) {
            StringBuilder sb = new StringBuilder("waiting read:");
            foreach(Thread t in waitingReadThread) {
                sb.Append(' ');
                sb.Append(String.IsNullOrEmpty(t.Name)?t.ManagedThreadId.ToString():t.Name);
            }
            if(logger != null)
                logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG, sb.ToString());
            if(w != null)
                w.WriteLine("RWLock[{0}]: {1}", this.Name, sb.ToString());
        }
        if((waitingWriteThread != null) && (waitingWriteThread.Count > 0)) {
            StringBuilder sb = new StringBuilder("waiting read:");
            foreach(Thread t in waitingWriteThread) {
                sb.Append(' ');
                sb.Append(String.IsNullOrEmpty(t.Name)?t.ManagedThreadId.ToString():t.Name);
            }
            if(logger != null)
                logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG, sb.ToString());
            if(w != null)
                w.WriteLine("RWLock[{0}]: {1}", this.Name, sb.ToString());
        }
        if((waitingUpgradeableReadThread != null) && (waitingUpgradeableReadThread.Count > 0)) {
            StringBuilder sb = new StringBuilder("waiting read:");
            foreach(Thread t in waitingUpgradeableReadThread) {
                sb.Append(' ');
                sb.Append(String.IsNullOrEmpty(t.Name)?t.ManagedThreadId.ToString():t.Name);
            }
            if(logger != null)
                logger.Log("RWLock["+this.Name+"]", OpeLog.Level.DEBUG, sb.ToString());
            if(w != null)
                w.WriteLine("RWLock[{0}]: {1}", this.Name, sb.ToString());
        }

    }

    /// <summary>
    ///   RWLockの一覧
    /// </summary>
    public static RWLock[] GlobalList {
        get {
            lock(globalRWLockList) {
                return globalRWLockList.ToArray();
            }
        }
    }

    /// <summary>
    ///   RWLockの一覧をデバッグダンプする
    /// </summary>
    public static void DumpAll(OpeLog logger, bool full=false) {
        dumpAll(logger, null, full);
    }

    /// <summary>
    ///   RWLockの一覧を出力する
    /// </summary>
    public static void DumpAll(TextWriter w, bool full=false) {
        dumpAll(null, w, full);
    }

    private static void dumpAll(OpeLog logger, TextWriter w, bool full) {
        RWLock[] list;
        lock(globalRWLockList) {
            list = globalRWLockList.ToArray();
        }
        Array.Sort(list, delegate(RWLock a, RWLock b){
                return a.Name.CompareTo(b.Name);
            });
        if(logger != null)
            logger.Log("RWLock", OpeLog.Level.DEBUG, "total {0} locks", list.Length);
        if(w != null)
            w.WriteLine("RWLock: total {0} locks", list.Length);
        foreach(RWLock rwlock in list) {
            rwlock.dump(logger, w, full);
        }
    }


    // 以下のメソッドは LockHandle派生クラスから呼び出されるためにpublicにして
    // いますが、それ以外の場所では呼び出さないでください

    public void doEnterReadLock() {
        if(waitingReadThread != null) {
            lock(waitingReadThread) {
                waitingReadThread.Add(Thread.CurrentThread);
            }
            EnterReadLock();
            lock(waitingReadThread) {
                waitingReadThread.Remove(Thread.CurrentThread);
            }
        } else {
            EnterReadLock();
        }
        if(readingThread != null) {
            lock(readingThread) {
                readingThread.Add(Thread.CurrentThread);
            }
        }
    }

    public void doExitReadLock() {
        if(readingThread != null) {
            lock(readingThread) {
                readingThread.Remove(Thread.CurrentThread);
            }
        }
        ExitReadLock();
    }

    public void doEnterWriteLock() {
        if(waitingWriteThread != null) {
            lock(waitingWriteThread) {
                waitingWriteThread.Add(Thread.CurrentThread);
            }
            EnterWriteLock();
            lock(waitingWriteThread) {
                waitingWriteThread.Remove(Thread.CurrentThread);
            }
        } else {
            EnterWriteLock();
        }
        if(writingThread != null) {
            lock(writingThread) {
                writingThread.Add(Thread.CurrentThread);
            }
        }
    }

    public void doExitWriteLock() {
        if(writingThread != null) {
            lock(writingThread) {
                writingThread.Remove(Thread.CurrentThread);
            }
        }
        ExitWriteLock();
    }

    public void doEnterUpgradeableReadLock() {
        if(waitingUpgradeableReadThread != null) {
            lock(waitingUpgradeableReadThread) {
                waitingUpgradeableReadThread.Add(Thread.CurrentThread);
            }
            EnterUpgradeableReadLock();
            lock(waitingUpgradeableReadThread) {
                waitingUpgradeableReadThread.Remove(Thread.CurrentThread);
            }
        } else {
            EnterUpgradeableReadLock();
        }
        if(upgradeableReadingThread != null) {
            lock(upgradeableReadingThread) {
                upgradeableReadingThread.Add(Thread.CurrentThread);
            }
        }
    }

    public void doExitUpgradeableReadLock() {
        if(upgradeableReadingThread != null) {
            lock(upgradeableReadingThread) {
                upgradeableReadingThread.Remove(Thread.CurrentThread);
            }
        }
        ExitUpgradeableReadLock();
    }


    private static List<RWLock> globalRWLockList = new List<RWLock>();

    private List<Thread> readingThread = null;
    private List<Thread> writingThread = null;
    private List<Thread> upgradeableReadingThread = null;
    private List<Thread> waitingReadThread = null;
    private List<Thread> waitingWriteThread = null;
    private List<Thread> waitingUpgradeableReadThread = null;
}

/// <summary>
///   ReaderWriterLockSlimを安全に用いるためのラッピングクラス
///   ReadLock, WriteLock, UpgradableLockクラスの共通基底
/// </summary>
public abstract class LockHandle : Loggable, IDisposable {

    public LockHandle(RWLock rwlock_) {
        rwlock = rwlock_;
    }

    ~LockHandle() {
        Dispose();
    }

    public abstract void Dispose();

    protected RWLock rwlock;
}

/// <summary>
///   RWLockに読み出しロックをかけるクラス
/// </summary>
public class ReadLock : LockHandle {

    public ReadLock(RWLock rwlock_) : base(rwlock_) {
        if(rwlock != null)
            rwlock.doEnterReadLock();
    }

    public override void Dispose() {
        if(rwlock != null) {
            rwlock.doExitReadLock();
            rwlock = null;
        }
    }

}

/// <summary>
///   RWLockに書き込みロックをかけるクラス
/// </summary>
public class WriteLock : LockHandle {

    public WriteLock(RWLock rwlock_) : base(rwlock_) {
        if(rwlock != null)
            rwlock.doEnterWriteLock();
    }

    public override void Dispose() {
        if(rwlock != null) {
            rwlock.doExitWriteLock();
            rwlock = null;
        }
    }

}

/// <summary>
///   RWLockに書き込み昇格可能な読み出しロックをかけるクラス
/// </summary>
public class UpgradeableReadLock : LockHandle {

    public UpgradeableReadLock(RWLock rwlock_) : base(rwlock_) {
        if(rwlock != null)
            rwlock.doEnterUpgradeableReadLock();
    }

    public override void Dispose() {
        if(rwlock != null) {
            rwlock.doExitUpgradeableReadLock();
            rwlock = null;
        }
    }

}

} // End of namespace
