/// CascadedAttribute: 階層的属性管理機構.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Xml;
using MACS;

namespace MACS {

/// <summary>
///   属性管理機構
/// </summary>
public class CascadedAttribute {

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    /// <param name="elem">属性を読み取るXML要素</param>
    /// <param name="parent_">親の属性管理機構</param>
    /// <param name="childClassKey_">子のクラス属性を探すときの付加キーワード</param>
    public CascadedAttribute(XmlElement elem, CascadedAttribute parent_=null, string childClassKey_="") {
        parent = parent_;
        defaultClassName = elem.Name;
        childClassKey = childClassKey_;
        load(elem);
    }

    /// <summary>
    ///   クラス定義を追加で取り込む
    /// </summary>
    public void LoadClass(XmlElement elem) {
        loadClass(elem);
    }
    
    /// <summary>
    ///   属性値取得
    /// </summary>
    public string Get(string name, string defValue=null, int depth=0) {
        if(depth >= 256) // 循環参照防止
            return defValue;
        string val;
        // XML属性に定義があればそれを利用する
        if(dict.TryGetValue(name, out val)) {
            return val;
        }
        if(parent != null) {
            // クラス指定があれば親属性からクラス属性を獲得する
            if(dict.TryGetValue("class", out val)) {
                val = parent.getClassAttribute(val, name, depth+1);
                if(val != null)
                    return val;
            }
            // デフォルトクラス属性を獲得する（トップレベルで呼ばれた時のみ）
            if(depth == 0) {
                if(!String.IsNullOrEmpty(defaultClassName)) {
                    val = parent.getClassAttribute(defaultClassName, name, depth+1);
                    if(val != null) {
                        return val;
                    }
                }
                val = parent.getClassAttribute("default", name, depth+1);
                if(val != null) {
                    return val;
                }
            }
        }
        return defValue;
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public int Get(string name, int defValue) {
        return StringUtil.ToInt(Get(name), defValue);
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public long Get(string name, long defValue) {
        return StringUtil.ToLong(Get(name), defValue);
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public uint Get(string name, uint defValue) {
        return StringUtil.ToUInt(Get(name), defValue);
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public ulong Get(string name, ulong defValue) {
        return StringUtil.ToULong(Get(name), defValue);
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public float Get(string name, float defValue) {
        return StringUtil.ToFloat(Get(name), defValue);
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public double Get(string name, double defValue) {
        return StringUtil.ToDouble(Get(name), defValue);
    }

    /// <summary>
    ///   属性値取得
    /// </summary>
    public bool Get(string name, bool defValue) {
        return StringUtil.ToBool(Get(name), defValue);
    }

    /// <summary>
    ///   name属性値取得
    /// </summary>
    public string Name {
        get {
            string name;
            if(dict.TryGetValue("name", out name)) {
                return name;
            }
            return "";
        }
    }

    /// <summary>
    ///   CascadedAttributeで特別扱いされるXMLタグかどうか
    /// </summary>
    public static bool IsSpecialTag(string tagname) {
        switch(tagname) {
        case "class":
        case "default":
            return true;
        default:
            return false;
        }
    }

    /// <summary>
    ///   指定クラス名の属性定義を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定名のクラスが無いときはthisを返します。
    ///   </para>
    /// </remarks>
    public CascadedAttribute GetClass(string classname) {
        if(parent == null)
            return this;
        CascadedAttribute attr = parent.getClass(classname);
        return (attr==null)?this:attr;
    }

    /// <summary>
    ///   本属性が指定したクラスに属しているか確認する
    /// </summary>
    public bool HasClass(string classname) {
        return hasClass(classname, 0);
    }
    
    
    protected virtual CascadedAttribute CreateChild(XmlElement elem) {
        return new CascadedAttribute(elem, this);
    }
    
    protected CascadedAttribute parent;
    protected string defaultClassName;
    protected string childClassKey;
    protected Dictionary<string,string> dict = new Dictionary<string,string>();
    protected Dictionary<string,CascadedAttribute> classDict = new Dictionary<string,CascadedAttribute>();

    
    private void load(XmlElement elem) {
        // 属性値をdictに取り込む
        foreach(XmlAttribute attr in elem.Attributes) {
            dict[attr.Name] = attr.Value;
        }
        loadClass(elem);
    }

    private void loadClass(XmlElement elem) {
        // クラス定義を取り込む
        foreach(XmlNode node in elem.ChildNodes) {
            if((node.NodeType == XmlNodeType.Element) && ((node.Name == "class") || (node.Name == "default"))) {
                CascadedAttribute classAttr = CreateChild((XmlElement)node);
                string className = (node.Name == "default")?"default":classAttr.Name;
                if(!String.IsNullOrEmpty(className)) {
                    classDict[className] = classAttr;
                }
            }
        }
    }
    
    /// <summary>
    ///   クラス属性値取得
    /// </summary>
    private string getClassAttribute(string className, string name, int depth=0) {
        if(depth >= 256) // 循環参照防止
            return null;
        CascadedAttribute classAttr;
        if(!String.IsNullOrEmpty(className)) {
            // 列挙されているクラス名を順に探す
            foreach(string cn in StringUtil.SplitCommand(className)) {
                if(classDict.TryGetValue(cn, out classAttr)) {
                    string val = classAttr.Get(name, null, depth+1);
                    if(val != null)
                        return val;
                }
                if(parent != null) {
                // 親のクラス定義を探す
                    string val = parent.getClassAttribute(cn, name, depth+1);
                    if(val != null)
                        return val;
                }
            }
        }
        // この属性のクラス定義を探す
        if(parent != null){
            string childClassName = this.Get(childClassKey+"class", null, depth+1);
            if(!String.IsNullOrEmpty(childClassName))
                return parent.getClassAttribute(childClassName, name, depth+1);
        }
        return null;
    }

    private CascadedAttribute getClass(string className) {
        CascadedAttribute classAttr;
        if(classDict.TryGetValue(className, out classAttr)) {
            return classAttr;
        }
        if(parent != null) {
            return parent.getClass(className);
        }
        return null;
    }

    private bool hasClass(string className, int depth) {
        if(depth >= 256)
            return false;
        string val;
        if((depth == 0) && (defaultClassName == className))
            return true;
        if(!dict.TryGetValue("class", out val))
            return false;
        foreach(string cname in StringUtil.SplitCommand(val)) {
            if(cname == className)
                return true;
            CascadedAttribute xclass = getClass(cname);
            if((xclass != null) && xclass.hasClass(className, depth+1))
                return true;
        }
        return false;
    }

}

} // End of namespace
