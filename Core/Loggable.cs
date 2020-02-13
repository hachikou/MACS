/// Loggable: 操作ログ記録アプリケーション用基底クラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;

namespace MACS {

/// <summary>
///   操作ログ記録アプリケーション用基底クラス
/// </summary>
public abstract class Loggable {

    /// <summary>
    ///   非常事態ログ
    /// </summary>
    public void LOG_EMERG(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.EMERG, logMessage(msg, objs));
    }

    /// <summary>
    ///   警告ログ
    /// </summary>
    public void LOG_ALERT(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.ALERT, logMessage(msg, objs));
    }

    /// <summary>
    ///   致命的ログ
    /// </summary>
    public void LOG_CRIT(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.CRIT, logMessage(msg, objs));
    }

    /// <summary>
    ///   エラーログ
    /// </summary>
    public void LOG_ERR(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.ERR, logMessage(msg, objs));
    }

    /// <summary>
    ///   注意ログ
    /// </summary>
    public void LOG_WARNING(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.WARNING, logMessage(msg, objs));
    }

    /// <summary>
    ///   報告ログ
    /// </summary>
    public void LOG_NOTICE(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.NOTICE, logMessage(msg, objs));
    }

    /// <summary>
    ///   情報ログ
    /// </summary>
    public void LOG_INFO(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.INFO, logMessage(msg, objs));
    }

    /// <summary>
    ///   デバッグログ
    /// </summary>
    public void LOG_DEBUG(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.DEBUG, logMessage(msg, objs));
    }

    /// <summary>
    ///   例外のログ
    /// </summary>
    public void LOG_EXCEPTION(Exception e) {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.CRIT, GetExceptionMessage(e), false);
    }

    /// <summary>
    ///   スタックトレースをログ出力する
    /// </summary>
    public void LOG_STACKTRACE() {
        if(enableLogging)
            Logger.Log(GetCategoryName(), OpeLog.Level.DEBUG, GetStackTraceMessage(Environment.StackTrace), false);
    }

    /// <summary>
    ///   例外表示の文字列を得る
    /// </summary>
    public static string GetExceptionMessage(Exception e) {
        StringBuilder sb = new StringBuilder();
        string indent = "";
        while(e != null) {
            sb.Append(indent);
            sb.AppendFormat("{0}: {1}\n", e.GetType().Name, e.Message);
            getStackTraceMessage(sb, indent, e.StackTrace);
            e = e.InnerException;
            if(e != null) {
                sb.Append(indent);
                sb.Append("InnerException:\n");
                indent += "    ";
            }
        }
        return sb.ToString();
    }

    /// <summary>
    ///   スタックトレースの文字列を得る
    /// </summary>
    public static string GetStackTraceMessage(string st) {
        StringBuilder sb = new StringBuilder();
        getStackTraceMessage(sb, "", st);
        return sb.ToString();
    }
    
    private static void getStackTraceMessage(StringBuilder sb, string indent, string stacktrace) {
        sb.AppendFormat(indent);
        if(String.IsNullOrEmpty(stacktrace)) {
            sb.Append("StackTrace: none\n");
            return;
        }
        sb.AppendFormat("StackTrace:\n");
        foreach(string st in stacktrace.Split("\n".ToCharArray())) {
            sb.Append(indent+"    ");
            sb.Append(st.Replace('\r',' '));
            sb.Append('\n');
        }
    }
    
    /// <summary>
    ///   グローバルなログ書き込みファイルを登録する。
    /// </summary>
    public static void SetGlobalLogger(string filename, Encoding enc=null, int size=1000, int rotation=9) {
        globalLogger = new OpeLog(filename, enc, size, rotation);
    }

    /// <summary>
    ///   グローバルなログ書き込みオブジェクトを登録する。
    /// </summary>
    public static void SetGlobalLogger(OpeLog logger) {
        globalLogger = logger;
    }

    /// <summary>
    ///   グローバルロガー
    /// </summary>
    public static OpeLog GlobalLogger {
        get {
            if(globalLogger == null)
                globalLogger = new OpeLog("operation.log");
            return globalLogger;
        }
        set { globalLogger = value; }
    }


    /// <summary>
    ///   独自のロガーをセットする
    /// </summary>
    public void SetLogger(OpeLog logger_) {
        mylogger = logger_;
    }

    /// <summary>
    ///   独自のロガーをセットする
    /// </summary>
    public void SetLogger(string filename, Encoding enc=null, int size=1000, int rotation=9) {
        mylogger = new OpeLog(filename, enc, size, rotation);
    }

    /// <summary>
    ///   ロガー
    /// </summary>
    public OpeLog Logger {
        get { return (mylogger != null)?mylogger:GlobalLogger; }
    }

    /// <summary>
    ///   一時的にログ記録を停止する
    /// </summary>
    public void DisableLogging() {
        enableLogging = false;
    }

    /// <summary>
    ///   DisableLoggingで止めたログ記録を再開する
    /// </summary>
    public void EnableLogging() {
        enableLogging = true;
    }

    /// <summary>
    ///   動作ページ名を獲得する。
    ///   派生クラスでoverrideする事。
    /// </summary>
    protected virtual string GetPageName() {
        return null;
    }

    /// <summary>
    ///   操作ユーザー名を獲得する。
    ///   派生クラスでoverrideする事。
    /// </summary>
    protected virtual string GetUserName() {
        return null;
    }


    /// <summary>
    ///   デフォルトのログカテゴリ名を獲得する。
    ///   派生クラスでoverrideする事。
    /// </summary>
    protected virtual string GetCategoryName() {
        return GetType().Name;
    }


    private static OpeLog globalLogger = null;
    private OpeLog mylogger = null;
    private bool enableLogging = true;



    private string logMessage(string msg, object[] objs) {
        StringBuilder sb = new StringBuilder();
        string pagename = GetPageName();
        if(pagename != null){
            sb.Append("PAGE:");
            sb.Append(pagename);
            sb.Append(" ");
        }
        string username = GetUserName();
        if(username != null){
            sb.Append("USER:");
            sb.Append(username);
            sb.Append(" ");
        }
        if((objs == null) || (objs.Length == 0))
            sb.Append(msg);
        else
            sb.AppendFormat(msg, objs);
        return sb.ToString();
    }

}

} // End of namespace
