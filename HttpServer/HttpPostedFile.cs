/// HttpPostedFile: ファイル受信オブジェクト.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   ファイル受信オブジェクト
/// </summary>
public class HttpPostedFile : IDisposable {

    /// <summary>
    ///   ファイル名
    /// </summary>
    public string FileName {
        get { return m_filename; }
    }

    /// <summary>
    ///   入力ストリーム
    /// </summary>
    public Stream InputStream {
        get {
            if(m_stream == null){
                if(string.IsNullOrEmpty(m_innerfilename) || !File.Exists(m_innerfilename))
                    return null;
                m_stream = FileUtil.BinaryReader(m_innerfilename);
            }
            return m_stream;
        }
    }

    /// <summary>
    ///   ファイル長
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     SaveAs(string) を行なった後は、本プロパティは0を返す。
    ///     その保存ファイルのサイズを確認する事。
    ///   </para>
    /// </remarks>
    public long ContentLength {
        get {
            if(string.IsNullOrEmpty(m_innerfilename) || !File.Exists(m_innerfilename))
                return 0;
            FileInfo fi = new FileInfo(m_innerfilename);
            return fi.Length;
        }
    }

    internal string m_filename;
    internal string m_innerfilename;
    internal FileStream m_stream;

    /// <summary>
    ///   デフォルトコンストラクタ。
    /// </summary>
    public HttpPostedFile() {
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~HttpPostedFile() {
        Dispose();
    }

    /// <summary>
    ///   ディスポーザ
    /// </summary>
    public void Dispose() {
        if(m_stream != null){
            m_stream.Close();
            m_stream = null;
        }
        if(!string.IsNullOrEmpty(m_innerfilename) && File.Exists(m_innerfilename)){
            File.Delete(m_innerfilename);
        }
        m_innerfilename = null;
    }

    /// <summary>
    ///   受信ファイルを名前を付けて保存する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本メソッド呼び出し以降はInputStreamが使えなくなる事に注意。
    ///   </para>
    /// </remarks>
    public void SaveAs(string path) {
        if(string.IsNullOrEmpty(m_innerfilename) || !File.Exists(m_innerfilename))
            throw new FileNotFoundException("HttpPostedFile does not have inner file.");
        if(m_stream != null){
            m_stream.Close();
            m_stream = null;
        }
        File.Delete(path);
        File.Move(m_innerfilename, path);
    }


} // End of class HttpPostedFile

} // End of namespace
