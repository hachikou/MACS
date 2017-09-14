/*
  * SQL検索条件管理クラス
  * $Id: DBCondition.cs 1890 2014-06-05 04:34:56Z shibuya $
  *
  * Copyright (C) 2011-2012 Microbrains Inc. All rights reserved.
  * This code was designed and coded by SHIBUYA K.
  */

using System;
using System.Text;
using MACS;

namespace MACS.DB {

/// <summary>
///   SQL検索条件管理クラス
/// </summary>
public class DBCondition {

    public const string StringParamWord = "XXX";
    public const string NumberParamWord = "###";


    /// <summary>
    ///   条件コード
    /// </summary>
    public enum Code {
        Free,
        Equals,
        NotEquals,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        GreaterOrEqual,
        GreaterThan,
        LessOrEqual,
        LessThan,
        Between,
        NotBetween,
        In,
        NotIn,
        Exists,
        NotExists,
        CollateEquals,
        NotCollateEquals,
        CollateContains,
        NotCollateContains,
        CollateStartsWith,
        CollateEndsWith,
    }

    public static string CodeName(Code code) {
        switch(code) {
        case Code.Equals:
            return "次である";
        case Code.NotEquals:
            return "次でない";
        case Code.Contains:
            return "次を含む";
        case Code.NotContains:
            return "次を含まない";
        case Code.StartsWith:
            return "次で始まる";
        case Code.EndsWith:
            return "次で終わる";
        case Code.GreaterOrEqual:
            return "次以上";
        case Code.GreaterThan:
            return "次より大きい";
        case Code.LessOrEqual:
            return "次以下";
        case Code.LessThan:
            return "次より小さい";
        case Code.Between:
            return "次の間";
        case Code.NotBetween:
            return "次の間でない";
        case Code.In:
            return "次のいずれか";
        case Code.NotIn:
            return "次のいずれでもない";
        case Code.CollateEquals:
            return "次と同じ言葉";
        case Code.NotCollateEquals:
            return "次と同じ言葉でない";
        case Code.CollateContains:
            return "次の言葉を含む";
        case Code.NotCollateContains:
            return "次の言葉を含まない";
        case Code.CollateStartsWith:
            return "次の言葉で始まる";
        case Code.CollateEndsWith:
            return "次の言葉で終わる";
        default:
            return "その他";
        }
    }


    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public DBCondition(string title_, string expr_) {
        if(String.IsNullOrEmpty(title_))
            title = "不明";
        else
            title = title_;
        setExpr(expr_);
    }

    /// <summary>
    ///   簡易設定コンストラクタ
    /// </summary>
    public DBCondition(DBColumnDef col) {
        Setup(col);
    }

    /// <summary>
    ///   簡易設定コンストラクタ
    /// </summary>
    public DBCondition(DBColumnDef col, Code code) {
        Setup(col, code);
    }

