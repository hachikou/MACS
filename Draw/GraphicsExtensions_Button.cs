/// GraphicsExtensions_Button: System.Drawing.Graphicsクラスの拡張メソッド: ボタン描画.
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

#region ボタン描画
        
    /// <summary>
    ///   ボタンを描画する
    /// </summary>
    public static void DrawButton(this Graphics g,
                                  Rectangle rect, float radius, string text,
                                  ButtonFace face) {
        if(String.IsNullOrEmpty(text)) {
            g.DrawButton(rect, radius, new string[0], face);
        } else {
            g.DrawButton(rect, radius, text.Split("\n".ToCharArray()), face);
        }
    }
    
    /// <summary>
    ///   ボタンを描画する
    /// </summary>
    public static void DrawButton(this Graphics g,
                                  Rectangle rect, float radius, string[] text,
                                  ButtonFace face) {
        if(face.TextFont == null)
            face.TextFont = System.Windows.Forms.Control.DefaultFont;

        // 内接長方形
        Rectangle iRect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        if((face.TopLeftPen != null) && (face.TopLeftPen.Width > 0)) {
            int sz = (int)Math.Ceiling(face.TopLeftPen.Width/2F);
            iRect.X += sz;
            iRect.Y += sz;
            iRect.Width -= sz;
            iRect.Height -= sz;
        }
        if((face.BottomRightPen != null) && (face.BottomRightPen.Width > 0)) {
            int sz = (int)Math.Ceiling(face.BottomRightPen.Width/2F);
            iRect.Width -= sz;
            iRect.Height -= sz;
        }

        // 内接長方形に合わせて角丸半径を調整
        if((int)radius > iRect.Width/2)
            radius = (float)iRect.Width/2.0F;
        if((int)radius > iRect.Height/2)
            radius = (float)iRect.Height/2.0F;
        
        // パスの作成
        int rr = (int)(radius*2F);
        if(rr <= 0)
            rr = 1;
        GraphicsPath upperPath = new GraphicsPath();
        upperPath.StartFigure();
        upperPath.AddArc(iRect.X, iRect.Y, rr, rr, 180F, 90F);
        upperPath.AddArc(iRect.X+iRect.Width-rr, iRect.Y, rr, rr, -90F, 90F);
        upperPath.AddLine(iRect.X+iRect.Width, iRect.Y+rr/2+1, iRect.X, iRect.Y+iRect.Height-rr/2+1);
        upperPath.CloseFigure();
        GraphicsPath lowerPath = new GraphicsPath();
        lowerPath.StartFigure();
        lowerPath.AddLine(iRect.X, iRect.Y+iRect.Height-rr/2, iRect.X+iRect.Width, iRect.Y+rr/2);
        lowerPath.AddArc(iRect.X+iRect.Width-rr, iRect.Y+iRect.Height-rr, rr, rr, 0F, 90F);
        lowerPath.AddArc(iRect.X, iRect.Y+iRect.Height-rr, rr, rr, 90F, 90F);
        lowerPath.CloseFigure();
        GraphicsPath topLeftPath = new GraphicsPath();
        topLeftPath.StartFigure();
        topLeftPath.AddArc(iRect.X, iRect.Y+iRect.Height-rr, rr, rr, 135F, 45F);
        topLeftPath.AddArc(iRect.X, iRect.Y, rr, rr, 180F, 90F);
        topLeftPath.AddArc(iRect.X+iRect.Width-rr, iRect.Y, rr, rr, -90F, 45F);
        GraphicsPath bottomRightPath = new GraphicsPath();
        bottomRightPath.StartFigure();
        bottomRightPath.AddArc(iRect.X+iRect.Width-rr, iRect.Y, rr, rr, -45F, 45F);
        bottomRightPath.AddArc(iRect.X+iRect.Width-rr, iRect.Y+iRect.Height-rr, rr, rr, 0F, 90F);
        bottomRightPath.AddArc(iRect.X, iRect.Y+iRect.Height-rr, rr, rr, 90F, 45F);

        // ボタン背景描画
        if(face.UpperBrush != null)
            g.FillPath(face.UpperBrush, upperPath);
        if(face.LowerBrush != null)
            g.FillPath(face.LowerBrush, lowerPath);
        if(face.TopLeftPen != null)
            g.DrawPath(face.TopLeftPen, topLeftPath);
        if(face.BottomRightPen != null)
            g.DrawPath(face.BottomRightPen, bottomRightPath);

        upperPath.Dispose();
        lowerPath.Dispose();
        topLeftPath.Dispose();
        bottomRightPath.Dispose();
        
        // 文字描画
        if((text != null) && (text.Length > 0)) {
            // テキスト描画エリア
            int mgn = (int)(radius/(1.41421356F*2F));
            if(mgn < (int)face.BorderWidth*2)
                mgn = (int)face.BorderWidth*2;
            Rectangle tRect = new Rectangle(rect.X+mgn+(int)face.TextOffset.X, rect.Y+mgn+(int)face.TextOffset.Y, rect.Width-mgn*2, rect.Height-mgn*2);

            g.DrawText(text, face.TextFont, face.TextPen, face.TextBrush, tRect,
                       TextHPosition.CenterShrink, TextVPosition.Proportional,
                       1.0F, face.FontRatio);
        }
    }
    
