/// DataArray: 文字列配列取り扱い用クラス.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MACS {

/// <summary>
///   文字列配列取り扱い用クラス
/// </summary>
/// <remarks>
///   <para>
///     データベースの1レコードのデータを表す際などに利用する。
///     本クラスはスレッドセーフである。ただし、Columns/Valuesを取り出して直接
///     扱う場合にはその限りではない。
///   </para>
/// </remarks>
public class DataArray {

    /// <summary>
    ///   カラム名一覧を指定したコンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     データ値はコピーされる。
    ///     データ値がnullの場合には、全データ値がnullである配列が作られる。
    ///     データ値の要素数がカラム名の個数より少ない場合、足りない部分のデータ値はnullになる。
    ///     データ値の要素数がカラム名の個数より多い場合、余計な部分のデータ値は無視される。
    ///   </para>
    /// </remarks>
    /// <param name="columns_">カラム名一覧</param>
    /// <param name="values_">データ値</param>
    public DataArray(string[] columns_, string[] values_) {
        if(columns_ == null)
            throw new ArgumentException("Column names must not be null");
        mutex = new object();
        columns = columns_;
        values = new string[columns.Length];
        if(values_ != null) {
            int n = (values.Length <= values_.Length)?values.Length:values_.Length;
            for(int i = 0; i < n; i++)
                values[i] = values_[i];
        }
    }

    /// <summary>
    ///   カラム数を指定したコンストラクタ
    /// </summary>
    /// <param name="ncolumns">カラム数</param>
    public DataArray(int ncolumns) {
        mutex = new object();
        columns = null;
        values = new string[ncolumns];
    }

