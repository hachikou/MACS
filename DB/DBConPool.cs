/*! @file DBConPool.cs
 * @brief DB接続プール
 * $Id: $
 *
 * Copyright (C) 2015 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Data.Common;
using System.Diagnostics;
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
    public DBConPool(int poolsize, DBCon.Type dbtype, string server, string dbname, string user, string passwd) {
        init(poolsize);
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
        init(inifile.Get(prefix+"pool", 32));
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
    ///   プールサイズ
    /// </summary>
    public int PoolSize {
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
            for(int i = 0; i < pool.Length; i++) {
                Pool p = pool[i];
                if((p.dbcon != null) && !p.busy) {
                    p.dbcon.Dispose();
                    p.dbcon = null;
                }
            }
        }
    }

    internal DbConnection GetDbConnection() {
        lock(mutex) {
            for(int i = 0; i < pool.Length; i++) {
                Pool p = pool[i];
                if(p.dbcon == null) {
                    p.dbcon = new DBCon(DBType, Server, DBName, User, Passwd);
                    p.busy = true;
                    p.dbcon.CommandTimeout = CommandTimeout;
                    return p.dbcon.DBConnection;
                } else if(!p.busy) {
                    p.busy = true;
                    if(p.timer.ElapsedMilliseconds >= ReuseTimeout*1000) {
                        p.dbcon.Dispose();
                        p.dbcon = new DBCon(DBType, Server, DBName, User, Passwd);
                        p.dbcon.CommandTimeout = CommandTimeout;
                    }
                    return p.dbcon.DBConnection;
                }
            }
            throw new InvalidOperationException("No more DBCon pool.");
        }
    }

    internal void Free(DbConnection con) {
        lock(mutex) {
            for(int i = 0; i < pool.Length; i++) {
                Pool p = pool[i];
                if((p.dbcon != null) && (p.dbcon.DBConnection == con)) {
                    p.busy = false;
                    p.timer.Restart();
                    break;
                }
            }
        }
    }


    private class Pool {
        public DBCon dbcon = null;
        public bool busy = false;
        public Stopwatch timer = new Stopwatch();
    }

    private Pool[] pool;
    private object mutex = new object();

    private void init(int poolsize) {
        pool = new Pool[poolsize];
        for(int i = 0; i < poolsize; i++) {
            pool[i] = new Pool();
        }
    }
}

} // End of namespace
