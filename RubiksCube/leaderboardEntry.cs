using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RubiksCube
{
    public partial class leaderboardEntry : Form
    {
        string playerMoveCount;
        string playerTime;
        string playerName;

        bool timedMode;

        Player player = new Player();

        public class Player
        {
            public string playerName;
            public string playerMoveCount;
            public string playerTime;
            bool playerTimedMode;

            public Player()
            {
                resetPlayer();
            }

            public string Name
            {
                get { return playerName; }
                set { playerName = value; }
            }

            public string MoveCount
            {
                get { return playerMoveCount; }
                set { playerMoveCount = value; }
            }

            public string Time
            {
                get { return playerTime; }
                set { playerTime= value; }
            }

            public bool TimedModeEnabled
            {
                get { return playerTimedMode; }
                set { playerTimedMode = value; }
            }
            public void resetPlayer()
            {
                playerName = "None";
                playerMoveCount = "None";
                playerTime = "None";
                playerTimedMode = false;
            }
        }

        public leaderboardEntry(string pMoveCount, string pTime, bool tMode)
        {
            player.MoveCount = pMoveCount;
            player.Time = pTime;
            player.TimedModeEnabled = tMode;

            InitializeComponent();
        }

        private void enterButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("Enter", myFont, Brushes.White, new Point(21, 4));
            }
        }

        private void leaderboardEntry_Load(object sender, EventArgs e)
        {
            if (!timedMode)
            {
                timeLabel.Visible = false;
            }

            moveCountLabel.Text = "You made " + playerMoveCount + " moves";
            timeLabel.Text = "and took " + playerTime;
        }

        private void enterButton_MouseUp(object sender, MouseEventArgs e)
        {
            enterButton.BackgroundImage = Properties.Resources.LongButtonBackground;
        }

        private void enterButton_MouseDown(object sender, MouseEventArgs e)
        {
            enterButton.BackgroundImage = Properties.Resources.LongButtonBackgroundClicked;
        }

        public string pName
        {
            get { return (player.Name); }
        }

        private void enterButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(nameEntryTextBox.Text))
            {
                MessageBox.Show("Please enter your name.", "Error");
            }
            else
            {
                player.Name = nameEntryTextBox.Text;
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
