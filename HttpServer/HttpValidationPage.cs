/*! @file HttpValidationPage.cs
 * @brief 入力バリデーションをサポートするクラス
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   入力バリデーションをサポートするクラス
/// </summary>
public abstract class HttpValidationPage : HttpTemplatePage {

    /// <summary>
    ///   txtの文字数がminlength以上maxlength以下であることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="minlength">最小の文字数</param>
    /// <param name="maxlength">最大の文字数</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にし、
    ///     ErrorMessageをセットする。
    ///   </para>
    /// </remarks>
    protected string ValidateLength(string txt, int minlength, int maxlength, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        if((minlength > 0) && (txt.Length == 0)) {
            AddValidationMessage(string.Format(_("{0}は必須です。"),fieldname),item);
            goto fail;
        }
        if((txt.Length < minlength) || (txt.Length > maxlength)) {
            AddValidationMessage(string.Format(_("{0}は{1}文字以上{2}文字以下でなければいけません。"), fieldname, minlength, maxlength),item);
            goto fail;
        }
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return txt;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return "";
    }

    /// <summary>
    ///   txtの文字数がencで示されるエンコーディングにおいてminlengthバイト以上
    ///   maxlengthバイト以下であることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="minlength">最小のバイト数</param>
    /// <param name="maxlength">最大のバイト数</param>
    /// <param name="enc">エンコーディング</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected string ValidateByteLength(string txt, int minlength, int maxlength, Encoding enc, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        int len = enc.GetBytes(txt).Length;
        if((minlength > 0) && (len == 0)) {
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }
        if((len < minlength) || (len > maxlength)) {
            AddValidationMessage(string.Format(_("{0}は{1}バイト以上{2}バイト以下でなければいけません。"), fieldname, minlength, maxlength),item);
            goto fail;
        }
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return txt;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return "";
    }

    /// <summary>
    ///   txtがminvalue以上maxvalue以下の数値であることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="minvalue">最小値</param>
    /// <param name="maxvalue">最大値</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <param name="defval">エラー時や未指定時に返す値</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected int ValidateInt(string txt, int minvalue, int maxvalue, bool required, string fieldname, WebControl item, int defval=0) {
        if (string.IsNullOrEmpty(txt)) {
            if(required) {
                AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
                goto fail;
            }
            if(item != null) {
                item.RemoveCssClass("error");
            }
            return defval;
        }
        bool sign = false;
        int i = 0, ret = 0;
        if (txt[i] == '-') {
            ++i;
            sign = true;
        } else if (txt[i] == '+') {
            ++i;
        }
        for(; i < txt.Length; ++i) {
            var ch = txt[i];
            if ('0' <= ch && ch <= '9')
                ret = ret * 10 + ((byte)ch - (byte)'0');
            else if (ch != ',') {
                // カンマは無視する
                AddValidationMessage(string.Format(_("{0}は数値でない文字が含まれています。"), fieldname), item);
                goto fail;
            }
        }
        if (sign)
            ret *= -1;

        if (ret < minvalue || maxvalue < ret) {
            AddValidationMessage(string.Format(_("{0}は{1}以上{2}以下でなければいけません。"), fieldname, minvalue, maxvalue),item);
            goto fail;
        }

        if(item != null) {
            item.RemoveCssClass("error");
        }
        return ret;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return defval;
    }

    /// <summary>
    ///   txtがminvalue以上maxvalue以下の数値であることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="minvalue">最小値</param>
    /// <param name="maxvalue">最大値</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected uint ValidateUInt(string txt, uint minvalue,uint maxvalue, bool  required, string fieldname, WebControl item) {
        if (string.IsNullOrEmpty(txt)) {
            if (required) {
                AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
                goto fail;
            }
            if (item != null) {
                item.RemoveCssClass("error");
            }
            return 0;
        }

        uint ret = 0;
        foreach(var ch in txt) {
            if ('0' <= ch && ch <= '9')
                ret = ret * 10 + ((uint)ch - (uint)'0');
            else if (ch != ',') {
                // カンマは無視する
                AddValidationMessage(string.Format(_("{0}は数値でない文字が含まれています。"), fieldname), item);
                goto fail;
            }
        }
        if (ret < minvalue || maxvalue < ret) {
            AddValidationMessage(string.Format(_("{0}は{1}以上{2}以下でなければいけません。"), fieldname, minvalue, maxvalue),item);
            goto fail;
        }
        return ret;

    fail:
        if (item != null)
            item.AddCssClass("error");
        return 0;
    }

    /// <summary>
    ///   txtがkeysで指定した文字列のいずれかであることを確認する。
    ///   本関数は、キーに対応するオブジェクトを返す。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="keys">文字列の配列</param>
    /// <param name="values">本関数の戻り値となる値の配列</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///
    ///     エラー時には nullを返す。
    ///   </para>
    /// </remarks>
    protected object ValidateSelection(string txt, string[] keys, Array values, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        int i = 0;
        foreach(string k in keys){
            if(k == txt) {
                if(item != null) {
                    item.RemoveCssClass("error");
                }
                return values.GetValue(i);
            }
            i++;
        }

        AddValidationMessage(string.Format(_("{0}に正しい値が選択されていません。"), fieldname),item);
        if(item != null) {
            item.AddCssClass("error");
        }
        return null;
    }

    /// <summary>
    ///   txtの文字列がIPアドレスの形式をしていることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected Ipaddr ValidateIpaddr(string txt, bool required, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        if(required && (txt == "")) {
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }
        Ipaddr a = new Ipaddr(txt);
        if(!a.IsCompleteOrNull()) {
            AddValidationMessage(string.Format(_("{0}はnnn.nnn.nnn.nnnの形式でなければいけません。"), fieldname),item);
            goto fail;
        }
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return a;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return new Ipaddr();
    }

    /// <summary>
    ///   txtの文字列がIPv4アドレスの形式をしていることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected Ipaddr ValidateIp4addr(string txt, bool required, string fieldname, WebControl item) {
        if (string.IsNullOrEmpty(txt)) {
            if (required) {
                AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
                goto fail;
            }
            if (item != null) {
                item.RemoveCssClass("error");
            }
            return new Ipaddr();
        }

        int pos = 0;
        bool has_number = false;
        byte[] bytes = new byte[] {(byte)0,(byte)0,(byte)0,(byte)0};
        foreach(var ch in txt) {
            if (ch == '.') {
                if (!has_number)
                    goto parsefail;
                if (++pos >= 4)
                    goto parsefail;
                has_number = false;
            } else if ('0' <= ch && ch <= '9') {
                var i = (bytes[pos] * 10 + ((byte)ch) - ((byte)'0'));
                if (i > 255)
                    goto fail;
                bytes[pos] = (byte)i;
                has_number = true;
            } else
                goto parsefail;
        }
        if (pos < 3 || !has_number)
            goto parsefail;
        if (item != null) {
            item.RemoveCssClass("error");
        }
        return new Ipaddr(bytes, 0, bytes.Length);

    parsefail:
        AddValidationMessage(string.Format(_("{0}はnnn.nnn.nnn.nnnの形式でなければいけません。"), fieldname),item);
        goto fail;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return new Ipaddr();
    }

    /// <summary>
    ///   txtの文字列がネットマスク形式をしていることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected Ipaddr ValidateNetmask(string txt, bool required, string fieldname, WebControl item) {
        Ipaddr a;
        if(String.IsNullOrEmpty(txt)) {
            if(required) {
                AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
                goto fail;
            } else {
                a = new Ipaddr();
                goto ok;
            }
        }
        if(txt.StartsWith("/")) {
            a = Ipaddr.GetNetmask(StringUtil.ToInt(txt.Substring(1)));
        } else {
            a = new Ipaddr(txt);
        }
        if(!a.IsCompleteOrNull()) {
            AddValidationMessage(string.Format(_("{0}は「nnn.nnn.nnn.nnn」の形式または「/マスクビット数」でなければいけません。"), fieldname),item);
            goto fail;
        }
        if(!a.IsNetmask()){
            AddValidationMessage(string.Format(_("{0}がネットマスク形式になっていません。"), fieldname),item);
            goto fail;
        }
    ok:
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return a;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return new Ipaddr();
    }

    /// <summary>
    ///   txtの文字列がMACアドレスの形式をしていることを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected Hwaddr ValidateHwaddr(string txt, bool required, string fieldname, WebControl item) {
        if (string.IsNullOrEmpty(txt)) {
            if (required) {
                AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
                goto fail;
            }
            if(item != null) {
                item.RemoveCssClass("error");
            }
            return new Hwaddr();
        }

        if (txt.Length != 17)
            goto parsefail;
        int pos = 0;
        byte[] bytes = new byte[] {(byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0};
        for(var i = 0; i < txt.Length; ++i) {
            var ch = txt[i];
            if (i % 3 < 2) {
                if ('0' <= ch && ch <= '9')
                    bytes[pos] = (byte)(bytes[pos] * 16 + ((byte)ch - (byte)'0'));
                else if ('A' <= ch && ch <= 'F')
                    bytes[pos] = (byte)(bytes[pos] * 16 + ((byte)ch - (byte)'A') + 10);
                else if ('a' <= ch && ch <= 'f')
                    bytes[pos] = (byte)(bytes[pos] * 16 + ((byte)ch - (byte)'a') + 10);
                else
                    goto parsefail;
            } else {
                if (ch != ':' && ch != '-' && ch != ' ')
                    goto parsefail;
                ++pos;
            }
        }

        if(item != null) {
            item.RemoveCssClass("error");
        }
        return new Hwaddr(bytes, 0, bytes.Length);

    parsefail:
        AddValidationMessage(string.Format(_("{0}はXX:XX:XX:XX:XX:XXの形式でなければいけません。"), fieldname),item);
        goto fail;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return new Hwaddr();
    }

    /// <summary>
    ///   txtがドメイン名として有効であるかどうかを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected string ValidateDomain(string txt, bool required, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        txt = txt.Trim();
        int len = txt.Length;
        if(required && (len == 0)) {
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }
        if(len > 255) {
            AddValidationMessage(string.Format(_("{0}が長すぎます。"), fieldname),item);
            goto fail;
        }
        if(len > 0) {
            string[] labels = txt.Split(".".ToCharArray());
            foreach(string i in labels){
                if((i.Length == 0) || (i.Length > 63)){
                    AddValidationMessage(string.Format(_("{0}がドメイン名の形式になっていません。"), fieldname),item);
                    goto fail;
                }
                foreach(char c in i){
                    if(!Char.IsLetterOrDigit(c) && (c != '-')){
                        AddValidationMessage(string.Format(_("{0}にドメイン名として使用できない文字が含まれています。"), fieldname),item);
                        goto fail;
                    }
                }
            }
        }
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return txt;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return "";
    }

    /// <summary>
    ///   txtがメールアドレスとして有効であるかどうかを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected string ValidateMailAddr(string txt, bool required, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        txt = txt.Trim();
        int len = txt.Length;
        if(required && (len == 0)) {
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }
        if(len > 0){
            string[] parts = txt.Split("@".ToCharArray());
            if(parts.Length != 2){
                AddValidationMessage(string.Format(_("{0}はメールアドレス形式でなければいけません。"), fieldname),item);
                goto fail;
            }
            Regex r = new Regex(@"^[a-zA-Z0-9\!\#\$\%\&\\\*\+\-\/\=\?\^_\`\{\|\}\~\.]+$");
            if(!r.IsMatch(parts[0])) {
                AddValidationMessage(string.Format(_("{0}にはメールアドレスに使用できない文字が含まれています。"), fieldname),item);
                goto fail;
            }
            if(ValidateDomain(parts[1], true, fieldname, item) == "")
                goto fail;
        }
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return txt;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return "";
    }

    /// <summary>
    ///   txtがメールアドレスとして有効であるかどうかを確認する。
    ///
    ///   RFC2821 ESMTP 4.5.3.1 Size limits and minimums
    ///   ユーザ部:64文字  ドメイン部:255文字  全体:256文字
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="totalLength">txtの長さチェック</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <param name="localLength">ローカルパートの文字数のチェック（0以下のときは、制限しない）</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected string ValidateMailAddr(string txt, bool required, int totalLength, string fieldname, WebControl item, int localLength = 0) {
        if (string.IsNullOrEmpty(txt)) {
            if (required) {
                if (item != null) {
                    item.RemoveCssClass("error");
                }
                return "";
            }
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }

        if (totalLength > 0 && txt.Length > totalLength) {
            AddValidationMessage(string.Format(_("{0}は{1}以下でなければいけません。"), fieldname, totalLength), item);
            goto fail;
        }

        string[] parts = txt.Split('@');
        if(parts.Length != 2){
            AddValidationMessage(string.Format(_("{0}はメールアドレス形式でなければいけません。"), fieldname),item);
            goto fail;
        }
        if (localLength > 0 && parts[0].Length > localLength) {
            AddValidationMessage(string.Format(_("{0}のメールアドレスのユーザ部は{1}以下でなければいけません。"), fieldname, localLength),item);
            goto fail;
        }

        Regex r = new Regex(@"^[a-zA-Z0-9\!\#\$\%\&\\\*\+\-\/\=\?\^_\`\{\|\}\~\.]+$");
        if(!r.IsMatch(parts[0])) {
            AddValidationMessage(string.Format(_("{0}にはメールアドレスに使用できない文字が含まれています。"), fieldname),item);
            goto fail;
        }
        if(ValidateDomain(parts[1], true, fieldname, item) == "")
            goto fail;

        if(item != null) {
            item.RemoveCssClass("error");
        }
        return txt;

    fail:
        if (item != null) {
            item.AddCssClass("error");
        }
        return "";
    }

    /// <summary>
    ///   txtの文字列が年月日時分秒の形式をしているかチェックする。
    ///   さらに、UNIX timeの値域に入っていることをチェックする。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected UnixTime ValidateUnixTime(string txt, bool required, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        txt = txt.Trim();
        if(required && (txt == "")) {
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }
        UnixTime x = new UnixTime(txt);
        if((txt != "") && x.IsNone()){
            AddValidationMessage(string.Format(_("{0}の書式が正しくありません。"), fieldname),item);
            goto fail;
        }

        if(item != null) {
            item.RemoveCssClass("error");
        }
        return x;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return UnixTime.None;
    }

    /// <summary>
    ///   txtがASCII文字列として有効であるかどうかを確認する。
    /// </summary>
    /// <param name="txt">確認する文字列</param>
    /// <param name="required">入力が必須かどうか</param>
    /// <param name="fieldname">入力欄の名称</param>
    /// <param name="item">フォーム要素に対応するWebControl</param>
    /// <remarks>
    ///   <para>
    ///     条件を満たさない時は m_validation_messageにエラーメッセージをセットする。
    ///
    ///     itemを指定しておくと、エラー時にそのitemのCssClassを"error"にする。
    ///   </para>
    /// </remarks>
    protected string ValidateASCIIString(string txt, bool required, string fieldname, WebControl item) {
        if(txt == null)
            txt = "";
        txt = txt.Trim();
        int len = txt.Length;
        if(required && (len == 0)) {
            AddValidationMessage(string.Format(_("{0}は必須です。"), fieldname),item);
            goto fail;
        }
        if(len > 0){ 
            Regex r = new Regex(@"^[a-zA-Z0-9\!""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^_\`\{\|\}\~]+$");
            if(!r.IsMatch(txt) {
                AddValidationMessage(string.Format(_("{0}にはASCII文字以外の使用できない文字が含まれています。"), fieldname),item);
                goto fail;
            }
        }
        if(item != null) {
            item.RemoveCssClass("error");
        }
        return txt;

    fail:
        if(item != null) {
            item.AddCssClass("error");
        }
        return "";
    }

    /// <summary>
    ///   WebControlフィールド全部にInLineErrorフラグを立てる
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     先にAssignWebControlsを実行するなどしてWebControlインスタンスの実体
    ///     を生成しておくこと。
    ///   </para>
    /// </remarks>
    protected void UseInLineError() {
        foreach(FieldInfo fi in GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)){
            if(!fi.FieldType.IsSubclassOf(typeof(WebControl)))
                continue;
            WebControl i = (WebControl)(fi.GetValue(this));
            if(i == null)
                continue;
            i.InLineError = true;
        }
    }


    /// <summary>
    ///   バリデーションエラーが発生しているかどうか
    /// </summary>
    protected bool IsValidationError {
        get {
            return (m_validation_message != null) && (m_validation_message != "");
        }
    }

    /// <summary>
    ///   バリデーションメッセージ
    /// </summary>
    protected string ValidationMessage {
        get { return (m_validation_message == null)?"":m_validation_message; }
    }

    /// <summary>
    ///   バリデーションメッセージに追記する
    /// </summary>
    protected void AddValidationMessage(string msg, WebControl item) {
        if(String.IsNullOrEmpty(m_validation_message))
            m_validation_message = msg;
        else
            m_validation_message += "<br/>"+msg;
        if(item != null) {
            if(String.IsNullOrEmpty(item.ErrorMessage))
                item.ErrorMessage = msg;
            else
                item.ErrorMessage += msg;
        }
    }

    /// <summary>
    ///   バリデーションメッセージ
    /// </summary>
    private string m_validation_message;

}

} // End of namespace
