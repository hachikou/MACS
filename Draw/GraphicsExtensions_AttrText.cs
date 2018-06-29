/// GraphicsExtensions_AttrText: System.Drawing.Graphicsクラスの拡張メソッド : 属性指定テキスト描画.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using MACS;

namespace MACS.Draw {

/// <summary>
///   System.Drawing.Graphicsクラスの拡張メソッド
/// </summary>
public static partial class GraphicsExtensions {

#region 属性指定テキスト描画

    /// <summary>
    ///   属性指定テキストを描画する
    /// </summary>
    /// <param name="text">描画文字列の配列</param>
    /// <param name="attr">表示属性指定</param>
    /// <param name="rect">描画範囲</param>
    /// <returns>描画した領域</returns>
    public static Rectangle DrawText(this Graphics g,
                                     string[] text, MPAttribute attr,
                                     Rectangle rect,
                                     Control control = null) {
        if((text == null) || (text.Length == 0) || (rect == null)) {
            return new Rectangle(0,0,0,0);
        }
        Rectangle urect = new Rectangle(0,0,0,0);
        float lineHeight = (float)rect.Height/(float)text.Length;
        Rectangle lineRect = new Rectangle();
        for(int i = 0; i < text.Length; i++) {
            MPAttribute xattr = attr;
            string xtext = text[i].Trim();
            Match m = pat_class.Match(xtext);
            if(m.Success) {
                xattr = (MPAttribute)attr.GetClass(m.Groups[1].Value);
                xtext = m.Groups[2].Value.Trim();
            }
            
            lineRect.X = rect.X;
            lineRect.Width = rect.Width;
            lineRect.Y = rect.Y+(int)(lineHeight*i);
            lineRect.Height = rect.Y+(int)(lineHeight*(i+1))-lineRect.Y;

            Color bgcolor = Color.White;
            Color color = Color.Black;
            TextHPosition hpos = TextHPosition.LeftShrink;
            TextVPosition vpos = TextVPosition.Middle;
            float outlineRatio = 0F;
            Color outlineColor = ColorUtil.Invalid;
            float lh = 1F;
            Color shadowColor = ColorUtil.Invalid;
            float shadowOffsetX = 0F;
            float shadowOffsetY = 0F;
            float fontRatio = 1.0F;
            Font refFont = Control.DefaultFont;

            if(control != null) {
                bgcolor = control.GetRealBackColor();
                color = control.ForeColor;
                refFont = control.Font;
                if(control is MPText) {
                    MPText cc = control as MPText;
                    hpos = cc.HPosition;
                    vpos = cc.VPosition;
                    outlineRatio = cc.OutlineRatio;
                    outlineColor = cc.OutlineColor;
                    shadowColor = cc.ShadowColor;
                    shadowOffsetX = cc.ShadowOffsetX;
                    shadowOffsetY = cc.ShadowOffsetY;
                }
            }

            // 文字色は"textcolor", "color"の定義の順に探す。
            color = xattr.GetColor("textcolor", xattr.GetColor("color", color));
            if(color == ColorUtil.Auto) {
                color = ColorUtil.GetBWColor(bgcolor);
            }
            xattr.FetchTextPosition(ref hpos, ref vpos);
            xattr.FetchTextStyle(ref outlineRatio, ref outlineColor, ref lh, ref shadowColor, ref shadowOffsetX, ref shadowOffsetY);
            if(outlineColor == ColorUtil.Auto) {
                outlineColor = ColorUtil.GetBWColor(color);
            }
            Font font = xattr.GetFont(refFont);
            float outlineWidth = font.GetEmSize()*outlineRatio;
            Brush brush = null;
            if(color.A > 0)
                brush = new SolidBrush(color);
            Pen pen = null;
            if((outlineColor.A > 0) && (outlineWidth > 0))
                pen = new Pen(outlineColor, outlineWidth);
            Brush shadow = null;
            if(shadowColor.A > 0)
                shadow = new SolidBrush(shadowColor);

            lineRect = g.DrawText(new string[]{xtext}, font,
                                  pen, brush,
                                  lineRect, hpos, TextVPosition.Middle,
                                  1F, fontRatio,
                                  shadow, shadowOffsetX, shadowOffsetY);
            if((urect.Width == 0) || (urect.Height == 0))
                urect = lineRect;
            else if((lineRect.Width != 0) && (lineRect.Height != 0))
                urect = Rectangle.Union(urect, lineRect);

            if(brush != null)
                brush.Dispose();
            if(pen != null)
                pen.Dispose();
            if(shadow != null)
                shadow.Dispose();
        }
        return urect;
    }

