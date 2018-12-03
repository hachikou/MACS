/// GraphicsExtensions_Text: System.Drawing.Graphicsクラスの拡張メソッド: アウトラインテキスト描画.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using MACS;

namespace MACS.Draw {

/// <summary>
///   System.Drawing.Graphicsクラスの拡張メソッド
/// </summary>
public static partial class GraphicsExtensions {

#region アウトラインテキスト描画

    /// <summary>
    ///   アウトラインテキストを描画する
    /// </summary>
    /// <param name="text">描画文字列の配列</param>
    /// <param name="font">使用フォント</param>
    /// <param name="outline">アウトラインを描画するペン。nullのときはアウトライン描画なし</param>
    /// <param name="fill">文字の描画色。nullのときは塗りつぶしなし</param>
    /// <param name="rect">描画範囲</param>
    /// <param name="hpos">水平描画位置</param>
    /// <param name="vpos">垂直描画位置</param>
    /// <param name="lineHeight">行間隔（行上端から次の行の上端までの間隔）フォント高さの倍数で指定する</param>
    /// <param name="fontRatio">文字の縦横比</param>
    /// <param name="shadow">影描画ブラシ。nullのときは影描画なし</param>
    /// <param name="shadowOffsetX">影描画時のXオフセット。未指定の場合フォント高さの10%</param>
    /// <param name="shadowOffsetY">影描画時のYオフセット。未指定の場合フォント高さの10%</param>
    /// <param name="shadowWidth">影描画時の太さ。未指定の場合shadowOffsetXとshadowOffsetYの二乗平均</param>
    /// <returns>描画した領域</returns>
    public static Rectangle DrawText(this Graphics g,
                                     string[] text, Font font,
                                     Pen outline, Brush fill,
                                     Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        if((text == null) || (text.Length == 0) || (rect == null)) {
            return new Rectangle(0,0,0,0);
        }
        float emSize = font.GetEmSize();
        GraphicsPath p = makeTextPath(text, font, rect, hpos, vpos, lineHeight, fontRatio, emSize);
        using(Matrix m = new Matrix()) {
            m.Translate((float)rect.X,(float)rect.Y);
            p.Transform(m);
        }

        // 影描画
        if(shadow != null) {
            using(GraphicsPath sp = (GraphicsPath)p.Clone()) {
                if(shadowOffsetX == float.MaxValue)
                    shadowOffsetX = emSize/10F;
                if(shadowOffsetY == float.MaxValue)
                    shadowOffsetY = shadowOffsetX;
                using(Matrix m = new Matrix()) {
                    m.Translate(shadowOffsetX, shadowOffsetY);
                    sp.Transform(m);
                }
                if(shadowWidth == float.MaxValue)
                    shadowWidth = (float)Math.Sqrt(shadowOffsetX*shadowOffsetX+shadowOffsetY*shadowOffsetY);
                using(Pen pen = new Pen(Color.Black, shadowWidth)) {
                    sp.Widen(pen);
                }
                g.FillPath(shadow, sp);
            }
        }

        // 文字描画
        if(outline != null) {
            g.DrawPath(outline, p);
        }
        if(fill != null) {
            g.FillPath(fill, p);
        }

        // 描画した領域
        Rectangle ret;
        if(outline == null) {
            ret = Rectangle.Ceiling(p.GetBounds());
        } else {
            ret = Rectangle.Ceiling(RectangleF.Inflate(p.GetBounds(), outline.Width/2F, outline.Width/2F));
        }

        p.Dispose();
        return ret;
    }

