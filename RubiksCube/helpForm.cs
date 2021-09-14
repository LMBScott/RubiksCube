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
    public partial class helpForm : Form
    {
        int helpIndex;
        int pageIndex = 0;

        static List<Image> gettingStartedImages = new List<Image> { Properties.Resources.Cubix_Splashscreen, Properties.Resources.Cubix_Rotation_Help, Properties.Resources.Cubix_Control_Help,
            Properties.Resources.Cubix_Sequence_Help, Properties.Resources.Cubix_Cube_Map, Properties.Resources.Cubix_Cube_Menu};

        static List<Image> keyCommandsImages = new List<Image> { Properties.Resources.Cubix_Control_Help };

        static List<Image> customisationImages = new List<Image> { Properties.Resources.Cubix_Customisation, Properties.Resources.Cubix_Color_Customisation, Properties.Resources.Cubix_Background_Customisation };

        List<List<Image>> helpGuideImages = new List<List<Image>> { gettingStartedImages, keyCommandsImages, customisationImages };

        static List<string> gettingStartedPage1 = new List<string> { "Welcome to Cubix!", "This is a program designed to simulate a Rubik's Cube in full 3D." };
        static List<string> gettingStartedPage2 = new List<string> { "To rotate the cube, simply hold Left-Click and drag in the desired direction." };
        static List<string> gettingStartedPage3 = new List<string> { "To rotate the layers of the cube, simply Left-Click the button corresponding to the desired move.", "",
            "The move abbreviations (shown on the buttons) are as follows:", "   - F: Rotate the front face clockwise", "   - F': Rotate the front face anti-clockwise",
            "   - ZM: Rotate the middle along the Z Axis clockwise", "   - ZM': Rotate the middle along the Z Axis anti-clockwise", "   - B: Rotate the back face clockwise",
            "   - B': Rotate the back face anti-clockwise", "", "   - L: Rotate the left face clockwise", "   - L': Rotate the left face anti-clockwise", 
            "   - XM: Rotate the middle along the X Axis clockwise", "   - XM': Rotate the middle along the X Axis anti-clockwise", "   - R: Rotate the right face clockwise",
            "   - R': Rotate the right face anti-clockwise", "", "   - U: Rotate the top face clockwise", "   - U': Rotate the top face anti-clockwise",
            "   - YM: Rotate the middle along the Y Axis clockwise", "   - YM': Rotate the middle along the Y Axis anti-clockwise", "   - D: Rotate the bottom face clockwise",
            "   - D': Rotate the bottom face anti-clockwise"};
        static List<string> gettingStartedPage4 = new List<string> { "Move Sequences:", "  It is possible to create sequences of moves that can be repeatedly executed in their exact order.",
            "This may be useful when it is neccessary to repeat long move sequences when solving the cube.", "", "  To do this:", 
            "   - Check the 'Sequence' box at the bottom of the control panel, below the control buttons", "   - Click the buttons corresponding to the moves in the sequence in the desired order",
            "   - The sequence can now be executed repeatedly by Left-Clicking the 'Execute' button", "   - Clear the current sequence by Left-Clicking the 'Clear' button that appears below the control buttons"};
        static List<string> gettingStartedPage5 = new List<string> { "The Cube Map:", "The Cube Map displays the state of each face of the cube in real time.", 
            "Additionally, the map can be used to automatically rotate the cube to show a desired face.", "Simply click on a face on the cube map and the cube model will be rotated to show that face." };
        static List<string> gettingStartedPage6 = new List<string> { "The Cube Menu:", "The Cube menu is accessed from the menu at the top of the cubix screen.", "", "Menu Options:", "- Scramble: Scramble the cube in a completely random series of moves.", 
            "- Solve: Solve the cube automatically. Cancels any attempts to solve the cube manually (no cheating!).", "- Timed Mode: Enables a timed solve mode that begins when the cube is next scrambled.",
            "- Reset Faces: Resets the faces of the cube to their default position instantly, without use of the solution algorithm.", "- Reset Position: Reset the cube to its default rotational position.",
            "- Show Cube Map: Toggles the cube map on or off.", "- Demo Mode: Rotates the cube continuously to show all sides of the 3D model." };

        static List<List<string>> gettingStarted = new List<List<string>> { gettingStartedPage1, gettingStartedPage2, gettingStartedPage3, gettingStartedPage4, gettingStartedPage5, gettingStartedPage6 };

        static List<string> keyCommands1 = new List<string> { "Key Commands", "For added convenience, experienced users can make use of key commands when solving the cube.", "", "Commands:",
            "- F key: Front clockwise", "- Z key: Z-axis middle clockwise", "- B key: Back clockwise", "- L key: Left clockwise", "- X key: X-axis middle clockwise", "- R key: Right clockwise",
            "- U key: Top clockwise", "- Y key: Y-axis middle clockwise", "- D key: Bottom clockwise", "", "*Note: for anti-clockwise rotations, hit the Shift key along with the corresponding  letter key" };

        static List<List<string>> keyCommands = new List<List<string>> { keyCommands1 };

        static List<string> customisation1 = new List<string> { "Customising Cubix", "Cubix allows you to customise both the cube and its background." };

        static List<string> customisation2 = new List<string> { "Changing the Cube's Color", "To change the color of the six faces of the cube, navigate to the settings menu (the gear icon on the menu strip) and click on the 'Cube Colors' option.", 
            "You will now be presented with the cube color interface. Click on one of the faces of the cube to set its color (you may not use the same color twice).", "Once you have chosen your preferred colors, click the 'Okay' button to return to Cubix." };

        static List<string> customisation3 = new List<string> { "Changing the Cube's Background", "To change the background of the cube, navigate to the settings menu and click on the 'Cube Background' option.", 
            "You may now select one of the five available background images (Default, Space, Dark Wood, Light Wood and Sky)." };

        static List<List<string>> customisation = new List<List<string>> { customisation1, customisation2, customisation3 };

        List<List<List<string>>> helpGuides = new List<List<List<string>>> { gettingStarted, keyCommands, customisation };

        public helpForm(int hIndex)
        {
            helpIndex = hIndex;
            InitializeComponent();
        }

        private void helpForm_Load(object sender, EventArgs e)
        {
            helpImageBox.BackgroundImage = helpGuideImages[helpIndex][pageIndex];
            helpFormTextBox.Lines = helpGuides[helpIndex][pageIndex].ToArray();

            if (pageIndex == helpGuides[helpIndex].Count - 1)
            {
                nextPageButton.Visible = false;
                previousPageButton.Visible = false;
            }
        }

        private void nextPageButton_Click(object sender, EventArgs e)
        {
            changePage(true);
        }

        private void previousPageButton_Click(object sender, EventArgs e)
        {
            changePage(false);
        }

        private void changePage(bool nextPage)
        {
            if (nextPage)
            {
                previousPageButton.Enabled = true;
                nextPageButton.Enabled = true;
                pageIndex++;
                if (pageIndex == helpGuides[helpIndex].Count - 1)
                {
                    nextPageButton.Enabled = false;
                }
            }
            else
            {
                previousPageButton.Enabled = true;
                nextPageButton.Enabled = true;
                pageIndex--;
                if (pageIndex == 0)
                {
                    previousPageButton.Enabled = false;
                }
            }
            helpFormTextBox.Lines = helpGuides[helpIndex][pageIndex].ToArray();
            helpImageBox.BackgroundImage = helpGuideImages[helpIndex][pageIndex];
        }

        private void helpFormTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            helpFormTextBox.Enabled = false;
            helpFormTextBox.Enabled = true;
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 16))
            {
                e.Graphics.DrawString("Next Page", myFont, Brushes.White, new Point(78, 18));
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 16))
            {
                e.Graphics.DrawString("Previous Page", myFont, Brushes.White, new Point(55, 18));
            }
        }

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            var b = sender as PictureBox;

            b.BackgroundImage = Properties.Resources.LongButtonBackgroundClicked;
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            var b = sender as PictureBox;

            b.BackgroundImage = Properties.Resources.LongButtonBackground;
        }
    }
}
