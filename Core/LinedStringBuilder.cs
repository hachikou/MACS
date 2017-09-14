/*! @file LinedStringBuilder.cs
 * @brief 行番号付きStringBuilder
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
///   行番号付きStringBuilder
/// </summary>
public class LinedStringBuilder {

    /// <summary>
    ///   空のコンストラクタ
    /// </summary>
    public LinedStringBuilder() {
        Clear();
    }

    /// <summary>
    ///   初期文字列付きコンストラクタ
    /// </summary>
    public LinedStringBuilder(String str) {
        Clear();
        Append(str);
    }

    /// <summary>
    ///   初期行番号
    /// </summary>
    public int StartLineNumber {
        get { return startLineNumber; }
        set { startLineNumber = value; }
    }

    /// <summary>
    ///   行番号のフォーマット文字列
    /// </summary>
    public string LineNumberFormat {
        get { return lineNumberFormat; }
        set { lineNumberFormat = value; }
    }

    /// <summary>
    ///   出力最大行数
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     0以下を指定すると制限無し。
    ///   </para>
    /// </remarks>
    public int MaxLines {
        get { return maxLines; }
        set { maxLines = value; }
    }

    /// <summary>
    ///   改行文字
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     デフォルトは"\n"
    ///   </para>
    /// </remarks>
    public string LineSeparator {
        get { return lineSeparator; }
        set { lineSeparator = value; }
    }

    /// <summary>
    ///   現在の行番号
    /// </summary>
    public int LineNumber {
        get { return startLineNumber+buffer.Count; }
    }

    /// <summary>
    ///   現在の行数
    /// </summary>
    public int Lines {
        get {
            if(sb.Length > 0)
                return buffer.Count+1;
            return buffer.Count;
        }
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(bool val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(byte val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(char val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(char[] val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(double val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(Int16 val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(Int32 val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(Int64 val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(object val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(float val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(string val) {
        sb.Append(val);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(char val, Int32 n) {
        sb.Append(val, n);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(char[] val, Int32 n, Int32 m) {
        sb.Append(val, n, m);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder Append(string val, Int32 n, Int32 m) {
        sb.Append(val, n, m);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder AppendFormat(string format, object a) {
        sb.AppendFormat(format, a);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder AppendFormat(string format, object a, object b) {
        sb.AppendFormat(format, a, b);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder AppendFormat(string format, object a, object b, object c) {
        sb.AppendFormat(format, a, b, c);
        return this;
    }

    /// <summary>
    ///   文字列追加
    /// </summary>
    public LinedStringBuilder AppendFormat(string format, object[] a) {
        sb.AppendFormat(format, a);
        return this;
    }

    /// <summary>
    ///   改行追加
    /// </summary>
    public LinedStringBuilder AppendLine() {
        sb.Append(lineSeparator);
        buffer.Add(sb.ToString());
        sb.Clear();
        return this;
    }

    /// <summary>
    ///   改行付き文字列追加
    /// </summary>
    public LinedStringBuilder AppendLine(string line) {
        sb.Append(line);
        sb.AppendLine();
        return this;
    }

    /// <summary>
    ///   内容を空にする
    /// </summary>
    public void Clear() {
        if(buffer == null)
            buffer = new List<string>();
        else
            buffer.Clear();
        if(sb == null)
            sb = new StringBuilder();
        else
            sb.Clear();
    }

    /// <summary>
    ///   文字列化して返す。
    /// </summary>
    public override string ToString() {
        return ToString(0, 0);
    }

    /// <summary>
    ///   指定した行の間を文字列化して返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     行指定は、先頭行が0である。
    ///   </para>
    /// </remarks>
    public string ToString(int startline, int endline) {
        StringBuilder xsb = new StringBuilder();
        if(startline < 0)
            startline = 0;
        if((endline <= 0) || (endline > buffer.Count))
            endline = buffer.Count;
        int lineno = startLineNumber+startline;
        int xlines = 0;
        for(int i = startline; i < endline; i++) {
            string line = buffer[i];
            if((maxLines > 0) && (xlines >= maxLines))
                break;
            if(lineNumberFormat == null)
                xsb.Append(lineno);
            else
                xsb.Append(String.Format(lineNumberFormat, lineno));
            xsb.Append(line);
            lineno++;
            xlines++;
        }
        return xsb.ToString();
    }


    private List<string> buffer;
    private StringBuilder sb;
    private string lineSeparator = "\n";
    private string lineNumberFormat = "{0}: ";
    private int maxLines = 0;
    private int startLineNumber = 1;

}

} // End of namespace