    public static Rectangle DrawText(this Graphics g,
                                     string text, Font font,
                                     Pen outline, Brush fill,
                                     Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        if(String.IsNullOrEmpty(text)) {
            return new Rectangle(0,0,0,0);
        }
        return g.DrawText(text.Split("\n".ToCharArray()), font,
                          outline, fill,
                          rect, hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    public static Rectangle DrawText(this Graphics g,
                                     string[] text, Font font,
                                     Brush fill,
                                     Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        return g.DrawText(text, font,
                          null, fill,
                          rect, hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    public static Rectangle DrawText(this Graphics g,
                                     string text, Font font,
                                     Brush fill,
                                     Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        if(String.IsNullOrEmpty(text)) {
            return new Rectangle(0,0,0,0);
        }
        return g.DrawText(text.Split("\n".ToCharArray()), font,
                          null, fill,
                          rect, hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    public static Rectangle DrawText(this Graphics g,
                                     string[] text, Font font,
                                     Pen outline, Brush fill,
                                     Point loc, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        return g.DrawText(text, font,
                          outline, fill,
                          new Rectangle(loc.X, loc.Y, 0, 0), hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    public static Rectangle DrawText(this Graphics g,
                                     string text, Font font,
                                     Pen outline, Brush fill,
                                     Point loc, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        if(String.IsNullOrEmpty(text)) {
            return new Rectangle(0,0,0,0);
        }
        return g.DrawText(text.Split("\n".ToCharArray()), font,
                          outline, fill,
                          new Rectangle(loc.X, loc.Y, 0, 0), hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    public static Rectangle DrawText(this Graphics g,
                                     string[] text, Font font,
                                     Brush fill,
                                     Point loc, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        return g.DrawText(text, font,
                          null, fill,
                          new Rectangle(loc.X, loc.Y, 0, 0), hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    public static Rectangle DrawText(this Graphics g,
                                     string text, Font font,
                                     Brush fill,
                                     Point loc, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                     float lineHeight=1.2F, float fontRatio=1.0F,
                                     Brush shadow=null, float shadowOffsetX=float.MaxValue, float shadowOffsetY=float.MaxValue, float shadowWidth=float.MaxValue) {
        if(String.IsNullOrEmpty(text)) {
            return new Rectangle(0,0,0,0);
        }
        return g.DrawText(text.Split("\n".ToCharArray()), font,
                          null, fill,
                          new Rectangle(loc.X, loc.Y, 0, 0), hpos, vpos,
                          lineHeight, fontRatio,
                          shadow, shadowOffsetX, shadowOffsetY, shadowWidth);
    }

    /// <summary>
    ///   アウトラインテキストを描画するときのサイズを得る
    /// </summary>
    /// <param name="text">描画文字列の配列</param>
    /// <param name="font">使用フォント</param>
    /// <param name="outline">アウトラインを描画するペン。nullのときはアウトライン描画なし</param>
    /// <param name="rect">描画範囲</param>
    /// <param name="hpos">水平描画位置</param>
    /// <param name="vpos">垂直描画位置</param>
    /// <param name="lineHeight">行間隔（行上端から次の行の上端までの間隔）フォント高さの倍数で指定する</param>
    /// <param name="fontRatio">文字の縦横比</param>
    /// <returns>サイズ</returns>
    public static SizeF MeasureText(this Graphics g,
                                    string[] text, Font font,
                                    Pen outline,
                                    Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font, outline, rect, hpos, vpos, lineHeight, fontRatio).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string text, Font font,
                                    Pen outline,
                                    Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font, outline, rect, hpos, vpos, lineHeight, fontRatio).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string[] text, Font font,
                                    Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            rect, hpos, vpos,
                            lineHeight, fontRatio).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string text, Font font,
                                    Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            rect, hpos, vpos,
                            lineHeight, fontRatio).Size;
    }

    public static SizeF MeasureText(this Graphics g,
                                    string[] text, Font font,
                                    Pen outline,
                                    TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            outline,
                            hpos, vpos,
                            lineHeight, fontRatio).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string text, Font font,
                                    Pen outline,
                                    TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            outline,
                            hpos, vpos,
                            lineHeight, fontRatio).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string[] text, Font font,
                                    TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            hpos, vpos,
                            lineHeight, fontRatio).Size;
    }
    
    public static SizeF MeasureText(this Graphics g,
                                    string text, Font font,
                                    TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                    float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            hpos, vpos,
                            lineHeight, fontRatio).Size;
    }

    /// <summary>
    ///   アウトラインテキストを描画するときの外枠を得る
    /// </summary>
    /// <param name="text">描画文字列の配列</param>
    /// <param name="font">使用フォント</param>
    /// <param name="outline">アウトラインを描画するペン。nullのときはアウトライン描画なし</param>
    /// <param name="rect">描画範囲</param>
    /// <param name="hpos">水平描画位置</param>
    /// <param name="vpos">垂直描画位置</param>
    /// <param name="lineHeight">行間隔（行上端から次の行の上端までの間隔）フォント高さの倍数で指定する</param>
    /// <param name="fontRatio">文字の縦横比</param>
    /// <returns>BoundRectangle</returns>
    public static RectangleF BoundsText(this Graphics g,
                                        string[] text, Font font,
                                        Pen outline,
                                        Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        if((text == null) || (text.Length == 0) || (rect == null)) {
            return new RectangleF(0,0,0,0);
        }
        float emSize = font.GetEmSize();
        using(GraphicsPath p = makeTextPath(text, font, rect, hpos, vpos, lineHeight, fontRatio, emSize)) {
            if(outline == null) {
                return p.GetBounds();
            } else {
                using(Matrix m = new Matrix()) {
                    return RectangleF.Inflate(p.GetBounds(), outline.Width/2F, outline.Width/2F);
                }
            }
        }
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string text, Font font,
                                        Pen outline,
                                        Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        if(String.IsNullOrEmpty(text))
            return new RectangleF(0,0,0,0);
        return g.BoundsText(text.Split("\n".ToCharArray()), font,
                            outline,
                            rect, hpos, vpos,
                            lineHeight, fontRatio);
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string[] text, Font font,
                                        Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            null,
                            rect, hpos, vpos,
                            lineHeight, fontRatio);
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string text, Font font,
                                        Rectangle rect, TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        if(String.IsNullOrEmpty(text))
            return new RectangleF(0,0,0,0);
        return g.BoundsText(text.Split("\n".ToCharArray()), font,
                            null,
                            rect, hpos, vpos,
                            lineHeight, fontRatio);
    }

    public static RectangleF BoundsText(this Graphics g,
                                        string[] text, Font font,
                                        Pen outline,
                                        TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            outline,
                            new Rectangle(0,0,0,0), hpos, vpos,
                            lineHeight, fontRatio);
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string text, Font font,
                                        Pen outline,
                                        TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            outline,
                            new Rectangle(0,0,0,0), hpos, vpos,
                            lineHeight, fontRatio);
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string[] text, Font font,
                                        TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            null,
                            new Rectangle(0,0,0,0), hpos, vpos,
                            lineHeight, fontRatio);
    }
    
