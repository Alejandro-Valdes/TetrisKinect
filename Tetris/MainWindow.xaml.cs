using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Tetris.Model;

namespace Tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region Fields
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


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Start playing the background music
            Settings.MusicPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Music/Gee.mp3", UriKind.Relative));
        }
        #region Events


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
            if (viewGame.IsDisplayed && !viewGame.Tetris.IsPaused)
                viewGame.Tetris.PauseGame();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Serialize the settings
            //Delete the temp files
            Settings.Instance.MusicPlayer.Dispose();
            Settings.Instance.SoundPlayer.Dispose();
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

      
    }
}
