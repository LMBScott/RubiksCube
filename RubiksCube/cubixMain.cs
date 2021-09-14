using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace RubiksCube
{
    public partial class cubixMain : Form
    {
        string[] cubeFaceStates = new string[54] { "White", "White", "White", "White", "White", "White", "White", "White", "White", 
         "Yellow", "Yellow", "Yellow", "Yellow", "Yellow", "Yellow", "Yellow", "Yellow", "Yellow", "Orange", "Orange", "Orange", 
         "Orange", "Orange", "Orange", "Orange", "Orange", "Orange", "Red", "Red", "Red", "Red", "Red", "Red", "Red", "Red", "Red", 
         "Blue", "Blue", "Blue", "Blue", "Blue", "Blue", "Blue", "Blue", "Blue", "Green", "Green", "Green", "Green", "Green", 
         "Green", "Green", "Green", "Green" };

        string[] solvedCubeState = new string[54];

        string[] defaultFaceColors = new string[6] { "White", "Yellow", "Orange", "Red", "Blue", "Green" };

        int sequenceIndex = 0;

        Stopwatch stopWatch = new Stopwatch();

        TimeSpan timeElapsed = new TimeSpan();

        List<int> currentSequence = new List<int>();

        List<int> cubeFaceDepth = new List<int>() { 0, 1, 2, 3, 4, 5 };

        PictureBox[] cubeMapFaces;

        Point mouseOrigin = new Point();

        bool AntiAliasing = true;
        bool FaceLabels = false;
        bool Lighting = true;
        bool SecretSetting = false;
        bool attemptInProgress;
        bool cubeSolved;
        bool keyShortcuts = true;

        int cubeSize = 210;
        int originPoint;

        double XRotation = 0;
        double YRotation = 0;
        double ZRotation = 0;

        ThreeDimensionalPoint cameraPosition = new ThreeDimensionalPoint();

        public ThreeDimensionalPoint cubeOrigin;

        public cubixMain()
        {
            InitializeComponent();
        }

        #region Main Form Handlers
        private void cubixMain_Load(object sender, EventArgs e)
        {
            //Format the moveListBox and its label appropriately
            moveListBox.Height = controlPanel.Height - 80;
            moveListBox.Location = new Point(moveListBox.Location.X, 11 + moveListBoxLabel.Height + 10);
            moveListBoxLabel.Location = new Point(moveListBoxLabel.Location.X, 11);

            //Apply the cubixMain_MouseWheel event handler to the main form
            this.MouseWheel += cubixMain_MouseWheel;
            
            //Set the cube at an oblique angle to allow the user to see three sides, making it clear that it is 3D
            XRotation = 10;
            YRotation = 24;
            ZRotation = 360;
            Render();
            updateCubeMap();

            this.DoubleBuffered = true;

            //Center the timer used for timed mode
            Timer.Parent = cubeDisplay;
            Timer.Location = new Point(cubeDisplay.Width / 2 - Timer.Width / 2, cubeDisplay.Height - Timer.Height);
            formatCubeMapControls();

            //Initialise the solvedLabel
            solvedLabel.Paint += new PaintEventHandler(solvedLabel_Paint);
        }

        public void formatCubeMapControls()
        {
            // Center and space the controls of the cube map appropriately on startup
            Control[] cubeMapControls = new Control[12] { frontCubeMapLabel, frontCubeMapPanel, backCubeMapLabel, backCubeMapPanel,
            leftCubeMapLabel, leftCubeMapPanel, rightCubeMapLabel, rightCubeMapPanel, topCubeMapLabel, topCubeMapPanel, bottomCubeMapLabel,
            bottomCubeMapPanel };

            foreach (Control c in cubeMapControls)
            {
                if (Array.IndexOf(cubeMapControls, c) == 0)
                {
                    c.Location = new Point(cubeMapGroupBox.Width / 2 - c.Width / 2, 3);
                }
                else
                {
                    Control prevControl = cubeMapControls[Array.IndexOf(cubeMapControls, c) - 1];
                    c.Location = new Point(cubeMapGroupBox.Width / 2 - c.Width / 2, prevControl.Location.Y + prevControl.Height + 3);
                }
            }
        }

        private void cubixMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Close the application when the cubixMain form is being closed
            Application.Exit();
        }

        private void cubixMain_MouseWheel(object sender, MouseEventArgs e)
        {
            //Increase or decrease the size of the cube based upon user mousewheel input
            if (e.Delta > 0 && cubeSize < 270)
            {
                cubeSize += 5;
            }
            else if (e.Delta < 0 && cubeSize > 150)
            {
                cubeSize -= 5;
            }
            Render();
        }
        #endregion

        #region 3D Engine

        private void Render()
        {
            //Cube is positioned based on center
            Point Origin = new Point(cubeDisplay.Width / 2, cubeDisplay.Height / 2);

            cubeDisplay.Image = drawCube(Origin, cubeFaceStates, AntiAliasing, FaceLabels, SecretSetting);
        }

        public class ThreeDimensionalPoint
        {
            //Class allowing three coordinates to be stored, representing a point in 3D space
            public double X;
            public double Y;
            public double Z;

            public ThreeDimensionalPoint(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public ThreeDimensionalPoint(double x)
            {
                X = x;
                Y = x;
                Z = x;
            }

            public ThreeDimensionalPoint()
            {
            }
        }

        public static ThreeDimensionalPoint RotateX(ThreeDimensionalPoint point, double Degrees)
        {
            //Uses Euler rotation methods to rotate the Y and Z coordinates about the X axis
            double Radians = (Degrees * Math.PI) / 180;
            double Y = (point.Y * Math.Cos(Radians)) + (point.Z * Math.Sin(Radians));
            double Z = (point.Y * -Math.Sin(Radians)) + (point.Z * Math.Cos(Radians));

            return new ThreeDimensionalPoint(point.X, Y, Z);
        }

        public static ThreeDimensionalPoint RotateY(ThreeDimensionalPoint point, double Degrees)
        {
            //Uses Euler rotation methods to rotate the X and Z coordinates about the Y axis
            double Radians = (Degrees * Math.PI) / 180;
            double X = (point.X * Math.Cos(Radians)) + (point.Z * Math.Sin(Radians));
            double Z = (point.X * -Math.Sin(Radians)) + (point.Z * Math.Cos(Radians));

            return new ThreeDimensionalPoint(X, point.Y, Z);
        }

        public static ThreeDimensionalPoint RotateZ(ThreeDimensionalPoint point, double Degrees)
        {
            //Uses Euler rotation methods to rotate the X and Y coordinates about the Z axis
            double Radians = (Degrees * Math.PI) / 180;
            double X = (point.X * Math.Cos(Radians)) + (point.Y * Math.Sin(Radians));
            double Y = (point.X * -Math.Sin(Radians)) + (point.Y * Math.Cos(Radians));

            return new ThreeDimensionalPoint(X, Y, point.Z);
        }

        public static ThreeDimensionalPoint TranslatePoint(ThreeDimensionalPoint point, ThreeDimensionalPoint Origin, ThreeDimensionalPoint Destination)
        {
            //Adds the difference between the current location of each point and its destination to the point's coordinates
            ThreeDimensionalPoint pointDisplacement = new ThreeDimensionalPoint(Destination.X - Origin.X, Destination.Y - Origin.Y, Destination.Z - Origin.Z);
            point.X += pointDisplacement.X;
            point.Y += pointDisplacement.Y;
            point.Z += pointDisplacement.Z;
            return point;
        }

        public static ThreeDimensionalPoint[] RotateX(ThreeDimensionalPoint[] points3D, double degrees)
        {
            //Allows the rotation of an array of ThreeDimensionalPoints about the X axis
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateX(points3D[i], degrees);
            }
            return points3D;
        }

        public static ThreeDimensionalPoint[] RotateY(ThreeDimensionalPoint[] points3D, double degrees)
        {
            //Allows the rotation of an array of ThreeDimensionalPoints about the Y axis
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateY(points3D[i], degrees);
            }
            return points3D;
        }

        public static ThreeDimensionalPoint[] RotateZ(ThreeDimensionalPoint[] points3D, double degrees)
        {
            //Allows the rotation of an array of ThreeDimensionalPoints about the Z axis
            for (int i = 0; i < points3D.Length; i++)
            {
                points3D[i] = RotateZ(points3D[i], degrees);
            }
            return points3D;
        }

        public static ThreeDimensionalPoint[] Translate(ThreeDimensionalPoint[] points, ThreeDimensionalPoint Origin, ThreeDimensionalPoint Destination)
        {
            //Allows the translation of an array of ThreeDimensionalPoints
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = TranslatePoint(points[i], Origin, Destination);
            }
            return points;
        }

            //Calulate the size of the bitmap needed to display the enire cube model at its current rotational position
            public static Rectangle getBounds(PointF[] cubePoints)
            {
                double leftBound = cubePoints[0].X;
                double rightBound = leftBound;
                double lowerBound = cubePoints[0].Y;
                double upperBound = lowerBound;

                foreach (PointF p in cubePoints)
                {
                    if (p.X < leftBound)
                    {
                        leftBound = p.X;
                    }
                    else if (p.X > rightBound)
                    {
                        rightBound = p.X;
                    }
                    if (p.Y < upperBound)
                    {
                        upperBound = p.Y;
                    }
                    else if (p.Y > lowerBound)
                    {
                        lowerBound = p.Y;
                    }
                }

                return new Rectangle(0, 0, (int)Math.Round(rightBound - leftBound), (int)Math.Round(lowerBound - upperBound));
            }

            public Bitmap drawCube(Point drawOrigin, string[] cFaceStates, bool antiAliasing, bool faceLabels, bool secretSetting)
            {
                originPoint = cubeSize / 2;
                cubeOrigin = new ThreeDimensionalPoint(originPoint);

                PointF[] TwoDimensionalPoints = new PointF[102];

                List<int> cubeFaceDepth = new List<int> { 0, 1, 2, 3, 4, 5 };
                List<float> cubeFaceHeight = new List<float> { };

                ThreeDimensionalPoint referencePoint = new ThreeDimensionalPoint(0, 0, 0);

                //Zoom is set with the monitor width to keep the cube from being distorted
                double Zoom = (double)Screen.PrimaryScreen.Bounds.Width / 1.5;

                //Set up the cube
                ThreeDimensionalPoint[] cubePoints = initialiseCubePoints(cubeSize);

                //Calculate the camera Z position to ensure that it remains stationary after rotation            
                ThreeDimensionalPoint anchorPoint = (ThreeDimensionalPoint)cubePoints[4];
                double cameraZ = -(((anchorPoint.X - cubeOrigin.X) * Zoom) / cubeOrigin.X) + anchorPoint.Z;
                cameraPosition = new ThreeDimensionalPoint(cubeOrigin.X, cubeOrigin.Y, cameraZ);            

                //Apply Rotations and translations
                cubePoints = Translate(cubePoints, cubeOrigin, referencePoint);
                cubePoints = RotateX(cubePoints, XRotation);
                cubePoints = RotateY(cubePoints, YRotation);
                cubePoints = RotateZ(cubePoints, ZRotation);
                cubePoints = Translate(cubePoints, referencePoint, cubeOrigin);

                orderCubeFaces(cubePoints, cubeFaceDepth);

                //Convert ThreeDimensionalPoints to 2D Points
                for (int i = 0; i < TwoDimensionalPoints.Length; i++)
                {
                    ThreeDimensionalPoint cubePoint = cubePoints[i];

                    TwoDimensionalPoints[i].X = (int)((float)((cubePoint.X - cameraPosition.X) / (cubePoint.Z - cameraPosition.Z) * Zoom + drawOrigin.X));
                    TwoDimensionalPoints[i].Y = (int)((float)(-(cubePoint.Y - cameraPosition.Y) / (cubePoint.Z - cameraPosition.Z) * Zoom + drawOrigin.Y));
                }

                //Initialise all point arrays needed to store each cube face
                PointF[] backTopLeftFace = new PointF[4] { TwoDimensionalPoints[72], TwoDimensionalPoints[28], TwoDimensionalPoints[0], TwoDimensionalPoints[24] };
                PointF[] backTopMiddleFace = new PointF[4] { TwoDimensionalPoints[73], TwoDimensionalPoints[72], TwoDimensionalPoints[24], TwoDimensionalPoints[26] };
                PointF[] backTopRightFace = new PointF[4] { TwoDimensionalPoints[29], TwoDimensionalPoints[73], TwoDimensionalPoints[26], TwoDimensionalPoints[3] };
                PointF[] backMiddleLeftFace = new PointF[4] { TwoDimensionalPoints[74], TwoDimensionalPoints[30], TwoDimensionalPoints[28], TwoDimensionalPoints[72] };
                PointF[] backMiddleMiddleFace = new PointF[4] { TwoDimensionalPoints[75], TwoDimensionalPoints[74], TwoDimensionalPoints[72], TwoDimensionalPoints[73] };
                PointF[] backMiddleRightFace = new PointF[4] { TwoDimensionalPoints[31], TwoDimensionalPoints[75], TwoDimensionalPoints[73], TwoDimensionalPoints[29] };
                PointF[] backBottomLeftFace = new PointF[4] { TwoDimensionalPoints[25], TwoDimensionalPoints[1], TwoDimensionalPoints[30], TwoDimensionalPoints[74] };
                PointF[] backBottomMiddleFace = new PointF[4] { TwoDimensionalPoints[27], TwoDimensionalPoints[25], TwoDimensionalPoints[74], TwoDimensionalPoints[75] };
                PointF[] backBottomRightFace = new PointF[4] { TwoDimensionalPoints[2], TwoDimensionalPoints[27], TwoDimensionalPoints[75], TwoDimensionalPoints[31] };

                PointF[] frontTopLeftFace = new PointF[4] { TwoDimensionalPoints[77], TwoDimensionalPoints[37], TwoDimensionalPoints[7], TwoDimensionalPoints[34] };
                PointF[] frontTopMiddleFace = new PointF[4] { TwoDimensionalPoints[36], TwoDimensionalPoints[77], TwoDimensionalPoints[34], TwoDimensionalPoints[4] };
                PointF[] frontTopRightFace = new PointF[4] { TwoDimensionalPoints[76], TwoDimensionalPoints[36], TwoDimensionalPoints[4], TwoDimensionalPoints[32] };
                PointF[] frontMiddleLeftFace = new PointF[4] { TwoDimensionalPoints[79], TwoDimensionalPoints[39], TwoDimensionalPoints[37], TwoDimensionalPoints[77] };
                PointF[] frontMiddleMiddleFace = new PointF[4] { TwoDimensionalPoints[78], TwoDimensionalPoints[79], TwoDimensionalPoints[77], TwoDimensionalPoints[76] };
                PointF[] frontMiddleRightFace = new PointF[4] { TwoDimensionalPoints[78], TwoDimensionalPoints[38], TwoDimensionalPoints[36], TwoDimensionalPoints[76] };
                PointF[] frontBottomLeftFace = new PointF[4] { TwoDimensionalPoints[35], TwoDimensionalPoints[6], TwoDimensionalPoints[39], TwoDimensionalPoints[79] };
                PointF[] frontBottomMiddleFace = new PointF[4] { TwoDimensionalPoints[33], TwoDimensionalPoints[35], TwoDimensionalPoints[79], TwoDimensionalPoints[78] };
                PointF[] frontBottomRightFace = new PointF[4] { TwoDimensionalPoints[5], TwoDimensionalPoints[33], TwoDimensionalPoints[78], TwoDimensionalPoints[38] };

                PointF[] leftTopLeftFace = new PointF[4] { TwoDimensionalPoints[85], TwoDimensionalPoints[45], TwoDimensionalPoints[12], TwoDimensionalPoints[40] };
                PointF[] leftTopMiddleFace = new PointF[4] { TwoDimensionalPoints[84], TwoDimensionalPoints[85], TwoDimensionalPoints[40], TwoDimensionalPoints[42] };
                PointF[] leftTopRightFace = new PointF[4] { TwoDimensionalPoints[44], TwoDimensionalPoints[84], TwoDimensionalPoints[42], TwoDimensionalPoints[13] };
                PointF[] leftMiddleLeftFace = new PointF[4] { TwoDimensionalPoints[87], TwoDimensionalPoints[47], TwoDimensionalPoints[45], TwoDimensionalPoints[85] };
                PointF[] leftMiddleMiddleFace = new PointF[4] { TwoDimensionalPoints[86], TwoDimensionalPoints[87], TwoDimensionalPoints[85], TwoDimensionalPoints[84] };
                PointF[] leftMiddleRightFace = new PointF[4] { TwoDimensionalPoints[46], TwoDimensionalPoints[86], TwoDimensionalPoints[84], TwoDimensionalPoints[44] };
                PointF[] leftBottomLeftFace = new PointF[4] { TwoDimensionalPoints[41], TwoDimensionalPoints[15], TwoDimensionalPoints[47], TwoDimensionalPoints[87] };
                PointF[] leftBottomMiddleFace = new PointF[4] { TwoDimensionalPoints[43], TwoDimensionalPoints[41], TwoDimensionalPoints[87], TwoDimensionalPoints[86] };
                PointF[] leftBottomRightFace = new PointF[4] { TwoDimensionalPoints[14], TwoDimensionalPoints[43], TwoDimensionalPoints[86], TwoDimensionalPoints[46] };

                PointF[] rightTopLeftFace = new PointF[4] { TwoDimensionalPoints[80], TwoDimensionalPoints[52], TwoDimensionalPoints[9], TwoDimensionalPoints[50] };
                PointF[] rightTopMiddleFace = new PointF[4] { TwoDimensionalPoints[81], TwoDimensionalPoints[80], TwoDimensionalPoints[50], TwoDimensionalPoints[48] };
                PointF[] rightTopRightFace = new PointF[4] { TwoDimensionalPoints[53], TwoDimensionalPoints[81], TwoDimensionalPoints[48], TwoDimensionalPoints[8] };
                PointF[] rightMiddleLeftFace = new PointF[4] { TwoDimensionalPoints[82], TwoDimensionalPoints[54], TwoDimensionalPoints[52], TwoDimensionalPoints[80] };
                PointF[] rightMiddleMiddleFace = new PointF[4] { TwoDimensionalPoints[83], TwoDimensionalPoints[82], TwoDimensionalPoints[80], TwoDimensionalPoints[81] };
                PointF[] rightMiddleRightFace = new PointF[4] { TwoDimensionalPoints[55], TwoDimensionalPoints[83], TwoDimensionalPoints[81], TwoDimensionalPoints[53] };
                PointF[] rightBottomLeftFace = new PointF[4] { TwoDimensionalPoints[51], TwoDimensionalPoints[10], TwoDimensionalPoints[54], TwoDimensionalPoints[82] };
                PointF[] rightBottomMiddleFace = new PointF[4] { TwoDimensionalPoints[49], TwoDimensionalPoints[51], TwoDimensionalPoints[82], TwoDimensionalPoints[83] };
                PointF[] rightBottomRightFace = new PointF[4] { TwoDimensionalPoints[11], TwoDimensionalPoints[49], TwoDimensionalPoints[83], TwoDimensionalPoints[55] };

                PointF[] topTopLeftFace = new PointF[4] { TwoDimensionalPoints[90], TwoDimensionalPoints[57], TwoDimensionalPoints[23], TwoDimensionalPoints[63] };
                PointF[] topTopMiddleFace = new PointF[4] { TwoDimensionalPoints[89], TwoDimensionalPoints[90], TwoDimensionalPoints[63], TwoDimensionalPoints[61] };
                PointF[] topTopRightFace = new PointF[4] { TwoDimensionalPoints[56], TwoDimensionalPoints[89], TwoDimensionalPoints[61], TwoDimensionalPoints[20] };
                PointF[] topMiddleLeftFace = new PointF[4] { TwoDimensionalPoints[91], TwoDimensionalPoints[59], TwoDimensionalPoints[57], TwoDimensionalPoints[90] };
                PointF[] topMiddleMiddleFace = new PointF[4] { TwoDimensionalPoints[89], TwoDimensionalPoints[90], TwoDimensionalPoints[91], TwoDimensionalPoints[88] };
                PointF[] topMiddleRightFace = new PointF[4] { TwoDimensionalPoints[58], TwoDimensionalPoints[88], TwoDimensionalPoints[89], TwoDimensionalPoints[56] };
                PointF[] topBottomLeftFace = new PointF[4] { TwoDimensionalPoints[62], TwoDimensionalPoints[22], TwoDimensionalPoints[59], TwoDimensionalPoints[91] };
                PointF[] topBottomMiddleFace = new PointF[4] { TwoDimensionalPoints[60], TwoDimensionalPoints[62], TwoDimensionalPoints[91], TwoDimensionalPoints[88] };
                PointF[] topBottomRightFace = new PointF[4] { TwoDimensionalPoints[21], TwoDimensionalPoints[60], TwoDimensionalPoints[88], TwoDimensionalPoints[58] };

                PointF[] bottomTopLeftFace = new PointF[4] { TwoDimensionalPoints[93], TwoDimensionalPoints[65], TwoDimensionalPoints[15], TwoDimensionalPoints[71] };
                PointF[] bottomTopMiddleFace = new PointF[4] { TwoDimensionalPoints[94], TwoDimensionalPoints[93], TwoDimensionalPoints[71], TwoDimensionalPoints[69] };
                PointF[] bottomTopRightFace = new PointF[4] { TwoDimensionalPoints[64], TwoDimensionalPoints[94], TwoDimensionalPoints[69], TwoDimensionalPoints[16] };
                PointF[] bottomMiddleLeftFace = new PointF[4] { TwoDimensionalPoints[95], TwoDimensionalPoints[67], TwoDimensionalPoints[65], TwoDimensionalPoints[93] };
                PointF[] bottomMiddleMiddleFace = new PointF[4] { TwoDimensionalPoints[92], TwoDimensionalPoints[95], TwoDimensionalPoints[93], TwoDimensionalPoints[94] };
                PointF[] bottomMiddleRightFace = new PointF[4] { TwoDimensionalPoints[66], TwoDimensionalPoints[92], TwoDimensionalPoints[94], TwoDimensionalPoints[64] };
                PointF[] bottomBottomLeftFace = new PointF[4] { TwoDimensionalPoints[70], TwoDimensionalPoints[14], TwoDimensionalPoints[67], TwoDimensionalPoints[95] };
                PointF[] bottomBottomMiddleFace = new PointF[4] { TwoDimensionalPoints[68], TwoDimensionalPoints[70], TwoDimensionalPoints[95], TwoDimensionalPoints[92] };
                PointF[] bottomBottomRightFace = new PointF[4] { TwoDimensionalPoints[17], TwoDimensionalPoints[68], TwoDimensionalPoints[92], TwoDimensionalPoints[66] };

                //Store point arrays in a two-dimensional array
                PointF[][] cubeFaces = new PointF[][] { frontTopLeftFace, frontTopMiddleFace, frontTopRightFace, frontMiddleLeftFace, frontMiddleMiddleFace, frontMiddleRightFace, frontBottomLeftFace, frontBottomMiddleFace, frontBottomRightFace,
                backTopLeftFace, backTopMiddleFace, backTopRightFace, backMiddleLeftFace, backMiddleMiddleFace, backMiddleRightFace, backBottomLeftFace, backBottomMiddleFace, backBottomRightFace,
                leftTopLeftFace, leftTopMiddleFace, leftTopRightFace, leftMiddleLeftFace, leftMiddleMiddleFace, leftMiddleRightFace, leftBottomLeftFace, leftBottomMiddleFace, leftBottomRightFace,
                rightTopLeftFace, rightTopMiddleFace, rightTopRightFace, rightMiddleLeftFace, rightMiddleMiddleFace, rightMiddleRightFace, rightBottomLeftFace, rightBottomMiddleFace, rightBottomRightFace,
                topTopLeftFace, topTopMiddleFace, topTopRightFace, topMiddleLeftFace, topMiddleMiddleFace, topMiddleRightFace, topBottomLeftFace, topBottomMiddleFace, topBottomRightFace,
                bottomTopLeftFace, bottomTopMiddleFace, bottomTopRightFace, bottomMiddleLeftFace, bottomMiddleMiddleFace, bottomMiddleRightFace, bottomBottomLeftFace, bottomBottomMiddleFace, bottomBottomRightFace };

                Rectangle bitmapBounds = getBounds(TwoDimensionalPoints);
                bitmapBounds.Width += drawOrigin.X;
                bitmapBounds.Height += drawOrigin.Y;

                //Create a bitmap of the cube using the updated coordinates of its vertices
                Bitmap cubeBitmap = new Bitmap(bitmapBounds.Width, bitmapBounds.Height);
                Graphics g = Graphics.FromImage(cubeBitmap);

                if (Lighting)
                {
                    cubeFaceHeight = getCubeFaceHeights(cubePoints, cubeFaceHeight, bitmapBounds.Height);
                }
                else
                {
                    cubeFaceHeight = new List<float> { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
                }

                //Enable smoothingMode AntiAlias if the user setting Anti-Aliasing is enabled
                if (antiAliasing) { g.SmoothingMode = SmoothingMode.AntiAlias; }

                //Using the cubeFaceDepth array, draw each cube Face in order of decreasing depth to ensure visual realism
                for (int i = 0; i < cubeFaceDepth.Count; i++ )
                {
                    switch (cubeFaceDepth[i])
                    {
                        case 0:
                            //Draw the front face of the cube
                            if (cubeFaces.Length == 54 && cFaceStates.Length == 54)
                            {
                                for (int x = 0; x < 9; x++)
                                {
                                    Color faceColor = Color.FromName(cFaceStates[x]);
                                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(faceColor.A, (int)(faceColor.R * cubeFaceHeight[0]), (int)(faceColor.G * cubeFaceHeight[0]), (int)(faceColor.B * cubeFaceHeight[0])));
                                    g.FillPolygon(b, cubeFaces[x], FillMode.Winding);
                                }
                            }

                            g.DrawLine(Pens.Black, TwoDimensionalPoints[4], TwoDimensionalPoints[5]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[5], TwoDimensionalPoints[6]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[6], TwoDimensionalPoints[7]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[7], TwoDimensionalPoints[4]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[32], TwoDimensionalPoints[33]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[34], TwoDimensionalPoints[35]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[36], TwoDimensionalPoints[37]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[38], TwoDimensionalPoints[39]);

                            if (faceLabels)
                            {
                                Brush labelBrush = new System.Drawing.SolidBrush(Color.FromName("Black"));
                                RectangleF Face = new RectangleF(Math.Min(cubeFaces[4][0].X, cubeFaces[4][2].X),
                                    Math.Min(cubeFaces[4][0].Y, cubeFaces[4][2].Y),
                                    Math.Abs(cubeFaces[4][0].X - cubeFaces[4][2].X),
                                    Math.Abs(cubeFaces[4][0].Y - cubeFaces[4][2].Y));
                                Font labelFont = new Font("Arial", 22.0F, FontStyle.Regular);
                                g.DrawString("F", labelFont, labelBrush, Face);
                            }
                            break;
                        case 1:
                            //Draw the back face of the cube
                            if (cubeFaces.Length == 54 && cFaceStates.Length == 54)
                            {
                                for (int x = 9; x < 18; x++)
                                {
                                    Color faceColor = Color.FromName(cFaceStates[x]);
                                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(faceColor.A, (int)(faceColor.R * cubeFaceHeight[1]), (int)(faceColor.G * cubeFaceHeight[1]), (int)(faceColor.B * cubeFaceHeight[1])));
                                    g.FillPolygon(b, cubeFaces[x], FillMode.Winding);
                                }
                            }

                            g.DrawLine(Pens.Black, TwoDimensionalPoints[0], TwoDimensionalPoints[1]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[1], TwoDimensionalPoints[2]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[2], TwoDimensionalPoints[3]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[3], TwoDimensionalPoints[0]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[24], TwoDimensionalPoints[25]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[26], TwoDimensionalPoints[27]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[28], TwoDimensionalPoints[29]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[30], TwoDimensionalPoints[31]);

                            //Draw the image for the secret setting if it's enabled
                            if (secretSetting)
                            {
                                RectangleF r = new RectangleF(Math.Min(TwoDimensionalPoints[0].X, TwoDimensionalPoints[2].X),
                                   Math.Min(TwoDimensionalPoints[0].Y, TwoDimensionalPoints[2].Y),
                                   Math.Abs(TwoDimensionalPoints[0].X - TwoDimensionalPoints[2].X),
                                   Math.Abs(TwoDimensionalPoints[0].Y - TwoDimensionalPoints[2].Y));
                                g.DrawImage(Properties.Resources.Pepe, r);
                            }

                            if (faceLabels)
                            {
                                Brush labelBrush = new System.Drawing.SolidBrush(Color.FromName("Black"));
                                RectangleF Face = new RectangleF(Math.Min(cubeFaces[13][0].X, cubeFaces[13][2].X),
                                    Math.Min(cubeFaces[13][0].Y, cubeFaces[13][2].Y),
                                    Math.Abs(cubeFaces[13][0].X - cubeFaces[13][2].X),
                                    Math.Abs(cubeFaces[13][0].Y - cubeFaces[13][2].Y));
                                Font labelFont = new Font("Arial", 22.0F, FontStyle.Regular);
                                g.DrawString("B", labelFont, labelBrush, Face);
                            }
                            break;
                        case 2:
                            //Draw the left face of the cube
                            if (cubeFaces.Length == 54 && cFaceStates.Length == 54)
                            {
                                for (int x = 27; x < 36; x++)
                                {
                                    Color faceColor = Color.FromName(cFaceStates[x]);
                                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(faceColor.A, (int)(faceColor.R * cubeFaceHeight[2]), (int)(faceColor.G * cubeFaceHeight[2]), (int)(faceColor.B * cubeFaceHeight[2])));
                                    g.FillPolygon(b, cubeFaces[x], FillMode.Winding);
                                }
                            }

                            g.DrawLine(Pens.Black, TwoDimensionalPoints[8], TwoDimensionalPoints[9]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[9], TwoDimensionalPoints[10]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[10], TwoDimensionalPoints[11]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[11], TwoDimensionalPoints[8]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[48], TwoDimensionalPoints[49]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[50], TwoDimensionalPoints[51]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[52], TwoDimensionalPoints[53]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[54], TwoDimensionalPoints[55]);

                            if (faceLabels)
                            {
                                Brush labelBrush = new System.Drawing.SolidBrush(Color.FromName("Black"));
                                RectangleF Face = new RectangleF(Math.Min(cubeFaces[31][0].X, cubeFaces[31][2].X),
                                    Math.Min(cubeFaces[31][0].Y, cubeFaces[31][2].Y),
                                    Math.Abs(cubeFaces[31][0].X - cubeFaces[31][2].X),
                                    Math.Abs(cubeFaces[31][0].Y - cubeFaces[31][2].Y));
                                Font labelFont = new Font("Arial", 22.0F, FontStyle.Regular);
                                g.DrawString("R", labelFont, labelBrush, Face);
                            }
                            break;
                        case 3:
                            //Draw the right face of the cube
                            if (cubeFaces.Length == 54 && cFaceStates.Length == 54)
                            {
                                for (int x = 18; x < 27; x++)
                                {
                                    Color faceColor = Color.FromName(cFaceStates[x]);
                                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(faceColor.A, (int)(faceColor.R * cubeFaceHeight[3]), (int)(faceColor.G * cubeFaceHeight[3]), (int)(faceColor.B * cubeFaceHeight[3])));
                                    g.FillPolygon(b, cubeFaces[x], FillMode.Winding);
                                }
                            }

                            g.DrawLine(Pens.Black, TwoDimensionalPoints[12], TwoDimensionalPoints[13]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[13], TwoDimensionalPoints[14]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[14], TwoDimensionalPoints[15]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[15], TwoDimensionalPoints[12]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[40], TwoDimensionalPoints[41]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[42], TwoDimensionalPoints[43]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[44], TwoDimensionalPoints[45]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[46], TwoDimensionalPoints[47]);
                            
                            if (faceLabels)
                            {
                                Brush labelBrush = new System.Drawing.SolidBrush(Color.FromName("Black"));
                                RectangleF Face = new RectangleF(Math.Min(cubeFaces[22][0].X, cubeFaces[22][2].X),
                                    Math.Min(cubeFaces[22][0].Y, cubeFaces[22][2].Y),
                                    Math.Abs(cubeFaces[22][0].X - cubeFaces[22][2].X),
                                    Math.Abs(cubeFaces[22][0].Y - cubeFaces[22][2].Y));
                                Font labelFont = new Font("Arial", 22.0F, FontStyle.Regular);
                                g.DrawString("L", labelFont, labelBrush, Face);
                            }
                            break;
                        case 4:
                            //Draw the top face of the cube
                            if (cubeFaces.Length == 54 && cFaceStates.Length == 54)
                            {
                                for (int x = 36; x < 45; x++)
                                {
                                    Color faceColor = Color.FromName(cFaceStates[x]);
                                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(faceColor.A, (int)(faceColor.R * cubeFaceHeight[4]), (int)(faceColor.G * cubeFaceHeight[4]), (int)(faceColor.B * cubeFaceHeight[4])));
                                    g.FillPolygon(b, cubeFaces[x], FillMode.Winding);
                                }
                            }

                            g.DrawLine(Pens.Black, TwoDimensionalPoints[20], TwoDimensionalPoints[21]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[21], TwoDimensionalPoints[22]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[22], TwoDimensionalPoints[23]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[23], TwoDimensionalPoints[20]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[56], TwoDimensionalPoints[57]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[58], TwoDimensionalPoints[59]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[60], TwoDimensionalPoints[61]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[62], TwoDimensionalPoints[63]);

                            if (faceLabels)
                            {
                                Brush labelBrush = new System.Drawing.SolidBrush(Color.FromName("Black"));
                                RectangleF Face = new RectangleF(Math.Min(cubeFaces[40][0].X, cubeFaces[40][2].X),
                                    Math.Min(cubeFaces[40][0].Y, cubeFaces[40][2].Y),
                                    Math.Abs(cubeFaces[40][0].X - cubeFaces[40][2].X),
                                    Math.Abs(cubeFaces[40][0].Y - cubeFaces[40][2].Y));
                                Font labelFont = new Font("Arial", 22.0F, FontStyle.Regular);
                                g.DrawString("T", labelFont, labelBrush, Face);
                            }
                            break;
                        case 5:
                            //Draw the bottom face of the cube
                            if (cubeFaces.Length == 54 && cFaceStates.Length == 54)
                            {
                                for (int x = 45; x < 54; x++)
                                {
                                    Color faceColor = Color.FromName(cFaceStates[x]);
                                    Brush b = new System.Drawing.SolidBrush(Color.FromArgb(faceColor.A, (int)(faceColor.R * cubeFaceHeight[5]), (int)(faceColor.G * cubeFaceHeight[5]), (int)(faceColor.B * cubeFaceHeight[5])));
                                    g.FillPolygon(b, cubeFaces[x], FillMode.Winding);
                                }
                            }

                            g.DrawLine(Pens.Black, TwoDimensionalPoints[16], TwoDimensionalPoints[17]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[17], TwoDimensionalPoints[18]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[18], TwoDimensionalPoints[19]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[19], TwoDimensionalPoints[16]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[64], TwoDimensionalPoints[65]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[66], TwoDimensionalPoints[67]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[68], TwoDimensionalPoints[69]);
                            g.DrawLine(Pens.Black, TwoDimensionalPoints[70], TwoDimensionalPoints[71]);

                            if (faceLabels)
                            {
                                Brush labelBrush = new System.Drawing.SolidBrush(Color.FromName("Black"));
                                RectangleF Face = new RectangleF(Math.Min(cubeFaces[49][0].X, cubeFaces[49][2].X),
                                    Math.Min(cubeFaces[49][0].Y, cubeFaces[49][2].Y),
                                    Math.Abs(cubeFaces[49][0].X - cubeFaces[49][2].X),
                                    Math.Abs(cubeFaces[49][0].Y - cubeFaces[49][2].Y));
                                Font labelFont = new Font("Arial", 22.0F, FontStyle.Regular);
                                g.DrawString("D", labelFont, labelBrush, Face);
                            }
                            break;
                    }
                }

                g.Dispose();

                return cubeBitmap;
            }

            public List<int> orderCubeFaces(ThreeDimensionalPoint[] Points, List<int> Faces)
            {
                //Order the faces of the cube by depth using the coordinates of the midpoint
                bool facesSorted = false;
                while (!facesSorted)
                {
                    facesSorted = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Points[Faces[i] + 96].Z > Points[Faces[i + 1] + 96].Z)
                        {
                            int temp = Faces[i];
                            Faces[i] = Faces[i + 1];
                            Faces[i + 1] = temp;
                            facesSorted = false;
                        }
                    }
                }
                return Faces;
            }

            public List<float> getCubeFaceHeights(ThreeDimensionalPoint[] Points, List<float> Heights, int maxHeight)
            {
                //Order the faces of the cube by height
                bool facesSorted = false;
                while (!facesSorted)
                {
                    facesSorted = true;
                    for (int i = 0; i < 6; i++)
                    {
                        Heights.Add((float)(1 - (Points[i + 96].Y / 600)));
                    }
                }
                return Heights;
            }

            public List<int> getCubeOrientation(List<int> Faces)
            {
                //Modified version of the orderCubeFaces method that does not require a pre-initialised Points array
                ThreeDimensionalPoint referencePoint = new ThreeDimensionalPoint(0, 0, 0);

                ThreeDimensionalPoint[] Points = initialiseCubePoints(cubeSize);

                Points = Translate(Points, cubeOrigin, referencePoint);
                Points = RotateX(Points, XRotation);
                Points = RotateY(Points, YRotation);
                Points = RotateZ(Points, ZRotation);
                Points = Translate(Points, referencePoint, cubeOrigin);

                bool facesSorted = false;
                while (!facesSorted)
                {
                    facesSorted = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Points[Faces[i] + 96].Z > Points[Faces[i + 1] + 96].Z)
                        {
                            int temp = Faces[i];
                            Faces[i] = Faces[i + 1];
                            Faces[i + 1] = temp;
                            facesSorted = false;
                        }
                    }
                }
                return Faces;
            }

            public static ThreeDimensionalPoint[] initialiseCubePoints(int Size)
            {
                //Define the exact position of each point on the cube based on a stationery cube at 0,0,0 rotation
                //Allows lines and rectangles to be drawn to the bitmap as part of the drawCube method
                ThreeDimensionalPoint[] initialCubePoints = new ThreeDimensionalPoint[102];

                //Back face
                initialCubePoints[0] = new ThreeDimensionalPoint(0, 0, 0);
                initialCubePoints[1] = new ThreeDimensionalPoint(0, Size, 0);
                initialCubePoints[2] = new ThreeDimensionalPoint(Size, Size, 0);
                initialCubePoints[3] = new ThreeDimensionalPoint(Size, 0, 0);
                initialCubePoints[97] = new ThreeDimensionalPoint(Size / 2, Size / 2, 0);

                //Third-line Points
                initialCubePoints[24] = new ThreeDimensionalPoint(Size / 3, 0, 0);
                initialCubePoints[25] = new ThreeDimensionalPoint(Size / 3, Size, 0);
                initialCubePoints[26] = new ThreeDimensionalPoint(2 * Size / 3, 0, 0);
                initialCubePoints[27] = new ThreeDimensionalPoint(2 * Size / 3, Size, 0);
                initialCubePoints[28] = new ThreeDimensionalPoint(0, Size / 3, 0);
                initialCubePoints[29] = new ThreeDimensionalPoint(Size, Size / 3, 0);
                initialCubePoints[30] = new ThreeDimensionalPoint(0, 2 * Size / 3, 0);
                initialCubePoints[31] = new ThreeDimensionalPoint(Size, 2 * Size / 3, 0);

                //Third-line intersections
                initialCubePoints[72] = new ThreeDimensionalPoint(Size / 3, Size / 3, 0);
                initialCubePoints[73] = new ThreeDimensionalPoint(2 * Size / 3, Size / 3, 0);
                initialCubePoints[74] = new ThreeDimensionalPoint(Size / 3, 2 * Size / 3, 0);
                initialCubePoints[75] = new ThreeDimensionalPoint(2 * Size / 3, 2 * Size / 3, 0);

                //Front face
                initialCubePoints[4] = new ThreeDimensionalPoint(0, 0, Size);
                initialCubePoints[5] = new ThreeDimensionalPoint(0, Size, Size);
                initialCubePoints[6] = new ThreeDimensionalPoint(Size, Size, Size);
                initialCubePoints[7] = new ThreeDimensionalPoint(Size, 0, Size);
                initialCubePoints[96] = new ThreeDimensionalPoint(Size / 2, Size / 2, Size);

                //Third-line Points
                initialCubePoints[32] = new ThreeDimensionalPoint(Size / 3, 0, Size);
                initialCubePoints[33] = new ThreeDimensionalPoint(Size / 3, Size, Size);
                initialCubePoints[34] = new ThreeDimensionalPoint(2 * Size / 3, 0, Size);
                initialCubePoints[35] = new ThreeDimensionalPoint(2 * Size / 3, Size, Size);
                initialCubePoints[36] = new ThreeDimensionalPoint(0, Size / 3, Size);
                initialCubePoints[37] = new ThreeDimensionalPoint(Size, Size / 3, Size);
                initialCubePoints[38] = new ThreeDimensionalPoint(0, 2 * Size / 3, Size);
                initialCubePoints[39] = new ThreeDimensionalPoint(Size, 2 * Size / 3, Size);

                //Third-line intersections
                initialCubePoints[76] = new ThreeDimensionalPoint(Size / 3, Size / 3, Size);
                initialCubePoints[77] = new ThreeDimensionalPoint(2 * Size / 3, Size / 3, Size);
                initialCubePoints[78] = new ThreeDimensionalPoint(Size / 3, 2 * Size / 3, Size);
                initialCubePoints[79] = new ThreeDimensionalPoint(2 * Size / 3, 2 * Size / 3, Size);

                //Left face
                initialCubePoints[8] = new ThreeDimensionalPoint(0, 0, 0);
                initialCubePoints[9] = new ThreeDimensionalPoint(0, 0, Size);
                initialCubePoints[10] = new ThreeDimensionalPoint(0, Size, Size);
                initialCubePoints[11] = new ThreeDimensionalPoint(0, Size, 0);
                initialCubePoints[98] = new ThreeDimensionalPoint(0, Size / 2, Size / 2);

                //Third-line Points
                initialCubePoints[40] = new ThreeDimensionalPoint(Size, 0, Size / 3);
                initialCubePoints[41] = new ThreeDimensionalPoint(Size, Size, Size / 3);
                initialCubePoints[42] = new ThreeDimensionalPoint(Size, 0, 2 * Size / 3);
                initialCubePoints[43] = new ThreeDimensionalPoint(Size, Size, 2 * Size / 3);
                initialCubePoints[44] = new ThreeDimensionalPoint(Size, Size / 3, Size);
                initialCubePoints[45] = new ThreeDimensionalPoint(Size, Size / 3, 0);
                initialCubePoints[46] = new ThreeDimensionalPoint(Size, 2 * Size / 3, Size);
                initialCubePoints[47] = new ThreeDimensionalPoint(Size, 2 * Size / 3, 0);

                //Third-line intersections
                initialCubePoints[80] = new ThreeDimensionalPoint(0, Size / 3, 2 * Size / 3);
                initialCubePoints[81] = new ThreeDimensionalPoint(0, Size / 3, Size / 3);
                initialCubePoints[82] = new ThreeDimensionalPoint(0, 2 * Size / 3, 2 * Size / 3);
                initialCubePoints[83] = new ThreeDimensionalPoint(0, 2 * Size / 3, Size / 3);

                //Right face
                initialCubePoints[12] = new ThreeDimensionalPoint(Size, 0, 0);
                initialCubePoints[13] = new ThreeDimensionalPoint(Size, 0, Size);
                initialCubePoints[14] = new ThreeDimensionalPoint(Size, Size, Size);
                initialCubePoints[15] = new ThreeDimensionalPoint(Size, Size, 0);
                initialCubePoints[99] = new ThreeDimensionalPoint(Size, Size / 2, Size / 2);

                //Third-line Points
                initialCubePoints[48] = new ThreeDimensionalPoint(0, 0, Size / 3);
                initialCubePoints[49] = new ThreeDimensionalPoint(0, Size, Size / 3);
                initialCubePoints[50] = new ThreeDimensionalPoint(0, 0, 2 * Size / 3);
                initialCubePoints[51] = new ThreeDimensionalPoint(0, Size, 2 * Size / 3);
                initialCubePoints[52] = new ThreeDimensionalPoint(0, Size / 3, Size);
                initialCubePoints[53] = new ThreeDimensionalPoint(0, Size / 3, 0);
                initialCubePoints[54] = new ThreeDimensionalPoint(0, 2 * Size / 3, Size);
                initialCubePoints[55] = new ThreeDimensionalPoint(0, 2 * Size / 3, 0);

                //Third-line intersections
                initialCubePoints[84] = new ThreeDimensionalPoint(Size, Size / 3, 2 * Size / 3);
                initialCubePoints[85] = new ThreeDimensionalPoint(Size, Size / 3, Size / 3);
                initialCubePoints[86] = new ThreeDimensionalPoint(Size, 2 * Size / 3, 2 * Size / 3);
                initialCubePoints[87] = new ThreeDimensionalPoint(Size, 2 * Size / 3, Size / 3);

                //Top face
                initialCubePoints[16] = new ThreeDimensionalPoint(0, Size, 0);
                initialCubePoints[17] = new ThreeDimensionalPoint(0, Size, Size);
                initialCubePoints[18] = new ThreeDimensionalPoint(Size, Size, Size);
                initialCubePoints[19] = new ThreeDimensionalPoint(Size, Size, 0);
                initialCubePoints[100] = new ThreeDimensionalPoint(Size / 2, 0, Size / 2);

                //Third-line Points
                initialCubePoints[56] = new ThreeDimensionalPoint(0, 0, Size / 3);
                initialCubePoints[57] = new ThreeDimensionalPoint(Size, 0, Size / 3);
                initialCubePoints[58] = new ThreeDimensionalPoint(0, 0, 2 * Size / 3);
                initialCubePoints[59] = new ThreeDimensionalPoint(Size, 0, 2 * Size / 3);
                initialCubePoints[60] = new ThreeDimensionalPoint(Size / 3, 0, Size);
                initialCubePoints[61] = new ThreeDimensionalPoint(Size / 3, 0, 0);
                initialCubePoints[62] = new ThreeDimensionalPoint(2 * Size / 3, 0, Size);
                initialCubePoints[63] = new ThreeDimensionalPoint(2 * Size / 3, 0, 0);

                //Third-line intersections
                initialCubePoints[88] = new ThreeDimensionalPoint(Size / 3, 0, 2 * Size / 3);
                initialCubePoints[89] = new ThreeDimensionalPoint(Size / 3, 0, Size / 3);
                initialCubePoints[90] = new ThreeDimensionalPoint(2 * Size / 3, 0, Size / 3);
                initialCubePoints[91] = new ThreeDimensionalPoint(2 * Size / 3, 0, 2 * Size / 3);

                //Bottom face
                initialCubePoints[20] = new ThreeDimensionalPoint(0, 0, 0);
                initialCubePoints[21] = new ThreeDimensionalPoint(0, 0, Size);
                initialCubePoints[22] = new ThreeDimensionalPoint(Size, 0, Size);
                initialCubePoints[23] = new ThreeDimensionalPoint(Size, 0, 0);
                initialCubePoints[101] = new ThreeDimensionalPoint(Size / 2, Size, Size / 2);

                //Third-line Points
                initialCubePoints[64] = new ThreeDimensionalPoint(0, Size, Size / 3);
                initialCubePoints[65] = new ThreeDimensionalPoint(Size, Size, Size / 3);
                initialCubePoints[66] = new ThreeDimensionalPoint(0, Size, 2 * Size / 3);
                initialCubePoints[67] = new ThreeDimensionalPoint(Size, Size, 2 * Size / 3);
                initialCubePoints[68] = new ThreeDimensionalPoint(Size / 3, Size, Size);
                initialCubePoints[69] = new ThreeDimensionalPoint(Size / 3, Size, 0);
                initialCubePoints[70] = new ThreeDimensionalPoint(2 * Size / 3, Size, Size);
                initialCubePoints[71] = new ThreeDimensionalPoint(2 * Size / 3, Size, 0);

                //Third-line intersections
                initialCubePoints[92] = new ThreeDimensionalPoint(Size / 3, Size, 2 * Size / 3);
                initialCubePoints[93] = new ThreeDimensionalPoint(2 * Size / 3, Size, Size / 3);
                initialCubePoints[94] = new ThreeDimensionalPoint(Size / 3, Size, Size / 3);
                initialCubePoints[95] = new ThreeDimensionalPoint(2 * Size / 3, Size, 2 * Size / 3);

                return initialCubePoints;
            }
        #endregion

        #region Auto-Solve
        public void Solve()
        {
            string topFaceColor;
            bool topFaceSolved = true;
            //Get the color of the top face (cubeFaceStates[40] will always remain the same color)
            topFaceColor = cubeFaceStates[40];
            for (int i = 36; i < 44; i++)
            {
                //Check if the top face is solved
                if (cubeFaceStates[i] != topFaceColor)
                {
                    topFaceSolved = false;
                    break;
                }
            }

            int topFaceRepCount = 0;
            //Solve the top face of the cube
            if (!topFaceSolved)
            {
                bool edgesInPlace = false;
                while (!edgesInPlace)
                {
                    Render();
                    if ((cubeFaceStates[37] == topFaceColor) && (cubeFaceStates[39] == topFaceColor) && (cubeFaceStates[41] == topFaceColor) && (cubeFaceStates[43] == topFaceColor))
                    {
                        edgesInPlace = true;
                        break;
                    }
                    //Check all edges of the top-adjacent faces for a match to the top face color
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 1; j < 8; j += 2)
                        {
                            int index = j + (9 * i);
                            //Upon finding a match, move the target piece to the top face
                            if (cubeFaceStates[index] == topFaceColor)
                            {
                                //Move the piece according to its location (using cube face rotation methods)
                                switch (index)
                                {
                                    //If piece is at the top of faces adjacent to the top face
                                    case 10:
                                        backCCW();
                                        yMiddleCCW();
                                        backCCW();
                                        break;
                                    case 19:
                                        leftCCW();
                                        yMiddleCCW();
                                        leftCCW();
                                        break;
                                    case 1:
                                        frontCCW();
                                        yMiddleCCW();
                                        frontCCW();
                                        break;
                                    case 28:
                                        rightCCW();
                                        yMiddleCCW();
                                        rightCCW();
                                        break;
                                    //If piece is on the right of faces adjacent to the top face
                                    case 5:
                                        yMiddleCW();
                                        if (cubeFaceStates[43] != topFaceColor)
                                        {
                                            frontCW();
                                        }
                                        break;
                                    case 23:
                                        yMiddleCW();
                                        if (cubeFaceStates[39] != topFaceColor)
                                        {
                                            leftCW();
                                        }
                                        break;
                                    case 14:
                                        yMiddleCW();
                                        if (cubeFaceStates[37] != topFaceColor)
                                        {
                                            backCW();
                                        }
                                        break;
                                    case 32:
                                        yMiddleCW();
                                        if (cubeFaceStates[41] != topFaceColor)
                                        {
                                            rightCW();
                                        }
                                        break;
                                    //If piece is on the left of faces adjacent to the top face
                                    case 3:
                                        yMiddleCCW();
                                        if (cubeFaceStates[43] != topFaceColor)
                                        {
                                            frontCCW();
                                        }
                                        break;
                                    case 21:
                                        yMiddleCCW();
                                        if (cubeFaceStates[39] != topFaceColor)
                                        {
                                            leftCCW();
                                        }
                                        break;
                                    case 12:
                                        yMiddleCCW();
                                        if (cubeFaceStates[37] != topFaceColor)
                                        {
                                            backCCW();
                                        }
                                        break;
                                    case 30:
                                        yMiddleCCW();
                                        if (cubeFaceStates[41] != topFaceColor)
                                        {
                                            rightCCW();
                                        }
                                        break;
                                    //If piece is at the bottom of faces adjacent to the top face
                                    case 7:
                                        if (cubeFaceStates[43] != topFaceColor)
                                        {
                                            frontCW();
                                            yMiddleCCW();
                                            frontCCW();
                                            break;
                                        }
                                        bottomCW();
                                        break;
                                    case 25:
                                        if (cubeFaceStates[39] != topFaceColor)
                                        {
                                            leftCW();
                                            yMiddleCCW();
                                            leftCCW();
                                            break;
                                        }
                                        bottomCW();
                                        break;
                                    case 16:
                                        if (cubeFaceStates[37] != topFaceColor)
                                        {
                                            backCW();
                                            yMiddleCCW();
                                            backCCW();
                                            break;
                                        }
                                        bottomCW();
                                        break;
                                    case 34:
                                        if (cubeFaceStates[41] != topFaceColor)
                                        {
                                            rightCW();
                                            yMiddleCCW();
                                            rightCCW();
                                            break;
                                        }
                                        bottomCW();
                                        break;
                                }
                                updateProgressBar();
                            }
                        }
                        updateProgressBar();
                    }
                    //Check all edges of the bottom face for a match to the top face color
                    for (int i = 46; i < 53; i += 2)
                    {
                        Render();
                        if (cubeFaceStates[i] == topFaceColor)
                        {
                            switch (i)
                            {
                                case 46:
                                    if (cubeFaceStates[37] != topFaceColor)
                                    {
                                        backCW();
                                        backCW();
                                        break;
                                    }
                                    bottomCW();
                                    break;
                                case 48:
                                    if (cubeFaceStates[39] != topFaceColor)
                                    {
                                        leftCW();
                                        leftCW();
                                        break;
                                    }
                                    bottomCW();
                                    break;
                                case 50:
                                    if (cubeFaceStates[41] != topFaceColor)
                                    {
                                        rightCW();
                                        rightCW();
                                        break;
                                    }
                                    bottomCW();
                                    break;
                                case 52:
                                    if (cubeFaceStates[43] != topFaceColor)
                                    {
                                        frontCW();
                                        frontCW();
                                        break;
                                    }
                                    bottomCW();
                                    break;
                            }
                        }
                        updateProgressBar();
                        topFaceRepCount++;
                        if (topFaceRepCount > 20)
                        {
                            MessageBox.Show("Something went wrong when trying to solve the cube, sorry.", "Error");
                            resetFacesToolStripMenuItem.PerformClick();
                            moveListBox.Items.Clear();
                            moveListBoxLabel.Text = "Moves";
                            return;
                        }
                    }
                }
                //All edges are in place on the top face, they must now be moved to the position matching the colors on their sides
                bool topEdgesCorrect = false;
                while (!topEdgesCorrect)
                {
                    Render();
                    topEdgesCorrect = true;
                    //Check three of the four top-face edge pieces to see if they can be swapped with another piece, thereby placing them in their correct positions
                    if (cubeFaceStates[1] != cubeFaceStates[4])
                    {
                        if ((cubeFaceStates[1] == cubeFaceStates[22]) && (cubeFaceStates[19] == cubeFaceStates[4]))
                        {
                            frontCW();
                            frontCW();
                            bottomCCW();
                            leftCW();
                            leftCW();
                            bottomCW();
                            frontCW();
                            frontCW();
                        }
                        else if ((cubeFaceStates[1] == cubeFaceStates[13]) && (cubeFaceStates[10] == cubeFaceStates[4]))
                        {
                            frontCW();
                            frontCW();
                            bottomCCW();
                            bottomCCW();
                            backCW();
                            backCW();
                            bottomCCW();
                            bottomCCW();
                            frontCW();
                            frontCW();
                        }
                        else if ((cubeFaceStates[1] == cubeFaceStates[31]) && (cubeFaceStates[28] == cubeFaceStates[4]))
                        {
                            frontCW();
                            frontCW();
                            bottomCW();
                            rightCW();
                            rightCW();
                            bottomCCW();
                            frontCW();
                            frontCW();
                        }
                    }

                    if (cubeFaceStates[28] != cubeFaceStates[31])
                    {
                        if ((cubeFaceStates[28] == cubeFaceStates[4]) && (cubeFaceStates[1] == cubeFaceStates[31]))
                        {
                            rightCW();
                            rightCW();
                            bottomCCW();
                            frontCW();
                            frontCW();
                            bottomCW();
                            rightCW();
                            rightCW();
                        }
                        else if ((cubeFaceStates[28] == cubeFaceStates[22]) && (cubeFaceStates[19] == cubeFaceStates[31]))
                        {
                            rightCW();
                            rightCW();
                            bottomCCW();
                            bottomCCW();
                            leftCW();
                            leftCW();
                            bottomCCW();
                            bottomCCW();
                            rightCW();
                            rightCW();
                        }
                        else if ((cubeFaceStates[28] == cubeFaceStates[13] && cubeFaceStates[10] == cubeFaceStates[31]))
                        {
                            rightCW();
                            rightCW();
                            bottomCW();
                            backCW();
                            backCW();
                            bottomCCW();
                            rightCW();
                            rightCW();
                        }
                    }

                    if (cubeFaceStates[19] != cubeFaceStates[22])
                    {
                        if ((cubeFaceStates[19] == cubeFaceStates[13] && cubeFaceStates[10] == cubeFaceStates[22]))
                        {
                            leftCW();
                            leftCW();
                            bottomCCW();
                            backCW();
                            backCW();
                            bottomCW();
                            leftCW();
                            leftCW();
                        }
                        else if ((cubeFaceStates[19] == cubeFaceStates[31] && cubeFaceStates[28] == cubeFaceStates[22]))
                        {
                            leftCW();
                            leftCW();
                            bottomCCW();
                            bottomCCW();
                            rightCW();
                            rightCW();
                            bottomCCW();
                            bottomCCW();
                            leftCW();
                            leftCW();
                        }
                        else if ((cubeFaceStates[19] == cubeFaceStates[4] && cubeFaceStates[1] == cubeFaceStates[22]))
                        {
                            leftCW();
                            leftCW();
                            bottomCW();
                            frontCW();
                            frontCW();
                            bottomCCW();
                            leftCW();
                            leftCW();
                        }
                    }
                    //If no swaps have been made, the top face must be rotated and the sequence repeats itself
                    if ((cubeFaceStates[1] != cubeFaceStates[4]) || (cubeFaceStates[28] != cubeFaceStates[31]) || (cubeFaceStates[19] != cubeFaceStates[22]))
                    {
                        topEdgesCorrect = false;
                        topCW();
                    }
                    updateProgressBar();
                }
                //Top face edge pieces are in their correct positions. Corners must now be positioned correctly.

                bool topCornersCorrect = false;

                while (!topCornersCorrect)
                {
                    Render();
                    string[,] cubeCorners;
                    //^A multidimensional array of arrays of three cubeFaceStates values that represent the corners of the cube^

                    //For each corner, check if it belongs on the top face, then move and position it correctly
                    for (int i = 0; i < 8; i++)
                    {
                        cubeCorners = new string[8, 3] { { cubeFaceStates[42], cubeFaceStates[0], cubeFaceStates[20] }, { cubeFaceStates[44],  cubeFaceStates[2], cubeFaceStates[27] },
                                { cubeFaceStates[38], cubeFaceStates[9], cubeFaceStates[29] }, { cubeFaceStates[36], cubeFaceStates[11], cubeFaceStates[18] }, { cubeFaceStates[53], cubeFaceStates[8], cubeFaceStates[33] },
                                { cubeFaceStates[51], cubeFaceStates[6], cubeFaceStates[26] }, { cubeFaceStates[47], cubeFaceStates[15], cubeFaceStates[35] }, { cubeFaceStates[45], cubeFaceStates[17], cubeFaceStates[24] } };
                        for (int j = 0; j < 3; j++)
                        {
                            Render();
                            if (cubeCorners[i, j] == topFaceColor)
                            {
                                //Find the colors of the two faces of the target corner that are not the top face color
                                List<int> cornerIndices = new List<int> { 0, 1, 2 };
                                cornerIndices.Remove(j);
                                string cornerEdgeColor1 = cubeCorners[i, cornerIndices[0]];
                                string cornerEdgeColor2 = cubeCorners[i, cornerIndices[1]];
                                //Now cross-reference the colors found with the colors of each side of the cube, then move the corner into position based upon where it currently is
                                switch (i)
                                {
                                    case 0:
                                        if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[22]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[22]))
                                        {
                                            while ((cubeFaceStates[42] != topFaceColor) || (cubeFaceStates[0] != cubeFaceStates[4]) || (cubeFaceStates[20] != cubeFaceStates[22]))
                                            {
                                                frontCCW();
                                                bottomCCW();
                                                frontCW();
                                                bottomCW();
                                            }
                                        }
                                        else
                                        {
                                            frontCCW();
                                            bottomCCW();
                                            frontCW();
                                        }
                                        break;
                                    case 1:
                                        if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            while ((cubeFaceStates[44] != topFaceColor) || (cubeFaceStates[2] != cubeFaceStates[4]) || (cubeFaceStates[27] != cubeFaceStates[31]))
                                            {
                                                rightCCW();
                                                bottomCCW();
                                                rightCW();
                                                bottomCW();
                                            }
                                        }
                                        else
                                        {
                                            rightCCW();
                                            bottomCCW();
                                            rightCW();
                                        }
                                        break;
                                    case 2:
                                        if ((cornerEdgeColor1 == cubeFaceStates[13] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[13] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            while ((cubeFaceStates[38] != topFaceColor) || (cubeFaceStates[9] != cubeFaceStates[13]) || (cubeFaceStates[29] != cubeFaceStates[31]))
                                            {
                                                backCCW();
                                                bottomCCW();
                                                backCW();
                                                bottomCW();
                                            }
                                        }
                                        else
                                        {
                                            backCCW();
                                            bottomCCW();
                                            backCW();
                                        }
                                        break;
                                    case 3:
                                        if ((cornerEdgeColor1 == cubeFaceStates[22] && cornerEdgeColor2 == cubeFaceStates[13]) || (cornerEdgeColor2 == cubeFaceStates[22] && cornerEdgeColor1 == cubeFaceStates[13]))
                                        {
                                            while ((cubeFaceStates[36] != topFaceColor) || (cubeFaceStates[18] != cubeFaceStates[22]) || (cubeFaceStates[11] != cubeFaceStates[13]))
                                            {
                                                leftCCW();
                                                bottomCCW();
                                                leftCW();
                                                bottomCW();
                                            }
                                        }
                                        else
                                        {
                                            leftCCW();
                                            bottomCCW();
                                            leftCW();
                                        }
                                        break;
                                    //Bottom right corner
                                    case 4:
                                        if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            while ((cubeFaceStates[44] != topFaceColor) || (cubeFaceStates[2] != cubeFaceStates[4]) || (cubeFaceStates[27] != cubeFaceStates[31]))
                                            {
                                                rightCCW();
                                                bottomCCW();
                                                rightCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[22]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[22]))
                                        {
                                            bottomCCW();
                                            while ((cubeFaceStates[42] != topFaceColor) || (cubeFaceStates[0] != cubeFaceStates[4]) || (cubeFaceStates[20] != cubeFaceStates[22]))
                                            {
                                                frontCCW();
                                                bottomCCW();
                                                frontCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[22] && cornerEdgeColor2 == cubeFaceStates[13]) || (cornerEdgeColor2 == cubeFaceStates[22] && cornerEdgeColor1 == cubeFaceStates[13]))
                                        {
                                            bottomCCW();
                                            bottomCCW();
                                            while ((cubeFaceStates[36] != topFaceColor) || (cubeFaceStates[18] != cubeFaceStates[22]) || (cubeFaceStates[11] != cubeFaceStates[13]))
                                            {
                                                leftCCW();
                                                bottomCCW();
                                                leftCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[13] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[13] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            bottomCW();
                                            while ((cubeFaceStates[38] != topFaceColor) || (cubeFaceStates[9] != cubeFaceStates[13]) || (cubeFaceStates[29] != cubeFaceStates[31]))
                                            {
                                                backCCW();
                                                bottomCCW();
                                                backCW();
                                                bottomCW();
                                            }
                                        }
                                        break;
                                    //Bottom left corner
                                    case 5:
                                        if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[22]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[22]))
                                        {
                                            while ((cubeFaceStates[42] != topFaceColor) || (cubeFaceStates[0] != cubeFaceStates[4]) || (cubeFaceStates[20] != cubeFaceStates[22]))
                                            {
                                                frontCCW();
                                                bottomCCW();
                                                frontCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[22] && cornerEdgeColor2 == cubeFaceStates[13]) || (cornerEdgeColor2 == cubeFaceStates[22] && cornerEdgeColor1 == cubeFaceStates[13]))
                                        {
                                            bottomCCW();
                                            while ((cubeFaceStates[36] != topFaceColor) || (cubeFaceStates[18] != cubeFaceStates[22]) || (cubeFaceStates[11] != cubeFaceStates[13]))
                                            {
                                                leftCCW();
                                                bottomCCW();
                                                leftCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[13] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[13] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            bottomCCW();
                                            bottomCCW();
                                            while ((cubeFaceStates[38] != topFaceColor) || (cubeFaceStates[9] != cubeFaceStates[13]) || (cubeFaceStates[29] != cubeFaceStates[31]))
                                            {
                                                backCCW();
                                                bottomCCW();
                                                backCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            bottomCW();
                                            while ((cubeFaceStates[44] != topFaceColor) || (cubeFaceStates[2] != cubeFaceStates[4]) || (cubeFaceStates[27] != cubeFaceStates[31]))
                                            {
                                                rightCCW();
                                                bottomCCW();
                                                rightCW();
                                                bottomCW();
                                            }
                                        }
                                        break;
                                    //Back bottom right corner
                                    case 6:
                                        if ((cornerEdgeColor1 == cubeFaceStates[13] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[13] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            while ((cubeFaceStates[38] != topFaceColor) || (cubeFaceStates[9] != cubeFaceStates[13]) || (cubeFaceStates[29] != cubeFaceStates[31]))
                                            {
                                                backCCW();
                                                bottomCCW();
                                                backCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[31] && cornerEdgeColor2 == cubeFaceStates[4]) || (cornerEdgeColor2 == cubeFaceStates[31] && cornerEdgeColor1 == cubeFaceStates[4]))
                                        {
                                            bottomCCW();
                                            while ((cubeFaceStates[44] != topFaceColor) || (cubeFaceStates[27] != cubeFaceStates[31]) || (cubeFaceStates[2] != cubeFaceStates[4]))
                                            {
                                                rightCCW();
                                                bottomCCW();
                                                rightCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[22]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[22]))
                                        {
                                            bottomCCW();
                                            bottomCCW();
                                            while ((cubeFaceStates[42] != topFaceColor) || (cubeFaceStates[0] != cubeFaceStates[4]) || (cubeFaceStates[20] != cubeFaceStates[22]))
                                            {
                                                frontCCW();
                                                bottomCCW();
                                                frontCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[22] && cornerEdgeColor2 == cubeFaceStates[13]) || (cornerEdgeColor2 == cubeFaceStates[22] && cornerEdgeColor1 == cubeFaceStates[13]))
                                        {
                                            bottomCW();
                                            while ((cubeFaceStates[36] != topFaceColor) || (cubeFaceStates[18] != cubeFaceStates[22]) || (cubeFaceStates[11] != cubeFaceStates[13]))
                                            {
                                                leftCCW();
                                                bottomCCW();
                                                leftCW();
                                                bottomCW();
                                            }
                                        }
                                        break;
                                    //back bottom left corner
                                    case 7:
                                        if ((cornerEdgeColor1 == cubeFaceStates[22] && cornerEdgeColor2 == cubeFaceStates[13]) || (cornerEdgeColor2 == cubeFaceStates[22] && cornerEdgeColor1 == cubeFaceStates[13]))
                                        {
                                            while ((cubeFaceStates[36] != topFaceColor) || (cubeFaceStates[18] != cubeFaceStates[22]) || (cubeFaceStates[11] != cubeFaceStates[13]))
                                            {
                                                leftCCW();
                                                bottomCCW();
                                                leftCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[13] && cornerEdgeColor2 == cubeFaceStates[31]) || (cornerEdgeColor2 == cubeFaceStates[13] && cornerEdgeColor1 == cubeFaceStates[31]))
                                        {
                                            bottomCCW();
                                            while ((cubeFaceStates[38] != topFaceColor) || (cubeFaceStates[9] != cubeFaceStates[13]) || (cubeFaceStates[29] != cubeFaceStates[31]))
                                            {
                                                backCCW();
                                                bottomCCW();
                                                backCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[31] && cornerEdgeColor2 == cubeFaceStates[4]) || (cornerEdgeColor2 == cubeFaceStates[31] && cornerEdgeColor1 == cubeFaceStates[4]))
                                        {
                                            bottomCCW();
                                            bottomCCW();
                                            while ((cubeFaceStates[44] != topFaceColor) || (cubeFaceStates[27] != cubeFaceStates[31]) || (cubeFaceStates[2] != cubeFaceStates[4]))
                                            {
                                                rightCCW();
                                                bottomCCW();
                                                rightCW();
                                                bottomCW();
                                            }
                                        }
                                        else if ((cornerEdgeColor1 == cubeFaceStates[4] && cornerEdgeColor2 == cubeFaceStates[22]) || (cornerEdgeColor2 == cubeFaceStates[4] && cornerEdgeColor1 == cubeFaceStates[22]))
                                        {
                                            bottomCW();
                                            while ((cubeFaceStates[42] != topFaceColor) || (cubeFaceStates[0] != cubeFaceStates[4]) || (cubeFaceStates[20] != cubeFaceStates[22]))
                                            {
                                                frontCCW();
                                                bottomCCW();
                                                frontCW();
                                                bottomCW();
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    if ((cubeFaceStates[36] == topFaceColor) && (cubeFaceStates[38] == topFaceColor) && (cubeFaceStates[42] == topFaceColor) && (cubeFaceStates[44] == topFaceColor))
                    {
                        topCornersCorrect = true;
                    }
                    updateProgressBar();
                }
            }
            topFaceSolved = true;
            //Top layer is complete

            //Solve the middle layer of the cube
            bool middleLayerSolved = false;
            int middleLayerRepCount = 0;
            while (!middleLayerSolved)
            {
                Render();
                string[,] bottomLayerEdges;

                string bottomLayerColor = cubeFaceStates[49];

                //Check if the middle layer has been solved
                if (cubeFaceStates[23] == cubeFaceStates[22] && cubeFaceStates[3] == cubeFaceStates[4] && cubeFaceStates[5] == cubeFaceStates[4] && cubeFaceStates[30] == cubeFaceStates[31]
                    && cubeFaceStates[32] == cubeFaceStates[31] && cubeFaceStates[12] == cubeFaceStates[13] && cubeFaceStates[14] == cubeFaceStates[13] && cubeFaceStates[21] == cubeFaceStates[22])
                {
                    middleLayerSolved = true;
                    break;
                }

                //Check if no edge pieces can be swapped and, if so, swap an unsuitable piece from the bottom to prevent an infinite loop
                if ((cubeFaceStates[52] == bottomLayerColor || cubeFaceStates[7] == bottomLayerColor) && (cubeFaceStates[48] == bottomLayerColor || cubeFaceStates[25] == bottomLayerColor) &&
                    (cubeFaceStates[50] == bottomLayerColor || cubeFaceStates[34] == bottomLayerColor) && (cubeFaceStates[46] == bottomLayerColor || cubeFaceStates[16] == bottomLayerColor))
                {
                    bottomCCW();
                    backCW();
                    rightCCW();
                    zMiddleCCW();
                    rightCW();
                    frontCCW();
                    rightCW();
                    zMiddleCW();
                    rightCCW();
                    frontCW();
                    backCCW();
                }

                //Check if any edges in the middle layer need to swap positions to prevent an infinite loop
                if (cubeFaceStates[23] == cubeFaceStates[4] && cubeFaceStates[3] == cubeFaceStates[22])
                {
                    bottomCW();
                    backCCW();
                    leftCW();
                    zMiddleCW();
                    leftCCW();
                    frontCW();
                    leftCW();
                    zMiddleCCW();
                    leftCCW();
                    frontCCW();
                    backCW();
                }
                else if (cubeFaceStates[5] == cubeFaceStates[31] && cubeFaceStates[30] == cubeFaceStates[4])
                {
                    bottomCCW();
                    backCW();
                    rightCCW();
                    zMiddleCCW();
                    rightCW();
                    frontCCW();
                    rightCW();
                    zMiddleCW();
                    rightCCW();
                    frontCW();
                    backCCW();
                }
                else if (cubeFaceStates[32] == cubeFaceStates[13] && cubeFaceStates[12] == cubeFaceStates[31])
                {
                    bottomCW();
                    frontCCW();
                    rightCW();
                    zMiddleCCW();
                    rightCCW();
                    backCW();
                    rightCW();
                    zMiddleCW();
                    rightCCW();
                    backCCW();
                    frontCW();
                }
                else if (cubeFaceStates[21] == cubeFaceStates[13] && cubeFaceStates[14] == cubeFaceStates[22])
                {
                    bottomCCW();
                    frontCW();
                    leftCCW();
                    zMiddleCW();
                    leftCW();
                    backCCW();
                    leftCW();
                    zMiddleCCW();
                    leftCCW();
                    backCW();
                    frontCCW();
                }
                else if (((cubeFaceStates[23] == cubeFaceStates[31] && cubeFaceStates[3] == cubeFaceStates[4]) || (cubeFaceStates[3] == cubeFaceStates[31] && cubeFaceStates[23] == cubeFaceStates[4]))
                    && ((cubeFaceStates[30] == cubeFaceStates[22] && cubeFaceStates[5] == cubeFaceStates[4]) || (cubeFaceStates[5] == cubeFaceStates[22] && cubeFaceStates[30] == cubeFaceStates[4])))
                {
                    bottomCW();
                    backCCW();
                    leftCW();
                    zMiddleCW();
                    leftCCW();
                    frontCW();
                    leftCW();
                    zMiddleCCW();
                    leftCCW();
                    frontCCW();
                    backCW();
                }
                else if (((cubeFaceStates[23] == cubeFaceStates[13] && cubeFaceStates[3] == cubeFaceStates[22]) || (cubeFaceStates[3] == cubeFaceStates[13] && cubeFaceStates[23] == cubeFaceStates[22]))
                    && ((cubeFaceStates[21] == cubeFaceStates[4] && cubeFaceStates[14] == cubeFaceStates[22]) || (cubeFaceStates[14] == cubeFaceStates[22] && cubeFaceStates[21] == cubeFaceStates[22])))
                {
                    bottomCCW();
                    rightCW();
                    frontCCW();
                    xMiddleCCW();
                    frontCW();
                    leftCCW();
                    frontCW();
                    xMiddleCW();
                    frontCCW();
                    leftCW();
                    rightCCW();
                }
                else if (((cubeFaceStates[21] == cubeFaceStates[13] && cubeFaceStates[14] == cubeFaceStates[31]) || (cubeFaceStates[14] == cubeFaceStates[13] && cubeFaceStates[21] == cubeFaceStates[31]))
                    && ((cubeFaceStates[12] == cubeFaceStates[22] && cubeFaceStates[32] == cubeFaceStates[13]) || (cubeFaceStates[32] == cubeFaceStates[22] && cubeFaceStates[12] == cubeFaceStates[13])))
                {
                    bottomCCW();
                    frontCW();
                    leftCCW();
                    zMiddleCW();
                    leftCW();
                    backCCW();
                    leftCW();
                    zMiddleCCW();
                    leftCCW();
                    backCW();
                    frontCCW();
                }
                else if (((cubeFaceStates[32] == cubeFaceStates[31] && cubeFaceStates[12] == cubeFaceStates[4]) || (cubeFaceStates[12] == cubeFaceStates[31] && cubeFaceStates[32] == cubeFaceStates[4]))
                    && ((cubeFaceStates[30] == cubeFaceStates[31] && cubeFaceStates[5] == cubeFaceStates[13]) || (cubeFaceStates[5] == cubeFaceStates[31] && cubeFaceStates[30] == cubeFaceStates[13])))
                {
                    bottomCW();
                    leftCCW();
                    frontCW();
                    xMiddleCCW();
                    frontCCW();
                    rightCW();
                    frontCW();
                    xMiddleCW();
                    frontCCW();
                    rightCCW();
                    leftCW();
                }
                else if (((cubeFaceStates[14] == cubeFaceStates[4] && cubeFaceStates[21] == cubeFaceStates[31]) || (cubeFaceStates[21] == cubeFaceStates[4] && cubeFaceStates[14] == cubeFaceStates[31]))
                    && ((cubeFaceStates[30] == cubeFaceStates[13] && cubeFaceStates[5] == cubeFaceStates[22]) || (cubeFaceStates[5] == cubeFaceStates[13] && cubeFaceStates[30] == cubeFaceStates[22])))
                {
                    bottomCCW();
                    frontCW();
                    leftCCW();
                    zMiddleCW();
                    leftCW();
                    backCCW();
                    leftCW();
                    zMiddleCCW();
                    leftCCW();
                    backCW();
                    frontCCW();
                }
                else if (((cubeFaceStates[12] == cubeFaceStates[4] && cubeFaceStates[32] == cubeFaceStates[22]) || (cubeFaceStates[32] == cubeFaceStates[4] && cubeFaceStates[12] == cubeFaceStates[22]))
                    && ((cubeFaceStates[23] == cubeFaceStates[13] && cubeFaceStates[3] == cubeFaceStates[31]) || (cubeFaceStates[3] == cubeFaceStates[13] && cubeFaceStates[23] == cubeFaceStates[31])))
                {
                    bottomCW();
                    frontCCW();
                    rightCW();
                    zMiddleCCW();
                    rightCCW();
                    backCW();
                    rightCW();
                    zMiddleCW();
                    rightCCW();
                    backCCW();
                    frontCW();
                }

                //Find edge pieces that belong in the middle layer, then move them into position
                for (int e = 0; e < 4; e++)
                {
                    bottomLayerEdges = new string[4, 2] { {cubeFaceStates[52], cubeFaceStates[7]}, {cubeFaceStates[48], cubeFaceStates[25]}, 
                            {cubeFaceStates[50], cubeFaceStates[34]}, {cubeFaceStates[46], cubeFaceStates[16]} };

                    for (int c = 0; c < 2; c++)
                    {
                        Render();
                        if (bottomLayerEdges[e, c] == bottomLayerColor)
                        {
                            break;
                        }
                        else if (c == 1)
                        {
                            string edgeColor1 = bottomLayerEdges[e, 0];
                            string edgeColor2 = bottomLayerEdges[e, 1];

                            switch (e)
                            {
                                case 0:
                                    if (edgeColor2 == cubeFaceStates[4])
                                    {
                                        if (edgeColor1 == cubeFaceStates[31])
                                        {
                                            bottomCCW();
                                            backCW();
                                            rightCCW();
                                            zMiddleCCW();
                                            rightCW();
                                            frontCCW();
                                            rightCW();
                                            zMiddleCW();
                                            rightCCW();
                                            frontCW();
                                            backCCW();
                                        }
                                        else
                                        {
                                            bottomCW();
                                            backCCW();
                                            leftCW();
                                            zMiddleCW();
                                            leftCCW();
                                            frontCW();
                                            leftCW();
                                            zMiddleCCW();
                                            leftCCW();
                                            frontCCW();
                                            backCW();
                                        }
                                    }
                                    else if (edgeColor2 == cubeFaceStates[22])
                                    {
                                        bottomCCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[31])
                                    {
                                        bottomCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[13])
                                    {
                                        bottomCW();
                                        bottomCW();
                                    }
                                    break;
                                case 1:
                                    if (edgeColor2 == cubeFaceStates[22])
                                    {
                                        if (edgeColor1 == cubeFaceStates[4])
                                        {
                                            bottomCCW();
                                            rightCW();
                                            frontCCW();
                                            xMiddleCCW();
                                            frontCW();
                                            leftCCW();
                                            frontCW();
                                            xMiddleCW();
                                            frontCCW();
                                            leftCW();
                                            rightCCW();
                                        }
                                        else
                                        {
                                            bottomCW();
                                            rightCCW();
                                            backCW();
                                            xMiddleCW();
                                            backCCW();
                                            leftCW();
                                            backCW();
                                            xMiddleCCW();
                                            backCCW();
                                            leftCCW();
                                            rightCW();
                                        }
                                    }
                                    else if (edgeColor2 == cubeFaceStates[13])
                                    {
                                        bottomCCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[4])
                                    {
                                        bottomCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[31])
                                    {
                                        bottomCW();
                                        bottomCW();
                                    }
                                    break;
                                case 2:
                                    if (edgeColor2 == cubeFaceStates[31])
                                    {
                                        if (edgeColor1 == cubeFaceStates[13])
                                        {
                                            bottomCCW();
                                            leftCW();
                                            backCCW();
                                            xMiddleCW();
                                            backCW();
                                            rightCCW();
                                            backCW();
                                            xMiddleCCW();
                                            backCCW();
                                            rightCW();
                                            leftCCW();
                                        }
                                        else
                                        {
                                            bottomCW();
                                            leftCCW();
                                            frontCW();
                                            xMiddleCCW();
                                            frontCCW();
                                            rightCW();
                                            frontCW();
                                            xMiddleCW();
                                            frontCCW();
                                            rightCCW();
                                            leftCW();
                                        }
                                    }
                                    else if (edgeColor2 == cubeFaceStates[4])
                                    {
                                        bottomCCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[13])
                                    {
                                        bottomCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[22])
                                    {
                                        bottomCW();
                                        bottomCW();
                                    }
                                    break;
                                case 3:
                                    if (edgeColor2 == cubeFaceStates[13])
                                    {
                                        if (edgeColor1 == cubeFaceStates[22])
                                        {
                                            bottomCCW();
                                            frontCW();
                                            leftCCW();
                                            zMiddleCW();
                                            leftCW();
                                            backCCW();
                                            leftCW();
                                            zMiddleCCW();
                                            leftCCW();
                                            backCW();
                                            frontCCW();
                                        }
                                        else
                                        {
                                            bottomCW();
                                            frontCCW();
                                            rightCW();
                                            zMiddleCCW();
                                            rightCCW();
                                            backCW();
                                            rightCW();
                                            zMiddleCW();
                                            rightCCW();
                                            backCCW();
                                            frontCW();
                                        }
                                    }
                                    else if (edgeColor2 == cubeFaceStates[31])
                                    {
                                        bottomCCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[22])
                                    {
                                        bottomCW();
                                    }
                                    else if (edgeColor2 == cubeFaceStates[4])
                                    {
                                        bottomCW();
                                        bottomCW();
                                    }
                                    break;
                            }
                            updateProgressBar();
                        }
                    }
                }
                updateProgressBar();
                middleLayerRepCount++;
                if (middleLayerRepCount > 20)
                {
                    MessageBox.Show("Something went wrong when trying to solve the cube, sorry.", "Error");
                    resetFacesToolStripMenuItem.PerformClick();
                    moveListBox.Items.Clear();
                    moveListBoxLabel.Text = "Moves";
                    return;
                }
            }

            //The middle layer is complete, begin solving the bottom layer.

            //Solve the top faces of the edges of the bottom layer, creating a "cross"
            bool bottomCrossSolved = false;
            while (!bottomCrossSolved)
            {
                Render();
                string bottomLayerColor = cubeFaceStates[49];
                if (cubeFaceStates[52] == bottomLayerColor && cubeFaceStates[48] == bottomLayerColor && cubeFaceStates[50] == bottomLayerColor && cubeFaceStates[46] == bottomLayerColor)
                {
                    bottomCrossSolved = true;
                    break;
                }
                //Check for "L" shapes formed by the top faces of bottom layer edges
                else if (cubeFaceStates[52] == bottomLayerColor && cubeFaceStates[48] == bottomLayerColor)
                {
                    backCW();
                    bottomCW();
                    rightCW();
                    bottomCCW();
                    rightCCW();
                    backCCW();
                }
                else if (cubeFaceStates[52] == bottomLayerColor && cubeFaceStates[50] == bottomLayerColor)
                {
                    leftCW();
                    bottomCW();
                    backCW();
                    bottomCCW();
                    backCCW();
                    leftCCW();
                }
                else if (cubeFaceStates[48] == bottomLayerColor && cubeFaceStates[46] == bottomLayerColor)
                {
                    rightCW();
                    bottomCW();
                    frontCW();
                    bottomCCW();
                    frontCCW();
                    rightCCW();
                }
                //Check for lines formed by the top faces of bottom layer edges
                else if (cubeFaceStates[48] == bottomLayerColor && cubeFaceStates[50] == bottomLayerColor)
                {
                    frontCW();
                    leftCW();
                    bottomCW();
                    leftCCW();
                    bottomCCW();
                    frontCCW();
                }
                else if (cubeFaceStates[52] == bottomLayerColor && cubeFaceStates[46] == bottomLayerColor)
                {
                    rightCW();
                    frontCW();
                    bottomCW();
                    frontCCW();
                    bottomCCW();
                    rightCCW();
                }
                //Check if none of the top faces of the edges are correct or only a single top face is correct
                else if ((cubeFaceStates[52] != bottomLayerColor && cubeFaceStates[46] != bottomLayerColor && ((cubeFaceStates[50] != bottomLayerColor) != (cubeFaceStates[48] != bottomLayerColor))) ||
                    (cubeFaceStates[50] != bottomLayerColor && cubeFaceStates[48] != bottomLayerColor && ((cubeFaceStates[52] != bottomLayerColor) != (cubeFaceStates[46] != bottomLayerColor))) ||
                    (cubeFaceStates[52] != bottomLayerColor && cubeFaceStates[46] != bottomLayerColor && cubeFaceStates[50] != bottomLayerColor && cubeFaceStates[48] != bottomLayerColor) ||
                    (cubeFaceStates[50] == bottomLayerColor && cubeFaceStates[46] == bottomLayerColor))
                {
                    //MessageBox.Show(".");
                    frontCW();
                    bottomCW();
                    leftCW();
                    bottomCCW();
                    leftCCW();
                    frontCCW();
                }
                updateProgressBar();
            }

            //Solve the top faces of the corners of the bottom layer
            bool bottomCornerFacesSolved = false;
            while (!bottomCornerFacesSolved)
            {
                Render();
                string bottomLayerColor = cubeFaceStates[49];
                if (cubeFaceStates[51] == bottomLayerColor && cubeFaceStates[53] == bottomLayerColor && cubeFaceStates[45] == bottomLayerColor && cubeFaceStates[47] == bottomLayerColor)
                {
                    bottomCornerFacesSolved = true;
                    break;
                }

                //Check if none of the top faces of the corners of the bottom face are correct
                if (cubeFaceStates[51] != bottomLayerColor && cubeFaceStates[53] != bottomLayerColor && cubeFaceStates[45] != bottomLayerColor && cubeFaceStates[47] != bottomLayerColor)
                {
                    if (cubeFaceStates[15] == bottomLayerColor)
                    {
                        bottomCCW();
                    }
                    else if (cubeFaceStates[6] == bottomLayerColor)
                    {
                        bottomCW();
                    }
                    else if (cubeFaceStates[24] == bottomLayerColor)
                    {
                        bottomCW();
                        bottomCW();
                    }
                }

                //Check if only one of the top faces of the corners of the bottom face is correct
                if ((cubeFaceStates[51] == bottomLayerColor && cubeFaceStates[45] != bottomLayerColor && cubeFaceStates[47] != bottomLayerColor && cubeFaceStates[53] != bottomLayerColor) ||
                    (cubeFaceStates[45] == bottomLayerColor && cubeFaceStates[51] != bottomLayerColor && cubeFaceStates[47] != bottomLayerColor && cubeFaceStates[53] != bottomLayerColor) ||
                    (cubeFaceStates[47] == bottomLayerColor && cubeFaceStates[45] != bottomLayerColor && cubeFaceStates[51] != bottomLayerColor && cubeFaceStates[53] != bottomLayerColor) ||
                    (cubeFaceStates[53] == bottomLayerColor && cubeFaceStates[45] != bottomLayerColor && cubeFaceStates[47] != bottomLayerColor && cubeFaceStates[51] != bottomLayerColor))
                {
                    if (cubeFaceStates[47] == bottomLayerColor)
                    {
                        bottomCCW();
                    }
                    else if (cubeFaceStates[45] == bottomLayerColor)
                    {
                        bottomCCW();
                        bottomCCW();
                    }
                    else if (cubeFaceStates[51] == bottomLayerColor)
                    {
                        bottomCW();
                    }
                }

                //Check if any two top faces of the corners of the bottom face are correct
                if ((cubeFaceStates[51] == bottomLayerColor && cubeFaceStates[45] == bottomLayerColor) ||
                    (cubeFaceStates[51] == bottomLayerColor && cubeFaceStates[53] == bottomLayerColor) ||
                    (cubeFaceStates[51] == bottomLayerColor && cubeFaceStates[47] == bottomLayerColor) ||
                    (cubeFaceStates[47] == bottomLayerColor && cubeFaceStates[45] == bottomLayerColor) ||
                    (cubeFaceStates[47] == bottomLayerColor && cubeFaceStates[53] == bottomLayerColor) ||
                    (cubeFaceStates[53] == bottomLayerColor && cubeFaceStates[45] == bottomLayerColor))
                {
                    if (cubeFaceStates[17] == bottomLayerColor)
                    {
                        bottomCW();
                        bottomCW();
                    }
                    else if (cubeFaceStates[26] == bottomLayerColor)
                    {
                        bottomCW();
                    }
                    else if (cubeFaceStates[35] == bottomLayerColor)
                    {
                        bottomCCW();
                    }
                }

                if ((cubeFaceStates[8] == bottomLayerColor) || (cubeFaceStates[33] == bottomLayerColor) || (cubeFaceStates[53] == bottomLayerColor))
                {
                    leftCW();
                    bottomCW();
                    leftCCW();
                    bottomCW();
                    leftCW();
                    bottomCW();
                    bottomCW();
                    leftCCW();
                }
                updateProgressBar();
            }

            //Bottom face is correct, the bottom layer must now be solved
            bool bottomCornersSolved = false;
            while (!bottomCornersSolved)
            {
                Render();
                if (cubeFaceStates[24] == cubeFaceStates[22] && cubeFaceStates[17] == cubeFaceStates[13] && cubeFaceStates[15] == cubeFaceStates[13] && cubeFaceStates[35] == cubeFaceStates[31] && 
                    cubeFaceStates[33] == cubeFaceStates[31] && cubeFaceStates[8] == cubeFaceStates[4])
                {
                    bottomCornersSolved = true;
                    break;
                }

                if ((cubeFaceStates[24] == cubeFaceStates[22] && cubeFaceStates[17] == cubeFaceStates[13] && cubeFaceStates[15] == cubeFaceStates[13]) ||
                    (cubeFaceStates[24] == cubeFaceStates[22] && cubeFaceStates[17] == cubeFaceStates[13] && cubeFaceStates[33] == cubeFaceStates[31] && cubeFaceStates[8] == cubeFaceStates[4])
                    || (cubeFaceStates[15] == cubeFaceStates[13] && cubeFaceStates[35] == cubeFaceStates[31] && cubeFaceStates[6] == cubeFaceStates[4] && cubeFaceStates[26] == cubeFaceStates[22]))
                {
                    leftCCW();
                    frontCW();
                    leftCCW();
                    backCW();
                    backCW();
                    leftCW();
                    frontCCW();
                    leftCCW();
                    backCW();
                    backCW();
                    leftCW();
                    leftCW();
                    bottomCCW();
                }
                else if ((cubeFaceStates[6] == cubeFaceStates[4] && cubeFaceStates[26] == cubeFaceStates[22] && cubeFaceStates[24] == cubeFaceStates[22] && cubeFaceStates[17] == cubeFaceStates[13]))
                {
                    frontCCW();
                    rightCW();
                    frontCCW();
                    leftCW();
                    leftCW();
                    frontCW();
                    rightCCW();
                    frontCCW();
                    leftCW();
                    leftCW();
                    frontCW();
                    frontCW();
                    bottomCCW();
                }
                else if ((cubeFaceStates[26] == cubeFaceStates[22] && cubeFaceStates[6] == cubeFaceStates[4] && cubeFaceStates[33] == cubeFaceStates[31] && cubeFaceStates[8] == cubeFaceStates[4]))
                {
                    rightCCW();
                    backCW();
                    rightCCW();
                    frontCW();
                    frontCW();
                    rightCW();
                    backCCW();
                    rightCCW();
                    frontCW();
                    frontCW();
                    rightCW();
                    rightCW();
                    bottomCCW();
                }
                else if ((cubeFaceStates[8] == cubeFaceStates[4] && cubeFaceStates[33] == cubeFaceStates[31] && cubeFaceStates[15] == cubeFaceStates[13] && cubeFaceStates[35] == cubeFaceStates[31]))
                {
                    backCCW();
                    leftCW();
                    backCCW();
                    rightCW();
                    rightCW();
                    backCW();
                    leftCCW();
                    backCCW();
                    rightCW();
                    rightCW();
                    backCW();
                    backCW();
                    bottomCCW();
                }
                else
                {
                    bottomCW();
                }
                updateProgressBar();
            }

            //Solve the top layer completely by placing the edges of the bottom layer into position
            bool topLayerSolved = false;
            while (!topLayerSolved)
            {
                Render();
                if (cubeFaceStates[16] == cubeFaceStates[13] && cubeFaceStates[25] == cubeFaceStates[22] && cubeFaceStates[34] == cubeFaceStates[31])
                {
                    topLayerSolved = true;
                    break;
                }

                //Check if all edges of the bottom layer are incorrect
                if (cubeFaceStates[16] != cubeFaceStates[13] && cubeFaceStates[25] != cubeFaceStates[22] && cubeFaceStates[7] != cubeFaceStates[4] && cubeFaceStates[34] != cubeFaceStates[31])
                {
                    frontCW();
                    frontCW();
                    bottomCCW();
                    rightCW();
                    leftCCW();
                    frontCW();
                    frontCW();
                    rightCCW();
                    leftCW();
                    bottomCCW();
                    frontCW();
                    frontCW();
                }

                if (cubeFaceStates[16] == cubeFaceStates[13])
                {
                    if (cubeFaceStates[25] == cubeFaceStates[13])
                    {
                        frontCW();
                        frontCW();
                        bottomCCW();
                        rightCW();
                        leftCCW();
                        frontCW();
                        frontCW();
                        rightCCW();
                        leftCW();
                        bottomCCW();
                        frontCW();
                        frontCW();
                    }
                    else
                    {
                        frontCW();
                        frontCW();
                        bottomCW();
                        rightCW();
                        leftCCW();
                        frontCW();
                        frontCW();
                        rightCCW();
                        leftCW();
                        bottomCW();
                        frontCW();
                        frontCW();
                    }
                }
                else if (cubeFaceStates[25] == cubeFaceStates[22])
                {
                    if (cubeFaceStates[16] == cubeFaceStates[31])
                    {
                        rightCW();
                        rightCW();
                        bottomCCW();
                        backCW();
                        frontCCW();
                        rightCW();
                        rightCW();
                        backCCW();
                        frontCW();
                        bottomCCW();
                        rightCW();
                        rightCW();
                    }
                    else
                    {
                        rightCW();
                        rightCW();
                        bottomCW();
                        backCW();
                        frontCCW();
                        rightCW();
                        rightCW();
                        backCCW();
                        frontCW();
                        bottomCW();
                        rightCW();
                        rightCW();
                    }
                }
                else if (cubeFaceStates[7] == cubeFaceStates[4])
                {
                    if (cubeFaceStates[34] == cubeFaceStates[13])
                    {
                        backCW();
                        backCW();
                        bottomCW();
                        leftCW();
                        rightCCW();
                        backCW();
                        backCW();
                        leftCCW();
                        rightCW();
                        bottomCW();
                        backCW();
                        backCW();
                    }
                    else
                    {
                        backCW();
                        backCW();
                        bottomCCW();
                        leftCW();
                        rightCCW();
                        backCW();
                        backCW();
                        leftCCW();
                        rightCW();
                        bottomCCW();
                        backCW();
                        backCW();
                    }
                }
                else if (cubeFaceStates[34] == cubeFaceStates[31])
                {
                    if (cubeFaceStates[43] == cubeFaceStates[33])
                    {
                        leftCW();
                        leftCW();
                        bottomCCW();
                        frontCW();
                        backCCW();
                        leftCW();
                        leftCW();
                        frontCCW();
                        backCW();
                        bottomCCW();
                        leftCW();
                        leftCW();
                    }
                    else
                    {
                        leftCW();
                        leftCW();
                        bottomCW();
                        frontCW();
                        backCCW();
                        leftCW();
                        leftCW();
                        frontCCW();
                        backCW();
                        bottomCW();
                        leftCW();
                        leftCW();
                    }
                }
                updateProgressBar();
            }
            updateProgressBar();
        }

        public void updateProgressBar()
        {
            //Calculate the exact percentage completion of the cube, then display this percentage on the solutionProgressBar
            int completionPercentage = 0;

            for (int i = 0; i < 52; i++)
            {
                solvedCubeState[i] = cubeFaceStates[((i / 9) * 9) + 4];
                if (solvedCubeState[i] == cubeFaceStates[i])
                {
                    completionPercentage++;
                }
            }

            solutionProgressBar.Value = (int)(completionPercentage / 52.0 * 100);
            solutionProgressBar.Refresh();
            cubeDisplay.Refresh();
        }
        #endregion

        #region Auto-Scramble
        public void Scramble()
        {
            Random randomRotation = new Random();
            //Define an integer of random value between 40 and 65
            //This determines the number of moves in the scrambling process
            int randomisationDuration = randomRotation.Next(40, 65);
            //BeginUpdate ensures that the moveListBox will not show the moves made during scrambling
            moveListBox.BeginUpdate();
            //Executes a random move from any of the 18 possible moves for each integer below randomisationDuration
            for (int i = 0; i < randomisationDuration; i++)
            {
                switch (randomRotation.Next(0, 18))
                {
                    case 0:
                        frontCW();
                        break;

                    case 1:
                        frontCCW();
                        break;

                    case 2:
                        zMiddleCW();
                        break;

                    case 3:
                        zMiddleCCW();
                        break;

                    case 4:
                        backCW();
                        break;

                    case 5:
                        backCCW();
                        break;

                    case 6:
                        leftCW();
                        break;

                    case 7:
                        leftCCW();
                        break;

                    case 8:
                        xMiddleCW();
                        break;

                    case 9:
                        xMiddleCCW();
                        break;

                    case 10:
                        rightCW();
                        break;

                    case 11:
                        rightCCW();
                        break;

                    case 12:
                        topCW();
                        break;

                    case 13:
                        topCCW();
                        break;

                    case 14:
                        yMiddleCW();
                        break;

                    case 15:
                        yMiddleCCW();
                        break;

                    case 16:
                        bottomCW();
                        break;

                    case 17:
                        bottomCCW();
                        break;
                }
            }
            Render();
            clearMovesButton.Visible = false;
            moveListBox.Items.Clear();
            moveListBoxLabel.Text = "Moves";
            moveListBox.EndUpdate();
            //Cube is now scrambled, moveListBox is now allowed to update, ensuring that none of the moves made are displayed
        }
        #endregion

        #region Control Panel

        #region Layer Rotation Methods
        //Methods to rotate each layer of the cube along each axis clockwise or counter-clockwise
        private void frontCW()
        {
            string tempColor1 = cubeFaceStates[42];
            string tempColor2 = cubeFaceStates[43];
            string tempColor3 = cubeFaceStates[44];
            string tempColor4 = cubeFaceStates[0];
            string tempColor5 = cubeFaceStates[1];

            //Top face
            cubeFaceStates[44] = cubeFaceStates[20];
            cubeFaceStates[43] = cubeFaceStates[23];
            cubeFaceStates[42] = cubeFaceStates[26];

            //Left face
            cubeFaceStates[20] = cubeFaceStates[51];
            cubeFaceStates[23] = cubeFaceStates[52];
            cubeFaceStates[26] = cubeFaceStates[53];

            //Bottom face
            cubeFaceStates[51] = cubeFaceStates[33];
            cubeFaceStates[52] = cubeFaceStates[30];
            cubeFaceStates[53] = cubeFaceStates[27];

            //Right face
            cubeFaceStates[33] = tempColor3;
            cubeFaceStates[30] = tempColor2;
            cubeFaceStates[27] = tempColor1;

            //Front face
            cubeFaceStates[0] = cubeFaceStates[6];
            cubeFaceStates[6] = cubeFaceStates[8];
            cubeFaceStates[8] = cubeFaceStates[2];
            cubeFaceStates[2] = tempColor4;
            cubeFaceStates[1] = cubeFaceStates[3];
            cubeFaceStates[3] = cubeFaceStates[7];
            cubeFaceStates[7] = cubeFaceStates[5];
            cubeFaceStates[5] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Front Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void frontCCW()
        {
            string tempColor1 = cubeFaceStates[42];
            string tempColor2 = cubeFaceStates[43];
            string tempColor3 = cubeFaceStates[44];
            string tempColor4 = cubeFaceStates[0];
            string tempColor5 = cubeFaceStates[1];

            //Top face
            cubeFaceStates[44] = cubeFaceStates[33];
            cubeFaceStates[43] = cubeFaceStates[30];
            cubeFaceStates[42] = cubeFaceStates[27];

            //Right face
            cubeFaceStates[27] = cubeFaceStates[53];
            cubeFaceStates[30] = cubeFaceStates[52];
            cubeFaceStates[33] = cubeFaceStates[51];

            //Bottom face
            cubeFaceStates[51] = cubeFaceStates[20];
            cubeFaceStates[52] = cubeFaceStates[23];
            cubeFaceStates[53] = cubeFaceStates[26];

            //Left face
            cubeFaceStates[26] = tempColor1;
            cubeFaceStates[23] = tempColor2;
            cubeFaceStates[20] = tempColor3;

            //Front face
            cubeFaceStates[0] = cubeFaceStates[2];
            cubeFaceStates[1] = cubeFaceStates[5];
            cubeFaceStates[2] = cubeFaceStates[8];
            cubeFaceStates[5] = cubeFaceStates[7];
            cubeFaceStates[7] = cubeFaceStates[3];
            cubeFaceStates[3] = tempColor5;
            cubeFaceStates[8] = cubeFaceStates[6];
            cubeFaceStates[6] = tempColor4;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Front Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void zMiddleCW()
        {
            string tempColor1 = cubeFaceStates[39];
            string tempColor2 = cubeFaceStates[40];
            string tempColor3 = cubeFaceStates[41];

            //Top face
            cubeFaceStates[41] = cubeFaceStates[19];
            cubeFaceStates[40] = cubeFaceStates[22];
            cubeFaceStates[39] = cubeFaceStates[25];

            //Left face
            cubeFaceStates[19] = cubeFaceStates[48];
            cubeFaceStates[22] = cubeFaceStates[49];
            cubeFaceStates[25] = cubeFaceStates[50];

            //Bottom face
            cubeFaceStates[48] = cubeFaceStates[34];
            cubeFaceStates[49] = cubeFaceStates[31];
            cubeFaceStates[50] = cubeFaceStates[28];

            //Right face
            cubeFaceStates[34] = tempColor3;
            cubeFaceStates[31] = tempColor2;
            cubeFaceStates[28] = tempColor1;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Z Middle Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void zMiddleCCW()
        {
            string tempColor1 = cubeFaceStates[39];
            string tempColor2 = cubeFaceStates[40];
            string tempColor3 = cubeFaceStates[41];

            //Top face
            cubeFaceStates[41] = cubeFaceStates[34];
            cubeFaceStates[40] = cubeFaceStates[31];
            cubeFaceStates[39] = cubeFaceStates[28];

            //Right face
            cubeFaceStates[28] = cubeFaceStates[50];
            cubeFaceStates[31] = cubeFaceStates[49];
            cubeFaceStates[34] = cubeFaceStates[48];

            //Bottom face
            cubeFaceStates[48] = cubeFaceStates[19];
            cubeFaceStates[49] = cubeFaceStates[22];
            cubeFaceStates[50] = cubeFaceStates[25];

            //Left face
            cubeFaceStates[25] = tempColor1;
            cubeFaceStates[22] = tempColor2;
            cubeFaceStates[19] = tempColor3;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Z Middle Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void backCW()
        {
            string tempColor1 = cubeFaceStates[36];
            string tempColor2 = cubeFaceStates[37];
            string tempColor3 = cubeFaceStates[38];
            string tempColor4 = cubeFaceStates[9];
            string tempColor5 = cubeFaceStates[10];

            //Top face
            cubeFaceStates[38] = cubeFaceStates[35];
            cubeFaceStates[37] = cubeFaceStates[32];
            cubeFaceStates[36] = cubeFaceStates[29];

            //Right face
            cubeFaceStates[35] = cubeFaceStates[45];
            cubeFaceStates[32] = cubeFaceStates[46];
            cubeFaceStates[29] = cubeFaceStates[47];

            //Bottom face
            cubeFaceStates[45] = cubeFaceStates[18];
            cubeFaceStates[46] = cubeFaceStates[21];
            cubeFaceStates[47] = cubeFaceStates[24];

            //Left face
            cubeFaceStates[18] = tempColor3;
            cubeFaceStates[21] = tempColor2;
            cubeFaceStates[24] = tempColor1;

            //Back face
            cubeFaceStates[9] = cubeFaceStates[15];
            cubeFaceStates[15] = cubeFaceStates[17];
            cubeFaceStates[17] = cubeFaceStates[11];
            cubeFaceStates[11] = tempColor4;
            cubeFaceStates[10] = cubeFaceStates[12];
            cubeFaceStates[12] = cubeFaceStates[16];
            cubeFaceStates[16] = cubeFaceStates[14];
            cubeFaceStates[14] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Back Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void backCCW()
        {
            string tempColor1 = cubeFaceStates[36];
            string tempColor2 = cubeFaceStates[37];
            string tempColor3 = cubeFaceStates[38];
            string tempColor4 = cubeFaceStates[9];
            string tempColor5 = cubeFaceStates[10];

            //Top face
            cubeFaceStates[38] = cubeFaceStates[18];
            cubeFaceStates[37] = cubeFaceStates[21];
            cubeFaceStates[36] = cubeFaceStates[24];

            //Left face
            cubeFaceStates[18] = cubeFaceStates[45];
            cubeFaceStates[21] = cubeFaceStates[46];
            cubeFaceStates[24] = cubeFaceStates[47];

            //Bottom face
            cubeFaceStates[45] = cubeFaceStates[35];
            cubeFaceStates[46] = cubeFaceStates[32];
            cubeFaceStates[47] = cubeFaceStates[29];

            //Right face
            cubeFaceStates[29] = tempColor1;
            cubeFaceStates[32] = tempColor2;
            cubeFaceStates[35] = tempColor3;

            //Back face
            cubeFaceStates[9] = cubeFaceStates[11];
            cubeFaceStates[11] = cubeFaceStates[17];
            cubeFaceStates[17] = cubeFaceStates[15];
            cubeFaceStates[15] = tempColor4;
            cubeFaceStates[10] = cubeFaceStates[14];
            cubeFaceStates[14] = cubeFaceStates[16];
            cubeFaceStates[16] = cubeFaceStates[12];
            cubeFaceStates[12] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Back Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void leftCW()
        {
            string tempColor1 = cubeFaceStates[36];
            string tempColor2 = cubeFaceStates[39];
            string tempColor3 = cubeFaceStates[42];
            string tempColor4 = cubeFaceStates[18];
            string tempColor5 = cubeFaceStates[19];

            //Top face
            cubeFaceStates[36] = cubeFaceStates[17];
            cubeFaceStates[39] = cubeFaceStates[14];
            cubeFaceStates[42] = cubeFaceStates[11];

            //Back face
            cubeFaceStates[11] = cubeFaceStates[45];
            cubeFaceStates[14] = cubeFaceStates[48];
            cubeFaceStates[17] = cubeFaceStates[51];

            //Bottom face
            cubeFaceStates[51] = cubeFaceStates[0];
            cubeFaceStates[48] = cubeFaceStates[3];
            cubeFaceStates[45] = cubeFaceStates[6];

            //Front face
            cubeFaceStates[0] = tempColor1;
            cubeFaceStates[3] = tempColor2;
            cubeFaceStates[6] = tempColor3;

            //Left face
            cubeFaceStates[18] = cubeFaceStates[24];
            cubeFaceStates[19] = cubeFaceStates[21];
            cubeFaceStates[24] = cubeFaceStates[26];
            cubeFaceStates[21] = cubeFaceStates[25];
            cubeFaceStates[26] = cubeFaceStates[20];
            cubeFaceStates[25] = cubeFaceStates[23];
            cubeFaceStates[20] = tempColor4;
            cubeFaceStates[23] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Left Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void leftCCW()
        {
            string tempColor1 = cubeFaceStates[36];
            string tempColor2 = cubeFaceStates[39];
            string tempColor3 = cubeFaceStates[42];
            string tempColor4 = cubeFaceStates[18];
            string tempColor5 = cubeFaceStates[19];

            //Top face
            cubeFaceStates[36] = cubeFaceStates[0];
            cubeFaceStates[39] = cubeFaceStates[3];
            cubeFaceStates[42] = cubeFaceStates[6];

            //Front face
            cubeFaceStates[0] = cubeFaceStates[51];
            cubeFaceStates[3] = cubeFaceStates[48];
            cubeFaceStates[6] = cubeFaceStates[45];

            //Bottom face
            cubeFaceStates[51] = cubeFaceStates[17];
            cubeFaceStates[48] = cubeFaceStates[14];
            cubeFaceStates[45] = cubeFaceStates[11];

            //Back face
            cubeFaceStates[11] = tempColor3;
            cubeFaceStates[14] = tempColor2;
            cubeFaceStates[17] = tempColor1;

            //Left face
            cubeFaceStates[18] = cubeFaceStates[20];
            cubeFaceStates[19] = cubeFaceStates[23];
            cubeFaceStates[20] = cubeFaceStates[26];
            cubeFaceStates[23] = cubeFaceStates[25];
            cubeFaceStates[26] = cubeFaceStates[24];
            cubeFaceStates[25] = cubeFaceStates[21];
            cubeFaceStates[24] = tempColor4;
            cubeFaceStates[21] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Left Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void xMiddleCW()
        {
            string tempColor1 = cubeFaceStates[37];
            string tempColor2 = cubeFaceStates[40];
            string tempColor3 = cubeFaceStates[43];

            //Top face
            cubeFaceStates[37] = cubeFaceStates[16];
            cubeFaceStates[40] = cubeFaceStates[13];
            cubeFaceStates[43] = cubeFaceStates[10];

            //Back face
            cubeFaceStates[10] = cubeFaceStates[46];
            cubeFaceStates[13] = cubeFaceStates[49];
            cubeFaceStates[16] = cubeFaceStates[52];

            //Bottom face
            cubeFaceStates[52] = cubeFaceStates[1];
            cubeFaceStates[49] = cubeFaceStates[4];
            cubeFaceStates[46] = cubeFaceStates[7];

            //Front face
            cubeFaceStates[1] = tempColor1;
            cubeFaceStates[4] = tempColor2;
            cubeFaceStates[7] = tempColor3;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " X Middle Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void xMiddleCCW()
        {
            string tempColor1 = cubeFaceStates[37];
            string tempColor2 = cubeFaceStates[40];
            string tempColor3 = cubeFaceStates[43];

            //Top face
            cubeFaceStates[37] = cubeFaceStates[1];
            cubeFaceStates[40] = cubeFaceStates[4];
            cubeFaceStates[43] = cubeFaceStates[7];

            //Front face
            cubeFaceStates[1] = cubeFaceStates[52];
            cubeFaceStates[4] = cubeFaceStates[49];
            cubeFaceStates[7] = cubeFaceStates[46];

            //Bottom face
            cubeFaceStates[52] = cubeFaceStates[16];
            cubeFaceStates[49] = cubeFaceStates[13];
            cubeFaceStates[46] = cubeFaceStates[10];

            //Back face
            cubeFaceStates[10] = tempColor3;
            cubeFaceStates[13] = tempColor2;
            cubeFaceStates[16] = tempColor1;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " X Middle Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void rightCW()
        {
            string tempColor1 = cubeFaceStates[38];
            string tempColor2 = cubeFaceStates[41];
            string tempColor3 = cubeFaceStates[44];
            string tempColor4 = cubeFaceStates[27];
            string tempColor5 = cubeFaceStates[28];

            //Top face
            cubeFaceStates[38] = cubeFaceStates[2];
            cubeFaceStates[41] = cubeFaceStates[5];
            cubeFaceStates[44] = cubeFaceStates[8];

            //Front face
            cubeFaceStates[2] = cubeFaceStates[53];
            cubeFaceStates[5] = cubeFaceStates[50];
            cubeFaceStates[8] = cubeFaceStates[47];

            //Bottom face
            cubeFaceStates[53] = cubeFaceStates[15];
            cubeFaceStates[50] = cubeFaceStates[12];
            cubeFaceStates[47] = cubeFaceStates[9];

            //Back face
            cubeFaceStates[15] = tempColor1;
            cubeFaceStates[12] = tempColor2;
            cubeFaceStates[9] = tempColor3;

            //Right face
            cubeFaceStates[27] = cubeFaceStates[33];
            cubeFaceStates[28] = cubeFaceStates[30];
            cubeFaceStates[33] = cubeFaceStates[35];
            cubeFaceStates[30] = cubeFaceStates[34];
            cubeFaceStates[35] = cubeFaceStates[29];
            cubeFaceStates[34] = cubeFaceStates[32];
            cubeFaceStates[29] = tempColor4;
            cubeFaceStates[32] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Right Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void rightCCW()
        {
            string tempColor1 = cubeFaceStates[38];
            string tempColor2 = cubeFaceStates[41];
            string tempColor3 = cubeFaceStates[44];
            string tempColor4 = cubeFaceStates[27];
            string tempColor5 = cubeFaceStates[28];

            //Top face
            cubeFaceStates[38] = cubeFaceStates[15];
            cubeFaceStates[41] = cubeFaceStates[12];
            cubeFaceStates[44] = cubeFaceStates[9];

            //Back face
            cubeFaceStates[9] = cubeFaceStates[47];
            cubeFaceStates[12] = cubeFaceStates[50];
            cubeFaceStates[15] = cubeFaceStates[53];

            //Bottom face
            cubeFaceStates[53] = cubeFaceStates[2];
            cubeFaceStates[50] = cubeFaceStates[5];
            cubeFaceStates[47] = cubeFaceStates[8];

            //Front face
            cubeFaceStates[2] = tempColor1;
            cubeFaceStates[5] = tempColor2;
            cubeFaceStates[8] = tempColor3;

            //Right face
            cubeFaceStates[27] = cubeFaceStates[29];
            cubeFaceStates[28] = cubeFaceStates[32];
            cubeFaceStates[29] = cubeFaceStates[35];
            cubeFaceStates[32] = cubeFaceStates[34];
            cubeFaceStates[35] = cubeFaceStates[33];
            cubeFaceStates[34] = cubeFaceStates[30];
            cubeFaceStates[33] = tempColor4;
            cubeFaceStates[30] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Right Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void topCW()
        {
            string tempColor1 = cubeFaceStates[0];
            string tempColor2 = cubeFaceStates[1];
            string tempColor3 = cubeFaceStates[2];
            string tempColor4 = cubeFaceStates[36];
            string tempColor5 = cubeFaceStates[37];

            //Front face
            cubeFaceStates[0] = cubeFaceStates[27];
            cubeFaceStates[1] = cubeFaceStates[28];
            cubeFaceStates[2] = cubeFaceStates[29];

            //Right face
            cubeFaceStates[27] = cubeFaceStates[9];
            cubeFaceStates[28] = cubeFaceStates[10];
            cubeFaceStates[29] = cubeFaceStates[11];

            //Back face
            cubeFaceStates[9] = cubeFaceStates[18];
            cubeFaceStates[10] = cubeFaceStates[19];
            cubeFaceStates[11] = cubeFaceStates[20];

            //Left face
            cubeFaceStates[18] = tempColor1;
            cubeFaceStates[19] = tempColor2;
            cubeFaceStates[20] = tempColor3;

            //Top face
            cubeFaceStates[36] = cubeFaceStates[42];
            cubeFaceStates[37] = cubeFaceStates[39];
            cubeFaceStates[42] = cubeFaceStates[44];
            cubeFaceStates[39] = cubeFaceStates[43];
            cubeFaceStates[43] = cubeFaceStates[41];
            cubeFaceStates[44] = cubeFaceStates[38];
            cubeFaceStates[41] = tempColor5;
            cubeFaceStates[38] = tempColor4;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Top Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void topCCW()
        {
            string tempColor1 = cubeFaceStates[0];
            string tempColor2 = cubeFaceStates[1];
            string tempColor3 = cubeFaceStates[2];
            string tempColor4 = cubeFaceStates[36];
            string tempColor5 = cubeFaceStates[37];

            //Front face
            cubeFaceStates[0] = cubeFaceStates[18];
            cubeFaceStates[1] = cubeFaceStates[19];
            cubeFaceStates[2] = cubeFaceStates[20];

            //Left face
            cubeFaceStates[18] = cubeFaceStates[9];
            cubeFaceStates[19] = cubeFaceStates[10];
            cubeFaceStates[20] = cubeFaceStates[11];

            //Back face
            cubeFaceStates[9] = cubeFaceStates[27];
            cubeFaceStates[10] = cubeFaceStates[28];
            cubeFaceStates[11] = cubeFaceStates[29];

            //Right face
            cubeFaceStates[27] = tempColor1;
            cubeFaceStates[28] = tempColor2;
            cubeFaceStates[29] = tempColor3;

            //Top face
            cubeFaceStates[36] = cubeFaceStates[38];
            cubeFaceStates[37] = cubeFaceStates[41];
            cubeFaceStates[38] = cubeFaceStates[44];
            cubeFaceStates[41] = cubeFaceStates[43];
            cubeFaceStates[43] = cubeFaceStates[39];
            cubeFaceStates[44] = cubeFaceStates[42];
            cubeFaceStates[39] = tempColor5;
            cubeFaceStates[42] = tempColor4;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Top Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void yMiddleCW()
        {
            string tempColor1 = cubeFaceStates[3];
            string tempColor2 = cubeFaceStates[4];
            string tempColor3 = cubeFaceStates[5];

            //Front face
            cubeFaceStates[3] = cubeFaceStates[30];
            cubeFaceStates[4] = cubeFaceStates[31];
            cubeFaceStates[5] = cubeFaceStates[32];

            //Left face
            cubeFaceStates[30] = cubeFaceStates[12];
            cubeFaceStates[31] = cubeFaceStates[13];
            cubeFaceStates[32] = cubeFaceStates[14];

            //Back face
            cubeFaceStates[14] = cubeFaceStates[23];
            cubeFaceStates[13] = cubeFaceStates[22];
            cubeFaceStates[12] = cubeFaceStates[21];

            //Right face
            cubeFaceStates[21] = tempColor1;
            cubeFaceStates[22] = tempColor2;
            cubeFaceStates[23] = tempColor3;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Y Middle Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void yMiddleCCW()
        {
            string tempColor1 = cubeFaceStates[3];
            string tempColor2 = cubeFaceStates[4];
            string tempColor3 = cubeFaceStates[5];

            //Front face
            cubeFaceStates[3] = cubeFaceStates[21];
            cubeFaceStates[4] = cubeFaceStates[22];
            cubeFaceStates[5] = cubeFaceStates[23];

            //Left face
            cubeFaceStates[21] = cubeFaceStates[12];
            cubeFaceStates[22] = cubeFaceStates[13];
            cubeFaceStates[23] = cubeFaceStates[14];

            //Back face
            cubeFaceStates[14] = cubeFaceStates[32];
            cubeFaceStates[13] = cubeFaceStates[31];
            cubeFaceStates[12] = cubeFaceStates[30];

            //Right face
            cubeFaceStates[30] = tempColor1;
            cubeFaceStates[31] = tempColor2;
            cubeFaceStates[32] = tempColor3;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Y Middle Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void bottomCW()
        {
            string tempColor1 = cubeFaceStates[6];
            string tempColor2 = cubeFaceStates[7];
            string tempColor3 = cubeFaceStates[8];
            string tempColor4 = cubeFaceStates[45];
            string tempColor5 = cubeFaceStates[46];

            //Front face
            cubeFaceStates[6] = cubeFaceStates[24];
            cubeFaceStates[7] = cubeFaceStates[25];
            cubeFaceStates[8] = cubeFaceStates[26];

            //Left face
            cubeFaceStates[24] = cubeFaceStates[15];
            cubeFaceStates[25] = cubeFaceStates[16];
            cubeFaceStates[26] = cubeFaceStates[17];

            //Back face
            cubeFaceStates[15] = cubeFaceStates[33];
            cubeFaceStates[16] = cubeFaceStates[34];
            cubeFaceStates[17] = cubeFaceStates[35];

            //Right face
            cubeFaceStates[33] = tempColor1;
            cubeFaceStates[34] = tempColor2;
            cubeFaceStates[35] = tempColor3;

            //Bottom face
            cubeFaceStates[45] = cubeFaceStates[47];
            cubeFaceStates[46] = cubeFaceStates[50];
            cubeFaceStates[47] = cubeFaceStates[53];
            cubeFaceStates[50] = cubeFaceStates[52];
            cubeFaceStates[53] = cubeFaceStates[51];
            cubeFaceStates[52] = cubeFaceStates[48];
            cubeFaceStates[51] = tempColor4;
            cubeFaceStates[48] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Bottom Rotated Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void bottomCCW()
        {
            string tempColor1 = cubeFaceStates[6];
            string tempColor2 = cubeFaceStates[7];
            string tempColor3 = cubeFaceStates[8];
            string tempColor4 = cubeFaceStates[45];
            string tempColor5 = cubeFaceStates[48];

            //Front face
            cubeFaceStates[6] = cubeFaceStates[33];
            cubeFaceStates[7] = cubeFaceStates[34];
            cubeFaceStates[8] = cubeFaceStates[35];

            //Right face
            cubeFaceStates[33] = cubeFaceStates[15];
            cubeFaceStates[34] = cubeFaceStates[16];
            cubeFaceStates[35] = cubeFaceStates[17];

            //Back face
            cubeFaceStates[15] = cubeFaceStates[24];
            cubeFaceStates[16] = cubeFaceStates[25];
            cubeFaceStates[17] = cubeFaceStates[26];

            //Left face
            cubeFaceStates[24] = tempColor1;
            cubeFaceStates[25] = tempColor2;
            cubeFaceStates[26] = tempColor3;

            //Bottom face
            cubeFaceStates[45] = cubeFaceStates[51];
            cubeFaceStates[48] = cubeFaceStates[52];
            cubeFaceStates[51] = cubeFaceStates[53];
            cubeFaceStates[52] = cubeFaceStates[50];
            cubeFaceStates[53] = cubeFaceStates[47];
            cubeFaceStates[50] = cubeFaceStates[46];
            cubeFaceStates[47] = tempColor4;
            cubeFaceStates[46] = tempColor5;

            updateCubeMap();

            //Update moveListBox
            moveListBox.Items.Add((moveListBox.Items.Count + 1).ToString() + " Bottom Rotated C-Clockwise");
            moveListBox.SelectedIndex = moveListBox.Items.Count - 1;
            moveListBoxLabel.Text = moveListBox.Items.Count.ToString() + " Moves";
            clearMovesButton.Visible = true;
        }

        private void checkIfSolved()
        {
            //Determine whether the user has solved the cube
            cubeSolved = true;

            //Iterate through each cube face, comparing it to the complete cube state
            for (int i = 0; i < 52; i++)
            {
                solvedCubeState[i] = cubeFaceStates[((i / 9) * 9) + 4];
                if (solvedCubeState[i] != cubeFaceStates[i])
                {
                    //If a difference is found in the two arrays, the cube is not solved
                    cubeSolved = false;
                }
            }
            
            if (cubeSolved)
            {
                //End the current attempt and stop the timer
                attemptInProgress = false;
                timedModeTimer.Enabled = false;

                //Display the appropriate controls
                demoModeTimer.Enabled = true;

                solvedLabel.Visible = true;
                solvedLabel.Parent = cubeDisplay;
                solvedLabel.Location = new Point(cubeDisplay.Width / 2 - solvedLabel.Width / 2, cubeDisplay.Height / 2 - solvedLabel.Height / 2);

                //Check which mode the user is in
                if (timeTrialToolStripMenuItem.Checked)
                {
                    //Determine whether their score can be placed on the leaderboard
                    string playerTime = Timer.Text.Remove(0, 6);

                    if (File.Exists("Timed_Leaderboard.lbd"))
                    {
                        //If the leaderboard file already exists, calculate the user's rank on the board and append their score/name to the board
                        string[] leaderTimes = new string[5];

                        //Load the times of each user on the leaderboard into an array
                        for (int i = 0; i < 5; i++)
                        {
                            leaderTimes[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 2).Take(1).First();

                            TimeSpan tryParseTemp;
                            bool validTime = TimeSpan.TryParse(leaderTimes[i], out tryParseTemp);

                            if (validTime)
                            {
                                if (TimeSpan.Parse(playerTime) < TimeSpan.Parse(leaderTimes[i]))
                                {
                                    appendToLeaderboard(true, moveListBox.Items.Count.ToString(), playerTime, i);
                                    break;
                                }
                            }
                            else if (!validTime)
                            {
                                appendToLeaderboard(true, moveListBox.Items.Count.ToString(), playerTime, i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        //If the file does not exists, create the file and append the user's score/name to the top of the blank board
                        System.IO.StreamWriter leaderboard = new System.IO.StreamWriter("Timed_Leaderboard.lbd", true);
                        using (leaderboard)
                        {
                            for (int i = 0; i < 15; i++)
                            {
                                leaderboard.WriteLine("None");
                            }
                        }
                        appendToLeaderboard(true, moveListBox.Items.Count.ToString(), playerTime, 0);
                    }
                }
                else
                {
                    //Determine whether their score can be placed on the leaderboard
                    int playerMoveCount = moveListBox.Items.Count;

                    if (File.Exists("Leaderboard.lbd"))
                    {
                        //If the leaderboard file already exists, calculate the user's rank on the board and append their score/name to the board
                        string[] leaderMoveCounts = new string[5];

                        for (int i = 0; i < 5; i++)
                        {
                            leaderMoveCounts[i] = File.ReadLines("Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();

                            int tryParseTemp;
                            bool validMoveCount = int.TryParse(leaderMoveCounts[i], out tryParseTemp);

                            if (validMoveCount)
                            {
                                if (playerMoveCount < int.Parse(leaderMoveCounts[i]))
                                {
                                    appendToLeaderboard(false, playerMoveCount.ToString(), "", i);
                                    break;
                                }
                            }
                            else if (!validMoveCount)
                            {
                                appendToLeaderboard(false, playerMoveCount.ToString(), "", i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        //If the file does not exists, create the file and append the user's score/name to the top of the blank board
                        System.IO.StreamWriter leaderboard = new System.IO.StreamWriter("Leaderboard.lbd", true);
                        using (leaderboard)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                leaderboard.WriteLine("None");
                            }
                        }
                        appendToLeaderboard(false, playerMoveCount.ToString(), "", 0);
                    }
                }
                resetCubeButton.Visible = true;
                resetCubeButton.Location = new Point(cubeDisplay.Width / 2, resetCubeButton.Location.Y);
                solvedLabel.Parent = cubeDisplay;
            }
        }

        private void appendToLeaderboard(bool timed, string moveCount, string Time, int rank)
        {
            //Adds the user's score to the leaderboard in the correct position
            leaderboardEntry lbdEntry = new leaderboardEntry(moveCount, Time, timed);
            lbdEntry.ShowDialog();

            if (rank > 4 || rank < 0)
            {
                MessageBox.Show("Input 'rank' outside expected range.", "Error");
                return;
            }

            //Allow the user to input a name
            if (lbdEntry.DialogResult == DialogResult.OK)
            {
                string[] leaderNames = new string[5];
                string[] leaderMoveCounts = new string[5];

                switch (timed)
                {
                    case true:
                        string[] leaderTimes = new string[5];

                        //Create arrays incorporating the  user's name and move count into the existing leaderboard entries
                        string prevNameTimed = null;
                        for (int i = 0; i < 5; i++)
                        {
                            if (rank == i)
                            {
                                leaderNames[i] = lbdEntry.pName;
                                prevNameTimed = File.ReadLines("Timed_Leaderboard.lbd").Skip(i * 2).Take(1).First();
                            }
                            else if (prevNameTimed != null)
                            {
                                leaderNames[i] = prevNameTimed;
                                if (File.ReadLines("Timed_Leaderboard.lbd").Skip(i * 2).Take(1).First() != "None")
                                {
                                    prevNameTimed = File.ReadLines("Timed_Leaderboard.lbd").Skip(i * 2).Take(1).First();
                                }
                                else
                                {
                                    prevNameTimed = null;
                                }
                            }
                            else
                            {
                                leaderNames[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip(i * 2).Take(1).First();
                            }
                        }

                        string prevMoveCountTimed = null;
                        for (int i = 0; i < 5; i++)
                        {
                            if (rank == i)
                            {
                                leaderMoveCounts[i] = moveCount;
                                prevMoveCountTimed = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                            }
                            else if (prevMoveCountTimed != null)
                            {
                                leaderMoveCounts[i] = prevMoveCountTimed;
                                if (File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First() != "None")
                                {
                                    prevMoveCountTimed = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                                }
                                else
                                {
                                    prevMoveCountTimed = null;
                                }
                            }
                            else
                            {
                                leaderMoveCounts[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                            }
                        }

                        //Create an array incorporating the user's time into the existing leaderboard entries
                        string prevTime = null;
                        for (int i = 0; i < 5; i++)
                        {
                            if (rank == i)
                            {
                                leaderTimes[i] = Time;
                                prevTime = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 2).Take(1).First();
                            }
                            else if (prevTime != null)
                            {
                                leaderTimes[i] = prevTime;
                                if (File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 2).Take(1).First() != "None")
                                {
                                    prevTime = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 2).Take(1).First();
                                }
                                else
                                {
                                    prevTime = null;
                                }
                            }
                            else
                            {
                                leaderTimes[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 2).Take(1).First();
                            }
                        }
                        
                        //Erase the timed leaderboard file and write the new data to the file in the correct order
                        System.IO.File.WriteAllText("Timed_Leaderboard.lbd", null);
                        System.IO.StreamWriter timedLeaderBoard = new System.IO.StreamWriter("Timed_Leaderboard.lbd", true);
                        using (timedLeaderBoard)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                timedLeaderBoard.WriteLine(leaderNames[i]);
                                timedLeaderBoard.WriteLine(leaderMoveCounts[i]);
                                timedLeaderBoard.WriteLine(leaderTimes[i]);
                            }
                        }
                        timedLeaderboardToolStripMenuItem.PerformClick();
                        break;

                    case false:
                        //Create arrays incorporating the  user's name and move count into the existing leaderboard entries
                        string prevName = null;
                        for (int i = 0; i < 5; i++)
                        {
                            if (rank == i)
                            {
                                leaderNames[i] = lbdEntry.pName;
                                prevName = File.ReadLines("Leaderboard.lbd").Skip(i * 2 ).Take(1).First();
                            }
                            else if (prevName != null)
                            {
                                leaderNames[i] = prevName;
                                if (File.ReadLines("Leaderboard.lbd").Skip(i * 2).Take(1).First() != "None")
                                {
                                    prevName = File.ReadLines("Leaderboard.lbd").Skip(i * 2).Take(1).First();
                                }
                                else
                                {
                                    prevName = null;
                                }
                            }
                            else
                            {
                                leaderNames[i] = File.ReadLines("Leaderboard.lbd").Skip(i * 2).Take(1).First();
                            }
                        }

                        string prevMoveCount = null;
                        for (int i = 0; i < 5; i++)
                        {
                            if (rank == i)
                            {
                                leaderMoveCounts[i] = moveCount;
                                prevMoveCount = File.ReadLines("Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                            }
                            else if (prevMoveCount != null)
                            {
                                leaderMoveCounts[i] = prevMoveCount;
                                if (File.ReadLines("Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First() != "None")
                                {
                                    prevMoveCount = File.ReadLines("Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                                }
                                else
                                {
                                    prevMoveCount = null;
                                }
                            }
                            else
                            {
                                leaderMoveCounts[i] = File.ReadLines("Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                            }
                        }

                        //Erase the leaderboard file and write the new data to the file in the correct order
                        System.IO.File.WriteAllText("Leaderboard.lbd", null);
                        System.IO.StreamWriter leaderBoard = new System.IO.StreamWriter("Leaderboard.lbd", true);
                        using (leaderBoard)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                leaderBoard.WriteLine(leaderNames[i]);
                                leaderBoard.WriteLine(leaderMoveCounts[i]);
                            }
                        }
                        leaderboardToolStripMenuItem.PerformClick();
                        break;
                }
            }
        }
        #endregion

        private void clearMovesButton_Click(object sender, EventArgs e)
        {
            //Clear the moveListBox and reset the move count
            clearMovesButton.Visible = false;

            moveListBox.Items.Clear();
            moveListBoxLabel.Text = "Moves";
        }

        private void sequenceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //Toggle visibltiy of the move sequence list box and format the positon of it and the surrounding controls
            if (sequenceCheckBox.Checked)
            {
                sequenceListBox.Visible = true;
                sequenceListBoxLabel.Visible = true;

                moveListBox.Height = sequenceListBox.Height;
                moveListBox.Location = new Point(sequenceListBox.Location.X, sequenceListBox.Location.Y + sequenceListBox.Height + moveListBoxLabel.Height + 10);
                moveListBoxLabel.Location = new Point(sequenceListBoxLabel.Location.X, moveListBox.Location.Y - moveListBoxLabel.Height - 5);

                executeSequenceButton.Visible = true;
                clearSequenceButton.Visible = true;
            }
            else
            {
                sequenceListBox.Visible = false;
                sequenceListBoxLabel.Visible = false;

                moveListBox.Height = controlPanel.Height - 80;
                moveListBox.Location = new Point(moveListBox.Location.X, 11 + moveListBoxLabel.Height + 10);
                moveListBoxLabel.Location = new Point(moveListBoxLabel.Location.X, 11);

                executeSequenceButton.Visible = false;
                clearSequenceButton.Visible = false;

                sequenceListBox.Items.Clear();
                currentSequence.Clear();
            }
        }

        #region Button Click Handlers

        //Execute the appropriate rotation when each button is pressed
        private void frontCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                frontCW();

                Render();

                if (timeTrialToolStripMenuItem.Checked)
                {
                    timedModeTimer.Enabled = true;
                    stopWatch.Start();
                }

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Front Clockwise");
                currentSequence.Add(0);
                executeSequenceButton.Enabled = true;
            }
        }

        private void frontCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                frontCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Front C-Clockwise");
                currentSequence.Add(1);
                executeSequenceButton.Enabled = true;
            }
        }

        private void zMiddleCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                zMiddleCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Z Middle Clockwise");
                currentSequence.Add(2);
                executeSequenceButton.Enabled = true;
            }
        }

        private void zMiddleCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                zMiddleCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Z Middle C-Clockwise");
                currentSequence.Add(3);
                executeSequenceButton.Enabled = true;
            }
        }

        private void backCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                backCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Back Clockwise");
                currentSequence.Add(4);
                executeSequenceButton.Enabled = true;
            }
        }

        private void backCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                backCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Back C-Clockwise");
                currentSequence.Add(5);
                executeSequenceButton.Enabled = true;
            }
        }

        private void leftCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                leftCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Left Clockwise");
                currentSequence.Add(6);
                executeSequenceButton.Enabled = true;
            }
        }

        private void leftCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                leftCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Left C-Clockwise");
                currentSequence.Add(7);
                executeSequenceButton.Enabled = true;
            }
        }

        private void xMiddleCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                xMiddleCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate X Middle Clockwise");
                currentSequence.Add(8);
                executeSequenceButton.Enabled = true;
            }
        }

        private void xMiddleCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                xMiddleCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate X Middle C-Clockwise");
                currentSequence.Add(9);
                executeSequenceButton.Enabled = true;
            }
        }

        private void rightCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                rightCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Right Clockwise");
                currentSequence.Add(10);
                executeSequenceButton.Enabled = true;
            }
        }

        private void rightCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                rightCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Right C-Clockwise");
                currentSequence.Add(11);
                executeSequenceButton.Enabled = true;
            }
        }

        private void upCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                topCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Top Clockwise");
                currentSequence.Add(12);
                executeSequenceButton.Enabled = true;
            }
        }

        private void upCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                topCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Top C-Clockwise");
                currentSequence.Add(13);
                executeSequenceButton.Enabled = true;
            }
        }

        private void yMiddleCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                yMiddleCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Y Middle Clockwise");
                currentSequence.Add(14);
                executeSequenceButton.Enabled = true;
            }
        }

        private void yMiddleCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                yMiddleCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Y Middle C-Clockwise");
                currentSequence.Add(15);
                executeSequenceButton.Enabled = true;
            }
        }

        private void bottomCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                bottomCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Bottom Clockwise");
                currentSequence.Add(16);
                executeSequenceButton.Enabled = true;
            }
        }

        private void bottomCCWButton_Click(object sender, EventArgs e)
        {
            if (!sequenceCheckBox.Checked)
            {
                bottomCCW();

                Render();

                if (attemptInProgress)
                {
                    //Check if the user has solved the cube
                    checkIfSolved();
                }
            }
            else
            {
                sequenceListBox.Items.Add((sequenceListBox.Items.Count + 1).ToString() + " Rotate Bottom C-Clockwise");
                currentSequence.Add(17);
                executeSequenceButton.Enabled = true;
            }
        }

        #endregion

