    partial class ColorUtilTestForm
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
            this.picColor = new System.Windows.Forms.PictureBox();
            this.txtColorName = new System.Windows.Forms.TextBox();
            this.labColorName = new System.Windows.Forms.Label();
            this.lblValue = new System.Windows.Forms.Label();
            this.picSV = new System.Windows.Forms.PictureBox();
            this.picHue = new System.Windows.Forms.PictureBox();
            this.picBD = new System.Windows.Forms.PictureBox();
            this.lblOnMouse = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBD)).BeginInit();
            this.SuspendLayout();
            //
            // picColor
            //
            this.picColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picColor.Location = new System.Drawing.Point(355, 12);
            this.picColor.Name = "picColor";
            this.picColor.Size = new System.Drawing.Size(277, 23);
            this.picColor.TabIndex = 0;
            this.picColor.TabStop = false;
            this.picColor.Paint += new System.Windows.Forms.PaintEventHandler(this.picColor_Paint);
            //
            // txtColorName
            //
            this.txtColorName.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.txtColorName.Location = new System.Drawing.Point(105, 12);
            this.txtColorName.Name = "txtColorName";
            this.txtColorName.Size = new System.Drawing.Size(227, 23);
            this.txtColorName.TabIndex = 1;
            this.txtColorName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtColorName_KeyDown);
            //
            // labColorName
            //
            this.labColorName.AutoSize = true;
            this.labColorName.Font = new System.Drawing.Font("MS UI Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.labColorName.Location = new System.Drawing.Point(12, 15);
            this.labColorName.Name = "labColorName";
            this.labColorName.Size = new System.Drawing.Size(87, 16);
            this.labColorName.TabIndex = 2;
            this.labColorName.Text = "Color Name";
            //
            // lblValue
            //
            this.lblValue.AutoSize = true;
            this.lblValue.Font = new System.Drawing.Font("ＭＳ ゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblValue.Location = new System.Drawing.Point(102, 46);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(64, 16);
            this.lblValue.TabIndex = 4;
            this.lblValue.Text = "RGB,HSV";
            //
            // picSV
            //
            this.picSV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picSV.Location = new System.Drawing.Point(15, 91);
            this.picSV.Name = "picSV";
            this.picSV.Size = new System.Drawing.Size(617, 282);
            this.picSV.TabIndex = 5;
            this.picSV.TabStop = false;
            this.picSV.Paint += new System.Windows.Forms.PaintEventHandler(this.picSV_Paint);
            //
            // picHue
            //
            this.picHue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picHue.Location = new System.Drawing.Point(15, 393);
            this.picHue.Name = "picHue";
            this.picHue.Size = new System.Drawing.Size(617, 33);
            this.picHue.TabIndex = 6;
            this.picHue.TabStop = false;
            this.picHue.Paint += new System.Windows.Forms.PaintEventHandler(this.picHue_Paint);
            //
            // picBD
            //
            this.picBD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picBD.Location = new System.Drawing.Point(15, 443);
            this.picBD.Name = "picBD";
            this.picBD.Size = new System.Drawing.Size(617, 33);
            this.picBD.TabIndex = 7;
            this.picBD.TabStop = false;
            this.picBD.Paint += new System.Windows.Forms.PaintEventHandler(this.picBD_Paint);
            //
            // lblOnMouse
            //
            this.lblOnMouse.AutoSize = true;
            this.lblOnMouse.Font = new System.Drawing.Font("ＭＳ ゴシック", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblOnMouse.Location = new System.Drawing.Point(102, 72);
            this.lblOnMouse.Name = "lblOnMouse";
            this.lblOnMouse.Size = new System.Drawing.Size(120, 16);
            this.lblOnMouse.TabIndex = 8;
            this.lblOnMouse.Text = "Color on Mouse";
            //
            // ColorUtilTestForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(644, 488);
            this.Controls.Add(this.lblOnMouse);
            this.Controls.Add(this.picBD);
            this.Controls.Add(this.picHue);
            this.Controls.Add(this.picSV);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.labColorName);
            this.Controls.Add(this.txtColorName);
            this.Controls.Add(this.picColor);
            this.Name = "ColorUtilTestForm";
            this.Text = "ColorUtilTest";
            this.VisibleChanged += new System.EventHandler(this.ColorUtilTestForm_VisibleChanged);
            this.Resize += new System.EventHandler(this.ColorUtilTestForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.picColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picColor;
        private System.Windows.Forms.TextBox txtColorName;
        private System.Windows.Forms.Label labColorName;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.PictureBox picSV;
        private System.Windows.Forms.PictureBox picHue;
        private System.Windows.Forms.PictureBox picBD;
        private System.Windows.Forms.Label lblOnMouse;
    }
