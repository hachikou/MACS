/*! @file DBConPool.cs
 * @brief DB接続プール
 * $Id: $
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using MACS;

namespace MACS.DB {


/// <summary>
///   DBConのプールを提供するクラス
/// </summary>
public class DBConPool : Loggable, IDisposable {

    /// <summary>
    ///   DB接続プールを用意する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DBとの接続は必要になるまで行なわれません。
    ///   </para>
    /// </remarks>
    public DBConPool(int initsize, int maxsize, DBCon.Type dbtype, string server, string dbname, string user, string passwd) {
        init(initsize, maxsize);
        DBType = dbtype;
        Server = server;
        DBName = dbname;
        User = user;
        Passwd = passwd;
    }

    /// <summary>
    ///   DB接続プールを用意する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DBとの接続は必要になるまで行なわれません。
    ///   </para>
    /// </remarks>
    public DBConPool(IniFile inifile, string prefix="db") {
        init(inifile.Get(prefix+"pool", 32), inifile.Get(prefix+"poolmax", 64));
        DBType = DBCon.ToType(inifile.Get(prefix+"engine",""));
        Server = inifile.Get(prefix+"server", "localhost");
        DBName = inifile.Get(prefix+"name", "dbname");
        User = inifile.Get(prefix+"user", "username");
        Passwd = inifile.Get(prefix+"pass", "password");
        CommandTimeout = inifile.Get(prefix+"timeout", 180);
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~DBConPool() {
        Dispose();
    }

    /// <summary>
    ///   使用リソースを解放する。
    /// </summary>
    public void Dispose() {
        for(int i = 0; i < pool.Length; i++) {
            if(pool[i].dbcon != null) {
                pool[i].dbcon.Dispose();
                pool[i].dbcon = null;
            }
        }
    }

    /// <summary>
    ///   最大プールサイズ
    /// </summary>
    public int MaxPoolSize {
        get { return pool.Length; }
    }

    /// <summary>
    ///   空きDBConプール数
    /// </summary>
    public int FreeCount {
        get {
            lock(mutex) {
                int c = 0;
                for(int i = 0; i < pool.Length; i++) {
                    if(!pool[i].busy)
                        c++;
                }
                return c;
            }
        }
    }

    /// <summary>
    ///   接続中DBCon数
    /// </summary>
    public int ConnectCount {
        get {
            lock(mutex) {
                return connectCount;
            }
        }
    }

    /// <summary>
    ///   現在までの最大同時接続数
    /// </summary>
    public int MaxConnectCount {
        get {
            lock(mutex) {
                return maxConnectCount;
            }
        }
    }
    
    /// <summary>
    ///   DBエンジン種別
    /// </summary>
    public readonly DBCon.Type DBType;

    /// <summary>
    ///   サーバ名
    /// </summary>
    public readonly string Server;

    /// <summary>
    ///   DB名
    /// </summary>
    public readonly string DBName;

    /// <summary>
    ///   DBユーザ
    /// </summary>
    public readonly string User;

    /// <summary>
    ///   パスワード
    /// </summary>
    internal readonly string Passwd;

    /// <summary>
    ///   SQL実行タイムアウト時間（秒）
    /// </summary>
    public int CommandTimeout = 180;

    /// <summary>
    ///   接続再利用可能時間（秒）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     利用し終えてからこの時間以上経過している接続は使わない（再接続する）。
    ///   </para>
    /// </remarks>
    public int ReuseTimeout = 60;

    /// <summary>
    ///   プールからDBConを取り出す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     必要に応じてDBとの接続が行なわれます。
    ///     以前接続して今は使っていない接続があれば、それを再利用します。
    ///     プールサイズが0のときは、常に新しいDB接続を作成します。
    ///   </para>
    /// </remarks>
    public DBCon Get() {
        return new DBCon(this);
    }

    /// <summary>
    ///   DBとの接続を切断する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     プール中の現在使用されていないDB接続を切断します。
    ///     現在使用中のDB接続は切断されません。
    ///   </para>
    /// </remarks>
    public void CloseAll() {
        lock(mutex) {
            purge(true);
        }
    }

    /// <summary>
    ///   接続中のDBConの状態を一覧表示する
    /// </summary>
    public void DumpStatus(Loggable logger=null) {
        DumpStatus((logger??this).Logger);
    }
    
    /// <summary>
    ///   接続中のDBConの状態を一覧表示する
    /// </summary>
    public void DumpStatus(OpeLog logger) {
        lock(mutex) {
            dumpStatus(logger, null);
        }
    }
    
    /// <summary>
    ///   接続中のDBConの状態を一覧表示する
    /// </summary>
    public void DumpStatus(TextWriter writer) {
        lock(mutex) {
            dumpStatus(null, writer);
        }
    }
    
    
    internal DbConnection GetDbConnection() {
        lock(mutex) {
            // まず有効期限切れDBConをDisposeする
            purge(false);
            // 空いているDBConを探す
            foreach(Pool p in pool) {
                if(p.dbcon == null) {
                    p.dbcon = new DBCon(DBType, Server, DBName, User, Passwd);
                    p.busy = true;
                    p.dbcon.CommandTimeout = CommandTimeout;
                    return p.dbcon.DBConnection;
                } else if(!p.busy) {
                    p.busy = true;
                    return p.dbcon.DBConnection;
                }
            }
            LOG_ERR("All DBCon pool ({0}) has been exhausted.", MaxPoolSize);
            dumpStatus(this.Logger, null);
            throw new InvalidOperationException("No more DBCon pool.");
        }
    }

    internal void Free(DbConnection con) {
        lock(mutex) {
            for(int i = 0; i < pool.Length; i++) {
                Pool p = pool[i];
                if((p.dbcon != null) && (p.dbcon.DBConnection == con)) {
                    NThread thread = NThread.CurrentThread;
                    if((thread != null) && thread.IsAborting) {
                        p.dbcon.Dispose();
                        p.dbcon = null;
                    }
                    p.busy = false;
                    p.timer.Restart();
                    break;
                }
            }
        }
    }


    private class Pool {
        public readonly int id;
        public DBCon dbcon = null;
        public bool busy = false;
        public Stopwatch timer = new Stopwatch();

        public Pool(int id_) {
            id = id_;
        }
    }
    
    private Pool[] pool;
    private int connectCount;
    private int maxConnectCount;
    private object mutex = new object();

    private void init(int initsize, int maxsize) {
        // Poolは大したサイズではないので、最初からmaxsize分の配列を作ってしまう
        // initsizeは現在のところ無視。
        pool = new Pool[maxsize];
        for(int i = 0; i < pool.Length; i++) {
            pool[i] = new Pool(i);
        }
        connectCount = 0;
        maxConnectCount = 0;
    }

    private void purge(bool force) {
        int count = 0; // 接続中のカウントをついでに行なう
        foreach(Pool p in pool) {
            if(p.busy) {
                count++;
            } else if((p.dbcon != null) && (force
                                            || (p.timer.ElapsedMilliseconds >= ReuseTimeout*1000)
                                            || (p.dbcon.DBConnection == null)
                                            || (p.dbcon.DBConnection.State != ConnectionState.Open))) {
                p.dbcon.Dispose();
                p.dbcon = null;
                p.busy = false;
            }
        }
        connectCount = count;
        if(maxConnectCount < count)
            maxConnectCount = count;
    }

    private void dumpStatus(OpeLog logger, TextWriter writer) {
        int count = 0;
        for(int i = 0; i < pool.Length; i++) {
            Pool p = pool[i];
            string stat = null;
            if(p.busy) {
                if(p.dbcon == null)
                    stat = "corrupted (busy but disposed)";
                else if(p.dbcon.DBConnection == null)
                    stat = "corrupted (busy but no DB connection)";
                else if(p.dbcon.DBConnection.State != ConnectionState.Open)
                    stat = "corrupted (busy but connection is not open)";
                else
                    stat = "busy, SQL='"+(p.dbcon.CurrentSQL??"")+"'";
            } else if(p.dbcon != null) {
                if(p.timer.ElapsedMilliseconds >= ReuseTimeout*1000)
                    stat = String.Format("expired ({0}msec), waiting for dispose", p.timer.ElapsedMilliseconds);
                else if(p.dbcon.DBConnection == null)
                    stat = "connection closed, waiting for dispose";
                else if(p.dbcon.DBConnection.State != ConnectionState.Open)
                    stat = "invalid connection state "+p.dbcon.DBConnection.State.ToString();
                else
                    stat = String.Format("free ({0}msec)", p.timer.ElapsedMilliseconds);
            }
            if(stat != null) {
                string title = String.Format("DBConPool[{0}:{1}]#{2:D4}", this.Server, this.DBName, i);
                if(logger != null)
                    logger.Log(title, OpeLog.Level.DEBUG, stat);
                if(writer != null)
                    writer.WriteLine(title+": "+stat);
                count++;
            }
        }
        if(count == 0) {
            string title = String.Format("DBConPool[{0}:{1}]", this.Server, this.DBName);
            string msg = "No alive connection pools.";
            if(logger != null)
                logger.Log(title, OpeLog.Level.DEBUG, msg);
            if(writer != null)
                writer.WriteLine(title+": "+msg);
        }
    }

}

} // End of namespace
