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
using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;
using Tetris.Model;
using System.Threading;
using System.Windows.Threading;

namespace Tetris.Views
{
    /// <summary>
    /// Interaktionslogik für GameControl.xaml
    /// </summary>
    public partial class GameView : OverlayUserControl
    {
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
                Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/death.mp3", UriKind.Relative));
                ctrlGameOver.Score = score;
                ctrlGameOver.Show();
            }

            private void Tetris_IsPausedChanged()
            {
                if (Tetris.IsPaused)
                    ctrlPause.Show();
            }

            private void ResumeGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = true;
            }

            private void ResumeGame_Executed(object sender, ExecutedRoutedEventArgs e)
            {
                ctrlPause.Hide();
                Tetris.ResumeGame();
            }

            private void QuitGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = true;
            }

            private void QuitGame_Executed(object sender, ExecutedRoutedEventArgs e)
            {
                ctrlPause.Hide();
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
                        __this.Tetris.IsPausedChanged += new Model.Tetris.IsPausedChangedEventHandler(__this.Tetris_IsPausedChanged);
                    #endregion
                }
            }
        #endregion


    }
}
