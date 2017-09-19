/**
 * 複数テーブルのダンプアウト
 * $Id: $
 *
 * Copyright (C) 2011-2012 Microbrains Inc. All rights reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Ionic.Zip;
using MACS;

namespace MACS.DB {

/// <summary>
///   DBテーブルバックアップクラス
/// </summary>
/// <remarks>
///   <para>
///     本クラスはIonic.Zip.dllを必要とします。
///   </para>
/// </remarks>
public class DBDump {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public DBDump() {}

    /// <summary>
    ///   テーブル名を指定したコンストラクタ
    /// </summary>
    public DBDump(params string[] tableNames) {
        foreach(string x in tableNames)
            tables.Add(x);
    }

    /// <summary>
    ///   対象テーブルを追加する
    /// </summary>
    public void AddTable(params string[] tableNames) {
        foreach(string x in tableNames)
            tables.Add(x);
    }

    /// <summary>
    ///   一時保存ファイルを作るディレクトリ
    /// </summary>
    public string TempDir = "/tmp";

    /// <summary>
    ///   動作状況をロギングするOpeLog
    /// </summary>
    public OpeLog Logger = null;

    /// <summary>
    ///   動作状況を書き出すTextWriter
    /// </summary>
    public TextWriter LogWriter = null;
    
    /// <summary>
    ///   バックアップファイルを作成する
    /// </summary>
    /// <param name="filepath">ファイル名</param>
    /// <param name="db">DB接続</param>
    /// <param name="password">ZIPパスワード</param>
    /// <param name="aesEncryption">AES符号化を用いるかどうか</param>
    public void Backup(string filepath, DBCon db, string password=null, bool aesEncryption=false) {
        db.LOG_DEBUG("Creating backup file {0}", filepath);
        using(Stream sw = FileUtil.BinaryWriter(filepath)) {
            if(sw == null)
                throw new IOException(String.Format("Can't open {0} for writing.", filepath));
            Backup(sw, db, password,aesEncryption);
        }
    }

    /// <summary>
    ///   バックアップファイルを指定ストリームに出力する
    /// </summary>
    /// <param name="sw">出力ストリーム</param>
    /// <param name="db">DB接続</param>
    /// <param name="password">ZIPパスワード</param>
    /// <param name="aesEncryption">AES符号化を用いるかどうか</param>
    public void Backup(Stream sw, DBCon db, string password=null, bool aesEncryption=false) {
        if(rnd == null)
            rnd = new Random();
        string postfix = "."+rnd.Next().ToString();
        try {
            timer.Restart();
            // まず一時ファイルにCSVダンプアウトする
            int tblcount = 0;
            foreach(string tblname in tables) {
                tblcount++;
                progress("dumping {0} ({1}/{2})", tblname, tblcount, tables.Count);
                string tmpfilepath = Path.Combine(TempDir, tblname+postfix);
                DBTable tbl = new DBTable(db, tblname);
                tbl.DumpCSV(tmpfilepath);
            }
            // それをZIPでまとめる
            using(ZipFile zip = new ZipFile()) {
                if(!String.IsNullOrEmpty(password)) {
                    zip.Password = password;
                    if(aesEncryption)
                        zip.Encryption = EncryptionAlgorithm.WinZipAes256;
                }
                tblcount = 0;
                foreach(string tblname in tables) {
                    tblcount++;
                    progress("compressing {0} ({1}/{2})", tblname, tblcount, tables.Count);
                    string tmpfilepath = Path.Combine(TempDir, tblname+postfix);
                    zip.AddFile(tmpfilepath).FileName = tblname;
                }
                progress("start saving");
                zip.Save(sw);
                progress("finished saving");
            }
        } finally {
            foreach(string tblname in tables) {
                FileInfo tmpfile = new FileInfo(Path.Combine(TempDir, tblname+postfix));
                if(tmpfile.Exists)
                    tmpfile.Delete();
            }
        }

    }

    /// <summary>
    ///   バックアップファイルの内容をチェックする
    /// </summary>
    /// <param name="filepath">ファイル名</param>
    /// <param name="db">DB接続</param>
    /// <param name="password">ZIPパスワード</param>
    /// <param name="aesEncryption">AES符号化を用いるかどうか</param>
    /// <remarks>
    ///   <para>
    ///     ファイルフォーマットが適切であるかがチェックされます。
    ///     各レコードのカラム数が適切であるかどうかはチェックされますが、データ
    ///     の内容が適切であるかどうか（数値であるべきカラムに文字列が入っている
    ///     など）はチェックされません。
    ///     不足するテーブルがあったり、逆に不要なテーブルがある場合でも、エラー
    ///     にはなりません。
    ///     ただし、ファイル内に必要なテーブルが一つもない場合には、エラーになり
    ///     ます。
    ///   </para>
    /// </remarks>
    public bool Check(string filepath, DBCon db, string password=null, bool aesEncryption=false) {
        db.LOG_DEBUG("Checking backup file {0}", filepath);
        using(Stream sr = FileUtil.BinaryReader(filepath)) {
            if(sr == null)
                throw new IOException(String.Format("Can't open {0} for reading.", filepath));
            return Check(sr, db, password, aesEncryption);
        }
    }

    /// <summary>
    ///   バックアップファイルの内容をチェックする
    /// </summary>
    /// <param name="sr">入力ストリーム</param>
    /// <param name="db">DB接続</param>
    /// <param name="password">ZIPパスワード</param>
    /// <param name="aesEncryption">AES符号化を用いるかどうか</param>
    public bool Check(Stream sr, DBCon db, string password=null, bool aesEncryption=false) {
        try {
            int count = load(sr, db, password, aesEncryption, true, false);
            if(count == 0) {
                db.LOG_NOTICE("Valid tables aren't found.");
                return false;
            }
            return true;
        } catch(Exception ex) {
            db.LOG_NOTICE("Bad backup file format: {0}", ex.Message);
            return false;
        }
    }

    /// <summary>
    ///   バックアップファイルから復元する
    /// </summary>
    /// <param name="filepath">ファイル名</param>
    /// <param name="db">DB接続</param>
    /// <param name="password">ZIPパスワード</param>
    /// <param name="aesEncryption">AES符号化を用いるかどうか</param>
    /// <param name="truncateFlag">テーブルを全てTruncateするかどうか</param>
    /// <returns>復元したテーブル数</returns>
    /// <remarks>
    ///   <para>
    ///     ファイル読み取りの前に、テーブルの全レコードが削除されます。
    ///     不適切なフォーマットのレコードは無視されます。
    ///   </para>
    /// </remarks>
    public int Load(string filepath, DBCon db, string password=null, bool aesEncryption=false, bool truncateFlag=false) {
        db.LOG_DEBUG("Loading backup file {0}", filepath);
        using(Stream sr = FileUtil.BinaryReader(filepath)) {
            if(sr == null)
                throw new IOException(String.Format("Can't open {0} for reading.", filepath));
            return Load(sr, db, password, aesEncryption, truncateFlag);
        }
    }

    /// <summary>
    ///   バックアップファイルから復元する
    /// </summary>
    /// <param name="sr">入力ストリーム</param>
    /// <param name="db">DB接続</param>
    /// <param name="password">ZIPパスワード</param>
    /// <param name="aesEncryption">AES符号化を用いるかどうか</param>
    /// <param name="truncateFlag">テーブルを全てTruncateするかどうか</param>
    /// <returns>復元したテーブル数</returns>
    public int Load(Stream sr, DBCon db, string password=null, bool aesEncryption=false, bool truncateFlag=false) {
        Stopwatch timer = new Stopwatch();
        bool oldUseDebugLog = db.UseDebugLog;
        bool oldUseConsoleLog = db.UseConsoleLog;
        db.UseDebugLog = false;
        db.UseConsoleLog = false;
        try {
            timer.Start();
            if(truncateFlag) {
                foreach(string x in tables) {
                    progress("truncating {0}", x);
                    DBTable tbl = new DBTable(db, x);
                    tbl.Truncate();
                }
            }
            return load(sr, db, password, aesEncryption, false, !truncateFlag);
            // ここでtruncateしているので、この先ではtruncateの必要が無いことに注意
        } finally {
            db.UseDebugLog = oldUseDebugLog;
            db.UseConsoleLog = oldUseConsoleLog;
        }
    }

    private int load(Stream sr, DBCon db, string password, bool aesEncryption, bool testRun, bool truncateFlag) {
        DirectoryInfo dir = null;
        try {
            dir = extractFiles(sr, password, aesEncryption);
            FileInfo[] files = dir.GetFiles();
            // ファイル内容チェック
            int fileCount = 0;
            int count = 0;
            foreach(FileInfo fi in files) {
                fileCount++;
                if(!tables.Contains(fi.Name)) {
                    progress("found {0} ({1}/{2}), skip it", fi.Name, fileCount, files.Length);
                    continue;
                }
                DBTable tbl = new DBTable(db, fi.Name);
                if(!testRun && truncateFlag) {
                    progress("truncationg {0}", fi.Name);
                    tbl.Truncate();
                }
                if(testRun)
                    progress("checking {0} ({1}/{2})", fi.Name, fileCount, files.Length);
                else
                    progress("loading {0} ({1}/{2})", fi.Name, fileCount, files.Length);
                int recs = tbl.LoadCSV(fi.FullName, !testRun, testRun);
                progress("table {0} has {1} records.", fi.Name, recs);
                count++;
            }
            return count;
        } finally {
            if((dir != null) && dir.Exists)
                dir.Delete(true);
        }
    }

    private DirectoryInfo extractFiles(Stream sr, string password, bool aesEncryption) {
        // 一時ディレクトリ作成
        if(rnd == null)
            rnd = new Random();
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(TempDir, "dbdump"+rnd.Next().ToString()));
        dir.Create();
        try {
            // ファイル展開
            using(ZipFile zip = ZipFile.Read(sr)) {
                foreach(ZipEntry e in zip) {
                    if(!String.IsNullOrEmpty(password)){
                        e.Password = password;
                        if(aesEncryption)
                            e.Encryption = EncryptionAlgorithm.WinZipAes256;
                    }
                    if(tables.Contains(e.FileName))
                        e.Extract(dir.FullName);
                }
            }
        } catch(Exception ex) {
            if(dir.Exists)
                dir.Delete(true);
            throw ex;
        }
        return dir;
    }

    private void progress(string fmt, params object[] args) {
        string msg = String.Format(fmt, args);
        if(Logger != null) {
            Logger.Log("DBDump Progress", OpeLog.Level.DEBUG, "{0,7:N0}msec {1}", timer.ElapsedMilliseconds, msg);
        }
        if(LogWriter != null) {
            LogWriter.WriteLine("DBDump Progress: {0,7:N0}msec {1}", timer.ElapsedMilliseconds, msg);
        }
    }


    private List<string> tables = new List<string>();
    private Stopwatch timer = new Stopwatch();

    private static Random rnd = null;
}

} // End of namespace
