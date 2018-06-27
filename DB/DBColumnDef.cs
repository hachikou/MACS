/// DBColumnDef: DBカラム定義.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Xml;
using System.Text;
using MACS;

namespace MACS.DB {

/// <summary>
///   DBテーブルのカラム定義
/// </summary>
public class DBColumnDef {

    /// <summary>
    ///   カラム名
    /// </summary>
    public string Name { get { return name; }}

    /// <summary>
    ///   カラム内容
    /// </summary>
    public string Expr {
        get { return String.IsNullOrEmpty(expr)?name:expr; }
        set { expr = value; }
    }

    /// <summary>
    ///   データ型
    /// </summary>
    public string Type { get { return type; }}

    /// <summary>
    ///   文字長
    /// </summary>
    public int Length { get { return length; }}

    /// <summary>
    ///   小数点下桁数
    /// </summary>
    public int FractionalLength { get { return fractionalLength; }}

    /// <summary>
    ///   NULL可かどうか
    /// </summary>
    public bool Nullable { get { return nullable; }}

    /// <summary>
    ///   主キーかどうか
    /// </summary>
    public bool PrimaryKey { get { return primaryKey; }}

    /// <summary>
    ///   デフォルト値
    /// </summary>
    public string DefaultValue { get { return defaultValue; }}

    /// <summary>
    ///   説明文
    /// </summary>
    public string Note {
        get { return note;}
        set { note = value; }
    }

    /// <summary>
    ///   元のカラム名（データ移行用）
    /// </summary>
    public string OrgName {
        get { return orgName; }
        set { orgName = value; }
    }

    /// <summary>
    ///   Btrieveファイル上のバイトサイズ（データ移行用）
    /// </summary>
    public int ByteSize {
        get { return byteSize; }
        set { byteSize = value; }
    }


    /// <summary>
    ///   各パラメータを指定したコンストラクタ
    /// </summary>
    public DBColumnDef(string name_, string type_, int length_, int fractionalLength_, bool nullable_, string defaultValue_) {
        name = name_;
        type = type_;
        length = length_;
        fractionalLength = fractionalLength_;
        nullable = nullable_;
        defaultValue = defaultValue_;
        primaryKey = false;
    }

    /// <summary>
    ///   各パラメータを指定したコンストラクタ（プライマリキー指定付き）
    /// </summary>
    public DBColumnDef(string name_, string type_, int length_, int fractionalLength_, bool nullable_, bool primaryKey_, string defaultValue_) {
        name = name_;
        type = type_;
        length = length_;
        fractionalLength = fractionalLength_;
        nullable = nullable_;
        primaryKey = primaryKey_;
        defaultValue = defaultValue_;
    }

    /// <summary>
    ///   XmlElementからのコンストラクタ
    /// </summary>
    public DBColumnDef(XmlElement el) {
        name = el.GetAttribute("name");
        expr = el.GetAttribute("expr");
        type = el.GetAttribute("type");
        length = StringUtil.ToInt(el.GetAttribute("length"),0);
        fractionalLength = StringUtil.ToInt(el.GetAttribute("fractional"),0);
        nullable = !String.IsNullOrEmpty(el.GetAttribute("nullable"));
        primaryKey = !String.IsNullOrEmpty(el.GetAttribute("pk"));
        defaultValue = el.HasAttribute("default")?el.GetAttribute("default"):null;
        note = XmlFile.GetText(el, false).Trim();
    }

    private string name;
    private string expr;
    private string note;
    private string type;
    private int length;
    private int fractionalLength;
    private bool nullable;
    private bool primaryKey;
    private string defaultValue;
    private string orgName;
    private int byteSize;

}

} // End of namespace