    public static RectangleF BoundsText(this Graphics g,
                                        string text, Font font,
                                        TextHPosition hpos=TextHPosition.Left, TextVPosition vpos=TextVPosition.Top,
                                        float lineHeight=1.2F, float fontRatio=1.0F) {
        return g.BoundsText(text, font,
                            null,
                            new Rectangle(0,0,0,0), hpos, vpos,
                            lineHeight, fontRatio);
    }

    /// <summary>
    ///  アウトラインテキストのパス取得
    /// </summary>
    /// <param name="text">描画文字列</param>
    /// <param name="font">使用フォント</param>
    /// <param name="rect">描画範囲</param>
    /// <param name="hpos">水平描画位置</param>
    /// <param name="vpos">垂直描画位置</param>
    /// <param name="lineHeight">行間隔（行上端から次の行の上端までの間隔）フォント高さの倍数で指定する</param>
    /// <param name="shadow">影描画ブラシ。nullのときは影描画なし</param>
    /// <param name="shadowOffsetX">影描画時のXオフセット。未指定の場合フォント高さの10%</param>
    /// <param name="shadowOffsetY">影描画時のYオフセット。未指定の場合フォント高さの10%</param>
    /// <param name="shadowWidth">影描画時の太さ。未指定の場合shadowOffsetXとshadowOffsetYの二乗平均</param>
    /// <param name="x">並行移動X座標</param>
    /// <param name="y">並行移動Y座標</param>
    /// <param name="fontRatio">文字の縦横比</param>
    /// <returns>アウトラインテキストのパス</returns>
    public static GraphicsPath GetTextPath(string text, Font font,
                                           Rectangle rect, TextHPosition hpos = TextHPosition.Left, TextVPosition vpos = TextVPosition.Top,
                                           float lineHeight = 1.2F, Brush shadow = null,
                                           float shadowOffsetX = float.MaxValue, float shadowOffsetY = float.MaxValue, float shadowWidth = float.MaxValue,
                                           float x = 0, float y = 0, float fontRatio = 1.0F) {
        
        //テキスト改行ごとに配列
        string[] textBuf = text.Split("\n".ToCharArray());
        
        if((text == null) || (text.Length == 0) || (rect == null)) {
            //テキストない場合、パスポイント0で返す
            return new GraphicsPath();
        }

        float emSize = font.GetEmSize();
        //テキストパス取得
        GraphicsPath p = makeTextPath(textBuf, font, rect, hpos, vpos, lineHeight, fontRatio, emSize);
        using(Matrix m = new Matrix()) {
            //テキストパス並行移動
            m.Translate(x + (float)rect.X, y + (float)rect.Y);
            p.Transform(m);
            p.CloseFigure();
        }

        // 影描画
        // TODO:影を作るとパスが崩れる以下の処理を見直す必要がある。
        // 当面は背景透過する場合は、影を作らないようにする。
        if(shadow != null) {
            p.StartFigure();
            if(shadowOffsetX == float.MaxValue)
                shadowOffsetX = emSize/10F;
            if(shadowOffsetY == float.MaxValue)
                shadowOffsetY = shadowOffsetX;
            using(Matrix m = new Matrix()) {
                m.Translate(shadowOffsetX, shadowOffsetY);
                p.Transform(m);
            }
            if(shadowWidth == float.MaxValue)
                shadowWidth = (float)Math.Sqrt(shadowOffsetX*shadowOffsetX+shadowOffsetY*shadowOffsetY);
            using(Pen pen = new Pen(Color.Black, shadowWidth)) {
                p.Widen(pen);
            }

            p.CloseFigure();
        }

        return p;
    }


