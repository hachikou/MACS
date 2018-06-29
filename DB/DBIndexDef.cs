/// DBIndexDef: インデックス定義.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Xml;
using MACS;

namespace MACS.DB {

/// <summary>
///   DBテーブルのインデックス定義
/// </summary>
public class DBIndexDef {

    /// <summary>
    ///   カラム名一覧
    /// </summary>
    public string[] Columns { get { return columns; }}

    /// <summary>
    ///   指定カラムがインデックスに含まれるか
    /// </summary>
    public bool HasColumn(string colname) {
        foreach(string col in columns) {
            if(col == colname)
                return true;
        }
        return false;
    }

    /// <summary>
    ///   ユニークかどうか
    /// </summary>
    public bool IsUnique { get { return isUnique; }}


    /// <summary>
    ///   カラム名を指定したコンストラクタ
    /// </summary>
    public DBIndexDef(string[] columns_, bool isUnique_=false) {
        columns = new string[columns_.Length];
        for(int i = 0; i < columns_.Length; i++)
            columns[i] = columns_[i];
        isUnique = isUnique_;
    }

    /// <summary>
    ///   XMLファイルからのコンストラクタ
    /// </summary>
    public DBIndexDef(XmlFile xml, XmlElement elem) {
        List<XmlElement> elemlist = xml.GetElements(elem, "column");
        columns = new string[elemlist.Count];
        for(int i = 0; i < elemlist.Count; i++)
            columns[i] = elemlist[i].GetAttribute("name");
        isUnique = ("yes" == elem.GetAttribute("unique"));
    }


    private string[] columns;
    private bool isUnique;
}

} // End of namespace
