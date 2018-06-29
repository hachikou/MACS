/// DrawTextTestForm: GraphicsExtensionsのテスト用フォーム.
///
/// Copyright (C) 2008-2018 by Microbrains Inc. and Nippon C.A.D. Co.,Ltd.
/// Released under the MIT license
/// See ../MITLicense.txt

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using MACS;
using MACS.Draw;

public partial class DrawTextTestForm : Form {

    public DrawTextTestForm() {
        InitializeComponent();
    }

    private void DrawTextTestForm_Paint(object sender, PaintEventArgs e) {
        Graphics g = e.Graphics;
        // グリッド描画
        using(Pen pen = new Pen(Color.White, 1.0F)) {
            for(int i = ClientRectangle.X; i < ClientRectangle.Width; i += 20) {
                g.DrawLine(pen, i, 0, i, ClientRectangle.Height);
            }
            for(int j = ClientRectangle.Y; j < ClientRectangle.Height; j += 20) {
                g.DrawLine(pen, 0, j, ClientRectangle.Width, j);
            }
        }

        g.SmoothingMode = SmoothingMode.AntiAlias;

        // DrawTextのテスト
        string[] text = new string[]{
            "Hello",
            "こんにちわみなさん",
            "Hola!"};
        Rectangle rect = new Rectangle(20, 20, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.Left, TextVPosition.Top);
        }
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        
        rect = new Rectangle(240, 20, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.Center, TextVPosition.Middle);
        }
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        
        rect = new Rectangle(460, 20, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.Right, TextVPosition.Bottom);
        }
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        
        // DrawTextのテスト（Fit, Shrink)
        rect = new Rectangle(20, 300, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.LeftShrink, TextVPosition.Middle);
        }
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        
        rect = new Rectangle(240, 300, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.CenterShrink, TextVPosition.Fit);
        }
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }

        rect = new Rectangle(460, 300, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.RightShrink, TextVPosition.Proportional);
        }
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }

        // 属性指定文字列描画のテスト
        XmlFile xml = new XmlFile("DrawTextTest.xml", "test");
        MPAttribute style = new MPAttribute(xml.Root);

        XmlElement elem = xml.GetSubRoot("text1");
        MPAttribute attr = new MPAttribute(elem, style);
        string xtext = elem.InnerXml.Trim();
        rect = new Rectangle(20, 560, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        rect = g.DrawText(xtext, attr, rect);
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        
        elem = xml.GetSubRoot("text2");
        attr = new MPAttribute(elem, style);
        xtext = elem.InnerXml.Trim();
        rect = new Rectangle(240, 560, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        rect = g.DrawText(xtext, attr, rect);
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }

        elem = xml.GetSubRoot("text3");
        attr = new MPAttribute(elem, style);
        xtext = elem.InnerXml.Trim();
        rect = new Rectangle(460, 560, 200, 200);
        using(Pen pen = new Pen(Color.Red, 1F)) {
            g.DrawRectangle(pen, rect);
        }
        rect = g.DrawText(xtext, attr,rect);
        using(Pen pen = new Pen(Color.Green, 1F)) {
            g.DrawRectangle(pen, rect);
        }

        xml.Dispose();

        // 影付き文字のテスト
        rect = new Rectangle(680, 20, 0, 0);
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Brush shadow = new SolidBrush(ColorUtil.Get("hsva(0,1,0,0.3)")))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.Left, TextVPosition.Top, shadow:shadow);
        }
        rect = new Rectangle(680, 140, 0, 0);
        using(Pen pen = new Pen(Color.Red, 2.0F))
        using(Brush brush = new SolidBrush(Color.Blue))
        using(Brush shadow = new SolidBrush(ColorUtil.Get("hsva(0,1,0,0.3)")))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, pen, brush, rect, TextHPosition.Left, TextVPosition.Top, shadow:shadow, fontRatio:0.5F);
        }
        rect = new Rectangle(680, 260, 0, 0);
        using(Brush brush = new SolidBrush(Color.White))
        using(Brush shadow = new SolidBrush(ColorUtil.Get("hsv(0,0.5,1)")))
        using(Font font = new Font("メイリオ", 20.0F)) {
            rect = g.DrawText(text, font, null, brush, rect, TextHPosition.Left, TextVPosition.Top, shadow:shadow, shadowOffsetX:0F, shadowOffsetY:0F, shadowWidth:5F);
        }
        
    }

}
