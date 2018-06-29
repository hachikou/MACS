/// GraphicsExtensions_Rectangle: System.Drawing.Graphicsクラスの拡張メソッド : 長方形描画.
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

#region 長方形描画

    /// <summary>
    ///   長方形を描く
    /// </summary>
    public static void DrawAndFillRectangle(this Graphics g, Pen pen, Brush brush, Rectangle rect) {
        if(brush != null)
            g.FillRectangle(brush, rect);
        if(pen != null)
            g.DrawRectangle(pen, rect);
    }
    
    /// <summary>
    ///   長方形を描く
    /// </summary>
    public static void DrawAndFillRectangle(this Graphics g, Pen pen, Brush brush, int x, int y, int width, int height) {
        if(brush != null)
            g.FillRectangle(brush, x, y, width, height);
        if(pen != null)
            g.DrawRectangle(pen, x, y, width, height);
    }
    
    /// <summary>
    ///   内接する長方形を描く
    /// </summary>
    public static void DrawAndFillInnerRectangle(this Graphics g, Pen pen, Brush brush, Rectangle rect) {
        g.DrawAndFillInnerRectangle(pen, brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    ///   内接する長方形を描く
    /// </summary>
    public static void DrawAndFillInnerRectangle(this Graphics g, Pen pen, Brush brush, int x, int y, int width, int height) {
        Rectangle r;
        if(pen == null) {
            r = new Rectangle(x, y, width, height);
        } else {
            int sz = (int)(pen.Width/2F);
            r = new Rectangle(x+sz, y+sz, width-(int)Math.Ceiling(pen.Width), height-(int)Math.Ceiling(pen.Width));
        }
        if(brush != null) {
            g.FillRectangle(brush, r);
        }
        if(pen != null) {
            g.DrawRectangle(pen, r);
        }
    }
    
    /// <summary>
    ///   内接する長方形を描く（枠線のみ）
    /// </summary>
    public static void DrawInnerRectangle(this Graphics g, Pen pen, Rectangle rect) {
        g.DrawAndFillInnerRectangle(pen, null, rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    ///   内接する長方形を描く（枠線のみ）
    /// </summary>
    public static void DrawInnerRectangle(this Graphics g, Pen pen, int x, int y, int width, int height) {
        g.DrawAndFillInnerRectangle(pen, null, x, y, width, height);
    }

    /// <summary>
    ///   外接する長方形を描く
    /// </summary>
    public static void DrawAndFillOuterRectangle(this Graphics g, Pen pen, Brush brush, Rectangle rect) {
        g.DrawAndFillOuterRectangle(pen, brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    ///   外接する長方形を描く
    /// </summary>
    public static void DrawAndFillOuterRectangle(this Graphics g, Pen pen, Brush brush, int x, int y, int width, int height) {
        Rectangle r;
        if(pen == null) {
            r = new Rectangle(x, y, width, height);
        } else {
            int sz = (int)Math.Ceiling(pen.Width/2F);
            r = new Rectangle(x-sz, y-sz, width+(int)Math.Ceiling(pen.Width), height+(int)Math.Ceiling(pen.Width));
        }
        if(brush != null) {
            g.FillRectangle(brush, r);
        }
        if(pen != null) {
            g.DrawRectangle(pen, r);
        }
    }
    
    /// <summary>
    ///   外接する長方形を描く（枠線のみ）
    /// </summary>
    public static void DrawOuterRectangle(this Graphics g, Pen pen, Rectangle rect) {
        g.DrawAndFillOuterRectangle(pen, null, rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    ///   外接する長方形を描く（枠線のみ）
    /// </summary>
    public static void DrawOuterRectangle(this Graphics g, Pen pen, int x, int y, int width, int height) {
        g.DrawAndFillOuterRectangle(pen, null, x, y, width, height);
    }
    

#endregion
    
#region 角丸長方形描画

    /// <summary>
    ///   角丸長方形のパスを得る
    /// </summary>
    public static GraphicsPath MakeRoundRectanglePath(this Graphics g, Rectangle rect, float radius) {
        return g.MakeRoundRectanglePath(rect.X, rect.Y, rect.Width, rect.Height, radius);
    }

    /// <summary>
    ///   角丸長方形のパスを得る
    /// </summary>
    public static GraphicsPath MakeRoundRectanglePath(this Graphics g, int x, int y, int width, int height, float radius) {
        GraphicsPath p = new GraphicsPath();
        if((width <= 0) || (height <= 0))
            return p;
        if(radius <= 1.0) {
            p.AddRectangle(new Rectangle(x, y, width, height));
            return p;
        }
        if((int)radius > width/2)
            radius = (float)width/2.0F;
        if((int)radius > height/2)
            radius = (float)height/2.0F;
        Rectangle r = new Rectangle();
        p.StartFigure();
        // top-left
        r.X = x;
        r.Y = y;
        r.Width = (int)(radius*2.0F);
        r.Height = (int)(radius*2.0F);
        p.AddArc(r, 180F, 90F);
        // top-right
        r.X = x+width-(int)(radius*2);
        r.Y = y;
        r.Width = (int)(radius*2);
        r.Height = (int)(radius*2);
        p.AddArc(r, -90F, 90F);
        // bottom-right
        r.X = x+width-(int)(radius*2);
        r.Y = y+height-(int)(radius*2);
        r.Width = (int)(radius*2);
        r.Height = (int)(radius*2);
        p.AddArc(r, 0F, 90F);
        // bottom-left
        r.X = x;
        r.Y = y+height-(int)(radius*2);
        r.Width = (int)(radius*2);
        r.Height = (int)(radius*2);
        p.AddArc(r, 90F, 90F);
        p.CloseFigure();
        return p;
    }
    
    /// <summary>
    ///   角丸長方形を描画する
    /// </summary>
    public static void DrawAndFillRoundRectangle(this Graphics g, Pen pen, Brush brush, Rectangle rect, float radius) {
        g.DrawAndFillRoundRectangle(pen, brush, rect.X, rect.Y, rect.Width, rect.Height, radius);
    }
    
    /// <summary>
    ///   角丸長方形を描画する
    /// </summary>
    public static void DrawAndFillRoundRectangle(this Graphics g, Pen pen, Brush brush, int x, int y, int width, int height, float radius) {
        using(GraphicsPath p = g.MakeRoundRectanglePath(x, y, width, height, radius)) {
            if(brush != null) {
                g.FillPath(brush, p);
            }
            if(pen != null) {
                g.DrawPath(pen, p);
            }
        }
    }

    /// <summary>
    ///   内接する角丸長方形を描く
    /// </summary>
    public static void DrawAndFillInnerRoundRectangle(this Graphics g, Pen pen, Brush brush, Rectangle rect, float radius) {
        g.DrawAndFillInnerRoundRectangle(pen, brush, rect.X, rect.Y, rect.Width, rect.Height, radius);
    }

    /// <summary>
    ///   内接する角丸長方形を描く
    /// </summary>
    public static void DrawAndFillInnerRoundRectangle(this Graphics g, Pen pen, Brush brush, int x, int y, int width, int height, float radius) {
        if(pen == null) {
            g.DrawAndFillRoundRectangle(pen, brush, x, y, width, height, radius);
        } else {
            int sz = (int)(pen.Width/2.0F);
            g.DrawAndFillRoundRectangle(pen, brush, x+sz, y+sz, width-(int)pen.Width, height-(int)pen.Width, radius);
        }
    }

    
    /// <summary>
    ///   角丸長方形を描画する（枠線のみ）
    /// </summary>
    public static void DrawRoundRectangle(this Graphics g, Pen pen, Rectangle rect, float radius) {
        g.DrawAndFillRoundRectangle(pen, null, rect.X, rect.Y, rect.Width, rect.Height, radius);
    }
    
    /// <summary>
    ///   角丸長方形を描画する（枠線のみ）
    /// </summary>
    public static void DrawRoundRectangle(this Graphics g, Pen pen, int x, int y, int width, int height, float radius) {
        g.DrawAndFillRoundRectangle(pen, null, x, y, width, height, radius);
    }

    /// <summary>
    ///   内接する角丸長方形を描く（枠線のみ）
    /// </summary>
    public static void DrawInnerRoundRectangle(this Graphics g, Pen pen, Rectangle rect, float radius) {
        g.DrawAndFillInnerRoundRectangle(pen, null, rect.X, rect.Y, rect.Width, rect.Height, radius);
    }

    /// <summary>
    ///   内接する角丸長方形を描く（枠線のみ）
    /// </summary>
    public static void DrawInnerRoundRectangle(this Graphics g, Pen pen, int x, int y, int width, int height, float radius) {
        g.DrawAndFillInnerRoundRectangle(pen, null, x, y, width, height, radius);
    }

    /// <summary>
    ///   角丸長方形を描画する（塗りつぶしのみ）
    /// </summary>
    public static void FillRoundRectangle(this Graphics g, Brush brush, Rectangle rect, float radius) {
        g.DrawAndFillRoundRectangle(null, brush, rect.X, rect.Y, rect.Width, rect.Height, radius);
    }
    
    /// <summary>
    ///   角丸長方形を描画する（塗りつぶしのみ）
    /// </summary>
    public static void FillRoundRectangle(this Graphics g, Brush brush, int x, int y, int width, int height, float radius) {
        g.DrawAndFillRoundRectangle(null, brush, x, y, width, height, radius);
    }

#endregion

#region 4辺独立ペンによる長方形描画

    /// <summary>
    ///   4辺独立ペンによる長方形描画
    /// </summary>
    public static void DrawRectangle(this Graphics g, Pen topPen, Pen rightPen, Pen bottomPen, Pen leftPen, int x, int y, int width, int height) {
        float topW = (topPen==null)?0F:(topPen.Width/2F);
        float rightW = (rightPen==null)?0F:(rightPen.Width/2F);
        float bottomW = (bottomPen==null)?0F:(bottomPen.Width/2F);
        float leftW = (leftPen==null)?0F:(leftPen.Width/2F);
        if(leftW > 0F)
            g.DrawLine(leftPen, x, y, x, y+height);
        if(rightW > 0F)
            g.DrawLine(rightPen, x+width, y, x+width, y+height);
        if(topW > 0F)
            g.DrawLine(topPen, x-(int)(leftW), y, x+width+(int)Math.Ceiling(rightW), y);
        if(bottomW > 0F)
            g.DrawLine(bottomPen, x-(int)leftW, y+height, x+width+(int)Math.Ceiling(rightW), y+height);
    }
    
    /// <summary>
    ///   4辺独立ペンによる長方形描画
    /// </summary>
    public static void DrawRectangle(this Graphics g, Pen topPen, Pen rightPen, Pen bottomPen, Pen leftPen, Rectangle rect) {
        g.DrawRectangle(topPen, rightPen, bottomPen, leftPen, rect.X, rect.Y, rect.Width, rect.Height);
    }
    
    /// <summary>
    ///   4辺独立ペンによる内接長方形描画
    /// </summary>
    public static void DrawInnerRectangle(this Graphics g, Pen topPen, Pen rightPen, Pen bottomPen, Pen leftPen, int x, int y, int width, int height) {
        float topW = (topPen==null)?0F:(topPen.Width/2F);
        float rightW = (rightPen==null)?0F:(rightPen.Width/2F);
        float bottomW = (bottomPen==null)?0F:(bottomPen.Width/2F);
        float leftW = (leftPen==null)?0F:(leftPen.Width/2F);
        x += (int)leftW;
        y += (int)topW;
        width -= (int)Math.Ceiling(leftW+rightW);
        height -= (int)Math.Ceiling(topW+bottomW);
        if(leftW > 0F)
            g.DrawLine(leftPen, x, y, x, y+height);
        if(rightW > 0F)
            g.DrawLine(rightPen, x+width, y, x+width, y+height);
        if(topW > 0F)
            g.DrawLine(topPen, x-(int)(leftW), y, x+width+(int)Math.Ceiling(rightW), y);
        if(bottomW > 0F)
            g.DrawLine(bottomPen, x-(int)leftW, y+height, x+width+(int)Math.Ceiling(rightW), y+height);
    }
    
    /// <summary>
    ///   4辺独立ペンによる内接長方形描画
    /// </summary>
    public static void DrawInnerRectangle(this Graphics g, Pen topPen, Pen rightPen, Pen bottomPen, Pen leftPen, Rectangle rect) {
        g.DrawInnerRectangle(topPen, rightPen, bottomPen, leftPen, rect.X, rect.Y, rect.Width, rect.Height);
    }
    
    /// <summary>
    ///   4辺独立ペンによる外接長方形描画
    /// </summary>
    public static void DrawOuterRectangle(this Graphics g, Pen topPen, Pen rightPen, Pen bottomPen, Pen leftPen, int x, int y, int width, int height) {
        float topW = (topPen==null)?0F:(topPen.Width/2F);
        float rightW = (rightPen==null)?0F:(rightPen.Width/2F);
        float bottomW = (bottomPen==null)?0F:(bottomPen.Width/2F);
        float leftW = (leftPen==null)?0F:(leftPen.Width/2F);
        x -= (int)Math.Ceiling(leftW);
        y -= (int)Math.Ceiling(topW);
        width += (int)Math.Ceiling(leftW+rightW);
        height += (int)Math.Ceiling(topW+bottomW);
        if(leftW > 0F)
            g.DrawLine(leftPen, x, y, x, y+height);
        if(rightW > 0F)
            g.DrawLine(rightPen, x+width, y, x+width, y+height);
        if(topW > 0F)
            g.DrawLine(topPen, x-(int)(leftW), y, x+width+(int)Math.Ceiling(rightW), y);
        if(bottomW > 0F)
            g.DrawLine(bottomPen, x-(int)leftW, y+height, x+width+(int)Math.Ceiling(rightW), y+height);
    }
    
    /// <summary>
    ///   4辺独立ペンによる外接長方形描画
    /// </summary>
    public static void DrawOuterRectangle(this Graphics g, Pen topPen, Pen rightPen, Pen bottomPen, Pen leftPen, Rectangle rect) {
        g.DrawOuterRectangle(topPen, rightPen, bottomPen, leftPen, rect.X, rect.Y, rect.Width, rect.Height);
    }
    
#endregion
    
}

} // End of namespace
