/*! @file HttpTemplatePage.cs
 * @brief テンプレートページレンダラ
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Web;
using System.Reflection;
using MACS;

namespace MACS.HttpServer {

/// <summary>
///   テンプレートページレンダラ
/// </summary>
public abstract class HttpTemplatePage : HttpNlsSupport {

    /// <summary>
    ///   テンプレートファイルを置くディレクトリを指定する。
    /// </summary>
    public static void SetTemplateDir(string dir) {
        m_defaulttemplatedir = dir;
    }

    /// <summary>
    ///   テンプレートファイルの拡張子を指定する。
    ///   デフォルトは".html"。
    /// </summary>
    public static void SetTemplateSuffix(string suffix) {
        m_templatesuffix = suffix;
    }

    /// <summary>
    ///   テンプレート変数のprefixとpostfixを指定する。
    /// </summary>
    public static void SetPrefixPostfix(string prefix, string postfix) {
        m_prefix = prefix;
        m_postfix = postfix;
    }

    /// <summary>
    ///   テンプレートコマンドのprefixとpostfixを指定する。
    /// </summary>
    public static void SetCommandPrefixPostfix(string prefix, string postfix) {
        m_commandprefix = prefix;
        m_commandpostfix = postfix;
    }

    /// <summary>
    ///   コメント開始終了記号を指定する。
    /// </summary>
    public static void SetCommentMark(string commentbegin, string commentend) {
        m_commentbegin = commentbegin;
        m_commentend = commentend;
    }

    /// <summary>
    ///   翻訳文字列のprefixとpostfixを指定する。
    /// </summary>
    public static void SetTranslatorPrefixPostfix(string prefix, string postfix) {
        m_transprefix = prefix;
        m_transpostfix = postfix;
    }

    /// <summary>
    ///   初期化
    /// </summary>
    public HttpTemplatePage() {
        m_dir = m_defaulttemplatedir;
        RegisterCommand("include", new Command(this.CommandInclude));
        RegisterCommand("if", new Command(this.CommandIf));
        RegisterCommand("endif", new Command(this.CommandEndif));
        RegisterCommand("assign", new Command(this.CommandAssign));
        RegisterCommand("for", new Command(this.CommandFor));
        RegisterCommand("endfor", new Command(this.CommandEndfor));
        RegisterCommand("def", new Command(this.CommandDef));
        RegisterCommand("enddef", new Command(this.CommandEnddef));
        RegisterCommand("extract", new Command(this.CommandExtract));
    }


    /// <summary>
    ///   テンプレート変数の値定義
    /// </summary>
    /// <param name="var">変数名</param>
    /// <param name="val">変数の値</param>
    /// <remarks>
    ///   <para>
    ///     変数の値は、ToString()メソッドが定義されている任意のオブジェクト。
    ///
    ///     このオブジェクトがメンバを持つ場合、テンプレート内では
    ///     "変数名.メンバ名"でそのメンバの値を取り出すことができる。
    ///   </para>
    /// </remarks>
    protected void Assign(string var, object val) {
        if(m_dict == null)
            m_dict = new ObjectDictionary(ObjectDictionary.KeyLengthComparer);
        m_dict[var] = val;
        //LOG_DEBUG("Assign "+var+"=("+val.GetType().Name+")'"+val.ToString()+"'");
    }

    /// <summary>
    ///   テンプレート変数の値消去
    /// </summary>
    /// <param name="var">変数名</param>
    protected void Clear(string var) {
        if(m_dict == null)
            return;
        m_dict.Remove(var);
    }

    /// <summary>
    ///   テンプレート変数の値消去（全部）
    /// </summary>
    protected void Clear() {
        m_dict = null;
    }

    /// <summary>
    ///   WebControlフィールドを全部自動的にAssignする。
    /// </summary>
    protected void AssignWebControls() {
        foreach(FieldInfo fi in GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy)){
            if(!fi.FieldType.IsSubclassOf(typeof(WebControl)))
                continue;
            WebControl i = (WebControl)(fi.GetValue(this));
            if(i == null){
                i = (WebControl)(fi.FieldType.GetConstructor(Type.EmptyTypes).Invoke(null));
                fi.SetValue(this, i);
            }
            if((fi.Name != null) && (fi.Name != "")) {
                i.Name = fi.Name;
                Assign(fi.Name, i);
            }
            if(i is TranslatableWebControl) {
                (i as TranslatableWebControl).Translator = this;
            }
        }
    }

    /// <summary>
    ///   デフォルトテンプレートファイルを出力する。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     "ページ名.html" または "クラス名.html" というテンプレートファイルが
    ///     出力される。
    ///   </para>
    /// </remarks>
    protected void RenderTemplate() {
        string fname1 = GetNlsFileName(Path.Combine(m_dir, m_pagename+m_templatesuffix));
        if(FileExists(fname1)){
            RenderTemplatePath(fname1);
            return;
        }
        string fname2 = GetNlsFileName(Path.Combine(m_dir, GetType().Name+m_templatesuffix));
        if(FileExists(fname2)){
            RenderTemplatePath(fname2);
            return;
        }
        LOG_ERR(string.Format("Template file '{0}' or '{1}' are needed.", fname1, fname2));
        Render(404);
    }

    /// <summary>
    ///   指定テンプレートファイルを出力する。
    /// </summary>
    /// <param name="templatefile">テンプレートファイル名</param>
    protected void RenderTemplate(string templatefile) {
        RenderTemplatePath(Path.Combine(m_dir, templatefile));
    }


    private static string m_defaulttemplatedir = "Page";
    private static string m_templatesuffix = ".html";
    private static string m_prefix = "{";
    private static string m_postfix = "}";
    private static string m_commandprefix = "{!";
    private static string m_commandpostfix = "}";
    private static string m_commentbegin = "{#";
    private static string m_commentend = "#}";
    private static string m_transprefix = "_(\"";
    private static string m_transpostfix = "\")";


    /// <summary>
    ///   テンプレート展開状態保持用クラス
    /// </summary>
    private class TemplateContext {
        public StringBuilder sb;      // 変換後文字列を入れるバッファ
        public ObjectDictionary dict; // 変数定義辞書

        public TemplateContext(StringBuilder sb_, ObjectDictionary dict_) {
            sb = sb_;
            dict = new ObjectDictionary(dict_);
        }
    }

    /// <summary>
    ///   テンプレートコマンドメソッドのデリゲート
    /// </summary>
    private delegate int Command(string line, int index, TemplateContext tc, string commandname, string param);

    private ObjectDictionary m_command;

    /// <summary>
    ///   テンプレートコマンドの登録
    /// </summary>
    private void RegisterCommand(string name, Command cmd) {
        if(m_command == null)
            m_command = new ObjectDictionary(ObjectDictionary.KeyLengthComparer);
        m_command[name] = cmd;
    }


    private ObjectDictionary m_dict;

    private void RenderTemplatePath(string fname) {
        Assign("self", m_pagename);
        TemplateContext tc = new TemplateContext(new StringBuilder(), m_dict);
        RenderTemplateAux(tc, fname);
        Render(tc.sb.ToString());
    }

    private void RenderTemplateAux(TemplateContext tc, string fname) {
        fname = GetNlsFileName(fname);
        string lines;
        using(Stream f = OpenFile(fname)) {
            if(f == null) {
                LOG_ERR(string.Format("Can't open template file '{0}'", fname));
                return;
            }
            using(StreamReader sr = new StreamReader(f, m_encoding)) {
                lines = sr.ReadToEnd();
                sr.Close();
            }
            f.Close();
        }

        // まずlinesを翻訳してしまう
        int i = 0;
        StringBuilder xlines = new StringBuilder();
        while(i < lines.Length){
            if(string.Compare(lines, i, m_transprefix, 0, m_transprefix.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                int endpos = lines.IndexOf(m_transpostfix, i+m_transprefix.Length);
                if(endpos > 0){
                    xlines.Append(_(lines.Substring(i+m_transprefix.Length, endpos-i-m_transprefix.Length)));
                    i = endpos+m_transpostfix.Length;
                }
            }else{
                xlines.Append(lines[i]);
                i++;
            }
        }

        // それを展開
        Dispatch(tc, xlines.ToString());
    }

    private void Dispatch(TemplateContext tc, string line) {
        int i = 0;
        while(i < line.Length){
            // コメント
            if(string.Compare(line, i, m_commentbegin, 0, m_commentbegin.Length, StringComparison.OrdinalIgnoreCase) == 0){
                i = Skip(line, i+m_commentbegin.Length, m_commentbegin, m_commentend);
                continue;
            }

            // テンプレートコマンドの展開
            if(string.Compare(line, i, m_commandprefix, 0, m_commandprefix.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                int endpos = line.IndexOf(m_commandpostfix, i+m_commandprefix.Length);
                if(endpos > 0){
                    string[] cmdparam = line.Substring(i+m_commandprefix.Length, endpos-i-m_commandprefix.Length).Trim().Split(" \t".ToCharArray(), 2);
                    Command cmd = null;
                    foreach(KeyValuePair<string,object> kv in m_command){
                        if(kv.Key == cmdparam[0].ToLower()){
                            cmd = (Command)kv.Value;
                            break;
                        }
                    }
                    if(cmd != null){
                        i = cmd(line, endpos+m_commandpostfix.Length, tc, cmdparam[0],
                                (cmdparam.Length > 1)?cmdparam[1].Trim():"");
                        continue;
                    } else {
                        LOG_ERR(string.Format("Unknown template command '{0}{1}{2}'", m_commandprefix, cmdparam[0], m_commandpostfix));
                    }
                }
            }

            // テンプレート変数の展開
            if(string.Compare(line, i, m_prefix, 0, m_prefix.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                int endpos = line.IndexOf(m_postfix, i+m_prefix.Length);
                if(endpos > 0){
                    string val = GetVariableValue(tc.dict, line.Substring(i+m_prefix.Length, endpos-i-m_prefix.Length));
                    if(val != null){
                        tc.sb.Append(val);
                        i = endpos+m_postfix.Length;
                        continue;
                    }
                }
            }

            // 一文字出力
            tc.sb.Append(line[i]);
            i++;
        }
    }

    private int Skip(string line, int i, string beginmark, string endmark) {
        int n = 0;
        while(i < line.Length) {
            if(string.Compare(line, i, endmark, 0, endmark.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                i += endmark.Length;
                if(n == 0)
                    return i;
                n--;
            } else if(string.Compare(line, i, beginmark, 0, beginmark.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                i += beginmark.Length;
                n++;
            } else {
                i++;
            }
        }
        LOG_ERR(string.Format("Error: Mismatched directive ({0} {1}) pair.", beginmark, endmark));
        return i;
    }

    private int Capture(string line, int i, string beginmark, string endmark, out string ret) {
        StringBuilder sb = new StringBuilder();
        int n = 0;
        while(i < line.Length) {
            if(string.Compare(line, i, endmark, 0, endmark.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                i += endmark.Length;
                if(n == 0) {
                    ret = sb.ToString();
                    return i;
                }
                sb.Append(endmark);
                n--;
            } else if(string.Compare(line, i, beginmark, 0, beginmark.Length, StringComparison.OrdinalIgnoreCase) == 0) {
                i += beginmark.Length;
                sb.Append(beginmark);
                n++;
            } else {
                sb.Append(line[i]);
                i++;
            }
        }
        LOG_ERR(string.Format("Error: Mismatched directive ({0} {1}) pair.", beginmark, endmark));
        ret = "";
        return i;
    }

    private string GetVariableValue(ObjectDictionary dict, string varparam) {
        if(varparam == null)
            return null;
        bool htmlescape = false;
        bool quoteescape = false;
        bool urlescape = false;
        char[] separators = " \t".ToCharArray();
        string[] p = varparam.Trim(separators).Split(separators, 2);
        // 最初の要素が変数名。
        if(p[0].EndsWith(":h")){
            p[0] = p[0].Substring(0,p[0].Length-2);
            htmlescape = true;
        } else if(p[0].EndsWith(":q")){
            p[0] = p[0].Substring(0,p[0].Length-2);
            quoteescape = true;
        } else if(p[0].EndsWith(":u")){
            p[0] = p[0].Substring(0,p[0].Length-2);
            urlescape = true;
        }
        object obj = dict.GetObject(p[0]);
        if(obj == null)
            return null;
        // アサインされたオブジェクトにパラメータをセットする
        if(p.Length > 1)
            SetParameters(p[0], obj, ObjectDictionary.FromString(p[1]));
        // WebControl用の特例: Nameを自動セットする。
        SetParameterIfNull(p[0], obj, "Name", p[0]);
        // オブジェクトの文字列表現を返す。
        if(htmlescape)
            return HE(obj.ToString());
        if(quoteescape)
            return QE(obj.ToString());
        if(urlescape)
            return UE(obj.ToString());
        return obj.ToString();
    }

    private static Regex ExprPattern = new Regex(@"^(.+)(==|!=|&&|\|\|)(.+)$");

    private object GetExpr(ObjectDictionary dict, string varname) {
        if(String.IsNullOrEmpty(varname))
            return "";

        Match m;
        // カッコを評価
        if((varname[0] == '(') && (varname[varname.Length-1] == ')')){
            return GetExpr(dict, varname.Substring(1,varname.Length-2).Trim());
        }

        // 式を評価
        m = ExprPattern.Match(varname);
        if(m.Success) {
            switch(m.Groups[2].ToString()){
            case "==":
                return (GetExpr(dict,m.Groups[1].ToString().Trim()).ToString() == GetExpr(dict,m.Groups[3].ToString().Trim()).ToString());
            case "!=":
                return (GetExpr(dict,m.Groups[1].ToString().Trim()).ToString() != GetExpr(dict,m.Groups[3].ToString().Trim()).ToString());
            case "&&":
                return (Cond(GetExpr(dict,m.Groups[1].ToString().Trim())) && Cond(GetExpr(dict,m.Groups[3].ToString().Trim())));
            case "||":
                return (Cond(GetExpr(dict,m.Groups[1].ToString().Trim())) || Cond(GetExpr(dict,m.Groups[3].ToString().Trim())));
            }
        }

        // 変数を評価
        object obj = dict.GetObject(varname);
        return (obj==null)?"":obj;
    }

    
    private static bool Cond(object obj) {
        if(obj == null)
            return false;
        switch(obj.ToString().ToLower()){
        case "false":
        case "no":
        case "off":
        case "0":
        case "":
            return false;
        }
        return true;
    }

    private void SetParameters(string vname, object obj, ObjectDictionary vars) {
        if(vars.Count == 0)
            return;
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy);
        PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy);
        foreach(KeyValuePair<string,object> kv in vars){
            bool ok = false;
            foreach(FieldInfo fi in fields){
                if(fi.Name.ToLower() == kv.Key.ToLower()){
                    try {
                        fi.SetValue(obj, kv.Value);
                    } catch(ArgumentException e){
                        LOG_WARNING(string.Format("Can't set template variable's field  {0}.{1}{2} ({3})", vname, kv.Key, m_postfix, e.Message));
                    }
                    ok = true;
                    break;
                }
            }
            foreach(PropertyInfo pi in props){
                if(pi.Name.ToLower() == kv.Key.ToLower()){
                    try {
                        pi.SetValue(obj, kv.Value, null);
                    } catch(ArgumentException e){
                        LOG_WARNING(string.Format("Can't set template variable's property  {0}.{1}{2} ({3})", vname, kv.Key, m_postfix, e.Message));
                    }
                    ok = true;
                    break;
                }
            }
            if(!ok){
                LOG_WARNING(string.Format("Template variable {0}{1} does not have '{2}' field.", vname, m_postfix, kv.Key));
            }
        }
    }

    private void SetParameterIfNull(string vname, object obj, string name, object val) {
        name = name.ToLower();
        foreach(FieldInfo fi in obj.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)){
            if(fi.Name.ToLower() == name){
                if(fi.GetValue(obj) == null)
                    fi.SetValue(obj, val);
            }
        }
        foreach(PropertyInfo pi in obj.GetType().GetProperties(BindingFlags.Instance|BindingFlags.Public|BindingFlags.FlattenHierarchy)){
            if(pi.Name.ToLower() == name){
                if(pi.GetValue(obj, null) == null)
                    pi.SetValue(obj, val, null);
            }
        }
    }


    private int CommandInclude(string line, int index, TemplateContext tc, string commandname, string param){
        ObjectDictionary args = ObjectDictionary.FromString(param);
        string fname = null;
        if(args.ContainsKey("0"))
            fname = args["0"].ToString();
        else if(args.ContainsKey("file"))
            fname = args["file"].ToString();
        if(string.IsNullOrEmpty(fname)){
            LOG_ERR(string.Format("Error: {0}{1}{2} directive must have filename argument.", m_commandprefix, commandname, m_commandpostfix));
            return index;
        }
        RenderTemplateAux(tc, Path.Combine(m_dir, fname));
        return index;
    }

    private int CommandIf(string line, int index, TemplateContext tc, string commandname, string param){
        string contents;
        index = Capture(line, index, m_commandprefix+commandname, m_commandprefix+"end"+commandname+m_commandpostfix, out contents);
        if(string.IsNullOrEmpty(param)){
            LOG_ERR(string.Format("Error: Invalid syntax, should be '{0}{1} expression{2}'", m_commandprefix, commandname, m_commandpostfix));
            return index;
        }
        if(param.StartsWith("!")){
            if(Cond(GetExpr(tc.dict,param.Substring(1).Trim())) == false)
                Dispatch(tc, contents);
        }else{
            if(Cond(GetExpr(tc.dict,param)) == true)
                Dispatch(tc, contents);
        }
        return index;
    }

    private int CommandEndif(string line, int index, TemplateContext tc, string commandname, string param){
        LOG_ERR(string.Format("Error: Mismatched {0} directive.", commandname));
        return index;
    }

    private int CommandAssign(string line, int index, TemplateContext tc, string commandname, string param){
        ObjectDictionary args = ObjectDictionary.FromString(param);
        string variable = null;
        if(args.ContainsKey("0"))
            variable = args["0"].ToString();
        else if(args.ContainsKey("var"))
            variable = args["var"].ToString();

        string value = null;
        if(args.ContainsKey("1"))
            value = args["1"].ToString();
        else if(args.ContainsKey("value"))
            value = args["value"].ToString();

        if((variable == null) || (value == null)){
            LOG_ERR(string.Format("Error: Invalid syntax, should be '{0}{1} var-name expression{2}'", m_commandprefix, commandname, m_commandpostfix));
            return index;
        }
        tc.dict[variable] = GetExpr(tc.dict, value);
        return index;
    }

    private int CommandFor(string line, int index, TemplateContext tc, string commandname, string param){
        string contents;
        index = Capture(line, index, m_commandprefix+commandname, m_commandprefix+"end"+commandname+m_commandpostfix, out contents);
        ObjectDictionary args = ObjectDictionary.FromString(param);
        string variable = null;
        if(args.ContainsKey("0"))
            variable = args["0"].ToString();
        else if(args.ContainsKey("var"))
            variable = args["var"].ToString();

        string list = null;
        if(args.ContainsKey("1") && (args["1"].ToString() == "in") && args.ContainsKey("2"))
            list = args["2"].ToString();
        else if(args.ContainsKey("list"))
            list = args["list"].ToString();

        string countvar = null;
        if(args.ContainsKey("count"))
            countvar = args["count"].ToString();
        int countstart = 1;
        if(args.ContainsKey("countstart")) {
            object countstartobj = tc.dict.GetObject(args["countstart"].ToString());
            if(countstartobj != null)
                countstart = StringUtil.ToInt(countstartobj.ToString(), countstart);
        }
        string indexvar = null;
        if(args.ContainsKey("index"))
            indexvar = args["index"].ToString();
        string evenvar = null;
        if(args.ContainsKey("even"))
            evenvar = args["even"].ToString();
        string oddvar = null;
        if(args.ContainsKey("odd"))
            oddvar = args["odd"].ToString();

        if(string.IsNullOrEmpty(variable) || string.IsNullOrEmpty(list)){
            LOG_ERR(string.Format("Error: Invalid syntax, should be '{0}{1} var-name in list-variable {2}'", m_commandprefix, commandname, m_commandpostfix));
            return index;
        }
        object listobj = tc.dict.GetObject(list);
        if(listobj == null) {
            LOG_ERR(string.Format("Error: Object {0} is not assigned.", list));
            return index;
        }
        if(!(listobj is IEnumerable)) {
            LOG_ERR(string.Format("Error: Object {0} is not an Enumerable.", list));
            return index;
        }
        TemplateContext innertc = new TemplateContext(tc.sb, tc.dict);
        int i = 0;
        foreach(object obj in (listobj as IEnumerable)) {
            innertc.dict[variable] = obj;
            if(!String.IsNullOrEmpty(countvar))
                innertc.dict[countvar] = i+countstart;
            if(!String.IsNullOrEmpty(indexvar))
                innertc.dict[indexvar] = i;
            if(!String.IsNullOrEmpty(evenvar))
                innertc.dict[evenvar] = ((i%2) == 0);
            if(!String.IsNullOrEmpty(oddvar))
                innertc.dict[oddvar] = ((i%2) == 1);
            Dispatch(innertc, contents);
            i++;
        }
        return index;
    }

    private int CommandEndfor(string line, int index, TemplateContext tc, string commandname, string param){
        LOG_ERR(string.Format("Error: Mismatched {0} directive.", commandname));
        return index;
    }

    private int CommandDef(string line, int index, TemplateContext tc, string commandname, string param){
        string contents;
        index = Capture(line, index, m_commandprefix+commandname, m_commandprefix+"end"+commandname+m_commandpostfix, out contents);
        ObjectDictionary args = ObjectDictionary.FromString(param);
        string variable = null;
        if(args.ContainsKey("0"))
            variable = args["0"].ToString();
        else if(args.ContainsKey("var"))
            variable = args["var"].ToString();

        if(string.IsNullOrEmpty(variable)){
            LOG_ERR(string.Format("Error: Invalid syntax, should be '{0}{1} var-name{2}'", m_commandprefix, commandname, m_commandpostfix));
            return index;
        }
        tc.dict[variable] = contents;
        return index;
    }

    private int CommandEnddef(string line, int index, TemplateContext tc, string commandname, string param){
        LOG_ERR(string.Format("Error: Mismatched {0} directive.", commandname));
        return index;
    }

    private int CommandExtract(string line, int index, TemplateContext tc, string commandname, string param){
        ObjectDictionary args = ObjectDictionary.FromString(param);
        string variable = null;
        if(args.ContainsKey("0"))
            variable = args["0"].ToString();
        else if(args.ContainsKey("var"))
            variable = args["var"].ToString();

        if(string.IsNullOrEmpty(variable)){
            LOG_ERR(string.Format("Error: Invalid syntax, should be '{0}{1} var-name{2}'", m_commandprefix, commandname, m_commandpostfix));
            return index;
        }
        object obj = tc.dict.GetObject(variable);
        if(obj == null){
            LOG_ERR(string.Format("Error: Object {0} is not assigned.", variable));
            return index;
        }
        Dispatch(tc, obj.ToString());
        return index;
    }


} // End of class HttpPage

} // End of namespace