    /// <summary>
    ///   空のコンストラクタ
    /// </summary>
    public DBCondition() {
        title = "全て";
        expr = "1=1";
        paramLength = new int[0];
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public DBCondition(DBCondition src) {
        title = src.title;
        expr = src.expr;
        if(src.paramLength != null) {
            paramLength = new int[src.paramLength.Length];
            for(int i = 0; i < paramLength.Length; i++)
                paramLength[i] = src.paramLength[i];
        }
        isCustom = src.isCustom;
    }

    /// <summary>
    ///   条件の標題
    /// </summary>
    public string Title {
        get { return title; }
        set { title = value; }
    }

    /// <summary>
    ///   条件式
    /// </summary>
    public string Expr {
        get { return expr; }
        set { setExpr(value); }
    }

    /// <summary>
    ///   パラメータ数
    /// </summary>
    public int NParams {
        get { return paramLength.Length; }
    }

    /// <summary>
    ///   パラメータの文字数
    /// </summary>
    public int[] ParamLengthList {
        get { return paramLength; }
    }

    /// <summary>
    ///   パラメータの文字数
    /// </summary>
    public int ParamLength1 {
        get { return GetParamLength(0); }
    }

    /// <summary>
    ///   パラメータの文字数
    /// </summary>
    public int ParamLength2 {
        get { return GetParamLength(1); }
    }

    /// <summary>
    ///   パラメータの文字数
    /// </summary>
    public int ParamLength3 {
        get { return GetParamLength(2); }
    }

    /// <summary>
    ///   パラメータの文字数
    /// </summary>
    public int ParamLength4 {
        get { return GetParamLength(3); }
    }

    /// <summary>
    ///   パラメータの文字数を獲得する
    /// </summary>
    public int GetParamLength(int i) {
        if((i < 0) || (i >= paramLength.Length))
            return 0;
        return paramLength[i];
    }

    /// <summary>
    ///   パラメータの文字数をセットする
    /// </summary>
    public void SetParamLength(int i, int len) {
        if((i < 0) || (i >= paramLength.Length))
            return;
        paramLength[i] = len;
    }

    /// <summary>
    ///   パラメータの文字数をセットする（全パラメータ一斉）
    /// </summary>
    public void SetParamLength(int len) {
        for(int i = 0; i < paramLength.Length; i++)
            paramLength[i] = len;
    }

    /// <summary>
    ///   カスタム条件項目かどうか
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     DBViewで検索条件を管理する時の便宜をはかるための項目。本クラス自身
    ///     ではなにも参照していない。
    ///   </para>
    /// </remarks>
    public bool IsCustom {
        get { return isCustom; }
        set { isCustom = value; }
    }


    /// <summary>
    ///   簡易設定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定したカラム定義に沿って標題と条件式を自動生成する
    ///   </para>
    /// </remarks>
    public void Setup(DBColumnDef col) {
        DBCondition.Code code;
        int width;
        switch(col.Type) {
        case "NUMBER":
            code = DBCondition.Code.Between;
            width = col.Length;
            break;
        case "BOOLEAN":
            code = DBCondition.Code.Equals;
            width = col.Length;
            break;
        case "DATETIME":
            code = DBCondition.Code.Between;
            width = 19;
            break;
        case "DATE":
            code = DBCondition.Code.Between;
            width = 10;
            break;
        case "TIME":
            code = DBCondition.Code.Between;
            width = 8;
            break;
        case "VARCHAR":
            if(col.Length >= 20)
                code = DBCondition.Code.Contains;
            else
                code = DBCondition.Code.Equals;
            width = col.Length;
            break;
        default:
            code = DBCondition.Code.Equals;
            width = col.Length;
            break;
        }
        Setup(col, code);
        SetParamLength((width>24)?24:width);
    }

    /// <summary>
    ///   簡易設定（条件コードを指定するバージョン）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定したカラム定義に沿って標題と条件式を自動生成する
    ///   </para>
    /// </remarks>
    public void Setup(DBColumnDef col, Code code) {
        title = col.Name+"が"+CodeName(code);
        expr = col.Name+" ";
        switch(code) {
        case Code.Equals:
            expr += "= "+paramString(col);
            break;
        case Code.NotEquals:
            expr += "<> "+paramString(col);
            break;
        case Code.Contains:
            expr += "LIKE '%"+StringParamWord+"%'";
            break;
        case Code.NotContains:
            expr += "NOT LIKE '%"+StringParamWord+"%'";
            break;
        case Code.StartsWith:
            expr += "LIKE '"+StringParamWord+"%'";
            break;
        case Code.EndsWith:
            expr += "LIKE '%"+StringParamWord+"'";
            break;
        case Code.GreaterOrEqual:
            expr += ">= "+paramString(col);
            break;
        case Code.GreaterThan:
            expr += "> "+paramString(col);
            break;
        case Code.LessOrEqual:
            expr += "<= "+paramString(col);
            break;
        case Code.LessThan:
            expr += "< "+paramString(col);
            break;
        case Code.Between:
            expr += "BETWEEN "+paramString(col)+" AND "+paramString(col);
            break;
        case Code.NotBetween:
            expr += "NOT BETWEEN "+paramString(col)+" AND "+paramString(col);
            break;
        case Code.In:
            expr += "IN ("+paramString(col)+","+paramString(col)+","+paramString(col)+","+paramString(col)+")";
            break;
        case Code.NotIn:
            expr += "NOT IN ("+paramString(col)+","+paramString(col)+","+paramString(col)+","+paramString(col)+")";
            break;
        case Code.CollateEquals:
            expr += "= "+paramString(col)+" COLLATE utf8_unicode_ci";
            break;
        case Code.NotCollateEquals:
            expr += "<> "+paramString(col)+" COLLATE utf8_unicode_ci";
            break;
        case Code.CollateContains:
            expr += "LIKE '%"+StringParamWord+"%' COLLATE utf8_unicode_ci";
            break;
        case Code.NotCollateContains:
            expr += "NOT LIKE '%"+StringParamWord+"%' COLLATE utf8_unicode_ci";
            break;
        case Code.CollateStartsWith:
            expr += "LIKE '"+StringParamWord+"%' COLLATE utf8_unicode_ci";
            break;
        case Code.CollateEndsWith:
            expr += "LIKE '%"+StringParamWord+"' COLLATE utf8_unicode_ci";
            break;
        default:
            expr += "IS NOT NULL"; // 適当
            break;
        }
        setExpr(expr);
        int len = col.Length+1;
        if(len < 3)
            len = 3;
        if(len > 40)
            len = 40;
        SetParamLength(len);
    }

    /// <summary>
    ///   SQLのWHERE条件文を生成する（パラメータ無し版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     'WHERE'キーワード自身は付かない。
    ///   </para>
    /// </remarks>
    public override string ToString() {
        return ToString(new object[0]);
    }

    /// <summary>
    ///   SQLのWHERE条件文を生成する（パラメータ1個版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     'WHERE'キーワード自身は付かない。
    ///   </para>
    /// </remarks>
    public string ToString(object param1) {
        return ToString(new object[]{param1});
    }

    /// <summary>
    ///   SQLのWHERE条件文を生成する（パラメータ2個版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     'WHERE'キーワード自身は付かない。
    ///   </para>
    /// </remarks>
    public string ToString(object param1, object param2) {
        return ToString(new object[]{param1, param2});
    }

    /// <summary>
    ///   SQLのWHERE条件文を生成する（パラメータ3個版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     'WHERE'キーワード自身は付かない。
    ///   </para>
    /// </remarks>
    public string ToString(object param1, object param2, object param3) {
        return ToString(new object[]{param1, param2, param3});
    }

    /// <summary>
    ///   SQLのWHERE条件文を生成する（パラメータ4個版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     'WHERE'キーワード自身は付かない。
    ///   </para>
    /// </remarks>
    public string ToString(object param1, object param2, object param3, object param4) {
        return ToString(new object[]{param1, param2, param3, param4});
    }

    /// <summary>
    ///   SQLのWHERE条件文を生成する（可変長パラメータ版）
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     'WHERE'キーワード自身は付かない。
    ///   </para>
    /// </remarks>
    public string ToString(object[] paramlist) {
        if(paramlist == null)
            throw new ArgumentException("Parameter idnew is null.");
        if(paramlist.Length != NParams)
            throw new ArgumentException(String.Format("Invalid number of parameters. (required={0}, supplied={1})", NParams, paramlist.Length));
        StringBuilder sb = new StringBuilder();
        int i = 0;
        int ptr = 0;
        int nptr;
        string pword;
        while((i < paramlist.Length) && ((pword = nextParamWord(expr, ptr, out nptr)) != null)) {
            sb.Append(expr.Substring(ptr, nptr-ptr));
            string x = (paramlist[i]==null)?null:paramlist[i].ToString();
            if(String.IsNullOrEmpty(x)) {
                if(pword == NumberParamWord)
                    x = "0";
                else
                    x = "";
            }
            sb.Append(DBCon.LikeEscape(x));
            i++;
            ptr = nptr+pword.Length;
        }
        sb.Append(expr.Substring(ptr));
        return sb.ToString();
    }


    private string title;
    private string expr;
    private int[] paramLength;
    private bool isCustom;


    private void setExpr(string ex) {
        if(String.IsNullOrEmpty(ex))
            expr = "1=1";
        else
            expr = ex;
        int paramCount = 0;
        int ptr = 0;
        string pword;
        while((pword = nextParamWord(ex, ptr, out ptr)) != null) {
            ptr += pword.Length;
            paramCount++;
        }
        if((paramLength == null) || (paramLength.Length == 0)) {
            paramLength = new int[paramCount];
            SetParamLength(10);
        } else if(paramLength.Length < paramCount) {
            int[] newParamLength = new int[paramCount];
            for(int i = 0; i < paramLength.Length; i++)
                newParamLength[i] = paramLength[i];
            for(int i = paramLength.Length; i < paramCount; i++)
                newParamLength[i] = paramLength[paramLength.Length-1];
            paramLength = newParamLength;
        } else if(paramLength.Length > paramCount) {
            int[] newParamLength = new int[paramCount];
            for(int i = 0; i < paramCount; i++)
                newParamLength[i] = paramLength[i];
            paramLength = newParamLength;
        }
    }

    private static string paramString(DBColumnDef col) {
        switch(col.Type) {
        case "NUMBER":
        case "BOOLEAN":
            return NumberParamWord;
        default:
            return "'"+StringParamWord+"'";
        }
    }

    private static string nextParamWord(string expr, int ptr, out int paramptr) {
        int sptr = expr.IndexOf(StringParamWord, ptr);
        int nptr = expr.IndexOf(NumberParamWord, ptr);
        if(sptr < 0) {
            if(nptr < 0) {
                paramptr = -1;
                return null;
            }
            paramptr = nptr;
            return NumberParamWord;
        }
        if((nptr < 0) || (sptr < nptr)) {
            paramptr = sptr;
            return StringParamWord;
        }
        paramptr = nptr;
        return NumberParamWord;
    }

}

} // End of namespace
