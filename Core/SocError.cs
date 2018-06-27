/// SocError: Windows SocketException Error codes.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;

namespace MACS {

public class SocError {

    public const int EINTR = 10004;
    public const int EACCES = 10013;
    public const int EFAULT = 10014;
    public const int EINVAL = 10022;
    public const int EMFILE = 10024;
    public const int EWOULDBLOCK = 10035;
    public const int EINPROGRESS = 10036;
    public const int EALREADY = 10037;
    public const int ENOTSOCK = 10038;
    public const int EDESTADDRREQ = 10039;
    public const int EMSGSIZE = 10040;
    public const int EPROTOTYPE = 10041;
    public const int ENOPROTOOPT = 10042;
    public const int EPROTONOSUPPORT = 10043;
    public const int ESOCKTNOSUPPORT = 10044;
    public const int EOPNOTSUPP = 10045;
    public const int EPFNOSUPPORT = 10046;
    public const int EAFNOSUPPORT = 10047;
    public const int EADDRINUSE = 10048;
    public const int EADDRNOTAVAIL = 10049;
    public const int ENETDOWN = 10050;
    public const int ENETUNREACH = 10051;
    public const int ENETRESET = 10052;
    public const int ECONNABORTED = 10053;
    public const int ECONNRESET = 10054;
    public const int ENOBUFS = 10055;
    public const int EISCONN = 10056;
    public const int ENOTCONN = 10057;
    public const int ESHUTDOWN = 10058;
    public const int ETIMEDOUT = 10060;
    public const int ECONNREFUSED = 10061;
    public const int EHOSTDOWN = 10064;
    public const int EHOSTUNREACH = 10065;
    public const int EPROCLIM = 10067;
    public const int SYSNOTREADY = 10091;
    public const int VERNOTSUPPORTED = 10092;
    public const int NOTINITIALISED = 10093;
    public const int EDISCON = 10101;
    public const int TYPE_NOT_FOUND = 10109;
    public const int HOST_NOT_FOUND = 11001;
    public const int TRY_AGAIN = 11002;
    public const int NO_RECOVERY = 11003;
    public const int NO_DATA = 11004;

    public static string GetMessage(int code) {
        string msg;
        switch(code) {
        case EINTR:
            msg = "関数呼び出しに割り込みがありました。";
            break;
        case EACCES:
            msg = "アクセスは拒否されました。";
            break;
        case EFAULT:
            msg = "アドレスが正しくありません。";
            break;
        case EINVAL:
            msg = "無効な引数です。";
            break;
        case EMFILE:
            msg = "開いているファイルが多すぎます。";
            break;
        case EWOULDBLOCK:
            msg = "リソースが一時的に利用できなくなっています。";
            break;
        case EINPROGRESS:
            msg = "操作は現在実行中です。";
            break;
        case EALREADY:
            msg = "操作は既に実行中です。";
            break;
        case ENOTSOCK:
            msg = "非ソケットに対してソケット操作を試みました。";
            break;
        case EDESTADDRREQ:
            msg = "送信先のアドレスが必要です。";
            break;
        case EMSGSIZE:
            msg = "メッセージが長すぎます。";
            break;
        case EPROTOTYPE:
            msg = "プロトコルの種類がソケットに対して正しくありません。";
            break;
        case ENOPROTOOPT:
            msg = "プロトコルのオプションが正しくありません。";
            break;
        case EPROTONOSUPPORT:
            msg = "プロトコルがサポートされていません。";
            break;
        case ESOCKTNOSUPPORT:
            msg = "サポートされていないソケットの種類です。";
            break;
        case EOPNOTSUPP:
            msg = "操作がソケット上でサポートされていません。";
            break;
        case EPFNOSUPPORT:
            msg = "プロトコル ファミリがサポートされていません。";
            break;
        case EAFNOSUPPORT:
            msg = "プロトコル ファミリはアドレス ファミリをサポートしていません。";
            break;
        case EADDRINUSE:
            msg = "アドレスは既に使用中です。";
            break;
        case EADDRNOTAVAIL:
            msg = "要求されたアドレスを割り当てられません。";
            break;
        case ENETDOWN:
            msg = "ネットワークがダウンしています。";
            break;
        case ENETUNREACH:
            msg = "ICMP ネットワークに到達できません。";
            break;
        case ENETRESET:
            msg = "ネットワークがリセットされたため切断されました。";
            break;
        case ECONNABORTED:
            msg = "ソフトウェアによって接続が中止されました。";
            break;
        case ECONNRESET:
            msg = "ピアによって接続がリセットされました。";
            break;
        case ENOBUFS:
            msg = "バッファ領域がサポートされていません。";
            break;
        case EISCONN:
            msg = "ソケットは既に接続されています。";
            break;
        case ENOTCONN:
            msg = "ソケットが接続されていません。";
            break;
        case ESHUTDOWN:
            msg = "ソケットのシャットダウン後に送信できません。";
            break;
        case ETIMEDOUT:
            msg = "接続がタイムアウトになりました。";
            break;
        case ECONNREFUSED:
            msg = "接続が拒否されました。";
            break;
        case EHOSTDOWN:
            msg = "ホストがダウンしています。";
            break;
        case EHOSTUNREACH:
            msg = "ホストに到達するためのルートがありません。";
            break;
        case EPROCLIM:
            msg = "プロセスが多すぎます。";
            break;
        case SYSNOTREADY:
            msg = "ネットワーク サブシステムが利用できません。";
            break;
        case VERNOTSUPPORTED:
            msg = "このバージョンの Winsock.dll はサポートされていません。";
            break;
        case NOTINITIALISED:
            msg = "WSAStartup の実行に成功していません。";
            break;
        case EDISCON:
            msg = "正常なシャットダウン処理が進行中です。";
            break;
        case TYPE_NOT_FOUND:
            msg = "この種類のクラスが見つかりません。";
            break;
        case HOST_NOT_FOUND:
            msg = "ホストが見つかりません。そのようなホストはありません。";
            break;
        case TRY_AGAIN:
            msg = "ホストが見つかりません。権限を持つサーバーからの応答がありません。";
            break;
        case NO_RECOVERY:
            msg = "回復不可能なエラーが発生しました。";
            break;
        case NO_DATA:
            msg = "要求された名前は有効ですが、要求された種類のデータ レコードがありません。";
            break;

        default:
            msg = "システムエラー";
            break;
        }
        return String.Format("{0}({1})", msg, code);
    }

}

} // End of namespace
