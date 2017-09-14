using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MACS;
using MACS.Draw;

public partial class ColorUtilTestForm : Form {

    private Color color;

    public ColorUtilTestForm() {
        InitializeComponent();
        foreach(Control ctl in this.Controls) {
            if(ctl is PictureBox) {
                ctl.MouseMove += onMouseMove;
                ctl.MouseClick += onMouseClick;
            }
        }
        txtColorName.Text = "#FF0000";
    }

    private void update() {
        color = ColorUtil.Get(txtColorName.Text);
        double h,s,v;
        ColorUtil.GetHSV(color, out h, out s, out v);
        lblValue.Text = String.Format("#{0:X2}{1:X2}{2:X2}, hsl({3,3:F0},{4,3:F0}%,{5,3:F0}%), A={6:X2}",
                                      color.R, color.G, color.B,
                                      h, s*100.0, v*100.0,
                                      color.A);
        picColor.Invalidate();
        picSV.Invalidate();
        picHue.Invalidate();
        picBD.Invalidate();
    }

    private void ColorUtilTestForm_VisibleChanged(object sender, EventArgs e) {
        if (this.Visible)
            update();
    }

    private void ColorUtilTestForm_Resize(object sender, EventArgs e) {
        Invalidate(true);
    }

    private void txtColorName_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Enter) {
            update();
        }

    }

    private void picColor_Paint(object sender, PaintEventArgs e) {
        Graphics g = e.Graphics;
        using(Brush br = new SolidBrush(color)) {
            g.FillRectangle(br, picColor.ClientRectangle);
        }

    }

    private void picSV_Paint(object sender, PaintEventArgs e) {
        Graphics g = e.Graphics;
        double h,s,v;
        ColorUtil.GetHSV(color, out h, out s, out v);
        int xstep = 21;
        int ystep = 11;
        int x = picSV.ClientRectangle.X;
        for(int i = 0; i < xstep; i++) {
            int xx = picSV.ClientRectangle.X+(int)((double)(i+1)*(double)picSV.ClientRectangle.Width/(double)xstep);
            s = (double)i/(double)(xstep-1);
            int y = picSV.ClientRectangle.Y;
            for(int j = 0; j < ystep; j++) {
                int yy = picSV.ClientRectangle.Y+(int)((double)(j+1)*(double)picSV.ClientRectangle.Height/(double)ystep);
                v = (double)j/(double)(ystep-1);
                Color col = ColorUtil.FromHSV(h,s,v);
                using(Brush br = new SolidBrush(col)) {
                    g.FillRectangle(br, x, y, xx-x, yy-y);
                }
                using(Brush br = new SolidBrush(ColorUtil.GetBWColor(col))) {
                    g.FillEllipse(br, x+(xx-x-4)/2, y+(yy-y-4)/2, 4, 4);
                }
                y = yy;
            }
            x = xx;
        }

    }

    private void picHue_Paint(object sender, PaintEventArgs e) {
        Graphics g = e.Graphics;
        double h,s,v;
        ColorUtil.GetHSV(color, out h, out s, out v);
        int xstep = 360;
        int x = picHue.ClientRectangle.X;
        for(int i = 0; i < xstep; i++) {
            double hh = (double)i*360.0/(double)xstep-180.0+h;
            int xx = picHue.ClientRectangle.X+(int)((double)(i+1)*(double)picHue.ClientRectangle.Width/(double)xstep);
            using(Brush br = new SolidBrush(ColorUtil.FromHSV(hh,s,v))) {
                g.FillRectangle(br, x, picHue.ClientRectangle.Y, xx-x, picHue.ClientRectangle.Height);
            }
            x = xx;
        }
    }

    private void picBD_Paint(object sender, PaintEventArgs e) {
        Graphics g = e.Graphics;
        int xstep = 12;
        int x = picBD.ClientRectangle.X;
        int xx;
        for(int i = 0; i < xstep; i++) {
            xx = picBD.ClientRectangle.X+(int)((double)(i+1)*(double)picBD.ClientRectangle.Width/(double)xstep);
            Color col;
            if(i < xstep/2) {
                col = ColorUtil.BrightColor(color,(double)(xstep/2-1-i)/(double)(xstep/2-1));
            } else {
                col = ColorUtil.DarkColor(color,(double)(i-(xstep/2-1)-1)/(double)(xstep/2-1));
            }
            using(Brush br = new SolidBrush(col)) {
                g.FillRectangle(br, x, picBD.ClientRectangle.Y, xx-x, picBD.ClientRectangle.Height);
            }
            x = xx;
        }

    }

    private void onMouseMove(object sender, MouseEventArgs e) {
        Control ctl = sender as Control;
        Point loc = ctl.PointToScreen(e.Location);
        Color col = pickupColor(loc);
        double h,s,v;
        ColorUtil.GetHSV(col, out h, out s, out v);
        double l = ColorUtil.GetLuminance(col);
        lblOnMouse.Text = String.Format("Color on mouse = #{2:X2}{3:X2}{4:X2}, hsl({5,3:F0},{6,3:F0}%,{7,3:F0}%), L={8,4:F2}",
                                        e.Location.X, e.Location.Y,
                                        col.R, col.G, col.B,
                                        h, s*100.0, v*100.0,
                                        l);
    }

    private void onMouseClick(object sender, MouseEventArgs e) {
        Control ctl = sender as Control;
        Point loc = ctl.PointToScreen(e.Location);
        Color col = pickupColor(loc);
        txtColorName.Text = String.Format("#{0:X2}{1:X2}{2:X2}", col.R, col.G, col.B);
        update();
    }

    private Bitmap screenBit = new Bitmap(1,1);

    private Color pickupColor(Point loc) {
        using(Graphics gr = Graphics.FromImage(screenBit)) {
            gr.CopyFromScreen(loc, new Point(0,0), screenBit.Size);
            return screenBit.GetPixel(0,0);
        }
    }

}