#endregion
    
}

/// <summary>
///   ボタン描画パラメータ
/// </summary>
public class ButtonFace: IDisposable {

    /// <summary>
    ///   デフォルトコンストラクタ
    /// </summary>
    public ButtonFace() {}

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Font font, Color color, float borderWidth=4.0F) {
        if(font != null)
            textFont = font.Copy();
        SetColor(color, borderWidth, ColorUtil.Auto);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Color color, float borderWidth=4.0F) {
        SetColor(color, borderWidth, ColorUtil.Auto);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Font font, Color color, float shadowStrength, float borderWidth=4.0F) {
        if(font != null)
            textFont = font.Copy();
        SetColor(color, shadowStrength, borderWidth, ColorUtil.Auto, 0.3F);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Color color, float shadowStrength, float borderWidth=4.0F) {
        SetColor(color, shadowStrength, borderWidth, ColorUtil.Auto, 0.3F);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Font font, Color color, float borderWidth, Color borderColor) {
        if(font != null)
            textFont = font.Copy();
        SetColor(color, borderWidth, borderColor);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Color color, float borderWidth, Color borderColor) {
        SetColor(color, borderWidth, borderColor);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Font font, Color color, float shadowStrength, float borderWidth, Color borderColor, float borderShadowStrength) {
        if(font != null)
            textFont = font.Copy();
        SetColor(color, shadowStrength, borderWidth, borderColor, borderShadowStrength);
    }

    /// <summary>
    ///   コンストラクタ
    /// </summary>
    public ButtonFace(Color color, float shadowStrength, float borderWidth, Color borderColor, float borderShadowStrength) {
        SetColor(color, shadowStrength, borderWidth, borderColor, borderShadowStrength);
    }

    /// <summary>
    ///   コピーコンストラクタ
    /// </summary>
    public ButtonFace(ButtonFace face) {
        CopyFrom(face);
    }

    /// <summary>
    ///   デストラクタ
    /// </summary>
    ~ButtonFace() {
        Dispose();
    }

    /// <summary>
    ///   資源解放
    /// </summary>
    public void Dispose() {
        cleanPenAndBrush();
        cleanFont();
    }

    /// <summary>
    ///   指定色に基づいてペンやブラシをセットアップする
    /// </summary>
    public void SetColor(Color color) {
        SetColor(color, BorderWidth, ColorUtil.Auto);
    }
        
    public void SetColor(Color color, float borderWidth) {
        SetColor(color, borderWidth, ColorUtil.Auto);
    }

    public void SetColor(Color color, float borderWidth, Color borderColor) {
        SetColor(color, 0.1F, borderWidth, borderColor, 0.3F);
    }

    public void SetColor(Color color, float shadowStrength, float borderWidth, Color borderColor, float borderShadowStrength) {
        cleanPenAndBrush();
        Color textColor = ColorUtil.GetBWColor(color);
        float textPenWidth = 2F;
        if(textFont != null) {
            textPenWidth = textFont.Height/20F+1F;
        }
        textPen = new Pen((ColorUtil.GetLuminance(textColor) > 0.5)?ColorUtil.Get("#AA000000"):ColorUtil.Get("#AAFFFFFF"), textPenWidth);
        textBrush = new SolidBrush(textColor);
        upperBrush = new SolidBrush(color);
        lowerBrush = new SolidBrush(ColorUtil.DarkColor(color, shadowStrength));
        Color topLeftColor, bottomRightColor;
        if((borderColor == ColorUtil.Auto) || (borderColor == ColorUtil.Invalid)) {
            topLeftColor = ColorUtil.BrightColor(color, 0.6);
            bottomRightColor = ColorUtil.DarkColor(color, 0.6);
        } else {
            topLeftColor = borderColor;
            bottomRightColor = ColorUtil.DarkColor(borderColor, borderShadowStrength);
        }
        topLeftPen = new Pen(topLeftColor, borderWidth);
        bottomRightPen = new Pen(bottomRightColor, borderWidth);
    }

    /// <summary>
    ///   文字表示色をセットする
    /// </summary>
    public void SetTextColor(Color color) {
        if(textBrush != null)
            textBrush.Dispose();
        textBrush = new SolidBrush(color);
        if(textPen != null) {
            Pen pen = new Pen((ColorUtil.GetLuminance(color) > 0.5)?ColorUtil.Get("#AA000000"):ColorUtil.Get("#AAFFFFFF"), textPen.Width);
            textPen.Dispose();
            textPen = pen;
        }
    }
    
    /// <summary>
    ///   フォントと文字の縦横比率をセットする
    /// </summary>
    public void SetFont(Font font, float ratio=float.MaxValue) {
        TextFont = font;
        if(ratio != float.MaxValue)
            fontRatio = ratio;
    }

    /// <summary>
    ///   枠線幅をセットする
    /// </summary>
    public void SetBorderWidth(float borderWidth) {
        if(topLeftPen != null) {
            Pen pen = new Pen(topLeftPen.Color, borderWidth);
            topLeftPen.Dispose();
            topLeftPen = pen;
        }
        if(bottomRightPen != null) {
            Pen pen = new Pen(bottomRightPen.Color, borderWidth);
            bottomRightPen.Dispose();
            bottomRightPen = pen;
        }
    }

    /// <summary>
    ///   枠線色をセットする
    /// </summary>
    public void SetBorderColor(Color color) {
        SetBorderColor(color, color);
    }
    
    public void SetBorderColor(Color topLeftColor, Color bottomRightColor) {
        if(topLeftPen != null) {
            Pen pen = new Pen(topLeftColor, topLeftPen.Width);
            topLeftPen.Dispose();
            topLeftPen = pen;
        }
        if(bottomRightPen != null) {
            Pen pen = new Pen(bottomRightColor, bottomRightPen.Width);
            bottomRightPen.Dispose();
            bottomRightPen = pen;
        }
    }
    
    /// <summary>
    ///   ボタンパラメータの複製を作成する
    /// </summary>
    public ButtonFace Copy() {
        ButtonFace face = new ButtonFace();
        face.CopyFrom(this);
        return face;
    }

    /// <summary>
    ///   ボタンパラメータを複製する
    /// </summary>
    public void CopyFrom(ButtonFace src) {
        this.TextFont = src.TextFont; // this.TextPenの幅が変わらないように先にセットする
        this.FontRatio = src.FontRatio;
        this.TextPen = src.TextPen;
        this.TextBrush = src.TextBrush;
        this.UpperBrush = src.UpperBrush;
        this.LowerBrush = src.LowerBrush;
        this.TopLeftPen = src.TopLeftPen;
        this.BottomRightPen = src.BottomRightPen;
        this.TextOffset = src.TextOffset;
    }
        
    
    /// <summary>
    ///   本ボタンパラメータを元に、押下時のパラメータを作成する
    /// </summary>
    public ButtonFace MakePushedFace() {
        ButtonFace face = new ButtonFace();
        face.TextFont = textFont; // face.TextPenの幅が変わらないように先にセットする
        face.FontRatio = fontRatio;
        face.TextPen = textPen;
        face.TextBrush = textBrush;
        face.UpperBrush = lowerBrush;
        face.LowerBrush = upperBrush;
        face.TopLeftPen = bottomRightPen;
        face.BottomRightPen = topLeftPen;
        if(face.TopLeftPen != null) {
            face.TextOffset = new PointF(textOffset.X+face.TopLeftPen.Width/2F, textOffset.Y+face.TopLeftPen.Width/2F);
        }
        return face;
    }
    
    /// <summary>
    ///   本ボタンパラメータを元に、Disabled時のパラメータを作成する
    /// </summary>
    public ButtonFace MakeDisabledFace() {
        ButtonFace face = new ButtonFace(this);
        face.ChangeSV(0.1, 0.5);
        return face;
    }

    /// <summary>
    ///   色の強さを変える
    /// </summary>
    public void ChangeSV(double ss, double vv) {
        double h, s, v;
        if(textPen != null) {
            ColorUtil.GetHSV(textPen.Color, out h, out s, out v);
            Pen p = new Pen(ColorUtil.FromHSV(h, s*ss, (v+vv)/2.0), textPen.Width);
            textPen.Dispose();
            textPen = p;
        }
        if((textBrush != null) && (textBrush is SolidBrush)) {
            Color c = (textBrush as SolidBrush).Color;
            ColorUtil.GetHSV(c, out h, out s, out v);
            Brush b = new SolidBrush(ColorUtil.FromHSV(h, s*ss, (v+vv)/2.0));
            textBrush.Dispose();
            textBrush = b;
        }
        if((upperBrush != null) && (upperBrush is SolidBrush)) {
            Color c = (upperBrush as SolidBrush).Color;
            ColorUtil.GetHSV(c, out h, out s, out v);
            Brush b = new SolidBrush(ColorUtil.FromHSV(h, s*ss, (v+vv)/2.0));
            upperBrush.Dispose();
            upperBrush = b;
        }
        if((lowerBrush != null) && (lowerBrush is SolidBrush)) {
            Color c = (lowerBrush as SolidBrush).Color;
            ColorUtil.GetHSV(c, out h, out s, out v);
            Brush b = new SolidBrush(ColorUtil.FromHSV(h, s*ss, (v+vv)/2.0));
            lowerBrush.Dispose();
            lowerBrush = b;
        }
        if(topLeftPen != null) {
            ColorUtil.GetHSV(topLeftPen.Color, out h, out s, out v);
            Pen p = new Pen(ColorUtil.FromHSV(h, s*ss, (v+vv)/2.0), topLeftPen.Width);
            topLeftPen.Dispose();
            topLeftPen = p;
        }
        if(bottomRightPen != null) {
            ColorUtil.GetHSV(bottomRightPen.Color, out h, out s, out v);
            Pen p = new Pen(ColorUtil.FromHSV(h, s*ss, (v+vv)/2.0), bottomRightPen.Width);
            bottomRightPen.Dispose();
            bottomRightPen = p;
        }
    }

    
    /// <summary>
    ///   テキスト描画用ペン
    /// </summary>
    public Pen TextPen {
        get { return textPen; }
        set { textPen = (value==null)?null:(Pen)value.Clone(); }
    }

    /// <summary>
    ///   テキスト描画用ブラシ
    /// </summary>
    public Brush TextBrush {
        get { return textBrush; }
        set { textBrush = (value==null)?null:(Brush)value.Clone(); }
    }

    /// <summary>
    ///   ボタン上半分描画用ブラシ
    /// </summary>
    public Brush UpperBrush {
        get { return upperBrush; }
        set { upperBrush = (value==null)?null:(Brush)value.Clone(); }
    }

    /// <summary>
    ///   ボタン下半分描画用ブラシ
    /// </summary>
    public Brush LowerBrush {
        get { return lowerBrush; }
        set { lowerBrush = (value==null)?null:(Brush)value.Clone(); }
    }

    /// <summary>
    ///   左上枠線描画用ペン
    /// </summary>
    public Pen TopLeftPen {
        get { return topLeftPen; }
        set { topLeftPen = (value==null)?null:(Pen)value.Clone(); }
    }

    /// <summary>
    ///   右下枠線描画用ペン
    /// </summary>
    public Pen BottomRightPen {
        get { return bottomRightPen; }
        set { bottomRightPen = (value==null)?null:(Pen)value.Clone(); }
    }

    /// <summary>
    ///   枠線太さ
    /// </summary>
    public float BorderWidth {
        get {
            float w = 0F;
            if(topLeftPen != null)
                w += topLeftPen.Width;
            if(bottomRightPen != null)
                w += bottomRightPen.Width;
            return w/2F;
        }
    }
    
    /// <summary>
    ///   フォント
    /// </summary>
    public Font TextFont {
        get { return textFont; }
        set {
            textFont = (value==null)?null:(Font)value.Clone();
            if((textFont != null) && (textPen != null)) {
                Color c = textPen.Color;
                textPen.Dispose();
                textPen = new Pen(c, textFont.Height/10F+1F);
            }
        }
    }

    /// <summary>
    ///   文字縦横比
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     1.0でフォント本来の縦横比、<1.0で細身、>1.0で幅広の文字を描く
    ///   </para>
    /// </remarks>
    public float FontRatio {
        get { return fontRatio; }
        set { fontRatio = value; }
    }

    /// <summary>
    ///   文字描画位置調整
    /// </summary>
    public PointF TextOffset {
        get { return textOffset; }
        set { textOffset = value; }
    }
    
    private Pen textPen = null;
    private Brush textBrush = null;
    private Brush upperBrush = null;
    private Brush lowerBrush = null;
    private Pen topLeftPen = null;
    private Pen bottomRightPen = null;
    private Font textFont = null;
    private float fontRatio = 1.0F;
    private PointF textOffset = new PointF(0,0);

    private void cleanPenAndBrush() {
        if(textPen != null) {
            textPen.Dispose();
            textPen = null;
        }
        if(textBrush != null) {
            textBrush.Dispose();
            textBrush = null;
        }
        if(upperBrush != null) {
            upperBrush.Dispose();
            upperBrush = null;
        }
        if(lowerBrush != null) {
            lowerBrush.Dispose();
            lowerBrush = null;
        }
        if(topLeftPen != null) {
            topLeftPen.Dispose();
            topLeftPen = null;
        }
        if(bottomRightPen != null) {
            bottomRightPen.Dispose();
            bottomRightPen = null;
        }
    }

    private void cleanFont() {
        if(textFont != null) {
            textFont.Dispose();
            textFont = null;
        }
        fontRatio = 1.0F;
    }
    
}

} // End of namespace
