using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using MACS;
using MACS.Draw;

public partial class MPWidgetTestForm : Form {

    public MPWidgetTestForm() {
        InitializeComponent();
    }

    private void MPWidgetTestForm_Load(object sender, EventArgs e)　{
        mpText1.AutoColor = false;
        mpText1.ForeColor = ColorUtil.Auto;
        mpText1.OutlineColor = ColorUtil.Auto;
        mpText1.OutlineRatio = 0F;
        mpText1.HPosition = TextHPosition.CenterShrink;
        mpText1.VPosition = TextVPosition.Top;
        mpButton1.BackColor = ColorUtil.Auto;
        txtText.Text = mpText1.Text;
        txtColor.Text = mpText1.ForeColor.ToRGBString();
        txtOutlineColor.Text = mpText1.OutlineColor.ToRGBString();
        txtOutlineRatio.Text = mpText1.OutlineRatio.ToString("F3");

    }

    private void txtText_TextChanged(object sender, EventArgs e) {
        mpText1.Text = txtText.Text;
        mpButton1.Text = txtText.Text;
    }

    private void txtColor_TextChanged(object sender, EventArgs e) {
        mpText1.ForeColor = ColorUtil.Get(txtColor.Text);
        mpText1.Invalidate();
        mpButton1.ForeColor = ColorUtil.Get(txtColor.Text);
        mpButton1.Invalidate();
    }

    private void txtBackColor_TextChanged(object sender, EventArgs e) {
        mpText1.BackColor = ColorUtil.Get(txtBackColor.Text);
        mpText1.Invalidate();
        mpButton1.BackColor = ColorUtil.Get(txtBackColor.Text);
        mpButton1.Invalidate();
    }

    private void txtOutlineColor_TextChanged(object sender, EventArgs e) {
        mpText1.OutlineColor = ColorUtil.Get(txtOutlineColor.Text);
        mpText1.Invalidate();
    }

    private void txtOutlineRatio_TextChanged(object sender, EventArgs e) {
        mpText1.OutlineRatio = StringUtil.ToFloat(txtOutlineRatio.Text);
        mpText1.Invalidate();
    }

    private void chkTextAttribute_CheckedChanged(object sender, EventArgs e) {
        if (chkTextAttribute.Checked) {
            using (XmlFile xml = new XmlFile("MPWidgetTest.xml", "test")) {
                MPAttribute style = new MPAttribute(xml.Root);
                mpText1.TextAttribute = new MPAttribute(xml.GetSubRoot("text"), style);
                mpButton1.TextAttribute = new MPAttribute(xml.GetSubRoot("button"), style);
            }
        } else {
            mpText1.TextAttribute = null;
            mpButton1.TextAttribute = null;
        }
        mpText1.Invalidate();
        mpButton1.Invalidate();
    }

    private void radHpos_CheckedChanged(object sender, EventArgs e) {
        if (radHposLeft.Checked) {
            mpText1.HPosition = TextHPosition.LeftShrink;
        } else if (radHposCenter.Checked) {
            mpText1.HPosition = TextHPosition.CenterShrink;
        } else if (radHposRight.Checked) {
            mpText1.HPosition = TextHPosition.RightShrink;
        }
        mpText1.Invalidate();
    }

    private void radVpos_CheckedChanged(object sender, EventArgs e) {
        if (radVposTop.Checked) {
            mpText1.VPosition = TextVPosition.Top;
        } else if (radVposMiddle.Checked) {
            mpText1.VPosition = TextVPosition.Middle;
        } else if (radVposBottom.Checked) {
            mpText1.VPosition = TextVPosition.Bottom;
        } else if (radVposFit.Checked) {
            mpText1.VPosition = TextVPosition.Fit;
        } else if (radVposProportional.Checked) {
            mpText1.VPosition = TextVPosition.Proportional;
        }
        mpText1.Invalidate();
    }

}
