using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;

namespace Tetris.Views
{

    /// <summary>
    /// Interaktionslogik für Titlescreen.xaml
    /// </summary>
    public partial class Titlescreen : UserControl
    {
        private KinectSensorChooser sensorChooser;
        /// <summary>
        /// miKinect,  this object represents the Kinect hooked up to the pc
        /// we use it to access the data of the different streams (Video, Depth, Skeleton)
        /// </summary>
        private KinectSensor miKinect;

        public Titlescreen()
        {
            InitializeComponent();
        }

        private void cmdCredits_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
