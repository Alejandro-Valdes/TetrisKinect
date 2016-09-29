using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tetris.Model;
using System.Windows.Media.Animation;
using System.ComponentModel;
using Tetris.Sounds;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Threading;
using Tetris.Views;

namespace Tetris
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields
            double mainX;
            double mainY;
            Point coordenadaJoint = new Point();
            DispatcherTimer timer;
            Skeleton skeletonManos;
            /// <summary>
            /// miKinect,  this object represents the Kinect hooked up to the pc
            /// we use it to access the data of the different streams (Video, Depth, Skeleton)
            /// </summary>
            private KinectSensor miKinect;

            private Settings _settings;
            public Settings Settings 
            {
                get { return _settings; }
                set { _settings = value; OnPropertyChanged("Settings"); }
            }
        #endregion

        #region Constructor
            public MainWindow()
            {
                InitializeComponent();
                Settings = Settings.Instance;
            }
        #endregion

        #region Events
            private void Window_Loaded(object sender, RoutedEventArgs e)
            {
                //Start playing the background music
                //Settings.MusicPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Music/Gee.mp3", UriKind.Relative));

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

                        timer = new DispatcherTimer();
                        timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
                        timer.Tick += new EventHandler(checaManos);
                        timer.Tick += new EventHandler(checaPecho);
                        timer.IsEnabled = true;

                        LineaIzq.X1 = this.ActualWidth * .3;
                        LineaIzq.X2 = LineaIzq.X1;
                        LineaIzq.Y1 = 0;
                        LineaIzq.Y2 = this.ActualHeight;

                        LineaDer.X1 = this.ActualWidth * .6;
                        LineaDer.X2 = LineaDer.X1;
                        LineaDer.Y1 = 0;
                        LineaDer.Y2 = this.ActualHeight;

                        LineaCadera.X1 = 0;
                        LineaCadera.X2 = this.ActualWidth;
                        LineaCadera.Y1 = this.ActualHeight * .5;
                        LineaCadera.Y2 = LineaCadera.Y1;

                        LineaManos.X1 = 0;
                        LineaManos.X2 = this.ActualWidth;
                        LineaManos.Y1 = this.ActualHeight * .5;
                        LineaManos.Y2 = LineaManos.Y1;

                }
                    catch (IOException)
                    {
                        this.miKinect = null;
                    }
                }
        }

            #region Execute incoming registered RoutedCommands
                private void StartGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = true;
                }

                private void StartGame_Executed(object sender, ExecutedRoutedEventArgs e)
                {
                    viewGame.Tetris = new Model.Tetris();
                    viewGame.Show();
                    viewGame.Tetris.StartGame();
                }


                private void EnterSettings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = true;
                }

                private void EnterSettings_Executed(object sender, ExecutedRoutedEventArgs e)
                {
                    viewSettings.Show();
                }


                private void QuitApplication_CanExecute(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = true;
                }

                private void QuitApplication_Executed(object sender, ExecutedRoutedEventArgs e)
                {
                    Application.Current.Shutdown();
                }

                private void EnterScores_CanExecute(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = true;
                }

                private void EnterScores_Executed(object sender, ExecutedRoutedEventArgs e)
                {
                    viewScores.Show();
                }

                private void EnterCredits_CanExecute(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = true;
                }

                private void EnterCredits_Executed(object sender, ExecutedRoutedEventArgs e)
                {
                    viewCredits.Show();
                }
            #endregion

            //Pass Key-Events to the Game
            private void Window_KeyDown(object sender, KeyEventArgs e)
            {
                var __key = Settings.KeySettings.FirstOrDefault(k => k.Key == e.Key);
                if (viewGame.Tetris != null && __key != null)
                    viewGame.Game_KeyDown(__key.Command);
            }

            private void Window_KeyUp(object sender, KeyEventArgs e)
            {
                var __key = Settings.KeySettings.FirstOrDefault(k => k.Key == e.Key);
                if (viewGame.Tetris != null && __key != null)
                    viewGame.Game_KeyUp(__key.Command);
            }

            private void Window_Deactivated(object sender, EventArgs e)
            {
                if(viewGame.IsDisplayed && !viewGame.Tetris.IsPaused)
                    viewGame.Tetris.PauseGame();
            }

        private void Window_Closed(object sender, EventArgs e)
            {
                //Serialize the settings
                //Delete the temp files
                Settings.Instance.MusicPlayer.Dispose();
                Settings.Instance.SoundPlayer.Dispose();

                if (null != this.miKinect)
                {
                    this.miKinect.Stop();
                }
        }   

            #region OnPropertyChanged Event
                public event PropertyChangedEventHandler PropertyChanged;

                /// <summary>
                /// Wraps the PropertyChanged-Event.
                /// </summary>
                /// <param name="property">The name of the property that changed.</param>
                private void OnPropertyChanged(string property)
                {
                    var handler = PropertyChanged;
                    if (handler != null)
                        handler(this, new PropertyChangedEventArgs(property));
                }
        #endregion
        #endregion

        #region Kinect Methods
        /// <summary>
        /// Manejador del evento SkeletonFrameReady
        /// </summary>
        private void miKinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Creamos el objeto Skeleton que usaremos para recibir el frame 
            Skeleton[] skeletons = new Skeleton[0];

            //Abrimos el frame que se ha recibido y lo copiamos a nuestro objeto skeletons
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
                foreach (Skeleton skeletonEncontrado in skeletons)
                {
                    if (skeletonEncontrado.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        this.obtenerCoordenadaDeJoint(skeletonEncontrado);
                    }

                }
            }

        }


        /// <summary>
        /// Obtiene las coordenadas del Joint seleccionado
        /// </summary>
        private void obtenerCoordenadaDeJoint(Skeleton skeleton)
        {
            skeletonManos = skeleton;
            //Creamos un Joint para acceder al Joint especificado por tipoJointDeseado y obtener sus propiedades (Position, con esta podemos abtener las coordenadas)
            var misJoints = new List<JointType> { JointType.ShoulderCenter, JointType.HandRight, JointType.HandLeft, JointType.HipCenter };
            var misPunteros = new List<UIElement> { PunteroPecho, PunteroManoDer, PunteroManoIzq, PunteroCadera };
            Joint miJoint;

            var jointsYpuntero = misJoints.Zip(misPunteros, (j, p) => new { Joint = j, Puntero = p });
            foreach (var jp in jointsYpuntero)
            {
                miJoint = skeleton.Joints[jp.Joint];
                dibujaJoint(miJoint, jp.Puntero);
            }

        }

        private void checaManos(object sender, EventArgs e)
        {

            if (skeletonManos != null)
            {
                Joint miManoIzq = skeletonManos.Joints[JointType.HandLeft];
                Joint miManoDer = skeletonManos.Joints[JointType.HandRight];

                coordenadaJoint = this.SkeletonPointToScreen(miManoIzq.Position);
                double mainXIzq = coordenadaJoint.X;
                double mainYIzq = coordenadaJoint.Y;

                coordenadaJoint = this.SkeletonPointToScreen(miManoDer.Position);
                double mainXDer = coordenadaJoint.X;
                double mainYDer = coordenadaJoint.Y;

                var rotate = Tetris.Model.TetrisCommand.ROTATE;

                if (viewGame.Tetris != null)
                {
                    if (mainYDer < (this.ActualHeight * .5) && mainYIzq < (this.ActualHeight * .5))
                    {
                        if (mainYIzq < mainYDer && (mainYDer - mainYIzq > 50))
                        {
                            viewGame.Game_KeyDown(rotate);
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        viewGame.Game_KeyUp(rotate);
                    }
                }
            }
        }

        private void checaPecho(object sender, EventArgs e)
        {
            var left = Tetris.Model.TetrisCommand.LEFT;
            var right = Tetris.Model.TetrisCommand.RIGHT;

            int iWidth = (int)this.ActualWidth;
            int iHeight = (int)this.ActualHeight;

            int ikChestLeftRegionEndX = (int)(iWidth * 0.3);
            int ikChestRightReginBeginX = (int)(iWidth * 0.6);


            if (skeletonManos != null)
            {
                Joint miPecho = skeletonManos.Joints[JointType.ShoulderCenter];

                if (viewGame.Tetris != null)
                {
                    if (mainX < ikChestLeftRegionEndX && mainX >= 0)
                    {
                        viewGame.Game_KeyDown(left);
                    }
                    else if (mainX > ikChestRightReginBeginX && mainX <= this.ActualWidth)
                    {
                        viewGame.Game_KeyDown(right);
                    }
                    else
                    {
                        viewGame.Game_KeyUp(left);
                        viewGame.Game_KeyUp(right);
                    }
                }
            }
        }

        private void dibujaJoint(Joint miJoint, UIElement puntero)
        {
            int iWidth = (int)this.ActualWidth;
            int iHeight = (int)this.ActualHeight;

            int ikHipCenterRegionEndY = (int)(iHeight * 0.5);

            // Si el Joint esta listo obtenemos sus coordenadas y pasamos a visualizalo en el Canvas
            if (miJoint.TrackingState == JointTrackingState.Tracked)
            {
                coordenadaJoint = this.SkeletonPointToScreen(miJoint.Position);
                mainX = coordenadaJoint.X;
                mainY = coordenadaJoint.Y;

                puntero.SetValue(Canvas.TopProperty, mainY - 12.5);
                puntero.SetValue(Canvas.LeftProperty, mainX - 12.5);

                var down = Tetris.Model.TetrisCommand.DOWN;

                if (miJoint.JointType.Equals(JointType.HipCenter) && viewGame.Tetris != null)
                {
                    if(mainY > ikHipCenterRegionEndY)
                    {
                        viewGame.Game_KeyDown(down);
                    }
                    else
                    {
                        viewGame.Game_KeyUp(down);
                    }
                }
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




        #endregion

    }
}