    private static GraphicsPath makeTextPath(string[] text, Font font,
                                             Rectangle rect, TextHPosition hpos, TextVPosition vpos,
                                             float lineHeight, float fontRatio, float emSize) {
        // 各行のテキストパスを得る
        GraphicsPath[] pp = new GraphicsPath[text.Length];
        using(Matrix m = new Matrix()) {
            if(fontRatio != 1F) {
                m.Scale(fontRatio, 1F);
            }
            for(int i = 0; i < text.Length; i++) {
                pp[i] = new GraphicsPath();
                pp[i].AddString(text[i], font.FontFamily, (int)font.Style, emSize, new PointF(0F,0F), StringFormat.GenericDefault);
                if(fontRatio != 1F) {
                    pp[i].Transform(m);
                }
            }
        }
        // 全体の描画幅、高さを得る
        float ww = 0F;
        for(int i = 0; i < pp.Length; i++) {
            RectangleF r = pp[i].GetBounds();
            if(ww < r.Width)
                ww = r.Width;
        }
        if((hpos == TextHPosition.LeftShrink) || (hpos == TextHPosition.CenterShrink) || (hpos == TextHPosition.RightShrink)) {
            if(ww > rect.Width)
                ww = rect.Width;
        }
        float hh, hhstep;
        switch(vpos) {
        case TextVPosition.Fit:
            hh = rect.Height;
            if((text.Length == 1) || (hh < emSize)) {
                hhstep = 0;
            } else {
                hhstep = (hh-emSize)/(float)(text.Length-1);
            }
            break;
        case TextVPosition.Proportional:
            hh = rect.Height;
            hhstep = (float)rect.Height/(float)text.Length;
            break;
        default:
            hh = emSize+(emSize*lineHeight)*(float)(text.Length-1);
            hhstep = emSize*lineHeight;
            break;
        }
        // テキストパスを一つにまとめる
        GraphicsPath p = new GraphicsPath();
        for(int i = 0; i < pp.Length; i++) {
            RectangleF bb = pp[i].GetBounds();
            if((bb.Width > 0) && (bb.Height > 0)) { // 空パスをAddPathするとエラーになるらしい
                float xoff = -bb.X;
                float xmag = 1F;
                switch(hpos) {
                case TextHPosition.Left:
                    //xoff += 0F;
                    //xmag = 1F;
                    break;
                case TextHPosition.Center:
                    xoff += ((float)rect.Width-bb.Width)/2F;
                    //xmag = 1F;
                    break;
                case TextHPosition.Right:
                    xoff += (float)rect.Width-bb.Width-1F;
                    //xmag = 1F;
                    break;
                case TextHPosition.LeftShrink:
                    if(ww < bb.Width) {
                        //xoff += 0F;
                        if(bb.Width == 0)
                            xmag = 1F;
                        else
                            xmag = (float)rect.Width/(bb.Width+1F);
                    } else {
                        //xoff += 0F;
                        xmag = 1F;
                    }
                    break;
                case TextHPosition.CenterShrink:
                    if(ww < bb.Width) {
                        //xoff += 0F;
                        if(bb.Width == 0)
                            xmag = 1F;
                        else
                            xmag = (float)rect.Width/(bb.Width+1F);
                    } else {
                        xoff += ((float)rect.Width-bb.Width)/2F;
                        xmag = 1F;
                    }
                    break;
                case TextHPosition.RightShrink:
                    if(ww < bb.Width) {
                        //xoff += 0F;
                        if(bb.Width == 0)
                            xmag = 1F;
                        else
                            xmag = (float)rect.Width/(bb.Width+1F);
                    } else {
                        xoff += (float)rect.Width-bb.Width-1F;
                        xmag = 1F;
                    }
                    break;
                }
                float yoff = hhstep*(float)i-bb.Y;
                if(hhstep <= 0)
                    yoff += ((float)rect.Height-emSize)/2F;
                float ymag = 1F;
                switch(vpos) {
                case TextVPosition.Top:
                    // yoff += 0F;
                    break;
                case TextVPosition.Middle:
                    yoff += ((float)rect.Height-hh)/2F;
                    break;
                case TextVPosition.Bottom:
                    yoff += (float)rect.Height-hh;
                    break;
                case TextVPosition.Fit:
                    // yoff += 0F;
                    break;
                case TextVPosition.Proportional:
                    yoff = hhstep*(float)i+(hhstep-bb.Height)/2F-bb.Y;
                    break;
                }
                if((xoff != 0F) || (yoff != 0F)) {
                    using(Matrix m = new Matrix()) {
                        m.Translate(xoff, yoff);
                        pp[i].Transform(m);
                    }
                }
                if((xmag != 1F) || (ymag != 1F)) {
                    using(Matrix m = new Matrix()) {
                        m.Scale(xmag, ymag);
                        pp[i].Transform(m);
                    }
                }
                p.AddPath(pp[i], false);
            }
            pp[i].Dispose();
        }
        return p;
    }
    
#endregion

}


/// <summary>
///   DrawTextの際のテキスト配置（水平方向）
/// </summary>
public enum TextHPosition {
    Left,   //< 左寄せ
    Center, //< 中央
    Right,  //< 右寄せ
    LeftShrink,   //< 左寄せで描画幅を超えるときは描画幅に合わせる
    CenterShrink, //< 中央で描画幅を超えるときは描画幅に合わせる
    RightShrink,  //< 右寄せで描画幅を超えるときは描画幅に合わせる
}

/// <summary>
///   DrawTextの際のテキスト配置（垂直方向）
/// </summary>
public enum TextVPosition {
    Top,    //< 上寄せ
    Middle, //< 中央
    Bottom, //< 下寄せ
    Fit,    //< 描画高さに合わせて行間を調整する(lineHeight無視)
    Proportional, //< 描画高さに合わせて行間調整(lineHeight無視、上下余白あり)
}

} // End of namespace
