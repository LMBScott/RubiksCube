namespace RubiksCube
{
    partial class leaderboardEntry
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
            this.congratulationsLabel = new System.Windows.Forms.Label();
            this.nameEntryLabel = new System.Windows.Forms.Label();
            this.nameEntryTextBox = new System.Windows.Forms.TextBox();
            this.moveCountLabel = new System.Windows.Forms.Label();
            this.timeLabel = new System.Windows.Forms.Label();
            this.enterButton = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.enterButton)).BeginInit();
            this.SuspendLayout();
            // 
            // congratulationsLabel
            // 
            this.congratulationsLabel.AutoSize = true;
            this.congratulationsLabel.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.congratulationsLabel.ForeColor = System.Drawing.Color.White;
            this.congratulationsLabel.Location = new System.Drawing.Point(12, 9);
            this.congratulationsLabel.Name = "congratulationsLabel";
            this.congratulationsLabel.Size = new System.Drawing.Size(247, 36);
            this.congratulationsLabel.TabIndex = 0;
            this.congratulationsLabel.Text = "Congratulations!";
            // 
            // nameEntryLabel
            // 
            this.nameEntryLabel.AutoSize = true;
            this.nameEntryLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameEntryLabel.ForeColor = System.Drawing.Color.White;
            this.nameEntryLabel.Location = new System.Drawing.Point(13, 67);
            this.nameEntryLabel.Name = "nameEntryLabel";
            this.nameEntryLabel.Size = new System.Drawing.Size(144, 27);
            this.nameEntryLabel.TabIndex = 1;
            this.nameEntryLabel.Text = "Enter name:";
            // 
            // nameEntryTextBox
            // 
            this.nameEntryTextBox.Location = new System.Drawing.Point(163, 69);
            this.nameEntryTextBox.MaxLength = 12;
            this.nameEntryTextBox.Name = "nameEntryTextBox";
            this.nameEntryTextBox.Size = new System.Drawing.Size(173, 26);
            this.nameEntryTextBox.TabIndex = 2;
            // 
            // moveCountLabel
            // 
            this.moveCountLabel.AutoSize = true;
            this.moveCountLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.moveCountLabel.ForeColor = System.Drawing.Color.White;
            this.moveCountLabel.Location = new System.Drawing.Point(13, 122);
            this.moveCountLabel.Name = "moveCountLabel";
            this.moveCountLabel.Size = new System.Drawing.Size(217, 27);
            this.moveCountLabel.TabIndex = 3;
            this.moveCountLabel.Text = "You made n moves";
            // 
            // timeLabel
            // 
            this.timeLabel.AutoSize = true;
            this.timeLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.timeLabel.ForeColor = System.Drawing.Color.White;
            this.timeLabel.Location = new System.Drawing.Point(13, 176);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(119, 27);
            this.timeLabel.TabIndex = 4;
            this.timeLabel.Text = "and took t";
            // 
            // enterButton
            // 
            this.enterButton.BackgroundImage = global::RubiksCube.Properties.Resources.LongButtonBackground;
            this.enterButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.enterButton.Location = new System.Drawing.Point(199, 210);
            this.enterButton.Name = "enterButton";
            this.enterButton.Size = new System.Drawing.Size(124, 40);
            this.enterButton.TabIndex = 5;
            this.enterButton.TabStop = false;
            this.enterButton.Click += new System.EventHandler(this.enterButton_Click);
            this.enterButton.Paint += new System.Windows.Forms.PaintEventHandler(this.enterButton_Paint);
            this.enterButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.enterButton_MouseDown);
            this.enterButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.enterButton_MouseUp);
            // 
            // leaderboardEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(522, 262);
            this.Controls.Add(this.enterButton);
            this.Controls.Add(this.timeLabel);
            this.Controls.Add(this.moveCountLabel);
            this.Controls.Add(this.nameEntryTextBox);
            this.Controls.Add(this.nameEntryLabel);
            this.Controls.Add(this.congratulationsLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "leaderboardEntry";
            this.Text = "Leaderboard Entry";
            this.Load += new System.EventHandler(this.leaderboardEntry_Load);
            ((System.ComponentModel.ISupportInitialize)(this.enterButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label congratulationsLabel;
        private System.Windows.Forms.Label nameEntryLabel;
        private System.Windows.Forms.TextBox nameEntryTextBox;
        private System.Windows.Forms.Label moveCountLabel;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.PictureBox enterButton;
    }
}