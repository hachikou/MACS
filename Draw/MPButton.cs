/// MPButton: Macs Powered Button : Forms.Buttonの機能強化版.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using MACS;

namespace MACS.Draw {

/// <summary>
///   Forms.Buttonの機能強化版
/// </summary>
#if USE_TOUCH
public class MPButton : WMTouchControl {
#else
public class MPButton : Control {
#endif

    public static readonly Color DefaultButtonColor = ColorUtil.Get("rgb(245,245,245)");

    /// <summary>
    ///   角の丸み
    /// </summary>
    public float Radius = 8F;

    /// <summary>
    ///   枠線太さ
    /// </summary>
    public float BorderWidth = 1F;

    /// <summary>
    ///   影付強さ
    /// </summary>
    public float ShadowStrength = 0.1F;

    /// <summary>
    ///   枠色
    /// </summary>
    public Color BorderColor = ColorUtil.Auto;
    
    /// <summary>
    ///   枠影付強さ
    /// </summary>
    public float BorderShadowStrength = 0.3F;

    /// <summary>
    ///   文字色自動設定
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     AutoForeColorをtrueにすると、背景色に応じて文字色が黒または白になります。
    ///   </para>
    /// </remarks>
    public bool AutoForeColor = true;

    /// <summary>
    ///   フォーカスが当たっているときの枠線の色
    /// </summary>
    public Color FocusColor = Color.Cyan;

    /// <summary>
    ///   マウスオーバー時の枠線の色
    /// </summary>
    public Color HoverColor = Color.Black;

    /// <summary>
    ///   テキスト表示属性定義
    /// </summary>
    public MPAttribute TextAttribute = null;
    
    
    public bool MultiState {
        get { return (nState > 0); }
        set {
            if(value)
                setNumberOfState(2);
            else
                setNumberOfState(0);
        }
    }
    
    public int NumberOfState {
        get { return nState; }
        set { setNumberOfState(value); }
    }
    
    public int State {
        get { return state; }
        set {
            if(state == value)
                return;
            state = value;
            this.Refresh();
        }
    }
    
    public Color[] StateColor;
    public bool UseEnabledColor = false;

    protected enum ButtonStatusCode {
        Normal,
        Hover,
        Clicked,
    }
    protected ButtonStatusCode ButtonStatus;


    public MPButton() : base() {
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        this.MouseDown += new MouseEventHandler(this.btnMouseDown);
        this.MouseUp += new MouseEventHandler(this.btnMouseUp);
        this.MouseEnter += new EventHandler(this.btnMouseEnter);
        this.MouseLeave += new EventHandler(this.btnMouseLeave);
        this.Click += new EventHandler(this.btnClick);
        this.Enter += new EventHandler(this.btnEnter);
        this.Leave += new EventHandler(this.btnLeave);
#if USE_TOUCH
        this.TouchDown += btnTouchDown;
        this.TouchUp += btnTouchUp;
        this.TouchMove += btnTouchMove;
#endif
        nState = 0;
    }

    protected override void OnPaint(PaintEventArgs e) {
        Graphics g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        Color backColor;
        if(nState <= 0) {
            // 通常のボタン色
            //if(this.UseVisualStyleBackColor) {
            //    backColor = DefaultButtonColor;
            //} else {
                backColor = this.BackColor;
            //}
        } else {
            // マルチステートボタン色
            if(State <= 0)
                backColor = StateColor[0];
            else if(State < nState)
                backColor = StateColor[State];
            else
                backColor = StateColor[nState-1];
        }
        ButtonFace face = new ButtonFace(this.Font, backColor, this.ShadowStrength, this.BorderWidth, this.BorderColor, this.BorderShadowStrength);
        if(!this.AutoForeColor)
            face.SetTextColor(this.ForeColor);
        if(this.UseEnabledColor || this.Enabled) {
            if((ButtonStatus == ButtonStatusCode.Hover) && (HoverColor.A > 0)) {
                face.SetBorderColor(HoverColor);
            } else if(ButtonStatus == ButtonStatusCode.Clicked) {
                ButtonFace xface = face.MakePushedFace();
                face.Dispose();
                face = xface;
            } else if(this.Focused && (FocusColor.A > 0)) {
                face.SetBorderColor(FocusColor, ColorUtil.DarkColor(FocusColor,0.2));
            }
        } else {
            ButtonFace xface = face.MakeDisabledFace();
            face.Dispose();
            face = xface;
        }
        if(TextAttribute == null) {
            g.DrawButton(this.ClientRectangle, Radius, this.Text, face);
        } else {
            g.DrawButton(this.ClientRectangle, Radius, this.Text, TextAttribute, face);
        }

        //Pathを計算する。
        GraphicsPath gPath = new GraphicsPath();
        addButtonPath(gPath);
        this.Region = new System.Drawing.Region(gPath);
        gPath.Dispose();

        face.Dispose();
    }

