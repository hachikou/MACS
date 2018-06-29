/// HttpBuiltinContentsSupport: ビルトインコンテンツサポート用基底クラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   ビルトインコンテンツサポート用基底クラス。
///   コンテンツファイルをEXE内に埋め込むことができるようにする。
/// </summary>
/// <remarks>
///   <para>
///     埋め込むコンテンツは、makebuiltincontentツールで生成することができます。
///   </para>
/// </remarks>
public abstract class HttpBuiltinContentsSupport : HttpPage {

    /// <summary>
    ///   ビルトインでないコンテンツが入るディレクトリを指定する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     派生クラスのコンストラクタで使用することを想定している
    ///   </para>
    /// </remarks>
    public void UseDirectory(string dir) {
        m_dir = dir;
    }

    /// <summary>
    ///   ビルトインコンテンツの指定
    /// </summary>
    public void SetBuiltinContents(HttpBuiltinContent[] contents) {
        m_contents = contents;
        m_datetime = DateTime.Now;
    }


    /// <summary>
    ///   ファイル存在確認
    /// </summary>
    protected bool FileExists(string path) {
        if(File.Exists(path))
            return true;
        if(!path.StartsWith(m_dir) || (m_contents == null))
            return false;
        path = path.Substring(m_dir.Length).Replace('\\','/');
        foreach(HttpBuiltinContent c in m_contents){
            if(c.m_filename == path)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   ディレクトリ存在確認
    /// </summary>
    protected bool DirectoryExists(string path) {
        if(!path.EndsWith("/"))
            path = path+"/";
        if(Directory.Exists(path))
            return true;
        if(!path.StartsWith(m_dir) || (m_contents == null))
            return false;
        path = path.Substring(m_dir.Length).Replace('\\','/');
        foreach(HttpBuiltinContent c in m_contents){
            if(c.m_filename.StartsWith(path))
                return true;
        }
        return false;
    }

    /// <summary>
    ///   ファイルの最終更新日時を獲得する
    /// </summary>
    protected DateTime GetLastWriteTimeUtc(string path) {
        if(File.Exists(path))
            return File.GetLastWriteTimeUtc(path);
        return m_datetime;
    }

    /// <summary>
    ///   ファイルを読み取り用にオープンする
    /// </summary>
    protected Stream OpenFile(string path) {
        if(File.Exists(path))
            return FileUtil.BinaryReader(path);
        if(!path.StartsWith(m_dir) || (m_contents == null))
            return null;
        path = path.Substring(m_dir.Length).Replace('\\','/');
        foreach(HttpBuiltinContent c in m_contents){
            if(c.m_filename == path)
                return new MemoryStream(c.m_content, false);
        }
        return null;
    }

    /// <summary>
    ///   ビルトインでないコンテンツが入るディレクトリ名
    /// </summary>
    protected string m_dir = ".";

    private HttpBuiltinContent[] m_contents = null;
    private DateTime m_datetime = DateTime.MinValue;

}

/// <summary>
///   埋め込むファイルを格納するクラス
/// </summary>
public class HttpBuiltinContent {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    /// <param name="filename">ファイル名。'/'で始まる。</param>
    /// <param name="content">ファイル内容のバイト列</param>
    public HttpBuiltinContent(string filename, byte[] content) {
        m_filename = filename;
        m_content = content;
    }

    internal string m_filename;
    internal byte[] m_content;
}

} // End of namespace
