/// GraphicsExtensionsTestForm: GraphicsExtensionsのテスト用フォーム.
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

public partial class GraphicsExtensionsTestForm : Form {

    public GraphicsExtensionsTestForm() {
        InitializeComponent();
    }

    private void GraphicsExtensionsTestForm_Paint(object sender, PaintEventArgs e) {
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

        // 角丸四角形描画
        for(int i = 0; i <= 200; i += 20) {
            using(Pen pen = new Pen(Color.Green, 1.0F+(float)i/20.0F)) {
                g.DrawRoundRectangle(pen, i, i, 200-i*2, 200-i*2, (float)(200-i)/10F);
            }
        }
        
        // 内接四角形描画
        for(int i = 0; i <= 200; i += 20) {
            using(Pen pen = new Pen(Color.Red, 1.0F+(float)i/20.0F)) {
                g.DrawInnerRectangle(pen, 220+i, i, 200-i*2, 200-i*2);
            }
        }
        
        // 内接角丸四角形描画
        for(int i = 0; i <= 200; i += 20) {
            using(Pen pen = new Pen(Color.Blue, 1.0F+(float)i/20.0F)) {
                g.DrawInnerRoundRectangle(pen, 440+i, i, 200-i*2, 200-i*2, (float)(200-i)/10F);
            }
        }

        // 外接四角形描画
        for(int i = 0; i <= 200; i += 20) {
            using(Pen pen = new Pen(Color.Red, 1.0F+(float)i/20.0F)) {
                g.DrawOuterRectangle(pen, 660+i, i, 200-i*2, 200-i*2);
            }
        }
        
        // DrawRoundRectangle
        using(Pen pen = new Pen(Color.Green, 5.0F)) {
            g.DrawRoundRectangle(pen, 20, 240, 40, 40, 10F);
        }
        // FillRoundRectangle
        using(Brush brush = new SolidBrush(Color.Blue)) {
            g.FillRoundRectangle(brush, 80, 240, 40, 40, 10F);
        }
        // DrawAndFillRoundRectangle
        using(Pen pen = new Pen(Color.Green, 5.0F))
        using(Brush brush = new SolidBrush(Color.Blue)) {
            g.DrawAndFillRoundRectangle(pen, brush, 140, 240, 40, 40, 10F);
        }

        // 4辺独立ペン長方形描画
        using(Pen top = new Pen(Color.Red, 5F))
        using(Pen right = new Pen(Color.Green, 10F))
        using(Pen bottom = new Pen(Color.Blue, 15F))
        using(Pen left = new Pen(Color.Black, 20F)) {
            g.DrawRectangle(top, right, bottom, left, 240, 240, 40, 40);
            g.DrawInnerRectangle(top, right, bottom, left, 320, 240, 40, 40);
            g.DrawOuterRectangle(top, right, bottom, left, 400, 240, 40, 40);
        }
        
        // ボタン描画
        using(Font font = new Font("メイリオ", 18F))
        using(ButtonFace buttonFace = new ButtonFace(font, ColorUtil.Get("hsv(30,1,1)")))
        using(ButtonFace pushFace = buttonFace.MakePushedFace()) {
            string[] buttonText = new string[]{"ボタン","サンプル"};
            g.DrawButton(new Rectangle(20, 380, 200, 80), 20F, buttonText, buttonFace);
            g.DrawButton(new Rectangle(240, 380, 200, 80), 20F, buttonText, pushFace);
        }
        using(Font font = new Font("メイリオ", 24F))
        using(ButtonFace buttonFace = new ButtonFace(font, ColorUtil.Get("hsv(130,0.8,0.6)"), 8F))
        using(ButtonFace pushFace = buttonFace.MakePushedFace())
        using(ButtonFace disabledFace = buttonFace.MakeDisabledFace()) {
            string[] buttonText = new string[]{"長いボタン名のサンプル"};
            g.DrawButton(new Rectangle(460, 380, 160, 80), 1F, buttonText, buttonFace);
            g.DrawButton(new Rectangle(640, 380, 160, 80), 10F, buttonText, pushFace);
            g.DrawButton(new Rectangle(820, 380, 160, 80), 100F, buttonText, disabledFace);
        }
        
        // 属性指定文字ボタン
        XmlFile xml = new XmlFile("DrawTextTest.xml", "test");
        MPAttribute style = new MPAttribute(xml.Root);
        
        using(ButtonFace buttonFace = new ButtonFace(ColorUtil.Get("hsv(130,1,1)"))) {
            XmlElement elem = xml.GetSubRoot("text1");
            MPAttribute attr = new MPAttribute(elem, style);
            string buttonText = elem.InnerXml;
            g.DrawButton(new Rectangle(20, 480, 200, 150), 40F, buttonText, attr, buttonFace);
            elem = xml.GetSubRoot("text2");
            attr = new MPAttribute(elem, style);
            buttonText = elem.InnerXml;
            g.DrawButton(new Rectangle(240, 480, 200, 150), 40F, buttonText, attr, buttonFace);
            elem = xml.GetSubRoot("text3");
            attr = new MPAttribute(elem, style);
            buttonText = elem.InnerXml;
            g.DrawButton(new Rectangle(460, 480, 200, 150), 40F, buttonText, attr, buttonFace);
        }
    }

}