    /// <summary>
    /// コントールパス追加
    /// </summary>
    /// <param name="gPath">パスを追加する領域</param>
    /// <param name="x">並行移動X座標</param>
    /// <param name="y">並行移動Y座標</param>
    public void AddButtonPath(GraphicsPath gPath, float x = 0, float y = 0) {        
        // 内接長方形
        Rectangle iRect = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
        // ボタン描画パラメータ
        ButtonFace face = new ButtonFace(this.Font, this.BackColor, this.ShadowStrength, this.BorderWidth, this.BorderColor, this.BorderShadowStrength);

        if ((face.TopLeftPen != null) && (face.TopLeftPen.Width > 0)) {
            int sz = (int)Math.Ceiling(face.TopLeftPen.Width / 2F);
            iRect.X += sz;
            iRect.Y += sz;
            iRect.Width -= sz;
            iRect.Height -= sz;
        }
        if ((face.BottomRightPen != null) && (face.BottomRightPen.Width > 0)) {
            int sz = (int)Math.Ceiling(face.BottomRightPen.Width / 2F);
            iRect.Width -= sz;
            iRect.Height -= sz;
        }

        // 内接長方形に合わせて角丸半径を調整
        if ((int)this.Radius > iRect.Width/2)
            this.Radius = (float)iRect.Width/2.0F;
        if((int)this.Radius > iRect.Height/2)
            this.Radius = (float)iRect.Height/2.0F;
        
        // パスの作成
        int rr = (int)(this.Radius*2F);
        if(rr <= 0)
            rr = 1;

        gPath.StartFigure();
        gPath.AddArc(x + iRect.X, y+ iRect.Y+iRect.Height-rr, rr, rr, 135F, 45F);
        gPath.AddArc(x + iRect.X, y+ iRect.Y, rr, rr, 180F, 90F);
        gPath.AddArc(x + iRect.X+iRect.Width-rr, y+ iRect.Y, rr, rr, -90F, 45F);

        gPath.AddArc(x + iRect.X+iRect.Width-rr, y+ iRect.Y, rr, rr, -45F, 45F);
        gPath.AddArc(x + iRect.X+iRect.Width-rr, y+ iRect.Y+iRect.Height-rr, rr, rr, 0F, 90F);
        gPath.AddArc(x + iRect.X, y+ iRect.Y+iRect.Height-rr, rr, rr, 90F, 45F);
        gPath.CloseFigure();

        face.Dispose();
    }

    protected override void OnPaintBackground(PaintEventArgs e) {
        if((Radius >= 1F) && (Parent != null)) {
            Parent.PaintBackground(e, this.Bounds);
        }
    }

    protected override void OnTextChanged(EventArgs e) {
        this.Invalidate();
    }
    
    private void btnMouseDown(object sender, MouseEventArgs e) {
        setBtnStatus(ButtonStatusCode.Clicked);
    }

    private void btnMouseEnter(object sender, EventArgs e) {
        setBtnStatus(ButtonStatusCode.Hover);
    }

    private void btnMouseLeave(object sender, EventArgs e) {
        setBtnStatus(ButtonStatusCode.Normal);
    }

    private void btnMouseUp(object sender, MouseEventArgs e) {
        setBtnStatus(ButtonStatusCode.Normal);
    }

