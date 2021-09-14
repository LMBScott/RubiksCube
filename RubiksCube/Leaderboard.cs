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
    public partial class Leaderboard : Form
    {
        Label[] leaderBoardLabels;

        bool timedLeaderboard;

        string[] playerNames;
        string[] playerMoveCounts;
        string[] playerTimes;

        public Leaderboard(bool tLeaderboard, string[] pNames, string[] mCounts, string[] times)
        {
            InitializeComponent();

            timedLeaderboard = tLeaderboard;
            playerNames = pNames;
            playerMoveCounts = mCounts;
            playerTimes = times;
        }

        private void Leaderboard_Load(object sender, EventArgs e)
        {
            updateLeaderboard();
        }

        public void updateLeaderboard()
        {
            leaderBoardLabels = new Label[15] { rank1Name, rank2Name, rank3Name, rank4Name, rank5Name, rank1MoveCount, rank2MoveCount, rank3MoveCount, rank4MoveCount, rank5MoveCount,
                                  rank1Time, rank2Time, rank3Time, rank4Time, rank5Time };

            for (int i = 0; i < 15; i++)
            {
                if (i / 5 < 1)
                {
                    leaderBoardLabels[i].Text = playerNames[i];
                }
                else if (i > 4 && i < 10)
                {
                    leaderBoardLabels[i].Text = playerMoveCounts[i - 5];
                }
                else if (timedLeaderboard)
                {
                    leaderBoardLabels[i].Text = playerTimes[i - 10];
                }
                else if (!timedLeaderboard)
                {
                    leaderBoardLabels[i].Visible = false;
                }
            }

            if (!timedLeaderboard)
            {
                timeLabel.Visible = false;
            }
        }

        private void clearLeaderboardButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the leaderboard?", "Confirm Clear", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                switch (timedLeaderboard)
                {
                    case true:
                        System.IO.File.WriteAllText("Timed_Leaderboard.lbd", null);
                        System.IO.StreamWriter timedLeaderboardData = new System.IO.StreamWriter("Timed_Leaderboard.lbd", true);
                        using (timedLeaderboardData)
                        {
                            for (int i = 0; i < 15; i++)
                            {
                                timedLeaderboardData.WriteLine("None");
                            }
                        }
                        playerNames = new string[5] { "None", "None", "None", "None", "None" };
                        playerMoveCounts = new string[5] { "None", "None", "None", "None", "None" };
                        playerTimes = new string[5] { "None", "None", "None", "None", "None" };
                        updateLeaderboard();
                        break;
                    case false:
                        System.IO.File.WriteAllText("Leaderboard.lbd", null);
                        System.IO.StreamWriter leaderboardData = new System.IO.StreamWriter("Leaderboard.lbd", true);
                        using (leaderboardData)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                leaderboardData.WriteLine("None");
                            }
                        }
                        playerNames = new string[5] { "None", "None", "None", "None", "None" };
                        playerMoveCounts = new string[5] { "None", "None", "None", "None", "None" };
                        playerTimes = new string[5] { null, null, null, null, null };
                        updateLeaderboard();
                        break;
                }
            }  
        }

        private void clearLeaderboardButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("Clear", myFont, Brushes.White, new Point(21, 4));
            }
        }

        private void clearLeaderboardButton_MouseUp(object sender, MouseEventArgs e)
        {
            clearLeaderboardButton.BackgroundImage = Properties.Resources.LongButtonBackground;
        }

        private void clearLeaderboardButton_MouseDown(object sender, MouseEventArgs e)
        {
            clearLeaderboardButton.BackgroundImage = Properties.Resources.LongButtonBackgroundClicked;
        }
    }
}
