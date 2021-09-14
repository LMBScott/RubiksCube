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
    public partial class colorDialog : Form
    {
        string selectedColor;

        public colorDialog(string sColor)
        {
            InitializeComponent();
            selectedColor = sColor;
        }

        private void ColorSwatch_Click(object sender, EventArgs e)
        {
            var swatch = sender as Panel;
            selectedColor = swatch.BackColor.Name;
            this.DialogResult = DialogResult.OK;
        }

        public string sColor
        {
            get { return (selectedColor); }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void cancelButton_MouseDown(object sender, MouseEventArgs e)
        {
            cancelButton.BackgroundImage = Properties.Resources.LongButtonBackgroundClicked;
        }

        private void cancelButton_MouseUp(object sender, MouseEventArgs e)
        {
            cancelButton.BackgroundImage = Properties.Resources.LongButtonBackground;
        }

        private void cancelButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 12))
            {
                e.Graphics.DrawString("Cancel", myFont, Brushes.White, new Point(12, 4));
            }
        }
    }
}
