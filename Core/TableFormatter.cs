/*! @file TableFormatter.cs
 * @brief リストデータを表形式でHTML化するツールクラス
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MACS {

/// <summary>
///   リストデータを表形式でHTML化するツールクラス
/// </summary>
public class TableFormatter {

    /// <summary>
    ///   テーブル化データ用レコード定義
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本クラスを基底にするクラスを作成し、そのListをTableFormatterに渡す。
    ///   </para>
    /// </remarks>
    public class Record {

        /* データメンバは無し。*/

        /* 表示用文字列バッファ */
        private string[] display_string = null;

        /// <summary>
        ///   デフォルトコンストラクタ（何もしない）
        /// </summary>
        public Record() {}

        /// <summary>
        ///   項目名一覧を獲得する。
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     派生クラスでオーバーライドすること
        ///   </para>
        /// </remarks>
        public virtual string[] GetRecordNames() {
            return new string[0];
        }

        /// <summary>
        ///   指定した番号の項目の値を文字列で獲得する。
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     派生クラスでオーバーライドすること
        ///   </para>
        /// </remarks>
        public virtual string GetString(int i) {
            return "";
        }

        /// <summary>
        ///   指定した名前の項目の項目番号を返す。
        ///   指定した名前の項目が存在しない場合には-1を返す。
        /// </summary>
        public int GetId(string name) {
            string[] names = GetRecordNames();
            for(int i = 0; i < names.Length; i++){
                if(names[i] == name)
                    return i;
            }
            return -1;
        }

        /// <summary>
        ///   指定した項目番号の値のHTML表示文字列を返す。
        /// </summary>
        public string GetDisplayString(int i) {
            if((display_string != null) && (i >= 0) && (i < display_string.Length) && (display_string[i] != null))
                return display_string[i];
            return HtmlTool.HE(GetString(i));
        }

        /// <summary>
        ///   指定した項目番号のHTML表示内容をセットする。
        /// </summary>
        public void SetDisplayString(int i, string txt) {
            if(display_string == null)
                display_string = new string[GetRecordNames().Length];
            if(i < display_string.Length)
                display_string[i] = txt;
        }

        /// <summary>
        ///   SetDisplayStringでセットされたHTML表示内容をすべてクリアする。
        /// </summary>
        public void ClearDisplayString() {
            display_string = null;
        }

    } /* End of class Record */


    /// <summary>
    ///   テーブル要素定義
    /// </summary>
    private class CellDef {

        /// <summary>
        ///   文字修飾
        /// </summary>
        public enum Decoration {
            NONE,  ///< 無し
            PAREN, ///< カッコ付き
            LTGT,  ///< カギ括弧付き
        };
        /// <summary>
        ///   表示スタイル
        /// </summary>
        public enum Style {
            NORMAL,    ///< 通常
            SUBINFO,   ///< 付加情報
            OPERATION, ///< 操作ボタン
            HIDE,      ///< 非表示
        };
        /// <summary>
        ///   日時表示形式
        /// </summary>
        public enum DateFormat {
            FULL,     ///< 全て表示
            SHORT,    ///< 短縮表示
            NOYEAR,   ///< 年を表示しない
            DATEONLY, ///< 日付のみ
        };
        /// <summary>
        ///   ソートフラグ
        /// </summary>
        public enum Sortable {
            NONE,    ///< ソート無し
            ASCEND,  ///< 昇順
            DESCEND, ///< 降順
        };

        /* フォーマットパラメータ */
        public string      recordname;   ///< 項目ID名
        public int         recordid;     ///< 項目ID番号
        public string      displayname;  ///< 表示名
        public int         width;        ///< 表示幅
        public Decoration  decoration;   ///< 装飾
        public Style       style;        ///< 文字スタイル
        public string      strikeout;    ///< 空欄時表示文字列
        public DateFormat  dateformat;   ///< 日時フォーマット
        public Sortable    sortable;     ///< ソート可能フラグ
        public string      tooltip;      ///< ツールチップ（対象項目ID名）

        /// <summary>
        ///   データ表示文字列の獲得
        /// </summary>
        public string GetData(Record rec) {
            if((recordname == null) || (recordname == ""))
                return "";
            if(recordid < 0)
                recordid = rec.GetId(recordname);
            return rec.GetString(recordid);
        }

        public StringBuilder GetDataHtml(Translatable page, StringBuilder sb, Record rec) {
            if((recordname == null) || (recordname == ""))
                return sb;
            if(recordid < 0)
                recordid = rec.GetId(recordname);
            if(style == Style.SUBINFO)
                sb.Append("<div class='subinfo'>");
            else if(style == Style.OPERATION)
                sb.Append("<div class='operation'>");
            switch(decoration){
            case Decoration.PAREN:
                sb.Append("(");
                break;
            case Decoration.LTGT:
                sb.Append("&lt;");
                break;
            }

            string txt = rec.GetDisplayString(recordid);
            if(txt == ""){
                if(strikeout == null)
                    sb.Append(txt);
                else
                    sb.Append(strikeout);
            } else {
                Format(page, sb, txt, rec);
            }

            switch(decoration){
            case Decoration.PAREN:
                sb.Append(")");
                break;
            case Decoration.LTGT:
                sb.Append("&gt;");
                break;
            }
            if(style != Style.NORMAL)
                sb.Append("</div>");
            return sb;
        }

        public StringBuilder GetHtmlDisplayName(Translatable page, StringBuilder sb) {
            switch(decoration){
            case Decoration.PAREN:
                sb.Append("(");
                break;
            case Decoration.LTGT:
                sb.Append("&lt;");
                break;
            }
            sb.Append(HtmlTool.HE(page._(displayname)));
            switch(decoration){
            case Decoration.PAREN:
                sb.Append(")");
                break;
            case Decoration.LTGT:
                sb.Append("&gt;");
                break;
            }
            return sb;
        }

        private void Format(Translatable page, StringBuilder sb, string txt, Record rec) {
            string tt = "";
            int i,j;
            switch(dateformat){
            case DateFormat.SHORT:
                tt += txt;
                i = txt.IndexOf('/');
                j = txt.LastIndexOf(':');
                if((i >= 0) && (j > i))
                    txt = txt.Substring(i+1,j-i-1).Replace(" ", "&nbsp;");
                break;
            case DateFormat.NOYEAR:
                tt += txt;
                i = txt.IndexOf('/');
                if(i >= 0)
                    txt = txt.Substring(i+1).Replace(" ", "&nbsp;");
                break;
            case DateFormat.DATEONLY:
                tt += txt;
                i = txt.IndexOf(' ');
                if(i >= 0)
                    txt = txt.Substring(0,i);
                break;
            }
            if(tooltip != null){
                if(tt != "")
                    tt += ": ";
                i = rec.GetId(tooltip);
                if(i >= 0){
                    tt += string.Format(page._("{0}における{1}"), rec.GetString(i), displayname);
                }
            }
            if(tt == ""){
                sb.Append(txt);
            }else{
                sb.Append("<span title=\"");
                sb.Append(HtmlTool.QE(tt));
                sb.Append("\">");
                sb.Append(txt);
                sb.Append("</span>");
            }
        }

    } /* End of Class CellDef */

    /* テーブル定義 */
    private List<List<CellDef>>    celldefs = null;
    private int nrow;
    private string cellclass;



    /// <summary>
    ///   セル定義を指定して、表フォーマッタを作る。
    ///   全セル定義を','でつなげた文字列で指定するバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     opflag==falseのときには、op項目は排除される。
    ///   </para>
    /// </remarks>
    public TableFormatter(string defstring, bool opflag) {
        nrow = 0;
        cellclass = "elem";
        ParseCellDefs(defstring, opflag);
    }

    /// <summary>
    ///   セル定義を指定して、表フォーマッタを作る。
    ///   各セルの定義文字列を配列で渡すバージョン。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     opflag==falseのときには、op項目は排除される。
    ///   </para>
    ///   <para>
    ///     セル定義テキストのフォーマットは次の通り。
    ///     - 1段目定義/2段目定義
    ///     - 個別定義 = データ名:表示名:オプション[:オプション...]
    ///     - オプション =
    ///       - W数値:表示幅
    ///       - P:カッコ付き, I:副情報, O:操作項目
    ///       - DF:日時FULL, DS:日時SHORT, DN:日時NOYEAR DD:日時DATEONLY
    ///       - K文字列:空欄時表示文字列
    ///       - SA: ソート可能(デフォルト昇順), SD:ソート可能(デフォルト降順)
    ///       - T項目名:項目名を使ったツールチップ付き
    ///   </para>
    /// </remarks>
    public TableFormatter(string[] defstrings, bool opflag=true) {
        nrow = 0;
        cellclass = "elem";
        ParseCellDefs(defstrings, opflag);
    }

    /// <summary>
    ///   カラム数を返す。
    /// </summary>
    public int GetColumns() {
        int n = 0;
        foreach(List<CellDef> column in celldefs){
            int ncell = 0;
            foreach(CellDef cell in column){
                if((cell.recordname != null) && (cell.recordname != "") && (cell.style != CellDef.Style.HIDE))
                    ncell++;
            }
            if(ncell > 0)
                n++;
        }
        return n;
    }

    /// <summary>
    ///   現在の行数を返す。
    /// </summary>
    public int GetRows() {
        return nrow;
    }

    /// <summary>
    ///   次に描画する行の番号をセットする。
    /// </summary>
    public void SetRows(int n) {
        nrow = n;
    }

    /// <summary>
    ///   セルの描画クラス(CSSクラス)をセットする。
    /// </summary>
    public void SetCellClass(string css) {
        cellclass = css;
    }

    /// <summary>
    ///   指定した項目名欄を非表示にする。
    /// </summary>
    public bool Hide(string recordname) {
        int n = 0;
        foreach(List<CellDef> column in celldefs){
            foreach(CellDef cell in column){
                if(cell.recordname == recordname){
                    cell.style = CellDef.Style.HIDE;
                    n++;
                }
            }
        }
        return (n > 0);
    }

    /// <summary>
    ///   recで示されるデータをCSV形式の1行にした文字列にして返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     データ値にカンマが含まれる場合には、データ値はダブルクォートされる。
    ///   </para>
    /// </remarks>
    public string GetCsv(Record rec) {
        StringBuilder text = new StringBuilder();
        bool first = true;
        foreach(List<CellDef> column in celldefs){
            foreach(CellDef cell in column){
                if(first)
                    first = false;
                else
                    text.Append(",");
                string txt = cell.GetData(rec);
                if(txt.Contains(",")){
                    text.Append("\"");
                    text.Append(txt);
                    text.Append("\"");
                }else{
                    text.Append(txt);
                }
            }
        }
        return text.ToString();
    }

    /// <summary>
    ///   ソート変更アンカー付きのテーブルヘッダHTML文字列を作成する。
    /// </summary>
    public StringBuilder GetSortableHeader(Translatable page, StringBuilder text, string sorttype, string sortorder, string url) {
        text.Append("<thead>\n");
        text.Append("<tr class='head'>\n");
        foreach(List<CellDef> column in celldefs){
            int ncell = 0;
            foreach(CellDef cell in column){
                if((cell.recordname != null) && (cell.recordname != "") && (cell.style != CellDef.Style.HIDE))
                    ncell++;
            }
            if(ncell == 0)
                continue;
            text.Append("<th>");
            foreach(CellDef cell in column){
                if((cell.recordname == null) || (cell.recordname == "") || (cell.style == CellDef.Style.HIDE))
                    continue;
                if(cell.style == CellDef.Style.SUBINFO)
                    text.Append("<div class='subinfo'>");
                else if(cell.style == CellDef.Style.OPERATION)
                    text.Append("<div class='operation'>");
                if(cell.sortable == CellDef.Sortable.NONE){
                    cell.GetHtmlDisplayName(page,text);
                }else if(cell.recordname == sorttype){
                    text.Append("<a href=\"");
                    text.Append(url);
                    if(url.Contains("?"))
                        text.Append("&");
                    else
                        text.Append("?");
                    text.Append("sort_order=");
                    if(sortorder == "ascend"){
                        text.Append("descend\">");
                        cell.GetHtmlDisplayName(page,text);
                        text.Append("▼</a>");
                    } else {
                        text.Append("ascend\">");
                        cell.GetHtmlDisplayName(page,text);
                        text.Append("▲</a>");
                    }
                }else{
                    text.Append("<a href=\"");
                    text.Append(url);
                    if(url.Contains("?"))
                        text.Append("&");
                    else
                        text.Append("?");
                    text.Append("sort_type=");
                    text.Append(cell.recordname);
                    text.Append("&sort_order=");
                    text.Append((cell.sortable == CellDef.Sortable.ASCEND)?"ascend" : "descend");
                    text.Append("\">");
                    cell.GetHtmlDisplayName(page,text);
                    text.Append("</a>");
                }
                if(cell.style != CellDef.Style.NORMAL)
                    text.Append("</div>");
            }
            text.Append("</th>\n");
        }
        text.Append("</tr>\n");
        return text;
    }

    /// <summary>
    ///   recで指定されたデータレコードの値を表示するHTMLを生成する。
    /// </summary>
    public StringBuilder GetHtml(Translatable page, StringBuilder text, Record rec) {
        text.Append("<tr class='");
        if(nrow%2 == 0)
            text.Append("list");
        else
            text.Append("list_alt");
        text.Append("'>\n");

        foreach(List<CellDef> column in celldefs){
            int ncell = 0;
            foreach(CellDef cell in column){
                if((cell.recordname != null) && (cell.recordname != "") && (cell.style != CellDef.Style.HIDE))
                    ncell++;
            }
            if(ncell == 0)
                continue;
            text.Append("<td class='");
            if((column.Count == 1) && (column[0].style == CellDef.Style.OPERATION))
                text.Append("operation");
            else
                text.Append(cellclass);
            text.Append("'>");
            foreach(CellDef cell in column){
                cell.GetDataHtml(page, text, rec);
            }
            text.Append("</td>\n");
        }
        text.Append("</tr>\n");

        nrow++;

        return text;
    }

    private static readonly int[] default_lineselections = {10, 20, 50, 100, 200, 500, 1000, 2000};

    /// <summary>
    ///   表の最後の行のHTMLを出力する。
    /// </summary>
    public StringBuilder GetLastRow(Translatable page, StringBuilder text, int n, string url, int lines) {
        return GetLastRow(page, text, n, url, lines, default_lineselections);
    }

    /// <summary>
    ///   表の最後の行のHTMLを出力する。表示行数選択肢指定バージョン。
    /// </summary>
    public StringBuilder GetLastRow(Translatable page, StringBuilder text, int n, string url, int lines, int[] lineselections) {
        text.Append("<tr class='list_last'><td colspan='");
        text.Append(GetColumns().ToString());
        text.Append("'>\n");
        if((url != null) && (url != "")){
            text.Append("<div class='float_right'>\n");
            text.Append(page._("表示行数変更:"));
            text.Append("<select name=\"lines\" onchange=\"this.form.action='");
            text.Append(url);
            text.Append("'; this.form.submit();\">");
            foreach(int i in lineselections){
                text.Append("<option value='");
                text.Append(i.ToString());
                text.Append("'");
                if(i == lines)
                    text.Append(" selected='selected'");
                text.Append(">");
                text.Append(i.ToString());
                text.Append("</option>");
            }
            text.Append("</select>\n");
            text.Append("</div>\n");
        }
        if(n > 0){
            text.Append(string.Format(page._("...他{0}件"), n.ToString()));
            text.Append("\n");
        }
        text.Append("</td></tr>\n");
        return text;
    }

    private bool ParseCellDefs(string txt, bool opflag) {
        if(celldefs == null)
            celldefs = new List<List<CellDef>>();
        foreach(string columntext in txt.Split(",".ToCharArray())){
            List<CellDef> column = new List<CellDef>();
            foreach(string celltext in columntext.Split("/".ToCharArray())){
                string[] def = celltext.Split(":".ToCharArray());
                CellDef cell = new CellDef();
                if(def.Length < 2){
                    cell.recordname = "ERROR";
                    cell.recordid = 99;
                    cell.displayname = "ERROR";
                    column.Add(cell);
                    continue;
                }
                cell.recordname = def[0];
                cell.recordid = -1;
                cell.displayname = def[1];
                for(int i = 2; i < def.Length; i++){
                    if(def[i].Length == 0)
                        continue;
                    string param = def[i].Substring(1);
                    switch(def[i][0]){
                    case 'W':
                        cell.width = StringUtil.ToInt(param);
                        break;
                    case 'P':
                        cell.decoration = CellDef.Decoration.PAREN;
                        break;
                    case 'G':
                        cell.decoration = CellDef.Decoration.LTGT;
                        break;
                    case 'I':
                        cell.style = CellDef.Style.SUBINFO;
                        break;
                    case 'O':
                        cell.style = CellDef.Style.OPERATION;
                        break;
                    case 'D':
                        if(param.Length == 0)
                            break;
                        switch(param[0]){
                        case 'S':
                            cell.dateformat = CellDef.DateFormat.SHORT;
                            break;
                        case 'N':
                            cell.dateformat = CellDef.DateFormat.NOYEAR;
                            break;
                        case 'D':
                            cell.dateformat = CellDef.DateFormat.DATEONLY;
                            break;
                        }
                        break;
                    case 'K':
                        cell.strikeout = param;
                        break;
                    case 'S':
                        if(param.Length == 0)
                            break;
                        switch(param[0]){
                        case 'A':
                            cell.sortable = CellDef.Sortable.ASCEND;
                            break;
                        case 'D':
                            cell.sortable = CellDef.Sortable.DESCEND;
                            break;
                        }
                        break;
                    case 'T':
                        cell.tooltip = param;
                        break;
                    }
                }
                if((cell.style != CellDef.Style.OPERATION) || opflag)
                    column.Add(cell);
            }
            if(column.Count > 0)
                celldefs.Add(column);
        }
        return true;
    }

    private bool ParseCellDefs(string[] strings, bool opflag) {
        foreach(string txt in strings){
            if(!ParseCellDefs(txt, opflag))
                return false;
        }
        return true;
    }

}

} // End of namespace
