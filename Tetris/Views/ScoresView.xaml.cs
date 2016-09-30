using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using System;
using System.Linq;
using System.Windows;
using Tetris.Model;
using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;

namespace Tetris.Views
{
    /// <summary>
    /// Interaktionslogik für ScoresView.xaml
    /// </summary>
    public partial class ScoresView : OverlayUserControl
    {
        /// <summary>
        /// miKinect,  this object represents the Kinect hooked up to the pc
        /// we use it to access the data of the different streams (Video, Depth, Skeleton)
        /// </summary>
        private KinectSensor miKinect;

        public ScoresView()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFlowFromRight(this);
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
                //this.miKinect.SkeletonStream.Enable();
                this.miKinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                try
                {
                    kinectRegion.KinectSensor = this.miKinect;
                }
                catch (Exception error)
                {

                };
            }
        }

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
