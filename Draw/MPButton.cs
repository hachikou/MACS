/*
 * Macs Powered Button : Forms.Buttonの機能強化版
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
        ButtonFace face = new ButtonFace(this.Font, backColor, this.ShadowStrength, this.BorderWidth);
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
}

} // End of namespace
