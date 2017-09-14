    partial class MPWidgetTestForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtColor = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOutlineColor = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOutlineRatio = new System.Windows.Forms.TextBox();
            this.chkTextAttribute = new System.Windows.Forms.CheckBox();
            this.radHposLeft = new System.Windows.Forms.RadioButton();
            this.radHposCenter = new System.Windows.Forms.RadioButton();
            this.radHposRight = new System.Windows.Forms.RadioButton();
            this.radVposTop = new System.Windows.Forms.RadioButton();
            this.radVposMiddle = new System.Windows.Forms.RadioButton();
            this.radVposBottom = new System.Windows.Forms.RadioButton();
            this.radVposFit = new System.Windows.Forms.RadioButton();
            this.radVposProportional = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.mpButton1 = new MACS.Draw.MPButton();
            this.mpText1 = new MACS.Draw.MPText();
            this.label5 = new System.Windows.Forms.Label();
            this.txtBackColor = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtText
            // 
            this.txtText.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtText.Location = new System.Drawing.Point(272, 9);
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.Size = new System.Drawing.Size(297, 101);
            this.txtText.TabIndex = 1;
            this.txtText.TextChanged += new System.EventHandler(this.txtText_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(228, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Text";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(222, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Color";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtColor
            // 
            this.txtColor.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtColor.Location = new System.Drawing.Point(272, 238);
            this.txtColor.Name = "txtColor";
            this.txtColor.Size = new System.Drawing.Size(133, 23);
            this.txtColor.TabIndex = 3;
            this.txtColor.TextChanged += new System.EventHandler(this.txtColor_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label3.Location = new System.Drawing.Point(175, 329);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "OutlineColor";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOutlineColor
            // 
            this.txtOutlineColor.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtOutlineColor.Location = new System.Drawing.Point(272, 326);
            this.txtOutlineColor.Name = "txtOutlineColor";
            this.txtOutlineColor.Size = new System.Drawing.Size(133, 23);
            this.txtOutlineColor.TabIndex = 5;
            this.txtOutlineColor.TextChanged += new System.EventHandler(this.txtOutlineColor_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label4.Location = new System.Drawing.Point(175, 358);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 16);
            this.label4.TabIndex = 8;
            this.label4.Text = "OutlineRatio";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOutlineRatio
            // 
            this.txtOutlineRatio.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtOutlineRatio.Location = new System.Drawing.Point(272, 355);
            this.txtOutlineRatio.Name = "txtOutlineRatio";
            this.txtOutlineRatio.Size = new System.Drawing.Size(133, 23);
            this.txtOutlineRatio.TabIndex = 7;
            this.txtOutlineRatio.TextChanged += new System.EventHandler(this.txtOutlineRatio_TextChanged);
            // 
            // chkTextAttribute
            // 
            this.chkTextAttribute.AutoSize = true;
            this.chkTextAttribute.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.chkTextAttribute.Location = new System.Drawing.Point(257, 384);
            this.chkTextAttribute.Name = "chkTextAttribute";
            this.chkTextAttribute.Size = new System.Drawing.Size(118, 20);
            this.chkTextAttribute.TabIndex = 9;
            this.chkTextAttribute.Text = "TextAttribute";
            this.chkTextAttribute.UseVisualStyleBackColor = true;
            this.chkTextAttribute.CheckedChanged += new System.EventHandler(this.chkTextAttribute_CheckedChanged);
            // 
            // radHposLeft
            // 
            this.radHposLeft.AutoSize = true;
            this.radHposLeft.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radHposLeft.Location = new System.Drawing.Point(84, 14);
            this.radHposLeft.Name = "radHposLeft";
            this.radHposLeft.Size = new System.Drawing.Size(54, 20);
            this.radHposLeft.TabIndex = 11;
            this.radHposLeft.TabStop = true;
            this.radHposLeft.Text = "Left";
            this.radHposLeft.UseVisualStyleBackColor = true;
            this.radHposLeft.CheckedChanged += new System.EventHandler(this.radHpos_CheckedChanged);
            // 
            // radHposCenter
            // 
            this.radHposCenter.AutoSize = true;
            this.radHposCenter.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radHposCenter.Location = new System.Drawing.Point(144, 13);
            this.radHposCenter.Name = "radHposCenter";
            this.radHposCenter.Size = new System.Drawing.Size(73, 20);
            this.radHposCenter.TabIndex = 12;
            this.radHposCenter.TabStop = true;
            this.radHposCenter.Text = "Center";
            this.radHposCenter.UseVisualStyleBackColor = true;
            this.radHposCenter.CheckedChanged += new System.EventHandler(this.radHpos_CheckedChanged);
            // 
            // radHposRight
            // 
            this.radHposRight.AutoSize = true;
            this.radHposRight.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radHposRight.Location = new System.Drawing.Point(225, 13);
            this.radHposRight.Name = "radHposRight";
            this.radHposRight.Size = new System.Drawing.Size(60, 20);
            this.radHposRight.TabIndex = 13;
            this.radHposRight.TabStop = true;
            this.radHposRight.Text = "Right";
            this.radHposRight.UseVisualStyleBackColor = true;
            this.radHposRight.CheckedChanged += new System.EventHandler(this.radHpos_CheckedChanged);
            // 
            // radVposTop
            // 
            this.radVposTop.AutoSize = true;
            this.radVposTop.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radVposTop.Location = new System.Drawing.Point(69, 13);
            this.radVposTop.Name = "radVposTop";
            this.radVposTop.Size = new System.Drawing.Size(51, 20);
            this.radVposTop.TabIndex = 14;
            this.radVposTop.TabStop = true;
            this.radVposTop.Text = "Top";
            this.radVposTop.UseVisualStyleBackColor = true;
            this.radVposTop.CheckedChanged += new System.EventHandler(this.radVpos_CheckedChanged);
            // 
            // radVposMiddle
            // 
            this.radVposMiddle.AutoSize = true;
            this.radVposMiddle.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radVposMiddle.Location = new System.Drawing.Point(126, 13);
            this.radVposMiddle.Name = "radVposMiddle";
            this.radVposMiddle.Size = new System.Drawing.Size(68, 20);
            this.radVposMiddle.TabIndex = 15;
            this.radVposMiddle.TabStop = true;
            this.radVposMiddle.Text = "Middle";
            this.radVposMiddle.UseVisualStyleBackColor = true;
            this.radVposMiddle.CheckedChanged += new System.EventHandler(this.radVpos_CheckedChanged);
            // 
            // radVposBottom
            // 
            this.radVposBottom.AutoSize = true;
            this.radVposBottom.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radVposBottom.Location = new System.Drawing.Point(200, 13);
            this.radVposBottom.Name = "radVposBottom";
            this.radVposBottom.Size = new System.Drawing.Size(76, 20);
            this.radVposBottom.TabIndex = 16;
            this.radVposBottom.TabStop = true;
            this.radVposBottom.Text = "Bottom";
            this.radVposBottom.UseVisualStyleBackColor = true;
            this.radVposBottom.CheckedChanged += new System.EventHandler(this.radVpos_CheckedChanged);
            // 
            // radVposFit
            // 
            this.radVposFit.AutoSize = true;
            this.radVposFit.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radVposFit.Location = new System.Drawing.Point(282, 13);
            this.radVposFit.Name = "radVposFit";
            this.radVposFit.Size = new System.Drawing.Size(44, 20);
            this.radVposFit.TabIndex = 17;
            this.radVposFit.TabStop = true;
            this.radVposFit.Text = "Fit";
            this.radVposFit.UseVisualStyleBackColor = true;
            this.radVposFit.CheckedChanged += new System.EventHandler(this.radVpos_CheckedChanged);
            // 
            // radVposProportional
            // 
            this.radVposProportional.AutoSize = true;
            this.radVposProportional.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.radVposProportional.Location = new System.Drawing.Point(69, 39);
            this.radVposProportional.Name = "radVposProportional";
            this.radVposProportional.Size = new System.Drawing.Size(108, 20);
            this.radVposProportional.TabIndex = 18;
            this.radVposProportional.TabStop = true;
            this.radVposProportional.Text = "Proportional";
            this.radVposProportional.UseVisualStyleBackColor = true;
            this.radVposProportional.CheckedChanged += new System.EventHandler(this.radVpos_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radHposCenter);
            this.groupBox1.Controls.Add(this.radHposLeft);
            this.groupBox1.Controls.Add(this.radHposRight);
            this.groupBox1.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBox1.Location = new System.Drawing.Point(212, 116);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(357, 39);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "HPosition";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radVposTop);
            this.groupBox2.Controls.Add(this.radVposMiddle);
            this.groupBox2.Controls.Add(this.radVposProportional);
            this.groupBox2.Controls.Add(this.radVposBottom);
            this.groupBox2.Controls.Add(this.radVposFit);
            this.groupBox2.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.groupBox2.Location = new System.Drawing.Point(212, 161);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(357, 61);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "VPosition";
            // 
            // mpButton1
            // 
            this.mpButton1.Font = new System.Drawing.Font("MS UI Gothic", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.mpButton1.Location = new System.Drawing.Point(12, 263);
            this.mpButton1.MultiState = false;
            this.mpButton1.Name = "mpButton1";
            this.mpButton1.NumberOfState = 0;
            this.mpButton1.Size = new System.Drawing.Size(129, 86);
            this.mpButton1.State = 0;
            this.mpButton1.TabIndex = 10;
            this.mpButton1.Text = "テスト";
            // 
            // mpText1
            // 
            this.mpText1.Font = new System.Drawing.Font("MS UI Gothic", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.mpText1.ForeColor = System.Drawing.Color.Transparent;
            this.mpText1.Location = new System.Drawing.Point(12, 12);
            this.mpText1.Name = "mpText1";
            this.mpText1.Size = new System.Drawing.Size(129, 210);
            this.mpText1.TabIndex = 0;
            this.mpText1.TabStop = false;
            this.mpText1.Text = "テスト";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label5.Location = new System.Drawing.Point(189, 270);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 16);
            this.label5.TabIndex = 22;
            this.label5.Text = "BackColor";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBackColor
            // 
            this.txtBackColor.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtBackColor.Location = new System.Drawing.Point(272, 267);
            this.txtBackColor.Name = "txtBackColor";
            this.txtBackColor.Size = new System.Drawing.Size(133, 23);
            this.txtBackColor.TabIndex = 21;
            this.txtBackColor.TextChanged += new System.EventHandler(this.txtBackColor_TextChanged);
            // 
            // MPWidgetTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.ClientSize = new System.Drawing.Size(579, 472);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtBackColor);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.mpButton1);
            this.Controls.Add(this.chkTextAttribute);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtOutlineRatio);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtOutlineColor);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtColor);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtText);
            this.Controls.Add(this.mpText1);
            this.Name = "MPWidgetTestForm";
            this.Text = "MPWidgetTest";
            this.Load += new System.EventHandler(this.MPWidgetTestForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MACS.Draw.MPText mpText1;
        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtColor;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOutlineColor;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtOutlineRatio;
        private System.Windows.Forms.CheckBox chkTextAttribute;
        private MACS.Draw.MPButton mpButton1;
        private System.Windows.Forms.RadioButton radHposLeft;
        private System.Windows.Forms.RadioButton radHposCenter;
        private System.Windows.Forms.RadioButton radHposRight;
        private System.Windows.Forms.RadioButton radVposTop;
        private System.Windows.Forms.RadioButton radVposMiddle;
        private System.Windows.Forms.RadioButton radVposBottom;
        private System.Windows.Forms.RadioButton radVposFit;
        private System.Windows.Forms.RadioButton radVposProportional;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtBackColor;

    }
