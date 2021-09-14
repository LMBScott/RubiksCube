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
    public partial class ColorPicker : Form
    {
        string[] cubeFaceColors;

        public ColorPicker(string[] cFaceColors)
        {
            InitializeComponent();
            cubeFaceColors = cFaceColors;
        }

        private void ColorPicker_Load(object sender, EventArgs e)
        {
            cubeFrontPanel.BackColor = Color.FromName(cFaceColors[0]);
            cubeBackPanel.BackColor = Color.FromName(cFaceColors[1]);
            cubeLeftPanel.BackColor = Color.FromName(cFaceColors[2]);
            cubeRightPanel.BackColor = Color.FromName(cFaceColors[3]);
            cubeTopPanel.BackColor = Color.FromName(cFaceColors[4]);
            cubeBottomPanel.BackColor = Color.FromName(cFaceColors[5]);
        }

        private void Panel_Click(object sender, EventArgs e)
        {
            var panel = sender as Panel;

            string selectedColor = "Red";

            colorDialog colorPicker = new colorDialog(selectedColor);
            DialogResult result = colorPicker.ShowDialog();

            if (result == DialogResult.OK)
            {
                bool uniqueColor = true;
                foreach (string c in cubeFaceColors)
                {
                    if (colorPicker.sColor.ToString() == c)
                    {
                        uniqueColor = false;
                        MessageBox.Show("That color is already in use.", "Error");
                        break;
                    }
                }
                if (uniqueColor)
                {
                    panel.BackColor = Color.FromName(colorPicker.sColor);
                    cFaceColors[(int)panel.Tag] = colorPicker.sColor;
                }
            }
        }

        private void okayButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 12))
            {
                e.Graphics.DrawString("Okay", myFont, Brushes.White, new Point(20, 4));
            }
        }

        private void okayButton_Click(object sender, EventArgs e)
        {
            cFaceColors[0] = cubeFrontPanel.BackColor.Name;
            cFaceColors[1] = cubeBackPanel.BackColor.Name;
            cFaceColors[2] = cubeLeftPanel.BackColor.Name;
            cFaceColors[3] = cubeRightPanel.BackColor.Name;
            cFaceColors[4] = cubeTopPanel.BackColor.Name;
            cFaceColors[5] = cubeBottomPanel.BackColor.Name;

            this.DialogResult = DialogResult.OK;
        }

        public string[] cFaceColors
        {
            get { return (cubeFaceColors); }
        }

        private void okayButton_MouseDown(object sender, MouseEventArgs e)
        {
            okayButton.BackgroundImage = Properties.Resources.LongButtonBackgroundClicked;
        }

        private void okayButton_MouseUp(object sender, MouseEventArgs e)
        {
            okayButton.BackgroundImage = Properties.Resources.LongButtonBackground;
        }
    }
}
