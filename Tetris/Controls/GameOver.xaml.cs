using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System.Windows;
using Tetris.Model;
using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für GameOver.xaml
    /// </summary>
    public partial class GameOver : OverlayUserControl
    {
        /// <summary>
        /// miKinect,  this object represents the Kinect hooked up to the pc
        /// we use it to access the data of the different streams (Video, Depth, Skeleton)
        /// </summary>
        private KinectSensor miKinect;

        #region Dependency Properties
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(int), typeof(GameOver), new PropertyMetadata(new PropertyChangedCallback(Score_Changed)));

            public int Score
            {
                get { return (int)GetValue(ScoreProperty); }
                set { SetValue(ScoreProperty, value); }
            }
        #endregion
        
        public GameOver()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFadeIn(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                kinectRegion.KinectSensor = this.miKinect;
            }
        }

        private void cmdSubmit_Click(object sender, RoutedEventArgs e)
        {
            Highscores.Instance.Add(Score, txtName.Text);
            grpName.Visibility = Visibility.Collapsed;
        }

        private static void Score_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = (GameOver)sender;

            if (Highscores.Instance.CheckScore((int)args.NewValue))
                __this.grpName.Visibility = Visibility.Visible;
            else
                __this.grpName.Visibility = Visibility.Hidden;
        }

        private void cmdQuit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
