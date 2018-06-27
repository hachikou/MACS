/// SJISReader: Shift_JISによる固定バイト数ファイル読み取りをサポートするクラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using System.Text;

namespace MACS {

/// <summary>
///   Shift_JISによる固定バイト数ファイル読み取りをサポートするクラス
/// </summary>
public class SJISReader : IDisposable {

    /// <summary>
    ///   Shift_JISによるファイル入力ストリームを開く
    /// </summary>
    public SJISReader(string filename) {
        fs = FileUtil.BinaryReader(filename);
        if(fs == null)
            throw new IOException(String.Format("Can't open '{0}' for reading.", filename));
    }

    /// <summary>
    ///   指定ファイルストリームをShift_JIS読み取り用ストリームとして使う
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本クラスでClose()すると、指定ストリームはClose()される。
    ///   </para>
    /// </remarks>
    public SJISReader(FileStream fs_) {
        if(fs_ == null)
            throw new IOException("FileStream is null");
        fs = fs_;
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~SJISReader() {
        Dispose();
    }

    /// <summary>
    ///   使用リソースの解放
    /// </summary>
    public void Dispose() {
        Close();
    }

    /// <summary>
    ///   ファイル読み取りを終了する。
    /// </summary>
    public void Close() {
        if(fs != null) {
            fs.Close();
            fs = null;
        }
    }

    /// <summary>
    ///   改行まで読み取る。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     返り値に改行文字は含まない。
    ///     改行文字はLFまたはCRLF。
    ///   </para>
    /// </remarks>
    public string ReadLine() {
        byte[] buf = new byte[1024];
        int ptr = 0;
        int r;
        while((r = fs.ReadByte()) >= 0) {
            if((char)r == '\n')
                break;
            if((char)r == '\r')
                continue;
            if(ptr >= buf.Length) {
                byte[] tmp = new byte[buf.Length+1024];
                for(int i = 0; i < ptr; i++)
                    tmp[i] = buf[i];
                buf = tmp;
            }
            buf[ptr] = (byte)r;
            ptr++;
        }
        return SJISDictionary.GetString(buf, 0, ptr);
    }

    /// <summary>
    ///   最大nバイト読み取る。
    /// </summary>
    /// <param name="n">最大読み取りバイト数</param>
    /// <param name="str">読み取った文字列を格納するstring</param>
    /// <returns>読み取ったバイト数。改行文字はバイト数に含まれない</returns>
    /// <remarks>
    ///   <para>
    ///     nバイト読み取る前にファイル末尾になるか改行文字が読み取られた場合、
    ///     そこで読み取りを打ち切る。改行文字は結果に含まれない。
    ///   </para>
    /// </remarks>
    public int Read(int n, out string str) {
        byte[] buf = new byte[n];
        int ptr = 0;
        int r;
        while((ptr < n) && ((r = fs.ReadByte()) >= 0)) {
            if((char)r == '\n')
                break;
            if((char)r == '\r')
                continue;
            buf[ptr] = (byte)r;
            ptr++;
        }
        str = SJISDictionary.GetString(buf, 0, ptr);
        return ptr;
    }

    /// <summary>
    ///   固定長フィールドを読み取り、DataArrayとして返す。
    /// </summary>
    /// <param name="columns">読み取る項目名一覧（DataArrayのColumnsになる）</param>
    /// <param name="lengths">columnsの各項目のバイト数</param>
    /// <param name="readout">指定項目を読み取った後、改行までを読み捨てるかどうか</param>
    /// <param name="checkcr">改行チェックをするかどうか</param>
    /// <remarks>
    ///   <para>
    ///     readoutがtrueの場合、指定項目を読み取った後、改行コードまでを読み捨てる。
    ///     readoutがfalseでcheckcrがtrueの場合、指定項目を読み取ったすぐ後に改行コードまたはファイル末尾が無い場合にはIOExceptionを発生する。
    ///
    ///     指定項目を読み取っている最中に改行コードまたはファイル末尾になった場合にはIOExceptionを発生する。
    ///     ただし、1項目も読み取らないでファイル末尾になった場合は例外は発生せず、nullを返す。
    ///   </para>
    /// </remarks>
    public DataArray GetRecordData(string[] columns, int[] lengths, bool readout=true, bool checkcr=true) {
        if(columns == null)
            throw new ArgumentNullException("columns must not be null.");
        if(lengths == null)
            throw new ArgumentNullException("lengths must not be null.");
        if(columns.Length != lengths.Length)
            throw new ArgumentException("columns.Length and lengths.Length must equal.");
        int n = 0;
        for(int i = 0; i < lengths.Length; i++)
            n += lengths[i];
        byte[] buf = new byte[n];
        int ptr = 0;
        int r = 0;
        while((ptr < n) && ((r = fs.ReadByte()) >= 0)) {
            if((char)r == '\n')
                break;
            if((char)r == '\r')
                continue;
            buf[ptr] = (byte)r;
            ptr++;
        }
        if((ptr == 0) && (r < 0))
            return null;
        if(ptr < n)
            throw new IOException(String.Format("Short line (must be longer than {0} bytes)", n));
        DataArray rec = new DataArray(columns, null);
        ptr = 0;
        for(int i = 0; i < columns.Length; i++) {
            rec[i] = SJISDictionary.GetString(buf, ptr, lengths[i]);
            ptr += lengths[i];
        }
        if(readout) {
            ReadLine();
        } else if(checkcr) {
            string val;
            if(Read(1, out val) != 0) {
                throw new IOException(String.Format("Illegal line length (must be {0} bytes).", n));
            }
        }
        return rec;
    }


    /// <summary>
    ///   内部ファイルストリーム
    /// </summary>
    private FileStream fs;

}

} // End of namespace