#endregion

        #region Execute Sequence Handlers
        private void executeSequenceButton_Click(object sender, EventArgs e)
        {
            //Enable the timer that allows each move in the selected sequence to be executed
            sequenceTimer.Enabled = true;
        }

        private void sequenceTimer_Tick(object sender, EventArgs e)
        {
            //With each tick of the timer, execute the next move in the sequence
            if (sequenceIndex < currentSequence.Count - 1)
            {
                switch (currentSequence[sequenceIndex])
                {
                    case 0:
                        frontCW();
                        break;

                    case 1:
                        frontCCW();
                        break;

                    case 2:
                        zMiddleCW();
                        break;

                    case 3:
                        zMiddleCCW();
                        break;

                    case 4:
                        backCW();
                        break;

                    case 5:
                        backCCW();
                        break;

                    case 6:
                        leftCW();
                        break;

                    case 7:
                        leftCCW();
                        break;

                    case 8:
                        xMiddleCW();
                        break;

                    case 9:
                        xMiddleCCW();
                        break;

                    case 10:
                        rightCW();
                        break;

                    case 11:
                        rightCCW();
                        break;

                    case 12:
                        topCW();
                        break;

                    case 13:
                        topCCW();
                        break;

                    case 14:
                        yMiddleCW();
                        break;

                    case 15:
                        yMiddleCCW();
                        break;

                    case 16:
                        bottomCW();
                        break;

                    case 17:
                        bottomCCW();
                        break;
                }
                Render();
                sequenceIndex++;
            }
            else
            {
                sequenceTimer.Enabled = false;
                checkIfSolved();
                sequenceIndex = 0;
            }
        }
        #endregion

        #region Cube Map

        private void updateCubeMap()
        {
            //Update each square of the cube map based upon the cubeFaceStates aray
            cubeMapFaces = new PictureBox[54] { F0, F1, F2, F3, F4, F5, F6, F7, F8, B0, B1, B2, B3, B4, B5, B6, B7, B8, 
                L0, L1, L2, L3, L4, L5, L6, L7, L8, R0, R1, R2, R3, R4, R5, R6, R7, R8, 
                T0, T1, T2, T3, T4, T5, T6, T7, T8, D0, D1, D2, D3, D4, D5, D6, D7, D8 };

            for (int i = 0; i < 54; i++)
            {
                cubeMapFaces[i].BackColor = Color.FromName(cubeFaceStates[i]);
            }
        }

        private void frontMapFace_Click(object sender, EventArgs e)
        {
            //Make the front face visible
            YRotation = 0;
            XRotation = 0;
            ZRotation = 0;

            Render();
        }

        private void backMapFace_Click(object sender, EventArgs e)
        {
            //Make the back face visible
            YRotation = 180;
            XRotation = 0;
            ZRotation = 0;

            Render();
        }

        private void leftMapFace_Click(object sender, EventArgs e)
        {
            //Make the left face visible
            YRotation = 270;
            XRotation = 0;
            ZRotation = 0;

            Render();
        }

        private void rightMapFace_Click(object sender, EventArgs e)
        {
            //Make the right face visible
            YRotation = 90;
            XRotation = 0;
            ZRotation = 0;

            Render();
        }

        private void topMapFace_Click(object sender, EventArgs e)
        {
            //Make the top face visible
            YRotation = 0;
            XRotation = 90;
            ZRotation = 0;

            Render();
        }

        private void bottomMapFace_Click(object sender, EventArgs e)
        {
            //Make the bottom face visible
            YRotation = 0;
            XRotation = 270;
            ZRotation = 0;

            Render();
        }

        #endregion

        #region Custom Buttons
        //Define the appearance and behaviour of the custom buttons

        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            var b = sender as PictureBox;

            b.BackgroundImage = Properties.Resources.ButtonBackgroundClicked;
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            var b = sender as PictureBox;

            b.BackgroundImage = Properties.Resources.ButtonBackground;
        }

        private void LongButton_MouseDown(object sender, MouseEventArgs e)
        {
            var b = sender as PictureBox;

            b.BackgroundImage = Properties.Resources.LongButtonBackgroundClicked;
        }

        private void LongButton_MouseUp(object sender, MouseEventArgs e)
        {
            var b = sender as PictureBox;

            b.BackgroundImage = Properties.Resources.LongButtonBackground;
        }

        private void clearSequenceButton_Click(object sender, EventArgs e)
        {
            sequenceListBox.Items.Clear();
            currentSequence.Clear();
        }

        private void resetCubeButton_Click(object sender, EventArgs e)
        {
            cubeSolved = false;

            demoModeTimer.Enabled = false;
            solvedLabel.Visible = false;

            resetToolStripMenuItem.PerformClick();

            resetCubeButton.Visible = false;
            clearMovesButton.Visible = false;

            moveListBox.Items.Clear();
            moveListBoxLabel.Text = "Moves";

            solutionProgressBar.Visible = false;
        }

        private void frontCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("F", myFont, Brushes.White, new Point(7, 5));
            }
        }

        private void frontCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("F'", myFont, Brushes.White, new Point(6, 5));
            }
        }

        private void zMiddleCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("ZM", myFont, Brushes.White, new Point(3, 7));
            }
        }

        private void zMiddleCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("ZM'", myFont, Brushes.White, new Point(2, 7));
            }
        }

        private void backCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("B", myFont, Brushes.White, new Point(7, 5));
            }
        }

        private void backCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("B'", myFont, Brushes.White, new Point(6, 5));
            }
        }

        private void leftCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("L", myFont, Brushes.White, new Point(7, 5));
            }
        }

        private void leftCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("L'", myFont, Brushes.White, new Point(6, 5));
            }
        }

        private void xMiddleCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("XM", myFont, Brushes.White, new Point(3, 7));
            }
        }

        private void xMiddleCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("XM'", myFont, Brushes.White, new Point(2, 7));
            }
        }

        private void rightCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("R", myFont, Brushes.White, new Point(7, 5));
            }
        }

        private void rightCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("R'", myFont, Brushes.White, new Point(6, 5));
            }
        }

        private void upCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("U", myFont, Brushes.White, new Point(7, 5));
            }
        }

        private void upCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("U'", myFont, Brushes.White, new Point(6, 5));
            }
        }

        private void yMiddleCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("YM", myFont, Brushes.White, new Point(3, 7));
            }
        }

        private void yMiddleCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 11))
            {
                e.Graphics.DrawString("YM'", myFont, Brushes.White, new Point(2, 7));
            }
        }

        private void bottomCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("D", myFont, Brushes.White, new Point(7, 5));
            }
        }

        private void bottomCCWButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 14))
            {
                e.Graphics.DrawString("D'", myFont, Brushes.White, new Point(6, 5));
            }
        }

        private void executeSequenceButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 12))
            {
                e.Graphics.DrawString("Execute", myFont, Brushes.White, new Point(9, 4));
            }
        }

        private void clearMovesButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 12))
            {
                e.Graphics.DrawString("Clear", myFont, Brushes.White, new Point(18, 4));
            }
        }

        private void clearSequenceButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 12))
            {
                e.Graphics.DrawString("Clear", myFont, Brushes.White, new Point(19, 4));
            }
        }

        private void resetCubeButton_Paint(object sender, PaintEventArgs e)
        {
            using (Font myFont = new Font("Arial", 16))
            {
                e.Graphics.DrawString("Reset", myFont, Brushes.White, new Point(19, 4));
            }
        }

        private void faceRotationButton_Hover(object sender, EventArgs e)
        {
            var button = sender as PictureBox;
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(button, button.Tag.ToString());
        }

        #endregion

        #region Menu Items

        private void scrambleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Execute the scramble method, disable timed mode toggle, set begin an attempt and start the stopwatch if timed mode is enabled
            timeTrialToolStripMenuItem.Enabled = false;
            Scramble();
            attemptInProgress = true;
            if (timeTrialToolStripMenuItem.Checked)
            {
                timedModeTimer.Enabled = true;
                stopWatch.Start();
            }
        }

        private void solveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //End an attempt if one is in progress, update the solution progress bar, execute the solve method and enable demo mode/display the solved label once solved
            if (attemptInProgress)
            {
                timedModeTimer.Enabled = false;
                Timer.Visible = false;
                timeTrialToolStripMenuItem.Enabled = true;
                attemptInProgress = false;
            }

            solutionProgressBar.Width = cubeDisplay.Width - 10;
            solutionProgressBar.Location = new Point(cubeDisplay.Location.X + (cubeDisplay.Width / 2 - solutionProgressBar.Width / 2), cubeDisplay.Height);
            solutionProgressBar.Visible = true;

            Solve();
            cubeSolved = true;
            Render();
            demoModeTimer.Enabled = true;

            solvedLabel.Visible = true;
            solvedLabel.Parent = cubeDisplay;
            solvedLabel.Location = new Point(cubeDisplay.Width / 2 - solvedLabel.Width / 2, cubeDisplay.Height / 2 - solvedLabel.Height / 2);

            resetCubeButton.Visible = true;
            resetCubeButton.Location = new Point(cubeDisplay.Width / 2, resetCubeButton.Location.Y);
            solvedLabel.Parent = cubeDisplay;
            
            Refresh();
        }

        private void solvedLabel_Paint(object sender, PaintEventArgs e)
        {
            //Draw the solved label, adding a black outline for visibility
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Font myFont = new Font("Arial", 32))
            {
                e.Graphics.DrawString("Solved!", myFont, Brushes.White, new Point(14, 6));
            }

            GraphicsPath p = new GraphicsPath();
            p.AddString("Solved!", new FontFamily("Arial"), (int)FontStyle.Regular, e.Graphics.DpiY * 32 / 72,
                new Point(13, 5), new StringFormat());
            e.Graphics.DrawPath(Pens.Black, p);
        }

        public class MyPanel : System.Windows.Forms.Panel
        {
            public MyPanel()
            {
                this.SetStyle(
                    System.Windows.Forms.ControlStyles.UserPaint |
                    System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                    System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                    true);
            }
        }

        private void timeTrialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the timer visiblity
            Timer.Location = new Point(cubeDisplay.Width / 2 - Timer.Width / 2, cubeDisplay.Height - Timer.Height - 10);
            Timer.Visible = !Timer.Visible;
            if (!timeTrialToolStripMenuItem.Checked)
            {
                Timer.Text = "Time: 00:00.00";
            }
        }

        private void timedModeTimer_Tick(object sender, EventArgs e)
        {
            //Update the elapsed time shown by the timer after each tick of the stopwatch
            timeElapsed = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", timeElapsed.Minutes,
            timeElapsed.Seconds, timeElapsed.Milliseconds / 10);

            Timer.Text = "Time: " + elapsedTime;
        }

        private void resetFacesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Reset the faces of the cube to their default configuration. End an attempt if one is in progress
            if (attemptInProgress)
            {
                timedModeTimer.Enabled = false;
                stopWatch.Reset();
                timeTrialToolStripMenuItem.Enabled = true;
                attemptInProgress = false;
            }

            clearMovesButton.Visible = false;

            moveListBox.Items.Clear();
            moveListBoxLabel.Text = "Moves";

            for (int i = 0; i < 9; i++)
            {
                cubeFaceStates[i] = defaultFaceColors[0];
            }

            for (int i = 9; i < 18; i++)
            {
                cubeFaceStates[i] = defaultFaceColors[1];
            }

            for (int i = 18; i < 27; i++)
            {
                cubeFaceStates[i] = defaultFaceColors[2];
            }

            for (int i = 27; i < 36; i++)
            {
                cubeFaceStates[i] = defaultFaceColors[3];
            }

            for (int i = 36; i < 45; i++)
            {
                cubeFaceStates[i] = defaultFaceColors[4];
            }

            for (int i = 45; i < 54; i++)
            {
                cubeFaceStates[i] = defaultFaceColors[5];
            }

            attemptInProgress = false;
            updateCubeMap();
            Render();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Reset the rotational position of the cube to the default
            XRotation = 10;
            YRotation = 24;
            ZRotation = 360;

            Render();
        }

        private void showCubeMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the visibility of the cube map
            switch (showCubeMapToolStripMenuItem.Checked)
            {
                case false:
                    cubeMapGroupBox.Visible = false;
                    cubeDisplay.Width += cubeMapGroupBox.Width;
                    cubeDisplay.Location = new Point(0, cubeDisplay.Location.Y);
                    break;
                case true:
                    cubeMapGroupBox.Visible = true;
                    cubeDisplay.Width -= cubeMapGroupBox.Width;
                    cubeDisplay.Location = new Point(cubeMapGroupBox.Width + 1, cubeDisplay.Location.Y);
                    break;
            }
            Timer.Location = new Point(cubeDisplay.Width / 2 - Timer.Width / 2, cubeDisplay.Height - Timer.Height - 10);
            solvedLabel.Location = new Point(cubeDisplay.Width / 2 - solvedLabel.Width / 2, cubeDisplay.Height / 2 - solvedLabel.Height / 2);
            solutionProgressBar.Width = cubeDisplay.Width - 10;
            solutionProgressBar.Location = new Point(cubeDisplay.Location.X + (cubeDisplay.Width / 2 - solutionProgressBar.Width / 2), cubeDisplay.Height);
            Render();
        }

        private void demoModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the demo mode timer
            demoModeTimer.Enabled = !demoModeTimer.Enabled;
        }

        private void demoModeTimer_Tick(object sender, EventArgs e)
        {
            //Increment the rotation of the cube along each axis with each tick of the demoModeTimer
            XRotation += 1;
            YRotation += 1;
            ZRotation += 1;

            Render();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Quit Cubix?", "Confirm Quit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Show the about form
            aboutForm aForm = new aboutForm();
            aForm.Show();
        }

        private void antiAliasingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the anti-aliasing setting
            AntiAliasing = antiAliasingToolStripMenuItem.Checked;
            Render();
        }

        private void faceLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the cube face labels
            FaceLabels = !FaceLabels;
            Render();
        }

        private void cubeColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Display the colorPicker form and change the colors of the cube to those selected
            ColorPicker colorPicker = new ColorPicker(defaultFaceColors);
            colorPicker.StartPosition = FormStartPosition.Manual;
            colorPicker.Location = new Point(this.Location.X - colorPicker.Width, this.Location.Y);
            colorPicker.ShowDialog();

            defaultFaceColors = colorPicker.cFaceColors;
            resetFacesToolStripMenuItem.PerformClick();
        }

        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set the background of the cubeDisplay to the default grey, toggle all other settings to false
            if (!defaultToolStripMenuItem.Checked)
            {
                defaultToolStripMenuItem.Checked = true;
                cubeDisplay.BackgroundImage = null;
                spaceToolStripMenuItem.Checked = false;
                darkWoodToolStripMenuItem.Checked = false;
                lightWoodToolStripMenuItem.Checked = false;
                skyToolStripMenuItem.Checked = false;
            }
        }

        private void spaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set the background of the cubeDisplay to the space variation, toggle all other settings to false
            if (!spaceToolStripMenuItem.Checked)
            {
                spaceToolStripMenuItem.Checked = true;
                cubeDisplay.BackgroundImage = Properties.Resources.SpaceBackground;
                defaultToolStripMenuItem.Checked = false;
                darkWoodToolStripMenuItem.Checked = false;
                lightWoodToolStripMenuItem.Checked = false;
                skyToolStripMenuItem.Checked = false;
            }
        }

        private void darkWoodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set the background of the cubeDisplay to the dark wood variation, toggle all other settings to false
            if (!darkWoodToolStripMenuItem.Checked)
            {
                darkWoodToolStripMenuItem.Checked = true;
                cubeDisplay.BackgroundImage = Properties.Resources.WoodBackground;
                spaceToolStripMenuItem.Checked = false;
                defaultToolStripMenuItem.Checked = false;
                lightWoodToolStripMenuItem.Checked = false;
                skyToolStripMenuItem.Checked = false;
            }
        }

        private void lightWoodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set the background of the cubeDisplay to the light wood variation, toggle all other settings to false
            if (!lightWoodToolStripMenuItem.Checked)
            {
                lightWoodToolStripMenuItem.Checked = true;
                cubeDisplay.BackgroundImage = Properties.Resources.LightWoodBackground;
                defaultToolStripMenuItem.Checked = false;
                darkWoodToolStripMenuItem.Checked = false;
                spaceToolStripMenuItem.Checked = false;
                skyToolStripMenuItem.Checked = false;
            }
        }

        private void skyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Set the background of the cubeDisplay to the sky variation, toggle all other settings to false
            if (!skyToolStripMenuItem.Checked)
            {
                skyToolStripMenuItem.Checked = true;
                cubeDisplay.BackgroundImage = Properties.Resources.SkyBackground;
                defaultToolStripMenuItem.Checked = false;
                darkWoodToolStripMenuItem.Checked = false;
                spaceToolStripMenuItem.Checked = false;
                lightWoodToolStripMenuItem.Checked = false;
            }
        }

        private void ultraSecretSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the secret setting...
            SecretSetting = ultraSecretSettingToolStripMenuItem.Checked;
            Render();
        }

        private void controlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Toggle the visiblity of the control panel
            switch (controlsToolStripMenuItem.Checked)
            {
                case true:
                    controlsToolStripMenuItem.Text = "< Show Controls";
                    controlPanel.Visible = false;
                    cubeDisplay.Width += controlPanel.Width;
                    if (showCubeMapToolStripMenuItem.Checked)
                    {
                        cubeDisplay.Location = new Point(cubeMapGroupBox.Width + 1, cubeDisplay.Location.Y);
                    }
                    break;

                case false:
                    controlsToolStripMenuItem.Text = "Hide Controls >";
                    controlPanel.Visible = true;
                    cubeDisplay.Width -= controlPanel.Width;
                    if (showCubeMapToolStripMenuItem.Checked)
                    {
                        cubeDisplay.Location = new Point(cubeMapGroupBox.Width + 1, cubeDisplay.Location.Y);
                    }
                    break;
            }
            Timer.Location = new Point(cubeDisplay.Width / 2 - Timer.Width / 2, cubeDisplay.Height - Timer.Height - 10);
            solvedLabel.Location = new Point(cubeDisplay.Width / 2 - solvedLabel.Width / 2, cubeDisplay.Height / 2 - solvedLabel.Height / 2);
            solutionProgressBar.Width = cubeDisplay.Width - 10;
            solutionProgressBar.Location = new Point(cubeDisplay.Location.X + (cubeDisplay.Width / 2 - solutionProgressBar.Width / 2), cubeDisplay.Height);
            Render();
        }

        private void lightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Lighting = !Lighting;
            Render();
        }

        private void leaderboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("Leaderboard.lbd"))
            {
                string[] leaderNames = new string[5];
                string[] leaderMoveCounts = new string[5];
                string[] leaderTimes = new string[5] { null, null, null, null, null };

                for (int i = 0; i < 5; i++)
                {
                    leaderNames[i] = File.ReadLines("Leaderboard.lbd").Skip(i * 2).Take(1).First();
                }

                for (int i = 0; i < 5; i++)
                {
                    leaderMoveCounts[i] = File.ReadLines("Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                }

                Leaderboard Leaderboard = new Leaderboard(false, leaderNames, leaderMoveCounts, leaderTimes);

                Leaderboard.Show();
            }
            else
            {
                string[] leaderNames = new string[5] { "None", "None", "None", "None", "None" };
                string[] leaderMoveCounts = new string[5] { "None", "None", "None", "None", "None" };
                string[] leaderTimes = new string[5] { null, null, null, null, null };

                System.IO.StreamWriter leaderboardData = new System.IO.StreamWriter("Leaderboard.lbd", true);

                using (leaderboardData)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        leaderboardData.WriteLine("None");
                    }
                }

                Leaderboard Leaderboard = new Leaderboard(false, leaderNames, leaderMoveCounts, leaderTimes);

                Leaderboard.Show();
            }
        }

        private void timedLeaderboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("Timed_Leaderboard.lbd"))
            {
                string[] leaderNames = new string[5];
                string[] leaderMoveCounts = new string[5];
                string[] leaderTimes = new string[5];

                for (int i = 0; i < 5; i++)
                {
                    leaderNames[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip(i * 2).Take(1).First();
                }

                for (int i = 0; i < 5; i++)
                {
                    leaderMoveCounts[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 1).Take(1).First();
                }

                for (int i = 0; i < 5; i++)
                {
                    leaderTimes[i] = File.ReadLines("Timed_Leaderboard.lbd").Skip((i * 2) + 2).Take(1).First();
                }

                Leaderboard Leaderboard = new Leaderboard(true, leaderNames, leaderMoveCounts, leaderTimes);

                Leaderboard.Show();
            }
            else
            {
                string[] leaderNames = new string[5] { "None", "None", "None", "None", "None" };
                string[] leaderMoveCounts = new string[5] { "None", "None", "None", "None", "None" };
                string[] leaderTimes = new string[5] { "None", "None", "None", "None", "None" };

                System.IO.StreamWriter leaderboardData = new System.IO.StreamWriter("Timed_Leaderboard.lbd", true);
                using (leaderboardData)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        leaderboardData.WriteLine("None");
                    }
                }

                Leaderboard Leaderboard = new Leaderboard(true, leaderNames, leaderMoveCounts, leaderTimes);

                Leaderboard.Show();
            }
        }

        private void gettingStartedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            helpForm helpForm = new helpForm(0);
            helpForm.Show();
        }

        private void keyCommandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            helpForm helpForm = new helpForm(1);
            helpForm.Show();
        }

        private void customisationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            helpForm helpForm = new helpForm(2);
            helpForm.Show();
        }
        #endregion

        #region cubeDisplay Event Handlers

        private void cubeDisplay_MouseUp(object sender, MouseEventArgs e)
        {
            draggingTimer.Enabled = false;
        }

        private void cubeDisplay_MouseDown(object sender, MouseEventArgs e)
        {
            mouseOrigin = PointToClient(Cursor.Position);
            cubeFaceDepth = getCubeOrientation(cubeFaceDepth);
            draggingTimer.Enabled = true;
        }

        private void draggingTimer_Tick(object sender, EventArgs e)
        {
            Point currentMousePosition = PointToClient(Cursor.Position);

            switch (cubeFaceDepth[0])
            {
                case 0:
                    if (currentMousePosition.X > mouseOrigin.X + 100)
                    {
                        YRotation -= (int)((currentMousePosition.X - mouseOrigin.X) / 100);
                    }
                    else if (currentMousePosition.X < mouseOrigin.X - 100)
                    {
                        YRotation += (int)(Math.Abs(currentMousePosition.X - mouseOrigin.X) / 100);
                    }

                    if (currentMousePosition.Y > mouseOrigin.Y + 100)
                    {
                        XRotation -= (int)((currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    else if (currentMousePosition.Y < mouseOrigin.Y - 100)
                    {
                        XRotation += (int)(Math.Abs(currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    break;
                case 1:
                    if (currentMousePosition.X > mouseOrigin.X + 100)
                    {
                        YRotation -= (int)((currentMousePosition.X - mouseOrigin.X) / 100);
                    }
                    else if (currentMousePosition.X < mouseOrigin.X - 100)
                    {
                        YRotation += (int)(Math.Abs(currentMousePosition.X - mouseOrigin.X) / 100);
                    }

                    if (currentMousePosition.Y > mouseOrigin.Y + 100)
                    {
                        XRotation += (int)((currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    else if (currentMousePosition.Y < mouseOrigin.Y - 100)
                    {
                        XRotation -= (int)(Math.Abs(currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    break;
                case 2:
                case 3:
                    if (currentMousePosition.X > mouseOrigin.X + 100)
                    {
                        YRotation -= (int)((currentMousePosition.X - mouseOrigin.X) / 100);
                    }
                    else if (currentMousePosition.X < mouseOrigin.X - 100)
                    {
                        YRotation += (int)(Math.Abs(currentMousePosition.X - mouseOrigin.X) / 100);
                    }

                    if (currentMousePosition.Y > mouseOrigin.Y + 100)
                    {
                        XRotation += (int)((currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    else if (currentMousePosition.Y < mouseOrigin.Y - 100)
                    {
                        XRotation -= (int)(Math.Abs(currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    break;
                case 4:
                case 5:
                    if (currentMousePosition.X > mouseOrigin.X + 100)
                    {
                        YRotation -= (int)((currentMousePosition.X - mouseOrigin.X) / 100);
                    }
                    else if (currentMousePosition.X < mouseOrigin.X - 100)
                    {
                        YRotation += (int)(Math.Abs(currentMousePosition.X - mouseOrigin.X) / 100);
                    }

                    if (currentMousePosition.Y > mouseOrigin.Y + 100)
                    {
                        XRotation += (int)((currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    else if (currentMousePosition.Y < mouseOrigin.Y - 100)
                    {
                        XRotation -= (int)(Math.Abs(currentMousePosition.Y - mouseOrigin.Y) / 100);
                    }
                    break;
            }

            if (XRotation > 360)
            {
                XRotation = XRotation - 360;
            }
            else if (XRotation < 1)
            {
                XRotation = 360 - XRotation;
            }

            if (YRotation > 360)
            {
                YRotation = YRotation - 360;
            }
            else if (YRotation < 1)
            {
                YRotation = 360 - YRotation;
            }

            if (ZRotation > 360)
            {
                ZRotation = ZRotation - 360;
            }
            else if (ZRotation < 1)
            {
                ZRotation = 360 - ZRotation;
            }

            xRotationLabel.Text = "Xr: " + XRotation.ToString();
            yRotationLabel.Text = "Yr: " + YRotation.ToString();
            zRotationLabel.Text = "Zr: " + ZRotation.ToString();

            Render();
        }
        #endregion

        #region Keystroke Handlers
        //Handle the key commands for each rotation button
        private void cubixMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (keyShortcuts)
            {
                if (e.Modifiers != Keys.Shift)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.F:
                            frontCWButton_Click(this.frontCWButton, null);
                            break;
                        case Keys.Z:
                            zMiddleCWButton_Click(this.zMiddleCWButton, null);
                            break;
                        case Keys.B:
                            backCWButton_Click(this.backCWButton, null);
                            break;
                        case Keys.L:
                            leftCWButton_Click(this.leftCWButton, null);
                            break;
                        case Keys.X:
                            xMiddleCWButton_Click(this.xMiddleCWButton, null);
                            break;
                        case Keys.R:
                            rightCWButton_Click(this.rightCWButton, null);
                            break;
                        case Keys.U:
                            upCWButton_Click(this.upCWButton, null);
                            break;
                        case Keys.Y:
                            yMiddleCWButton_Click(this.yMiddleCWButton, null);
                            break;
                        case Keys.D:
                            bottomCWButton_Click(this.bottomCWButton, null);
                            break;
                    }
                }
                else
                {
                    switch (e.KeyCode)
                    {
                        case Keys.F:
                            frontCCWButton_Click(this.frontCCWButton, null);
                            break;
                        case Keys.Z:
                            zMiddleCCWButton_Click(this.zMiddleCCWButton, null);
                            break;
                        case Keys.B:
                            backCCWButton_Click(this.backCCWButton, null);
                            break;
                        case Keys.L:
                            leftCCWButton_Click(this.leftCCWButton, null);
                            break;
                        case Keys.X:
                            xMiddleCCWButton_Click(this.xMiddleCCWButton, null);
                            break;
                        case Keys.R:
                            rightCCWButton_Click(this.rightCCWButton, null);
                            break;
                        case Keys.U:
                            upCCWButton_Click(this.upCCWButton, null);
                            break;
                        case Keys.Y:
                            yMiddleCCWButton_Click(this.yMiddleCCWButton, null);
                            break;
                        case Keys.D:
                            bottomCCWButton_Click(this.bottomCCWButton, null);
                            break;
                    }
                }
            }
        }
        #endregion

        #region Secret Setting Activation
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            keyShortcuts = false;
            TextBox passwordEntry = new TextBox();
            this.Controls.Add(passwordEntry);

            passwordEntry.Location = new Point(this.Width / 2 - passwordEntry.Width / 2, this.Height / 2 - passwordEntry.Height / 2);
            passwordEntry.BringToFront();
            passwordEntry.MaxLength = 10;
            passwordEntry.PasswordChar = '⬤';
            passwordEntry.CharacterCasing = CharacterCasing.Lower;
            passwordEntry.TextAlign = HorizontalAlignment.Center;
            passwordEntry.Focus();

            passwordEntry.KeyPress += passwordEntry_KeyPress;
        }

        private void passwordEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            var passwordEntry = sender as TextBox;

            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                if (passwordEntry.Text == "itsasecret")
                {
                    toolStripSeparator5.Visible = true;
                    ultraSecretSettingToolStripMenuItem.Visible = true;
                    settingsToolStripMenuItem.ShowDropDown();
                    ultraSecretSettingToolStripMenuItem.Select();
                }
                this.Controls.Remove(passwordEntry);
                keyShortcuts = true;
                toolStripStatusLabel1.Visible = false;
            }
        }
        #endregion
    }
}