    public static Rectangle DrawText(this Graphics g,
                                     string text, MPAttribute attr,
                                     Rectangle rect,
                                     Control control=null) {
        if(String.IsNullOrEmpty(text)) {
            return new Rectangle(0,0,0,0);
        }
        return g.DrawText(g.SplitXMLText(text), attr, rect, control);
    }

    /// <summary>
    ///   アウトラインテキストを描画するときのサイズを得る
    /// </summary>
    /// <param name="text">描画文字列の配列</param>
    /// <param name="attr">表示属性指定</param>
    /// <param name="rect">描画範囲</param>
    /// <returns>サイズ</returns>
    public static SizeF MeasureText(this Graphics g,
                                    string[] text, MPAttribute attr,
                                    Rectangle rect,
                                    Control control=null) {
        return g.BoundsText(text, attr, rect, control).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string text, MPAttribute attr,
                                    Rectangle rect,
                                    Control control=null) {
        return g.BoundsText(text, attr, rect, control).Size;
    }

    /// <summary>
    ///   アウトラインテキストを描画するときの外枠を得る
    /// </summary>
    /// <param name="text">描画文字列の配列</param>
    /// <param name="attr">表示属性指定</param>
    /// <param name="rect">描画範囲</param>
    /// <returns>サイズ</returns>
    public static RectangleF BoundsText(this Graphics g,
                                        string[] text, MPAttribute attr,
                                        Rectangle rect,
                                        Control control=null) {
        if((text == null) || (text.Length == 0) || (rect == null)) {
            return new RectangleF(0,0,0,0);
        }
        RectangleF urect = new RectangleF(0,0,0,0);
        float lineHeight = (float)rect.Height/(float)text.Length;
        Rectangle lineRect = new Rectangle();
        for(int i = 0; i < text.Length; i++) {
            MPAttribute xattr = attr;
            string xtext = text[i].Trim();
            Match m = pat_class.Match(xtext);
            if(m.Success) {
                xattr = (MPAttribute)attr.GetClass(m.Groups[1].Value);
                xtext = m.Groups[2].Value.Trim();
            }
            
            lineRect.X = rect.X;
            lineRect.Width = rect.Width;
            lineRect.Y = rect.Y+(int)(lineHeight*i);
            lineRect.Height = rect.Y+(int)(lineHeight*(i+1))-lineRect.Y;

            TextHPosition hpos = TextHPosition.LeftShrink;
            TextVPosition vpos = TextVPosition.Middle;
            float outlineRatio = 0F;
            Color outlineColor = ColorUtil.Invalid;
            float lh = 1F;
            Color shadowColor = ColorUtil.Invalid;
            float shadowOffsetX = 0F;
            float shadowOffsetY = 0F;
            float fontRatio = 1.0F;

            xattr.FetchTextPosition(ref hpos, ref vpos);
            xattr.FetchTextStyle(ref outlineRatio, ref outlineColor, ref lh, ref shadowColor, ref shadowOffsetX, ref shadowOffsetY);
            Font font = xattr.GetFont((control != null)?control.Font:Control.DefaultFont);
            float outlineWidth = font.GetEmSize()*outlineRatio;
            Pen pen = null;
            if((outlineColor.A > 0) && (outlineWidth > 0))
                pen = new Pen(outlineColor, outlineWidth);

            RectangleF xrect = g.BoundsText(new string[]{xtext}, font,
                                            pen,
                                            lineRect, hpos, TextVPosition.Middle,
                                            1F, fontRatio);
            if((urect.Width == 0) || (urect.Height == 0))
                urect = xrect;
            else if((xrect.Width != 0) && (xrect.Height != 0))
                urect = RectangleF.Union(urect, xrect);

            font.Dispose();
            if(pen != null)
                pen.Dispose();
        }
        return urect;
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string text, MPAttribute attr,
                                        Rectangle rect,
                                        Control control=null) {
        if(String.IsNullOrEmpty(text))
            return new RectangleF(0,0,0,0);
        return g.BoundsText(g.SplitXMLText(text), attr, rect, control);
    }


    public static string[] SplitXMLText(this Graphics g, string text) {
        if(String.IsNullOrEmpty(text))
            return new string[]{""};
        string[] atext = pat_split.Split(text.Replace("\r","").Trim());
        if(atext[atext.Length-1] == "")
            Array.Resize(ref atext, atext.Length-1);
        return atext;
    }

    private static Regex pat_split = new Regex(@"\n|</\s*\w+\s*>\n?");
    private static Regex pat_class = new Regex(@"<\s*(\w+)\s*>(.*)");
    
#endregion

}

} // End of namespace
