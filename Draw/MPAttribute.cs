/// MPAttribute: 描画属性管理機構.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using MACS;

namespace MACS.Draw {

/// <summary>
///   描画属性管理機構
/// </summary>
public class MPAttribute : CascadedAttribute {

    public MPAttribute(XmlElement elem, MPAttribute parent_=null, string parentKey_="") : base(elem, parent_, parentKey_) {}

    
    public Color GetColor(string name, Color defValue) {
        string cname = Get(name);
        if(String.IsNullOrEmpty(cname))
            return defValue;
        return ColorUtil.Get(cname, defValue);
    }

    public TextureBrush GetTextureBrush(string filepath) {
        if(!File.Exists(filepath))
            return null;
        return new TextureBrush(Image.FromFile(filepath));
    }

    public TextureBrush GetTextureBrush(string name, string defaultPath) {
        string cname = Get(name);
        if(String.IsNullOrEmpty(cname))
            return null;
        return GetTextureBrush(Path.Combine(defaultPath, cname));
    }

    public Font GetFont(Font defFont, string key="") {
        if(defFont == null)
            defFont = Control.DefaultFont; // Fail-safe
        float mag = MinMag;
        float size = Get(key+"fontsize", defFont.Size/mag);
        return defFont.GetNewFont(size*mag, Get(key+"font"), Get(key+"fontstyle"), Get(key+"fontunit"));
    }
        
    public float GetXValue(string name, float defValue) {
        string x = Get(name);
        if(String.IsNullOrEmpty(x))
            return defValue*MagX;
        return StringUtil.ToFloat(x)*MagX;
    }

    public float GetYValue(string name, float defValue) {
        string x = Get(name);
        if(String.IsNullOrEmpty(x))
            return defValue*MagY;
        return StringUtil.ToFloat(x)*MagY;
    }

    public float GetMagValue(string name, float defValue) {
        string x = Get(name);
        if(String.IsNullOrEmpty(x))
            return defValue*MinMag;
        return StringUtil.ToFloat(x)*MinMag;
    }
            
    public void FetchTextPosition(ref TextHPosition hpos, ref TextVPosition vpos, string key="") {
        bool shrink = this.Get(key+"shrink", true);
        string val = this.Get(key+"align");
        if(!String.IsNullOrEmpty(val)) {
            switch(val.ToUpper()) {
            case "LEFTTOP":
            case "TOPLEFT":
            case "TOP":
                hpos = shrink?TextHPosition.LeftShrink:TextHPosition.Left;
                vpos = TextVPosition.Top;
                break;
            case "CENTERTOP":
            case "TOPCENTER":
                hpos = shrink?TextHPosition.CenterShrink:TextHPosition.Center;
                vpos = TextVPosition.Top;
                break;
            case "RIGHTTOP":
            case "TOPRIGHT":
                hpos = shrink?TextHPosition.RightShrink:TextHPosition.Right;
                vpos = TextVPosition.Top;
                break;
            case "LEFTFIT":
            case "FITLEFT":
            case "FIT":
                hpos = shrink?TextHPosition.LeftShrink:TextHPosition.Left;
                vpos = TextVPosition.Fit;
                break;
            case "CENTERFIT":
            case "FITCENTER":
                hpos = shrink?TextHPosition.CenterShrink:TextHPosition.Center;
                vpos = TextVPosition.Fit;
                break;
            case "RIGHTFIT":
            case "FITRIGHT":
                hpos = shrink?TextHPosition.RightShrink:TextHPosition.Right;
                vpos = TextVPosition.Fit;
                break;
            case "LEFT":
            case "LEFTMIDDLE":
            case "MIDDLELEFT":
            case "MIDDLE":
                hpos = shrink?TextHPosition.LeftShrink:TextHPosition.Left;
                vpos = TextVPosition.Middle;
                break;
            case "CENTER":
            case "CENTERMIDDLE":
            case "MIDDLECENTER":
                hpos = shrink?TextHPosition.CenterShrink:TextHPosition.Center;
                vpos = TextVPosition.Middle;
                break;
            case "RIGHT":
            case "RIGHTMIDDLE":
            case "MIDDLERIGHT":
                hpos = shrink?TextHPosition.RightShrink:TextHPosition.Right;
                vpos = TextVPosition.Middle;
                break;
            case "LEFTBOTTOM":
            case "BOTTOMLEFT":
            case "BOTTOM":
                hpos = shrink?TextHPosition.LeftShrink:TextHPosition.Left;
                vpos = TextVPosition.Bottom;
                break;
            case "CENTERBOTTOM":
            case "BOTTOMCENTER":
                hpos = shrink?TextHPosition.CenterShrink:TextHPosition.Center;
                vpos = TextVPosition.Bottom;
                break;
            case "RIGHTBOTTOM":
            case "BOTTOMRIGHT":
                hpos = shrink?TextHPosition.RightShrink:TextHPosition.Right;
                vpos = TextVPosition.Bottom;
                break;
            }
        }
    }
    
    public void FetchTextStyle(ref float outlineRatio, ref Color outlineColor, ref float lineHeight, ref Color shadowColor, ref float shadowOffsetX, ref float shadowOffsetY, string key="") {
        outlineRatio = this.Get(key+"outlineratio", outlineRatio);
        outlineColor = this.GetColor(key+"outline", outlineColor);
        lineHeight = this.Get(key+"lineheight", lineHeight);
        shadowColor = this.GetColor(key+"shadow", shadowColor);
        string val = this.Get(key+"shadowoffset");
        if(!String.IsNullOrEmpty(val)) {
            string[] x = val.Split(",".ToCharArray());
            if(x.Length == 1) {
                shadowOffsetX = shadowOffsetY = StringUtil.ToFloat(x[0], shadowOffsetX);
            } else {
                shadowOffsetX = StringUtil.ToFloat(x[0], shadowOffsetX);
                shadowOffsetY = StringUtil.ToFloat(x[1], shadowOffsetY);
            }
        }
    }

    /// <summary>
    ///   属性定義上のX方向表示倍率
    /// </summary>
    public float MagX {
        get {
            if(magX == 0) {
                MPAttribute p = parent as MPAttribute;
                if(p == null)
                    magX = 1.0F;
                else
                    magX = p.MagX;
            }
            return magX;
        }
        set { magX = value; }
    }
    
    /// <summary>
    ///   属性定義上のY方向表示倍率
    /// </summary>
    public float MagY {
        get {
            if(magY == 0) {
                MPAttribute p = parent as MPAttribute;
                if(p == null)
                    magY = 1.0F;
                else
                    magY = p.MagY;
            }
            return magY;
        }
        set { magY = value; }
    }
    
    /// <summary>
    ///   属性定義上の表示倍率（MagXとMagYの小さい方）
    /// </summary>
    public float MinMag {
        get { return (MagX<=MagY)?MagX:MagY; }
    }
    

    protected override CascadedAttribute CreateChild(XmlElement elem) {
        return new MPAttribute(elem, this);
    }

    private float magX = 0;
    private float magY = 0;
    
}
    
} // End of namespace
