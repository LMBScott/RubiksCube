namespace RubiksCube
{
    partial class helpForm
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
            this.helpFormTextBox = new System.Windows.Forms.RichTextBox();
            this.helpImageBox = new System.Windows.Forms.PictureBox();
            this.previousPageButton = new System.Windows.Forms.PictureBox();
            this.nextPageButton = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.helpImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.previousPageButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nextPageButton)).BeginInit();
            this.SuspendLayout();
            // 
            // helpFormTextBox
            // 
            this.helpFormTextBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.helpFormTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.helpFormTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.helpFormTextBox.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpFormTextBox.ForeColor = System.Drawing.SystemColors.Window;
            this.helpFormTextBox.Location = new System.Drawing.Point(8, 12);
            this.helpFormTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.helpFormTextBox.Name = "helpFormTextBox";
            this.helpFormTextBox.ReadOnly = true;
            this.helpFormTextBox.Size = new System.Drawing.Size(554, 413);
            this.helpFormTextBox.TabIndex = 0;
            this.helpFormTextBox.Text = "";
            this.helpFormTextBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.helpFormTextBox_MouseDown);
            // 
            // helpImageBox
            // 
            this.helpImageBox.BackgroundImage = global::RubiksCube.Properties.Resources.Cubix_Control_Help;
            this.helpImageBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.helpImageBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.helpImageBox.Location = new System.Drawing.Point(568, 12);
            this.helpImageBox.Name = "helpImageBox";
            this.helpImageBox.Size = new System.Drawing.Size(434, 413);
            this.helpImageBox.TabIndex = 2;
            this.helpImageBox.TabStop = false;
            // 
            // previousPageButton
            // 
            this.previousPageButton.BackgroundImage = global::RubiksCube.Properties.Resources.LongButtonBackground;
            this.previousPageButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.previousPageButton.Location = new System.Drawing.Point(8, 432);
            this.previousPageButton.Name = "previousPageButton";
            this.previousPageButton.Size = new System.Drawing.Size(260, 63);
            this.previousPageButton.TabIndex = 6;
            this.previousPageButton.TabStop = false;
            this.previousPageButton.Click += new System.EventHandler(this.previousPageButton_Click);
            this.previousPageButton.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.previousPageButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Button_MouseDown);
            this.previousPageButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Button_MouseUp);
            // 
            // nextPageButton
            // 
            this.nextPageButton.BackgroundImage = global::RubiksCube.Properties.Resources.LongButtonBackground;
            this.nextPageButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.nextPageButton.Location = new System.Drawing.Point(302, 432);
            this.nextPageButton.Name = "nextPageButton";
            this.nextPageButton.Size = new System.Drawing.Size(260, 63);
            this.nextPageButton.TabIndex = 7;
            this.nextPageButton.TabStop = false;
            this.nextPageButton.Click += new System.EventHandler(this.nextPageButton_Click);
            this.nextPageButton.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            this.nextPageButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Button_MouseDown);
            this.nextPageButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Button_MouseUp);
            // 
            // helpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1005, 502);
            this.Controls.Add(this.nextPageButton);
            this.Controls.Add(this.previousPageButton);
            this.Controls.Add(this.helpImageBox);
            this.Controls.Add(this.helpFormTextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "helpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cubix Help";
            this.Load += new System.EventHandler(this.helpForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.helpImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.previousPageButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nextPageButton)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox helpFormTextBox;
        private System.Windows.Forms.PictureBox helpImageBox;
        private System.Windows.Forms.PictureBox previousPageButton;
        private System.Windows.Forms.PictureBox nextPageButton;
    }
}