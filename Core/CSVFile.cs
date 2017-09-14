/*! @file CSVFile.cs
 * @brief CSV形式のファイルを取り扱うオブジェクト
 * $Id: $
 *
 * Copyright (C) 2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace MACS {


/// <summary>
///   CSV形式のファイルを取り扱うオブジェクト
/// </summary>
public class CSVFile : IDisposable {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public CSVFile(string filename, Encoding enc) {
        m_filename = filename;
        m_enc = enc;
    }
    /// <summary>
    ///   コンストラクタ。
    ///   デフォルトエンコーディング版。
    /// </summary>
    public CSVFile(string filename) {
        m_filename = filename;
        m_enc = null;
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~CSVFile() {
        Dispose();
    }

    /// <summary>
    ///   使用リソースの解放
    /// </summary>
    public void Dispose() {
        Close();
    }

    /// <summary>
    ///   ファイル読み取りを終了する
    /// </summary>
    public void Close() {
        m_list = null;
    }

    /// <summary>
    ///   全行読み取る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     空行と"#"で始まる行は読み飛ばされる。
    ///   </para>
    /// </remarks>
    public List<string[]> ReadAll() {
        return readAll(false);
    }

    /// <summary>
    ///   全行読み取る。先頭カラムに行番号を入れる
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     空行と"#"で始まる行は読み飛ばされる。
    ///   </para>
    /// </remarks>
    public List<string[]> ReadAllWithLineNo() {
        return readAll(true);
    }

    /// <summary>
    ///   全行読み取り、List<DataArray>を返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     先頭行が"#"で始まる場合、その行はカラム名定義として扱われる。
    ///   </para>
    /// </remarks>
    public List<DataArray> ReadAllData() {
        List<DataArray> list = new List<DataArray>();
        foreach(string[] values in ReadAll()) {
            if(m_columns == null)
                list.Add(new DataArray(values));
            else
                list.Add(new DataArray(m_columns, values));
        }
        return list;
    }

    /// <summary>
    ///   全行読み取り、List<DataArray>を返す。カラム名一覧を指定するバージョン
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カラム名の個数より多いデータ値は捨てられる。
    ///     カラム名の個数よりデータ値の個数が少ない場合、足りないデータ値は
    ///     nullになる。
    ///   </para>
    /// </remarks>
    public List<DataArray> ReadAllData(string[] columns) {
        List<DataArray> list = new List<DataArray>();
        foreach(string[] values in ReadAll()) {
            DataArray x = new DataArray(columns, null);
            int len = columns.Length;
            if(len > values.Length)
                len = values.Length;
            for(int i = 0; i < len; i++)
                x[i] = values[i];
            list.Add(x);
        }
        return list;
    }


    private List<string[]> readAll(bool linenoflag) {
        if((m_list != null) && (m_linenoflag == linenoflag))
            return m_list;
        m_list = new List<string[]>();
        m_linenoflag = linenoflag;
        using(FileStream fs = FileUtil.BinaryReader(m_filename)) {
            if(fs == null)
                return m_list;
            byte[] linebuf = new byte[4096];
            int linelen = 0;
            bool eof = false;
            int lineno = 0;
            while(!eof) {
                int ch = fs.ReadByte();
                if(ch < 0) {
                    eof = true;
                    if(linelen == 0)
                        break;
                } else if(ch == 0x0d) { // ignore CR
                    continue;
                } else if(ch == 0x0a) { // LF
                    // go through
                } else if(linelen < linebuf.Length) {
                    linebuf[linelen] = (byte)ch;
                    linelen++;
                    continue;
                }

                lineno++;
                string line;
                if(m_enc == null)
                    line = SJISDictionary.GetString(linebuf, 0, linelen);
                else
                    line = m_enc.GetString(linebuf, 0, linelen);
                linelen = 0;

                line = line.Trim();
                if(line == "")
                    continue;
                if(line.StartsWith("#")) {
                    if(lineno == 1) {
                        m_columns = line.Substring(1).Split(",".ToCharArray());
                        for(int i = 0; i < m_columns.Length; i++)
                            m_columns[i] = m_columns[i].Trim();
                    }
                    continue;
                }
                List<string> data = new List<string>();
                if(m_linenoflag)
                    data.Add(lineno.ToString());
                int ptr = 0;
                int nptr;
                while(ptr < line.Length) {
                    if(line[ptr] == ' ') {
                        ptr++;
                        continue;
                    }
                    if(line[ptr] == '"') {
                        ptr++;
                        nptr = line.IndexOf('"', ptr);
                        if(nptr < 0)
                            nptr = line.Length;
                        data.Add(line.Substring(ptr, nptr-ptr));
                        ptr = line.IndexOf(',', nptr);
                        if(ptr < 0)
                            ptr = line.Length;
                        else
                            ptr++;
                        continue;
                    }
                    if(line[ptr] == ',') {
                        data.Add("");
                        ptr++;
                        continue;
                    }
                    nptr = line.IndexOf(',', ptr);
                    if(nptr < 0)
                        nptr = line.Length;
                    data.Add(line.Substring(ptr, nptr-ptr).Trim());
                    ptr = nptr+1;
                }
                if(line[line.Length-1] == ',')
                    data.Add("");
                m_list.Add(data.ToArray());
            }
        }
        return m_list;
    }


    private readonly string m_filename;
    private readonly Encoding m_enc;
    private List<string[]> m_list;
    private string[] m_columns;
    private bool m_linenoflag;

}

} // End of namespace