    private void btnClick(object sender, EventArgs e) {
        if (!this.Enabled)
            return;
        this.Focus();
        if(nState <= 0)
            return;
        state++;
        if(state >= nState)
            state = 0;
        this.Refresh();
    }

    private void btnEnter(object sender, EventArgs e) {
        this.Invalidate();
    }
    
    private void btnLeave(object sender, EventArgs e) {
        this.Invalidate();
    }
    
    private int state;
    private int nState;
    private void setNumberOfState(int n) {
        if(n == nState)
            return;
        nState = n;
        StateColor = new Color[nState];
        if(nState > 0)
            StateColor[0] = DefaultButtonColor;
        if(nState > 1)
            StateColor[1] = Color.FromArgb(0xf5, 0xf5, 0x30);
        if(nState > 2)
            StateColor[2] = Color.FromArgb(0xf5, 0x30, 0x30);
        for(int i = 3; i < nState; i++)
            StateColor[i] = Color.FromArgb(0x30, 0x30, 0x30);
    }

#if USE_TOUCH
    private void btnTouchDown(object sender, WMTouchEventArgs e) {
        setBtnStatus(ButtonStatusCode.Clicked);
    }

    private void btnTouchUp(object sender, WMTouchEventArgs e) {
        setBtnStatus(ButtonStatusCode.Normal);
    }

    private void btnTouchMove(object sender, WMTouchEventArgs e) {
    }
    
#endif

    private void setBtnStatus(ButtonStatusCode btn_status) {
        ButtonStatus = btn_status;
        this.Invalidate();
    }

    /// <summary>
    /// コントールパス追加
    /// </summary>
    /// <param name="gPath">パスを追加する領域</param>
    /// <param name="x">並行移動X座標</param>
    /// <param name="y">並行移動Y座標</param>
    private void addButtonPath(GraphicsPath gPath, float x = 0, float y = 0) {        
        // 内接長方形
        Rectangle iRect = new Rectangle(this.ClientRectangle.X, this.ClientRectangle.Y, this.ClientRectangle.Width, this.ClientRectangle.Height);
        // ボタン描画パラメータ
        ButtonFace face = new ButtonFace(this.Font, this.BackColor, this.ShadowStrength, this.BorderWidth);

        if ((face.TopLeftPen != null) && (face.TopLeftPen.Width > 0)) {
            int sz = (int)Math.Ceiling(face.TopLeftPen.Width / 2F);
            iRect.X += sz;
            iRect.Y += sz;
            iRect.Width -= sz;
            iRect.Height -= sz;
        }
        if ((face.BottomRightPen != null) && (face.BottomRightPen.Width > 0)) {
            int sz = (int)Math.Ceiling(face.BottomRightPen.Width / 2F);
            iRect.Width -= sz;
            iRect.Height -= sz;
        }

        // 内接長方形に合わせて角丸半径を調整
        if ((int)this.Radius > iRect.Width/2)
            this.Radius = (float)iRect.Width/2.0F;
        if((int)this.Radius > iRect.Height/2)
            this.Radius = (float)iRect.Height/2.0F;
        
        // パスの作成
        int rr = (int)(this.Radius*2F);
        if(rr <= 0)
            rr = 1;

        gPath.StartFigure();
        gPath.AddArc(x + iRect.X, y+ iRect.Y+iRect.Height-rr, rr, rr, 135F, 45F);
        gPath.AddArc(x + iRect.X, y+ iRect.Y, rr, rr, 180F, 90F);
        gPath.AddArc(x + iRect.X+iRect.Width-rr, y+ iRect.Y, rr, rr, -90F, 45F);

        gPath.AddArc(x + iRect.X+iRect.Width-rr, y+ iRect.Y, rr, rr, -45F, 45F);
        gPath.AddArc(x + iRect.X+iRect.Width-rr, y+ iRect.Y+iRect.Height-rr, rr, rr, 0F, 90F);
        gPath.AddArc(x + iRect.X, y+ iRect.Y+iRect.Height-rr, rr, rr, 90F, 45F);
        gPath.CloseFigure();

        face.Dispose();
    }
}

} // End of namespace
