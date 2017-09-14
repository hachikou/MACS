/*! @file JsonTool.cs
 * @brief JSONコード作成に便利なユーティリティ集。
 * $Id: $
 *
 * Copyright (C) 2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Text;
using System.Collections.Generic;

namespace MACS {

/// <summary>
///   JSONコード作成に便利なユーティリティ集。
/// </summary>
/// <remarks>
///   <para>
///     全てグローバルな関数であり、本来はクラス化する必要が無い。
///     コード表記を簡略化するためのネームスペース代りに使っているにすぎない。
///   </para>
/// </remarks>
public class JsonTool {

    /// <summary>
    ///   JSONエスケープ
    /// </summary>
    public static string JE(string str) {
        if(str == null)
            return "";
        StringBuilder sb = new StringBuilder();
        foreach(char ch in str) {
            if(ch == '\\')
                sb.Append("\\\\");
            else if(ch == '"')
                sb.Append("\\\"");
            else if(ch == '\t')
                sb.Append("\\t");
            else if(ch == '\n')
                sb.Append("\\n");
            else if((int)ch >= 0x20)
                sb.Append(ch);
        }
        return sb.ToString();
    }

    /// <summary>
    ///   JSONエスケープしてクオートした文字列
    /// </summary>
    public static string JS(string str) {
        return "\""+JE(str)+"\"";
    }

    /// <summary>
    ///   整数をJSON化
    /// </summary>
    public static string JS(int val) {
        return val.ToString();
    }

    /// <summary>
    ///   浮動小数点数をJSON化
    /// </summary>
    public static string JS(double val) {
        return val.ToString();
    }

    /// <summary>
    ///   文字列配列をJSON化
    /// </summary>
    public static string JS(string[] list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(string str in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(JS(str));
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   int配列をJSON化
    /// </summary>
    public static string JS(int[] list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(int val in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(val);
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   double配列をJSON化
    /// </summary>
    public static string JS(double[] list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(double val in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(val);
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   bool配列をJSON化
    /// </summary>
    public static string JS(bool[] list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(bool val in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(val?"true":"false");
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   文字列配列のリストをJSON化
    /// </summary>
    public static string JS(List<string[]> list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(string[] strs in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(JS(strs));
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   DataArrayをJSON化
    /// </summary>
    public static string JS(DataArray rec) {
        if(rec.Columns == null)
            return JS(rec.Values);
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool first = true;
        for(int i = 0; i < rec.Length; i++) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append('\"');
            sb.Append(rec.Columns[i]);
            sb.Append("\":");
            sb.Append(JS(rec.Values[i]));
        }
        sb.Append("}");
        return sb.ToString();
    }

    /// <summary>
    ///   DataArrayのリストをJSON化
    /// </summary>
    public static string JS(List<DataArray> list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(DataArray rec in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(JS(rec));
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   DataArrayの配列をJSON化
    /// </summary>
    public static string JS(DataArray[] list) {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        bool first = true;
        foreach(DataArray rec in list) {
            if(first)
                first = false;
            else
                sb.Append(',');
            sb.Append(JS(rec));
        }
        sb.Append("]");
        return sb.ToString();
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, string val) {
        return JS(key)+":"+JS(val);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, int val) {
        return JS(key)+":"+val.ToString();
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, double val) {
        return JS(key)+":"+val.ToString();
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, bool val) {
        return JS(key)+":"+(val?"true":"false");
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, string[] rec) {
        return JS(key)+":"+JS(rec);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, int[] rec) {
        return JS(key)+":"+JS(rec);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, double[] rec) {
        return JS(key)+":"+JS(rec);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, bool[] rec) {
        return JS(key)+":"+JS(rec);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, List<string[]> list) {
        return JS(key)+":"+JS(list);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, DataArray rec) {
        return JS(key)+":"+JS(rec);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, List<DataArray> list) {
        return JS(key)+":"+JS(list);
    }

    /// <summary>
    ///   JSONのキーバリューペア
    /// </summary>
    public static string JS(string key, DataArray[] list) {
        return JS(key)+":"+JS(list);
    }

}

} // End of namespace
