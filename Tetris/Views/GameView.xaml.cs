using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Tetris.Model;
using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;

namespace Tetris.Views
{
    /// <summary>
    /// Interaktionslogik für GameControl.xaml
    /// </summary>
    public partial class GameView : OverlayUserControl
    {
        #region Kinect Fields

        /// <summary>
        /// miKinect,  this object represents the Kinect hooked up to the pc
        /// we use it to access the data of the different streams (Video, Depth, Skeleton)
        /// </summary>
        private KinectSensor miKinect;

        Point coordenadaJoint = new Point();
        DispatcherTimer timer;
        Skeleton skeletonKinect;

        double mainX;
        double mainY;

        int iLeftAreaBeginX;
        int iLeftAreaEndX;
        int iRightAreaBeginX;
        int iRigthAreaEndX;
        int iHipAreaBeginY;
        int iHipAreaEndY;
        int iHandsAreaBeginY;
        int iHandsAreaEndY;

        Joint jntHip;
        Joint jntHead;
        Joint myHandL;
        Joint myHandR;

        #endregion

        #region Fields
        GameViewController _controller;
        public static readonly DependencyProperty TetrisProperty = DependencyProperty.Register("Tetris", typeof(Model.Tetris), typeof(GameView), new PropertyMetadata(new PropertyChangedCallback(Tetris_Changed)));
        #endregion

        #region Properties
        public Model.Tetris Tetris
        {
            get { return (Model.Tetris)GetValue(TetrisProperty); }
            set { SetValue(TetrisProperty, value); }
        }
        #endregion

        #region Constructor
        public GameView()
        {
            InitializeComponent();

            DisplayBehaviour = new DisplayFlowFromRight(this);
            _controller = new GameViewController(); //The DependencyProperty changed ensures, that an actual instance with Tetris will be created
        }
        #endregion

        #region Methods/Events
        /// <summary>
        /// Only real tetris commands reach this method.
        /// </summary>
        /// <param name="command"></param>
        public void Game_KeyDown(TetrisCommand command)
        {
            _controller.KeyDown(command);
        }

        /// <summary>
        /// Only real tetris commands reach this method.
        /// </summary>
        public void Game_KeyUp(TetrisCommand command)
        {
            _controller.KeyUp(command);
        }

        private void Tetris_GameOver(int score)
        {
            var uiElements = new List<UIElement> { PointHead, PointHandR, PointHandL, PointHip, LineL, LineR, LineHip, LineHands };
            foreach(var uiElement in uiElements)
            {
                uiElement.Visibility = Visibility.Hidden;
            }

            Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/death.mp3", UriKind.Relative));
            ctrlGameOver.Score = score;
            ctrlGameOver.Show();
            
        }


        private void QuitGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void QuitGame_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ctrlGameOver.Hide();
            
            this.Hide();
        }


        private static void Tetris_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = sender as Views.GameView;
            __this.DataContext = __this.Tetris;

