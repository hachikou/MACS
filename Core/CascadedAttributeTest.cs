/// CascadedAttributeTest: CascadedAttributeクラステストハンドラ.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Collections.Generic;
using System.Xml;
using MACS;

public class Program {

    public class Element {
        public string Name;
        public CascadedAttribute Attr;

        public Element(XmlElement elem, Element parent, string childClassKey) {
            Name = elem.Name;
            Attr = new CascadedAttribute(elem, (parent==null)?null:parent.Attr, childClassKey);
        }
    }

    public static int Main(string[] args) {
        List<Element> elementList = new List<Element>();
        using(XmlFile xml = new XmlFile("CascadedAttributeTest.xml", "attribute")) {
            loadElement(xml.Root, null, elementList);
        }
        foreach(Element elem in elementList) {
            Console.WriteLine("{0}: a={1}, b={2}, c={3}, d={4}, e={5}", elem.Name,
                              elem.Attr.Get("a", "undef"),
                              elem.Attr.Get("b", "undef"),
                              elem.Attr.Get("c", "undef"),
                              elem.Attr.Get("d", "undef"),
                              elem.Attr.Get("e", "undef"));
        }

        return 0;
    }

    private static void loadElement(XmlElement elem, Element parent, List<Element> elementList) {
        Element el = new Element(elem, parent, elem.Name.StartsWith("container")?"elem":"");
        elementList.Add(el);
        foreach(XmlNode node in elem.ChildNodes) {
            if((node.NodeType != XmlNodeType.Element) || CascadedAttribute.IsSpecialTag(node.Name))
                continue;
            loadElement((XmlElement)node, el, elementList);
        }
    }

}
