namespace RubiksCube
{
    partial class ColorPicker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cubeTopPanel = new System.Windows.Forms.Panel();
            this.cubeLeftPanel = new System.Windows.Forms.Panel();
            this.cubeFrontPanel = new System.Windows.Forms.Panel();
            this.cubeRightPanel = new System.Windows.Forms.Panel();
            this.cubeBottomPanel = new System.Windows.Forms.Panel();
            this.cubeBackPanel = new System.Windows.Forms.Panel();
            this.okayButton = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.okayButton)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cubeTopPanel
            // 
            this.cubeTopPanel.BackColor = System.Drawing.Color.Blue;
            this.cubeTopPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cubeTopPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cubeTopPanel.Location = new System.Drawing.Point(132, 18);
            this.cubeTopPanel.Name = "cubeTopPanel";
            this.cubeTopPanel.Size = new System.Drawing.Size(100, 99);
            this.cubeTopPanel.TabIndex = 0;
            this.cubeTopPanel.Tag = "4";
            this.cubeTopPanel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // cubeLeftPanel
            // 
            this.cubeLeftPanel.BackColor = System.Drawing.Color.Orange;
            this.cubeLeftPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cubeLeftPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cubeLeftPanel.Location = new System.Drawing.Point(22, 128);
            this.cubeLeftPanel.Name = "cubeLeftPanel";
            this.cubeLeftPanel.Size = new System.Drawing.Size(100, 99);
            this.cubeLeftPanel.TabIndex = 1;
            this.cubeLeftPanel.Tag = "2";
            this.cubeLeftPanel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // cubeFrontPanel
            // 
            this.cubeFrontPanel.BackColor = System.Drawing.Color.White;
            this.cubeFrontPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cubeFrontPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cubeFrontPanel.Location = new System.Drawing.Point(132, 128);
            this.cubeFrontPanel.Name = "cubeFrontPanel";
            this.cubeFrontPanel.Size = new System.Drawing.Size(100, 99);
            this.cubeFrontPanel.TabIndex = 2;
            this.cubeFrontPanel.Tag = "0";
            this.cubeFrontPanel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // cubeRightPanel
            // 
            this.cubeRightPanel.BackColor = System.Drawing.Color.Red;
            this.cubeRightPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cubeRightPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cubeRightPanel.Location = new System.Drawing.Point(242, 128);
            this.cubeRightPanel.Name = "cubeRightPanel";
            this.cubeRightPanel.Size = new System.Drawing.Size(100, 99);
            this.cubeRightPanel.TabIndex = 3;
            this.cubeRightPanel.Tag = "3";
            this.cubeRightPanel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // cubeBottomPanel
            // 
            this.cubeBottomPanel.BackColor = System.Drawing.Color.Green;
            this.cubeBottomPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cubeBottomPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cubeBottomPanel.Location = new System.Drawing.Point(132, 235);
            this.cubeBottomPanel.Name = "cubeBottomPanel";
            this.cubeBottomPanel.Size = new System.Drawing.Size(100, 99);
            this.cubeBottomPanel.TabIndex = 4;
            this.cubeBottomPanel.Tag = "5";
            this.cubeBottomPanel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // cubeBackPanel
            // 
            this.cubeBackPanel.BackColor = System.Drawing.Color.Yellow;
            this.cubeBackPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cubeBackPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cubeBackPanel.Location = new System.Drawing.Point(132, 345);
            this.cubeBackPanel.Name = "cubeBackPanel";
            this.cubeBackPanel.Size = new System.Drawing.Size(100, 99);
            this.cubeBackPanel.TabIndex = 5;
            this.cubeBackPanel.Tag = "1";
            this.cubeBackPanel.Click += new System.EventHandler(this.Panel_Click);
            // 
            // okayButton
            // 
            this.okayButton.BackgroundImage = global::RubiksCube.Properties.Resources.LongButtonBackground;
            this.okayButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.okayButton.Location = new System.Drawing.Point(120, 488);
            this.okayButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.okayButton.Name = "okayButton";
            this.okayButton.Size = new System.Drawing.Size(124, 40);
            this.okayButton.TabIndex = 6;
            this.okayButton.TabStop = false;
            this.okayButton.Click += new System.EventHandler(this.okayButton_Click);
            this.okayButton.Paint += new System.Windows.Forms.PaintEventHandler(this.okayButton_Paint);
            this.okayButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.okayButton_MouseDown);
            this.okayButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.okayButton_MouseUp);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panel1.Controls.Add(this.cubeRightPanel);
            this.panel1.Controls.Add(this.okayButton);
            this.panel1.Controls.Add(this.cubeTopPanel);
            this.panel1.Controls.Add(this.cubeBackPanel);
            this.panel1.Controls.Add(this.cubeLeftPanel);
            this.panel1.Controls.Add(this.cubeBottomPanel);
            this.panel1.Controls.Add(this.cubeFrontPanel);
            this.panel1.Location = new System.Drawing.Point(18, 18);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(364, 566);
            this.panel1.TabIndex = 7;
            // 
            // ColorPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 603);
            this.Controls.Add(this.panel1);
            this.Name = "ColorPicker";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Customise Cube Colors";
            this.Load += new System.EventHandler(this.ColorPicker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.okayButton)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel cubeTopPanel;
        private System.Windows.Forms.Panel cubeLeftPanel;
        private System.Windows.Forms.Panel cubeFrontPanel;
        private System.Windows.Forms.Panel cubeRightPanel;
        private System.Windows.Forms.Panel cubeBottomPanel;
        private System.Windows.Forms.Panel cubeBackPanel;
        private System.Windows.Forms.PictureBox okayButton;
        private System.Windows.Forms.Panel panel1;
    }
}