    /// <summary>
    ///   データ値を指定したコンストラクタ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     データ値にnullを指定すると、要素数0の配列を生成する。
    ///   </para>
    /// </remarks>
    public DataArray(string[] values_) {
        mutex = new object();
        columns = null;
        if(values_ == null) {
            values = new string[0];
            return;
        }
        values = new string[values_.Length];
        for(int i = 0; i < values.Length; i++)
            values[i] = values_[i];
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public DataArray(DataArray src) {
        mutex = new object();
        columns = src.columns;
        values = new string[src.values.Length];
        for(int i = 0; i < values.Length; i++)
            values[i] = src.values[i];
    }

    /// <summary>
    ///   カラム名一覧
    /// </summary>
    public string[] Columns {
        get { return columns; }
    }

    /// <summary>
    ///   データ値配列
    /// </summary>
    public string[] Values {
        get { return values; }
    }

    /// <summary>
    ///   カラム数
    /// </summary>
    public int Length {
        get { lock(mutex) { return values.Length;}}
    }

    /// <summary>
    ///   カラム名を持つかどうか
    /// </summary>
    public bool HasColumnName {
        get { return (columns != null); }
    }

    /// <summary>
    ///   カラム名を指定したインデクサ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定カラムが存在しない時にはIndexOutOfRangeException。
    ///   </para>
    /// </remarks>
    public string this[string col] {
        get {
            lock(mutex) {
                int i = _columnNum(col);
                if(i < 0)
                    throw new IndexOutOfRangeException(String.Format("No such column ('{0}')", col));
                return values[i];
            }
        }
        set {
            lock(mutex) {
                int i = _columnNum(col);
                if(i < 0)
                    throw new IndexOutOfRangeException(String.Format("No such column ('{0}')", col));
                values[i] = value;
            }
        }
    }

    /// <summary>
    ///   カラム番号で指定したインデクサ
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定カラムが存在しない時にはIndexOutOfRangeException。
    ///   </para>
    /// </remarks>
    public string this[int col] {
        get {
            lock(mutex) {
                if((col < 0) || (col >= values.Length))
                    throw new IndexOutOfRangeException(String.Format("Invalid column number ({0})", col));
                return values[col];
            }
        }
        set {
            lock(mutex) {
                if((col < 0) || (col >= values.Length))
                    throw new IndexOutOfRangeException(String.Format("Invalid column number ({0})", col));
                values[col] = value;
            }
        }
    }

    /// <summary>
    ///   カラム名を指定して値を取り出す
    /// </summary>
    /// <param name="col">カラム名</param>
    /// <param name="defval">指定カラムが存在しないときの値</param>
    public string Get(string col, string defval=null) {
        lock(mutex) {
            int i = _columnNum(col);
            if(i < 0)
                return defval;
            return values[i];
        }
    }

    /// <summary>
    ///   カラム名を指定して値をセットする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定カラムが存在しない時には何もしない
    ///   </para>
    /// </remarks>
    public void Set(string col, string value) {
        lock(mutex) {
            int i = _columnNum(col);
            if(i < 0)
                return;
            values[i] = value;
        }
    }

    /// <summary>
    ///   カラム番号を指定して値を取り出す
    /// </summary>
    /// <param name="col">カラム番号</param>
    /// <param name="defval">指定カラムが存在しないときの値</param>
    public string Get(int col, string defval) {
        lock(mutex) {
            if((col < 0) || (col >= values.Length))
                return defval;
            return values[col];
        }
    }

    /// <summary>
    ///   カラム番号を指定して値をセットする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定カラムが存在しない時には何もしない
    ///   </para>
    /// </remarks>
    public void Set(int col, string value) {
        lock(mutex) {
            if((col < 0) || (col >= values.Length))
                return;
            values[col] = value;
        }
    }

    /// <summary>
    ///   カラム名に対応するカラム番号を獲得する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     先頭カラムは0番。
    ///     指定した名前のカラムが存在しない場合は-1を返す。
    ///   </para>
    /// </remarks>
    public int ColumnNum(string col) {
        lock(mutex) {
            return _columnNum(col);
        }
    }

    /// <summary>
    ///   指定カラム名が存在するかどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Contains(x)でチェックしてからthis[x]を使うのは効率が悪い。
    ///     ColumnNum(x)でインデックス番号を獲得してチェックするか、Get(x)を使う
    ///     方がよい。
    ///   </para>
    /// </remarks>
    public bool Contains(string col) {
        lock(mutex) {
            if((columns == null) || (col == null))
                return false;
            foreach(string c in columns) {
                if(col == c)
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    ///   データ値をコピーする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     自分自身もコピー元もカラム名を持つとき、同じカラム名のカラムを探して
    ///     代入が行なわれる。この際、同じカラム名のカラムが存在しない場合は無視
    ///     される。
    ///     自分自身かコピー元がカラム名を持たないとき、同じカラム番号のデータ値
    ///     をコピーする。この際、コピー元の要素数の方が自分自身の要素数より多い
    ///     場合は、余った分は無視される。また、コピー元の要素数の方が自分自身の
    ///     要素数より少ない場合は、足りない分はnullが代入される。
    ///   </para>
    /// </remarks>
    public DataArray CopyFrom(DataArray src) {
        if((columns == null) || (src.columns == null))
            return CopyFrom(src.Values);
        lock(mutex) {
            if(columns == src.columns) {
                for(int i = 0; i < values.Length; i++)
                    values[i] = src.values[i];
                return this;
            }
            for(int i = 0; i < columns.Length; i++) {
                for(int j = 0; j < src.columns.Length; j++) {
                    if(columns[i] == src.columns[j]) {
                        values[i] = src.values[j];
                        break;
                    }
                }
            }
        }
        return this;
    }

    /// <summary>
    ///   データ値を代入する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     コピー元の要素数の方が自分自身の要素数より多い場合は、余った分は無視
    ///     される。
    ///     また、コピー元の要素数の方が自分自身の要素数より少ない場合は、足りな
    ///     い分はdefvalが代入される。
    ///   </para>
    /// </remarks>
    public DataArray CopyFrom(string[] src, string defval=null) {
        lock(mutex) {
            if(src == null) {
                for(int i = 0; i < values.Length; i++)
                    values[i] = defval;
            } else {
                int i = 0;
                while((i < values.Length) && (i < src.Length)) {
                    values[i] = src[i];
                    i++;
                }
                while(i < values.Length) {
                    values[i] = defval;
                    i++;
                }
            }
        }
        return this;
    }

    /// <summary>
    ///   データ値をすべて指定値にする
    /// </summary>
    public DataArray Fill(string value) {
        lock(mutex) {
            for(int i = 0; i < values.Length; i++)
                values[i] = value;
        }
        return this;
    }

    /// <summary>
    ///   データ値をすべてnullにする
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カラムを削除するのではないことに注意。
    ///   </para>
    /// </remarks>
    public DataArray Clear() {
        return Fill(null);
    }

    /// <summary>
    ///   カラムを指定位置に追加する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Insert(0, colname, value)で先頭に追加。
    ///     Insert(this.Length, colname, value)はAdd(columnname, value)と等価。
    ///     posがゼロ以下の場合、先頭に追加する。
    ///     posが要素数以上の場合、末尾に追加する。
    ///     カラム名を持たないDataArrayに対しては本関数は利用できない。
    ///   </para>
    /// </remarks>
    public DataArray Insert(int pos, string colname, string value) {
        if(columns == null)
            throw new ArgumentException("This DataArray doesn't have column names.");
        lock(mutex) {
            if(pos < 0)
                pos = 0;
            else if(pos > values.Length)
                pos = values.Length;
            string[] newcolumns = new string[columns.Length+1];
            string[] newvalues = new string[values.Length+1];
            int i = 0;
            while(i < pos) {
                newcolumns[i] = columns[i];
                newvalues[i] = values[i];
                i++;
            }
            newcolumns[i] = colname;
            newvalues[i] = value;
            while(i < values.Length) {
                newcolumns[i+1] = columns[i];
                newvalues[i+1] = values[i];
                i++;
            }
            columns = newcolumns;
            values = newvalues;
        }
        return this;
    }

    /// <summary>
    ///   カラムを指定位置に追加する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Insert(0, value)で先頭に追加。
    ///     Insert(this.Length, value)はAdd(value)と等価。
    ///     posがゼロ以下の場合、先頭に追加する。
    ///     posが要素数以上の場合、末尾に追加する。
    ///     カラム名を持つDataArrayに対して本関数を用いると、空のカラム名が
    ///     付けられる。
    ///   </para>
    /// </remarks>
    public DataArray Insert(int pos, string value) {
        if(columns != null)
            return Insert(pos, "", value);
        lock(mutex) {
            if(pos < 0)
                pos = 0;
            else if(pos > values.Length)
                pos = values.Length;
            string[] newvalues = new string[values.Length+1];
            int i = 0;
            while(i < pos) {
                newvalues[i] = values[i];
                i++;
            }
            newvalues[i] = value;
            while(i < values.Length) {
                newvalues[i+1] = values[i];
                i++;
            }
            values = newvalues;
        }
        return this;
    }

    /// <summary>
    ///   カラムを追加する
    /// </summary>
    public DataArray Add(string colname, string value) {
        return Insert(values.Length, colname, value);
    }

    /// <summary>
    ///   カラムを追加する
    /// </summary>
    public DataArray Add(string value) {
        return Insert(values.Length, value);
    }

    /// <summary>
    ///   指定位置のカラムを削除する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     posが0より小さい、またはLength以上の場合はエラー。
    ///   </para>
    /// </remarks>
    public DataArray Remove(int pos) {
        lock(values) {
            return _remove(pos);
        }
    }

    /// <summary>
    ///   指定名のカラムを削除する
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定名のカラムが存在しない場合は何もしない。
    ///     指定名のカラムが複数存在する場合は全て削除される。
    ///   </para>
    /// </remarks>
    public DataArray Remove(string col) {
        lock(values) {
            int pos;
            while((pos = _columnNum(col)) >= 0)
                _remove(pos);
        }
        return this;
    }

    /// <summary>
    ///   全カラムを削除する
    /// </summary>
    public DataArray RemoveAll() {
        lock(mutex) {
            if(columns != null)
                columns = new string[0];
            values = new string[0];
        }
        return this;
    }

    /// <summary>
    ///   List<string[]>をList<DataArray>に変換する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     string[]はコピーされて利用される。
    ///   </para>
    /// </remarks>
    public static List<DataArray> Convert(List<string[]> src) {
        List<DataArray> dst = new List<DataArray>();
        foreach(string[] values in src) {
            dst.Add(new DataArray(values));
        }
        return dst;
    }

    /// <summary>
    ///   List<string[]>をList<DataArray>に変換する。カラム名指定バージョン
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     string[]はコピーされて利用される。
    ///   </para>
    /// </remarks>
    public static List<DataArray> Convert(List<string[]> src, string[] columns) {
        List<DataArray> dst = new List<DataArray>();
        foreach(string[] values in src) {
            dst.Add(new DataArray(columns, values));
        }
        return dst;
    }

    /// <summary>
    ///   ファイルを読み取りDataArrayを作る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カラム名=値 の形式の行が1行でもあればカラム名付きのDataArrayを生成
    ///     する。
    ///     カラム名=値 の形式の行が1行もない場合は、カラム名無しのDataArrayを
    ///     生成する。このとき、行全体が値となる。
    ///     カラム名および値の前後の空白は除去される。
    ///     値をダブルクォートもしくはシングルクォートで囲ってもよい。
    ///     空行は読み飛ばされる。また、"#"で始まる行は読み飛ばされる。
    ///     ファイルエンコーディングはSJIS。
    ///   </para>
    /// </remarks>
    public static DataArray FromFile(string filename) {
        return FromFile(filename, FileUtil.DefaultEncoding);
    }

    /// <summary>
    ///   ファイルを読み取りDataArrayを作る。ファイルエンコーディング指定版
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カラム名=値 の形式の行が1行でもあればカラム名付きのDataArrayを生成
    ///     する。
    ///     カラム名=値 の形式の行が1行もない場合は、カラム名無しのDataArrayを
    ///     生成する。このとき、行全体が値となる。
    ///     カラム名および値の前後の空白は除去される。
    ///     値をダブルクォートもしくはシングルクォートで囲ってもよい。
    ///     空行は読み飛ばされる。また、"#"で始まる行は読み飛ばされる。
    ///   </para>
    /// </remarks>
    public static DataArray FromFile(string filename, Encoding enc) {
        using(StreamReader sr = FileUtil.Reader(filename, enc)) {
            return From(sr);
        }
    }

    /// <summary>
    ///   ストリームを読み取りDataArrayを作る
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     カラム名=値 の形式の行を読み取りカラム名付きのDataArrayを生成する。
    ///     カラム名= を持たない行は行番号を文字列にしたダミーのカラム名がつけら
    ///     れる。（カラム名=を持たない最初の行が"0"、次の行が"1"...となる。）
    ///     カラム名および値の前後の空白は除去される。
    ///     値をダブルクォートもしくはシングルクォートで囲ってもよい。
    ///     空行は読み飛ばされる。また、"#"で始まる行は読み飛ばされる。
    ///   </para>
    /// </remarks>
    public static DataArray From(StreamReader sr) {
        List<string> columns = new List<string>();
        List<string> values = new List<string>();
        int lineno = 0;
        while(!sr.EndOfStream) {
            string line = sr.ReadLine().Trim();
            if((line.Length == 0) || (line[0] == '#'))
                continue;
            string key, val;
            string[] x = line.Split("=".ToCharArray(), 2);
            if(x.Length == 1) {
                key = lineno.ToString();
                val = line;
                lineno++;
            } else {
                key = x[0].Trim();
                val = x[1].Trim();
            }
            if((val.Length >= 2)
               && (((val[0] == '"') && (val[val.Length-1] == '"'))
                   || ((val[0] == '"') && (val[val.Length-1] == '"')))) {
                val = val.Substring(1, val.Length-2);
            }
            columns.Add(key);
            values.Add(val);
        }
        return new DataArray(columns.ToArray(), values.ToArray());
    }


    /// <summary>
    ///   標準出力にデバッグダンプする
    /// </summary>
    public void Dump(string title) {
        for(int i = 0; i < values.Length; i++) {
            Console.Write(title);
            Console.Write(".");
            if(columns != null)
                Console.Write(columns[i]);
            else
                Console.Write(i.ToString());
            Console.Write(" = ");
            if(values[i] == null)
                Console.Write("null");
            else
                Console.Write("\"{0}\"", values[i]);
            Console.WriteLine();
        }
    }

    private readonly object mutex;
    private string[] columns;
    private string[] values;

    private int _columnNum(string col) {
        if((col == null) || (columns == null))
            return -1;
        for(int i = 0; i < columns.Length; i++)
            if(col == columns[i])
                return i;
        return -1;
    }

    private DataArray _remove(int pos) {
        if((pos < 0) || (pos >= values.Length))
            throw new ArgumentException("Invalid column position for remove.");
        int i;
        if(columns != null) {
            string[] newcolumns = new string[columns.Length-1];
            i = 0;
            while(i < pos) {
                newcolumns[i] = columns[i];
                i++;
            }
            i++;
            while(i < columns.Length) {
                newcolumns[i-1] = columns[i];
                i++;
            }
            columns = newcolumns;
        }
        string[] newvalues = new string[values.Length-1];
        i = 0;
        while(i < pos) {
            newvalues[i] = values[i];
            i++;
        }
        i++;
        while(i < values.Length) {
            newvalues[i-1] = values[i];
            i++;
        }
        values = newvalues;
        return this;
    }

}

} // End of namespace
