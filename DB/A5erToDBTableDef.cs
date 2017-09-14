/*
  * A5ER形式のDBテーブル定義からDBTableDef用のXMLファイルを生成する。
  * $Id: A5erToDBTableDef.cs 1890 2014-06-05 04:34:56Z shibuya $
  *
  * Copyright (C) 2013 Microbrains Inc. All rights reserved.
  * This code was designed and coded by SHIBUYA K.
  */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using MACS;

namespace MACS.DB {

/// <summary>
///   A5ERからDBTableDefのコンバータ
/// </summary>
public class A5erToDBTableDef {

    /// <summary>
    ///   コンバータコンストラクタ
    /// </summary>
    public A5erToDBTableDef() {}

    /// <summary>
    ///   コンバートする
    /// </summary>
    public int Convert(string a5erfile, string outdir) {
        if(!Directory.Exists(outdir))
            Directory.CreateDirectory(outdir);
        int count = 0;
        using(StreamReader sr = FileUtil.Reader(a5erfile, Encoding.UTF8)) {
            if(sr == null)
                throw new IOException("Can't open "+a5erfile+" for reading");
            Regex pat_type = new Regex(@"^[\*@]?(\w+)(\(([0-9,]+)\))?$");
            DBTableDef tabledef = null;
            string line;
            while((line = sr.ReadLine()) != null) {
                line = line.Trim();
                if(line.StartsWith("[") && line.EndsWith("]")) {
                    if((tabledef != null) && (tabledef.Columns != null)) {
                        tabledef.SaveAs(Path.Combine(outdir, tabledef.XmlFileName));
                        count++;
                    }
                    tabledef = null;
                }
                if(line == "[Entity]") {
                    tabledef = new DBTableDef();
                    continue;
                }
                if(tabledef == null)
                    continue;
                string[] kv = line.Split("=".ToCharArray(), 2);
                if(kv.Length < 2)
                    continue;
                switch(kv[0]) {
                case "PName":
                    tabledef.Id = kv[1];
                    tabledef.Name = kv[1];
                    break;
                case "LName":
                    tabledef.Expr = kv[1];
                    break;
                case "Comment":
                    tabledef.Note = kv[1].Replace(@"\n","\n");
                    break;
                case "Field":
                    {
                        string[] x = StringUtil.SplitCSV(kv[1]);
                        if(x.Length < 7)
                            throw new Exception("Invalid Field declaration.");
                        Match mo = pat_type.Match(x[2]);
                        if(!mo.Success)
                            throw new Exception("Invalid type declaration");
                        string type = mo.Groups[1].Value;
                        int length = 0;
                        int fractional = 0;
                        if(mo.Groups[2].Success) {
                            string[] xx = mo.Groups[3].Value.Split(",".ToCharArray());
                            if(xx.Length < 2) {
                                length = StringUtil.ToInt(xx[0]);
                                fractional = 0;
                            } else {
                                length = StringUtil.ToInt(xx[0]);
                                fractional = StringUtil.ToInt(xx[1]);
                            }
                        }
                        DBColumnDef coldef = new DBColumnDef(x[1], // Name
                                                             type, length, fractional,
                                                             x[3]=="", // Nullable
                                                             x[4]!="", // PK
                                                             x[5]);
                        coldef.Expr = x[0];
                        coldef.Note = x[6].Replace(@"\n","\n");
                        tabledef.AddColumn(coldef);
                    }
                    break;
                case "Index":
                    {
                        kv = kv[1].Split("=".ToCharArray(), 2);
                        if(kv.Length < 2)
                            throw new Exception("Invalid Index declaration.");
                        string[] x = StringUtil.SplitCSV(kv[1]);
                        if(x.Length < 2)
                            throw new Exception("Invalid Index declaration..");
                        string[] columns = new string[x.Length-1];
                        for(int i = 0; i < x.Length-1; i++)
                            columns[i] = x[i+1];
                        DBIndexDef inddef = new DBIndexDef(columns, x[0]=="1");
                        tabledef.AddIndex(inddef);
                    }
                    break;
                }
            }
            if((tabledef != null) && (tabledef.Columns != null)) {
                tabledef.SaveAs(Path.Combine(outdir, tabledef.XmlFileName));
                count++;
            }
            sr.Close();
        }
        return count;
    }

}

} // End of namespace
