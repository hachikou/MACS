/// MPText: Macs Powered Text : Forms.Labelの機能強化版.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

/*
 * Macs Powered Text : Forms.Labelの機能強化版
 *
 * Copyright (C) 2017 Nippon C.A.D. Co.,Ltd. All rights reserved.
 * This code was designed and coded by SHIBUYA K. (Microbrains Inc.)
 */

using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using MACS;
using MACS.Draw;

namespace MACS.Draw {

/// <summary>
///   Forms.Labelの機能強化版
/// </summary>
public class MPText : System.Windows.Forms.Control {

    /// <summary>
    ///   文字色自動設定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     AutoColorをtrueにすると、背景色に応じて文字色が黒または白になります。
    ///   </para>
    /// </remarks>
    public bool AutoColor = true;

    /// <summary>
    ///   テキスト配置（水平方向）
    /// </summary>
    public TextHPosition HPosition = TextHPosition.LeftShrink;

    /// <summary>
    ///   テキスト位置（垂直方向）
    /// </summary>
    public TextVPosition VPosition = TextVPosition.Fit;
    
    /// <summary>
    ///   アウトライン幅（フォントサイズに対する比率）
    /// </summary>
    public float OutlineRatio = 0.1F;

    /// <summary>
    ///   アウトライン色
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     AutoColorがtrueのときまたはOutlineColor=ColorUtil.Autoのときは
    ///     文字色に応じて白か黒が自動的に選ばれます。
    ///   </para>
    /// </remarks>
    public Color OutlineColor = ColorUtil.Auto;

    /// <summary>
    ///   複数行あるときの行間隔（フォントサイズに対する倍率）
    /// </summary>
    public float LineHeight = 1.2F;

    /// <summary>
    ///   影付色
    /// </summary>
    public Color ShadowColor = Color.Transparent;

    /// <summary>
    ///   影付オフセット（X方向）（フォントサイズに対する比率）
    /// </summary>
    public float ShadowOffsetX = 0.1F;

    /// <summary>
    ///   影付オフセット（Y方向）（フォントサイズに対する比率）
    /// </summary>
    public float ShadowOffsetY = 0.1F;

    /// <summary>
    ///   テキスト表示属性定義
    /// </summary>
    public MPAttribute TextAttribute = null;
    
    
    public MPText() : base() {
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        this.DoubleBuffered = true;
        this.TabStop = false;
    }

    protected override void OnTextChanged(EventArgs e) {
        this.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e) {
        Graphics g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        if(TextAttribute == null)
            paintText(g);
        else
            paintAttrText(g);
    }

    private void paintText(Graphics g) {
        Color color, outlineColor;
        if(AutoColor || (ForeColor == ColorUtil.Auto) || (ForeColor == ColorUtil.Invalid)) {
            color = ColorUtil.GetBWColor(this.GetRealBackColor());
        } else {
            color = ForeColor;
        }
        if(AutoColor || (OutlineColor == ColorUtil.Auto) || (OutlineColor == ColorUtil.Invalid)) {
            outlineColor = ColorUtil.GetBWColor(color);
        } else {
            outlineColor = OutlineColor;
        }

        Brush  bgBrush = null;
        if((BackColor != null) && (BackColor.A > 0)) {
            //始点を-1,-1にしないとスキマが出来てしまうため、幅、高さを+1した
            Rectangle target = new Rectangle(-1, -1,this.Width + 1,this.Height + 1);
            bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, target);
        }
        float outlineWidth = Font.GetEmSize()*OutlineRatio;
        Brush brush = null;
        if(color.A > 0)
            brush = new SolidBrush(color);
        Pen pen = null;
        if((outlineColor.A > 0) && (outlineWidth > 0))
            pen = new Pen(outlineColor, outlineWidth);
        Brush shadow = null;
        if(ShadowColor.A > 0) {
            shadow = new SolidBrush(ShadowColor);
        }
        float shadowOffsetX = Font.GetEmSize()*ShadowOffsetX;
        float shadowOffsetY = Font.GetEmSize()*ShadowOffsetY;
        g.DrawText(Text, Font, pen, brush, ClientRectangle, HPosition, VPosition, LineHeight,
                   shadow:shadow, shadowOffsetX:shadowOffsetX, shadowOffsetY:shadowOffsetY);
        if(brush != null)
            brush.Dispose();
        if(pen != null)
            pen.Dispose();
        if(shadow != null)
            shadow.Dispose();
        if(bgBrush != null)
            bgBrush.Dispose();
    }

    private void paintAttrText(Graphics g) {
        g.DrawText(Text, TextAttribute, ClientRectangle, this);
    }

    protected override void OnPaintBackground(PaintEventArgs e) {
        if(Parent != null) {
            Parent.PaintBackground(e, this.Bounds);
        }
    }

}

} // End of namespace
