/*! @file XmlFile.cs
 * @brief XMLファイルを取り扱うクラス
 * $Id: $
 *
 * Copyright (C) 2008-2012 Microbrains Inc.
 * All Rights Reserved.
 * This code was designed and coded by SHIBUYA K.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MACS {

/// <summary>
///   XMLファイルを取り扱うクラス
/// </summary>
public class XmlFile : IDisposable {

    protected static readonly Encoding encoding = Encoding.UTF8;


    /// <summary>
    ///   XMLファイルを読み込み専用で開く。
    ///   指定ファイルがある場合にはそれを読み込む。
    /// </summary>
    public XmlFile(string filepath, string rootelementname) {
        setup(filepath, rootelementname, false);
    }

    /// <summary>
    ///   XMLファイルを読み込み用/書き込み用を指定して開く。
    ///   指定ファイルがある場合にはそれを読み込む。
    /// </summary>
    public XmlFile(string filepath, string rootelementname, bool writable_) {
        setup(filepath, rootelementname, writable_);
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~XmlFile() {
        Dispose();
    }

    /// <summary>
    ///   使用リソースを解放する。
    /// </summary>
    public void Dispose() {
        Close();
    }

    /// <summary>
    ///   リソースを解放する。
    /// </summary>
    public void Close() {
        if(writable)
            unlockFile();
        rootElement = null;
        document = null;
    }

    /// <summary>
    ///   XMLファイルを書き出す
    /// </summary>
    public void Save() {
        Save(filePath);
    }

    /// <summary>
    ///   XMLファイルを指定名のファイルに書き出す
    /// </summary>
    public void Save(string filename) {
        if(!writable)
            throw new Exception("XML file has not been opened for writing.");
        document.Save(filename);
    }

    /// <summary>
    ///   ドキュメント
    /// </summary>
    public XmlDocument Document {
        get { return document; }
    }

    /// <summary>
    ///   ルート要素
    /// </summary>
    public XmlElement Root {
        get { return rootElement; }
    }

    /// <summary>
    ///   ルート直下のエレメントを得る。
    ///   当該エレメントが無い場合は作成する。
    /// </summary>
    public XmlElement GetSubRoot(string tagname) {
        List<XmlElement> elements = GetElements(rootElement, tagname);
        if(elements.Count == 0) {
            XmlElement el = document.CreateElement(tagname);
            rootElement.AppendChild(el);
            return el;
        }
        return elements[0];
    }

    /// <summary>
    ///   parentで指定したエレメント内のnameエレメントを全て探す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     指定したエレメントが一つも存在しない場合は、空のリストを返す。
    ///   </para>
    /// </remarks>
    public List<XmlElement> GetElements(XmlElement parent, string name) {
        List<XmlElement> list = new List<XmlElement>();
        foreach(XmlNode i in parent.ChildNodes) {
            if((i.NodeType == XmlNodeType.Element) && (i.Name == name))
                list.Add((XmlElement)i);
        }
        return list;
    }

    /// <summary>
    ///   parentで指定したエレメント内の最初に見つかったnameエレメントを返す。
    ///   指定エレメントが無い場合にはnullを返す。
    /// </summary>
    public XmlElement GetElement(XmlElement parent, string name) {
        foreach(XmlNode i in parent.ChildNodes) {
            if((i.NodeType == XmlNodeType.Element) && (i.Name == name))
                return (XmlElement)i;
        }
        return null;
    }

    /// <summary>
    ///   指定エレメント直下のエレメントのうち、アトリビュートが指定値のものを得る。
    ///   当該エレメントが存在しない場合はnullを返す。
    /// </summary>
    public XmlElement GetElementByAttribute(XmlElement parent, string tagname, string attrname, string attrvalue) {
        if(attrvalue == null)
            return null;
        foreach(XmlNode child in parent.ChildNodes) {
            if((child.NodeType == XmlNodeType.Element) && (tagname == child.Name)) {
                if(attrvalue == ((XmlElement)child).GetAttribute(attrname)) {
                    return (XmlElement)child;
                }
            }
        }
        return null;
    }

    /// <summary>
    ///   指定エレメント内に新たなエレメントを追加し、追加したエレメントを返す。
    /// </summary>
    public XmlElement AddElement(XmlElement parent, string tagname) {
        XmlElement el = document.CreateElement(tagname);
        parent.AppendChild(el);
        return el;
    }

    /// <summary>
    ///   parentエレメント内の指定エレメントをすべて削除する。
    ///   削除したエレメント数を返す。
    /// </summary>
    public int RemoveElement(XmlElement parent, string tagname) {
        int count = 0;
        bool removed = true;
        while(removed) {
            removed = false;
            foreach(XmlNode child in parent.ChildNodes) {
                if(child.NodeType == XmlNodeType.Text) { // インデント用のテキストも削除してしまう
                    parent.RemoveChild(child);
                    removed = true;
                    break;
                } else if((child.NodeType == XmlNodeType.Element) && (tagname == child.Name)) {
                    parent.RemoveChild(child);
                    removed = true;
                    count++;
                    break;
                }
            }
        }
        return count;
    }

    /// <summary>
    ///   parentエレメント内のn番目の指定エレメントを削除する。
    ///   削除したらtrue、削除しなかったらfalseを返す。
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     nは0から始まる数値。
    ///   </para>
    /// </remarks>
    public bool RemoveElement(XmlElement parent, string tagname, int n) {
        bool removed = true;
        while(removed){
            removed = false;
            foreach(XmlNode child in parent.ChildNodes) {
                if(child.NodeType == XmlNodeType.Text) { // インデント用のテキストを削除してしまう
                    parent.RemoveChild(child);
                    removed = true;
                    break;
                }
            }
        }
        int c = 0;
        foreach(XmlNode child in parent.ChildNodes) {
            if((child.NodeType == XmlNodeType.Element) && (tagname == child.Name)) {
                if(c == n) {
                    parent.RemoveChild(child);
                    return true;
                }
                c++;
            }
        }
        return false;
    }

    /// <summary>
    ///   parentエレメント内で、属性が指定されたものであるエレメントを削除する。
    ///   削除したエレメント数を返す。
    /// </summary>
    public int RemoveElementByAttribute(XmlElement parent, string tagname, string attrname, string attrvalue) {
        if(attrvalue == null)
            return 0;
        int count = 0;
        bool removed = true;
        while(removed) {
            removed = false;
            foreach(XmlNode child in parent.ChildNodes) {
                if(child.NodeType == XmlNodeType.Text) { // インデント用のテキストも削除してしまう
                    parent.RemoveChild(child);
                    removed = true;
                    break;
                } else if((child.NodeType == XmlNodeType.Element) && (tagname == child.Name)) {
                    if(attrvalue == ((XmlElement)child).GetAttribute(attrname)) {
                        parent.RemoveChild(child);
                        count++;
                        removed = true;
                        break;
                    }
                }
            }
        }
        return count;
    }

    /// <summary>
    ///   指定した名前のエレメントをソートする。
    /// </summary>
    public void Sort(XmlElement parent, string tagname, IComparer<XmlElement> comp) {
        List<XmlElement> list = GetElements(parent, tagname);
        list.Sort(comp);
        foreach(XmlElement el in list)
            parent.AppendChild(el);
    }

    /// <summary>
    ///   指定した名前のエレメントを属性値に基づいてソートする。
    /// </summary>
    public void Sort(XmlElement parent, string tagname, string attrname) {
        Sort(parent, tagname, new ElementComparator(attrname));
    }

    /// <summary>
    ///   テキスト要素を全て取り出す。
    /// </summary>
    /// <param name="elem">対象エレメント</param>
    /// <param name="recursiveFlag">子要素内のテキストも抽出するかどうか</param>
    /// <remarks>
    ///   <para>
    ///     本メソッドはクラスメソッドである事に注意する事
    ///   </para>
    /// </remarks>
    public static string GetText(XmlElement elem, bool recursiveFlag=true) {
        StringBuilder sb = new StringBuilder();
        foreach(XmlNode child in elem.ChildNodes) {
            if((child.NodeType == XmlNodeType.Element) && recursiveFlag)
                sb.Append(GetText((XmlElement)child));
            else if(child.NodeType == XmlNodeType.Text)
                sb.Append(child.Value);
        }
        return sb.ToString();
    }

    /// <summary>
    ///   属性値を読み出す。属性値が未定義のときはデフォルト値を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本メソッドはクラスメソッドである事に注意する事
    ///   </para>
    /// </remarks>
    public static string GetAttribute(XmlElement elem, string attrname, string def) {
        string v = elem.GetAttribute(attrname);
        if(String.IsNullOrEmpty(v))
            return def;
        return v;
    }

    /// <summary>
    ///   属性値を読み出す。属性値が未定義のときはデフォルト値を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本メソッドはクラスメソッドである事に注意する事
    ///   </para>
    /// </remarks>
    public static int GetAttribute(XmlElement elem, string attrname, int def) {
        string v = elem.GetAttribute(attrname);
        if(String.IsNullOrEmpty(v))
            return def;
        return StringUtil.ToInt(v, def);
    }

    /// <summary>
    ///   属性値を読み出す。属性値が未定義のときはデフォルト値を返す
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     本メソッドはクラスメソッドである事に注意する事
    ///   </para>
    /// </remarks>
    public static double GetAttribute(XmlElement elem, string attrname, double def) {
        string v = elem.GetAttribute(attrname);
        if(String.IsNullOrEmpty(v))
            return def;
        return StringUtil.ToDouble(v, def);
    }

    /// <summary>
    ///   テキスト要素を追加する
    /// </summary>
    public void AddText(XmlElement elem, string txt) {
        XmlText t = document.CreateTextNode(txt);
        elem.AppendChild(t);
    }

    /// <summary>
    ///   ファイル名
    /// </summary>
    public string FilePath {
        get { return filePath; }
    }


    /// <summary>
    ///   コンストラクタ下請け
    /// </summary>
    private void setup(string filepath_, string rootelementname, bool writable_) {
        filePath = filepath_;
        writable = writable_;
        lockFile();
        try {
            document = new XmlDocument();
            rootElement = null;
            if(File.Exists(filePath)) {
                using(StreamReader sr = FileUtil.Reader(filePath, encoding)) {
                    if(sr == null)
                        throw new FileNotFoundException("Can't open "+filePath+" to read.");
                    document.Load(sr);
                    sr.Close();
                }
                XmlNodeList nodes = document.GetElementsByTagName(rootelementname);
                if(nodes.Count > 0)
                    rootElement = (XmlElement)nodes[0];
            }
            if(rootElement == null) {
                rootElement = document.CreateElement(rootelementname);
                document.AppendChild(rootElement);
                document.InsertBefore(document.CreateXmlDeclaration("1.0", "UTF-8", null), rootElement);
            }
        } catch(Exception e) {
            unlockFile();
            throw e;
        }
        if(!writable)
            unlockFile();
    }

    /// <summary>
    ///   ファイルをロックする
    /// </summary>
    public void lockFile() {
        string lockFileName = filePath+".lock";
        lockStream = FileUtil.BinaryWriter(lockFileName,FileMode.CreateNew);
        if(lockStream == null) {
            // 誰かがロック張ったまま糞詰まっているらしい
            try {
                File.Delete(lockFileName);
            } catch (IOException) {
                // ぎりぎりのタイミングで他のプロセスが削除をした場合はIOException
                // になるが、特に問題はない。
                // 次のBinaryWriterで排他処理が行なわれる。
            }
            lockStream = FileUtil.BinaryWriter(lockFileName,FileMode.CreateNew);
        }
        // 万が一ロックファイルを作れないときに例外を発生させたい場合には、次のコードを使う
        //if(lockStream == null)
        //    throw new IOException("Can't lock file "+filePath);
    }

    /// <summary>
    ///   ファイルをアンロックする
    /// </summary>
    private void unlockFile() {
        if(lockStream == null)
            return;
        lockStream.Close();
        try {
            File.Delete(filePath + ".lock");
        } catch (IOException) {
            // ぎりぎりのタイミングで他のプロセスが削除をした場合はIOException
            // になるが、特に問題はない。
        }
    }


    private XmlDocument document;
    private XmlElement rootElement;
    private string filePath;
    private bool writable;
    private FileStream lockStream;

    /// <summary>
    ///   属性値によるソート用のクラス
    /// </summary>
    private class ElementComparator : IComparer<XmlElement> {
        public ElementComparator(string attrname_) {
            attrname = attrname_;
        }

        public int Compare(XmlElement x, XmlElement y) {
            if(x == null) {
                if(y == null)
                    return 0;
                return -1;
            }
            if(y == null)
                return 1;
            return String.Compare(x.GetAttribute(attrname), y.GetAttribute(attrname));
        }

        private string attrname;
    }

#if SELFTEST
    public static void Main(string[] args) {
        XmlFile xml = new XmlFile("xmltest.xml", "test", true);
        XmlElement el = xml.AddElement(xml.GetSubRoot("hello"), "yupi");
        el.SetAttribute("hoge", "moge");
        xml.RemoveElement(xml.GetSubRoot("my"), "boy");
        xml.Sort(xml.GetSubRoot("sorttest"), "elem", "id");
        xml.Save("xmltest2.xml");
    }
#endif

}

} // End of namespace