            if (__this.Tetris != null)
            {
                __this._controller = new GameViewController(__this.Tetris);

                #region Register some Event Handlers here (GameOver and Pause)
                __this.Tetris.GameOver += new Model.Tetris.GameOverEventHandler(__this.Tetris_GameOver);
                #endregion
            }
        }
        #endregion

        #region Kinect Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Buscamos el Kinect conectado a la computadora, mediante la propiedad KinectSensors, al descubrir el primer elemento con el estado Connected
            // lo asignamos a nuestro objeto declarado al inicio (KinectSensor miKinect)
            foreach (KinectSensor kinectConectado in KinectSensor.KinectSensors)
            {
                if (kinectConectado.Status == KinectStatus.Connected)
                {
                    this.miKinect = kinectConectado;
                    break;
                }
            }

            if (null != this.miKinect)
            {
                // Habilitamos el Stream de Skeleton
                this.miKinect.SkeletonStream.Enable();

                this.miKinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);


                // Asignamos el event handler que se llamara cada vez que SkeletonStream tenga un frame de datos disponible 
                this.miKinect.SkeletonFrameReady += this.miKinectSkeletonFrameReady;

                // Iniciamos miKinect
                try
                {
                    this.miKinect.Start();

                    //tick timer
                    timer = new DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    timer.Tick += new EventHandler(checkHands);
                    timer.Tick += new EventHandler(checkHead);
                    timer.Tick += new EventHandler(checkHip);
                    timer.IsEnabled = true;

                    //Chest Limits
                    iLeftAreaBeginX = 0;
                    iLeftAreaEndX = (int)(this.ActualWidth * 0.43);
                    iRightAreaBeginX = (int)(this.ActualWidth * 0.58);
                    iRigthAreaEndX = (int)this.ActualWidth;

                    //Hip Limits
                    iHipAreaBeginY = (int)(this.ActualHeight * 0.5);
                    iHipAreaEndY = (int)(this.ActualHeight);

                    //Hands Limits
                    iHandsAreaBeginY = 0;
                    iHandsAreaEndY = (int)(this.ActualHeight * 0.35);

                    drawLines();


                }
                catch (IOException)
                {
                    this.miKinect = null;
                }

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (null != this.miKinect)
            {
                this.miKinect.Stop();
            }
        }

        #endregion

        #region Kinect Methods
        /// <summary>
        /// Controls the SkeletonFrameReady event
        /// </summary>
        private void miKinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //create the Skeleton object that will get the frame
            Skeleton[] skeletons = new Skeleton[0];

            //We open the frame and we coky to our skeletons object
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            if (skeletons.Length != 0)
            {
                foreach (Skeleton foundSkeleton in skeletons)
                {
                    if (foundSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        this.getJointCoordinates(foundSkeleton);
                        drawLines();
                    }
                }
            }

        }


        /// <summary>
        /// Gets the coordinates of the selected joint
        /// </summary>
        private void getJointCoordinates(Skeleton skeleton)
        {
            skeletonKinect = skeleton;
            jntHip = skeletonKinect.Joints[JointType.HipCenter];
            jntHead = skeletonKinect.Joints[JointType.Head];
            myHandL = skeletonKinect.Joints[JointType.HandLeft];
            myHandR = skeletonKinect.Joints[JointType.HandRight];

            var myJoints = new List<JointType> { JointType.Head, JointType.HandRight, JointType.HandLeft, JointType.HipCenter };
            var myPoints = new List<UIElement> { PointHead, PointHandR, PointHandL, PointHip };
            Joint myJoint;

            //create a dictionary that boundles up joints with the points
            var jointsAndPoints = myJoints.Zip(myPoints, (j, p) => new { Joint = j, Puntero = p });
            foreach (var jp in jointsAndPoints)
            {
                myJoint = skeleton.Joints[jp.Joint];
                drawJoint(myJoint, jp.Puntero);
            }

        }

        //Check the hands
        private void checkHands(object sender, EventArgs e)
        {
            int iDeltaHands = 50;

            if (skeletonKinect != null)
            {
                coordenadaJoint = this.SkeletonPointToScreen(myHandL.Position);
                double mainXL = coordenadaJoint.X;
                double mainYL = coordenadaJoint.Y;

                coordenadaJoint = this.SkeletonPointToScreen(myHandR.Position);
                double mainXR = coordenadaJoint.X;
                double mainYR = coordenadaJoint.Y;

                if (mainYR < iHandsAreaEndY && mainYL < iHandsAreaEndY)
                {
                    if (mainYL < mainYR && (mainYR - mainYL > iDeltaHands))
                    {
                        kinectCommand("ROTATE");
                    }
                }
            }
        }

        //check the head
        private void checkHead(object sender, EventArgs e)
        {

            if (skeletonKinect != null)
            {
                coordenadaJoint = this.SkeletonPointToScreen(jntHead.Position);
                double mainXHead = coordenadaJoint.X;

                if (mainXHead < iLeftAreaEndX)
                {
                    kinectCommand("LEFT");
                }
                else if (mainXHead > iRightAreaBeginX)
                {
                    kinectCommand("RIGHT");
                }
            }
        }

        //check the hip
        private void checkHip(object sender, EventArgs e)
        {
            if (skeletonKinect != null)
            {
                coordenadaJoint = this.SkeletonPointToScreen(jntHip.Position);
                double mainYHip = coordenadaJoint.Y;

                if (mainYHip > iHipAreaBeginY)
                {
                    kinectCommand("DOWN", "down");
                }
                else
                {
                    kinectCommand("DOWN", "up");
                }
            }
        }

        private void kinectCommand(String strCommand, String cmdType = "both")
        {
            var command = TetrisCommand.LEFT;
            bool bError = false;

            switch (strCommand)
            {
                case "LEFT":
                    command = TetrisCommand.LEFT;
                    break;
                case "RIGHT":
                    command = TetrisCommand.RIGHT;
                    break;
                case "DOWN":
                    command = TetrisCommand.DOWN;
                    break;
                case "ROTATE":
                    command = TetrisCommand.ROTATE;
                    break;
                default:
                    bError = true;
                    break;
            }

            if (!bError)
            {
                if (cmdType.Equals("both"))
                {
                    _controller.KeyDown(command);
                    _controller.KeyUp(command);
                }
                else if (cmdType.Equals("down"))
                {
                    _controller.KeyDown(command);
                }
                else if (cmdType.Equals("up"))
                {
                    _controller.KeyUp(command);
                }
            }

        }

        private void drawJoint(Joint jntJoint, UIElement pointer)
        {
            // Si el Joint esta listo obtenemos sus coordenadas y pasamos a visualizalo en el Canvas
            if (jntJoint.TrackingState == JointTrackingState.Tracked)
            {
                coordenadaJoint = this.SkeletonPointToScreen(jntJoint.Position);
                mainX = coordenadaJoint.X;
                mainY = coordenadaJoint.Y;

                pointer.SetValue(Canvas.TopProperty, mainY - 12.5);
                pointer.SetValue(Canvas.LeftProperty, mainX - 12.5);
            }
        }

        /// <summary>
        /// Mapea un SkeletonPoint ajustandolo a las dimensiones de deseamos
        /// </summary>
        private Point SkeletonPointToScreen(SkeletonPoint posicionDeJoint)
        {
            // Convierte la posicion del Joint a "depth space".  
            // Ajustamos la posicion a una resolucion de  640x480.
            DepthImagePoint depthPoint = this.miKinect.CoordinateMapper.MapSkeletonPointToDepthPoint(posicionDeJoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        } 

        private void drawLines()
        {
            LineL.X1 = iLeftAreaEndX;
            LineL.X2 = iLeftAreaEndX;
            LineL.Y1 = 0;
            LineL.Y2 = this.ActualHeight;

            LineR.X1 = iRightAreaBeginX;
            LineR.X2 = iRightAreaBeginX;
            LineR.Y1 = 0;
            LineR.Y2 = this.ActualHeight;

            LineHip.X1 = 0;
            LineHip.X2 = this.ActualWidth;
            LineHip.Y1 = iHipAreaBeginY;
            LineHip.Y2 = iHipAreaBeginY;

            LineHands.X1 = 0;
            LineHands.X2 = this.ActualWidth;
            LineHands.Y1 = iHandsAreaEndY;
            LineHands.Y2 = iHandsAreaEndY;

            var uiElements = new List<UIElement> { PointHead, PointHandR, PointHandL, PointHip, LineL, LineR, LineHip, LineHands };
            foreach (var uiElement in uiElements)
            {
                uiElement.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
