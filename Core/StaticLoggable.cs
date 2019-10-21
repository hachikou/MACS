/// StaticLoggable: 操作ログ記録アプリケーション用基底クラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace MACS {

/// <summary>
///   操作ログ記録アプリケーション用基底クラス（staticメソッドタイプ）
/// </summary>
public abstract class StaticLoggable {

    /// <summary>
    ///   非常事態ログ
    /// </summary>
    public static void LOG_EMERG(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.EMERG, logMessage(msg, objs));
    }

    /// <summary>
    ///   警告ログ
    /// </summary>
    public static void LOG_ALERT(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.ALERT, logMessage(msg, objs));
    }

    /// <summary>
    ///   致命的ログ
    /// </summary>
    public static void LOG_CRIT(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.CRIT, logMessage(msg, objs));
    }

    /// <summary>
    ///   エラーログ
    /// </summary>
    public static void LOG_ERR(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.ERR, logMessage(msg, objs));
    }

    /// <summary>
    ///   注意ログ
    /// </summary>
    public static void LOG_WARNING(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.WARNING, logMessage(msg, objs));
    }

    /// <summary>
    ///   報告ログ
    /// </summary>
    public static void LOG_NOTICE(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.NOTICE, logMessage(msg, objs));
    }

    /// <summary>
    ///   情報ログ
    /// </summary>
    public static void LOG_INFO(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.INFO, logMessage(msg, objs));
    }

    /// <summary>
    ///   デバッグログ
    /// </summary>
    public static void LOG_DEBUG(string msg, params object[] objs) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.DEBUG, logMessage(msg, objs));
    }

    /// <summary>
    ///   例外ログ
    /// </summary>
    public static void LOG_EXCEPTION(Exception e) {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.CRIT, Loggable.GetExceptionMessage(e), false);
    }

    /// <summary>
    ///   スタックトレースをログ出力する
    /// </summary>
    public static void LOG_STACKTRACE() {
        if(enableLogging)
            Logger.Log(className(), OpeLog.Level.DEBUG, Loggable.GetStackTraceMessage(Environment.StackTrace), false);
    }

    /// <summary>
    ///   独自のロガーをセットする
    /// </summary>
    public static void SetLogger(OpeLog logger_) {
        mylogger = logger_;
    }

    /// <summary>
    ///   独自のロガーをセットする
    /// </summary>
    public static void SetLogger(string filename, Encoding enc=null, int size=1000, int rotation=9) {
        mylogger = new OpeLog(filename, enc, size, rotation);
    }

    /// <summary>
    ///   ロガー
    /// </summary>
    public static OpeLog Logger {
        get { return (mylogger!=null)?mylogger:Loggable.GlobalLogger; }
    }

    /// <summary>
    ///   一時的にログ記録を停止する
    /// </summary>
    public static void DisableLogging() {
        enableLogging = false;
    }

    /// <summary>
    ///   DisableLoggingで止めたログ記録を再開する
    /// </summary>
    public static void EnableLogging() {
        enableLogging = true;
    }


    private static OpeLog mylogger = null;
    private static bool enableLogging = true;

    private static string className() {
        StackTrace st = new StackTrace(false);
        if(st.FrameCount < 3)
            return "";
        StackFrame sf = st.GetFrame(2);
        MethodBase method = sf.GetMethod();
        Type type = method.DeclaringType;
        return type.Name;
    }

    private static string logMessage(string msg, object[] objs) {
        if((objs == null) || (objs.Length == 0))
            return msg;
        return String.Format(msg, objs);
    }

}

} // End of namespace
