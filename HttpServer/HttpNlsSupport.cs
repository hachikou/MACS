/// HttpNlsSupport: 多言語化サポート用基底クラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   多言語化サポート用基底クラス。
/// </summary>
public abstract class HttpNlsSupport : HttpBuiltinContentsSupport, Translatable {

    /// <summary>
    ///   デフォルトの言語指定
    /// </summary>
    public static string DefaultLanguage = null;


    /// <summary>
    ///   初期化
    /// </summary>
    public HttpNlsSupport() {
        SetLanguage(DefaultLanguage);
    }

    /// <summary>
    ///   コンテンツの言語コードを指定する。
    /// </summary>
    /// <param name="lang">言語コード名("ja", "en"など）</param>
    /// <remarks>
    ///   <para>
    ///     langにnullまたは空文字列を指定すると多言語サポートがオフになる。
    ///     また、"auto"を指定すると、ブラウザの要求する言語コードに合わせる。
    ///   </para>
    /// </remarks>
    public void SetLanguage(string lang) {
        if(string.IsNullOrEmpty(lang)){
            m_langselect = false;
            m_autolang = false;
            m_lang = null;
        } else {
            m_langselect = true;
            if(lang == "auto"){
                m_autolang = true;
                m_lang = null;
            } else {
                m_autolang = false;
                m_lang = lang;
            }
        }
    }


    /// <summary>
    ///   多言語化ファイル名を獲得する
    /// </summary>
    /// <param name="path">ファイル名</param>
    /// <returns>多言語化ファイル名。多言語化無効時や多言語化ファイルが見つからない場合にはpathが返る</returns>
    /// <remarks>
    ///   <para>
    ///     SetLanguage("auto") が指定されている場合、HTTPリクエスト内の
    ///     クライアントの要求言語を確認し、ファイル名の末尾に".言語コード"を
    ///     付けたファイルがあればそれを返します。
    ///
    ///     SetLanguage(string) で言語コードが指定されている場合、ファイル名の
    ///     末尾に".言語コード"を付けたファイルがあればそれを返します。
    ///
    ///     要求言語が無い場合や、".言語コード"を付加したファイルが無い場合には
    ///     指定されたファイルを返します。
    ///   </para>
    /// </remarks>
    public string GetNlsFileName(string path) {
        if(!m_langselect)
            return path;
        if(!m_autolang) {
            string f = path+"."+m_lang;
            if(FileExists(f))
                return f;
            return path;
        }

        // ブラウザのリクエストによるファイル選択
        if(Request.UserLanguages == null)
            return path;
        string q = "q=0";
        foreach(string lang in Request.UserLanguages){
            string[] lq = lang.Split(";".ToCharArray(),2);
            string lcode = lq[0].Trim();
            string quality = (lq.Length > 1)?lq[1].Trim():"";
            while(lcode.Length > 0) {
                string f = path+"."+lcode;
                if(FileExists(f)){
                    if(quality != "") {
                        if(q.CompareTo(quality) < 0){
                            path = f;
                            q = quality;
                        }
                    }else{
                        path = f;
                        return path;
                    }
                }
                int i = lcode.LastIndexOf('-');
                if(i < 0)
                    break;
                lcode = lcode.Substring(0,i);
            }
        }
        return path;
    }

    /// <summary>
    ///   ブラウザのリクエストによる言語コードを得る
    /// </summary>
    /// <param name="langlist">サポートする言語コードの一覧</param>
    /// <remarks>
    ///   <para>
    ///     langlistで指定した言語コードの中で、ブラウザが要求している言語に一番
    ///     近いものを探して返す。
    ///     ブラウザが要求言語を指定していない場合やブラウザの要求にマッチする
    //      サポート言語が無い場合にはnullを返す。
    ///   </para>
    /// </remarks>
    public string GetNlsCode(string[] langlist) {
        if((Request.UserLanguages == null) || (langlist == null))
            return null;
        string qlang = null;
        string q = "q=0";
        foreach(string lang in Request.UserLanguages){
            string[] lq = lang.Split(";".ToCharArray(),2);
            string lcode = lq[0].Trim();
            string quality = (lq.Length > 1)?lq[1].Trim():"";
            while(lcode.Length > 0) {
                bool exist = false;
                foreach(string l in langlist) {
                    if(l == lcode) {
                        exist = true;
                        break;
                    }
                }
                if(exist){
                    if(quality != "") {
                        if(q.CompareTo(quality) < 0){
                            qlang = lcode;
                            q = quality;
                        }
                    }else{
                        qlang = lcode;
                        return qlang;
                    }
                }
                int i = lcode.LastIndexOf('-');
                if(i < 0)
                    break;
                lcode = lcode.Substring(0,i);
            }
        }
        return qlang;
    }

    /// <summary>
    ///   多言語化用文字列置換
    /// </summary>
    public string _(string msg) {
        if(!m_langselect)
            return msg;

        if(m_trans == null){
            if(!m_autolang) {
                m_trans = Translator.Get(m_lang);
            } else {
                if(Request.UserLanguages != null){
                    string q = "q=0";
                    foreach(string lang in Request.UserLanguages){
                        string[] lq = lang.Split(";".ToCharArray(),2);
                        string lcode = lq[0].Trim();
                        string quality = (lq.Length > 1)?lq[1].Trim():"q=1";
                        while(lcode.Length > 0) {
                            if(Translator.Exists(lcode)){
                                if(quality != ""){
                                    if(q.CompareTo(quality) < 0){
                                        m_trans = Translator.Get(lcode);
                                        q = quality;
                                    }
                                }else{
                                    m_trans = Translator.Get(lcode);
                                    break;
                                }
                            }
                            int i = lcode.LastIndexOf('-');
                            if(i < 0)
                                break;
                            lcode = lcode.Substring(0,i);
                        }
                    }
                }
            }
            if(m_trans == null)
                m_trans = Translator.Default;
        }

        return m_trans.Trans(msg,this.GetType().Name);
    }


    private bool m_langselect = false;
    private bool m_autolang = false;
    private string m_lang = null;
    private Translator m_trans = null;

}

} // End of namespace
