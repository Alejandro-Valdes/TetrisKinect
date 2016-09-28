using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.Model;
using System.Windows.Threading;

namespace Tetris.Views
{
    /// <summary>
    /// Wraps Events and Input controlling of the GameView
    /// (Not the display and animations, those are done by the TetrisGrid control)
    /// </summary>
    class GameViewController
    {
        private Dictionary<TetrisCommand, DispatcherTimer> _timers;
        private Model.Tetris _tetris;

        public GameViewController()
        {
            _timers = new Dictionary<TetrisCommand, DispatcherTimer>();
        }

        public GameViewController(Model.Tetris tetris) : this()
        {
            _tetris = tetris;
            _tetris.RowsCompleting += new Model.Tetris.RowsCompletingEventHandler(Tetris_RowsCompleting);
            _tetris.GameOver += new Model.Tetris.GameOverEventHandler(Tetris_GameOver);
            _tetris.IsPausedChanged += new Model.Tetris.IsPausedChangedEventHandler(Tetris_IsPausedChanged);

            _timers.Add(TetrisCommand.RIGHT, new DispatcherTimer(TimeSpan.FromMilliseconds(170), DispatcherPriority.Normal, InputKeyTickRight, Dispatcher.CurrentDispatcher));
            _timers.Add(TetrisCommand.LEFT, new DispatcherTimer(TimeSpan.FromMilliseconds(170), DispatcherPriority.Normal, InputKeyTickLeft, Dispatcher.CurrentDispatcher));
            _timers.Add(TetrisCommand.DOWN, new DispatcherTimer(TimeSpan.FromMilliseconds(75), DispatcherPriority.Normal, InputKeyTickDown, Dispatcher.CurrentDispatcher));
            //For some reason the timers created using this constructor seem to start directly without Start()
            foreach (var __timer in _timers.Values)
                __timer.Stop();
        }

        public void KeyDown(TetrisCommand command)
        {
            if (_tetris != null && !_tetris.IsPaused)
            {
                //If the current command has a corresponding timer and it is already active, than don't do anything
                if (_timers.ContainsKey(command) && _timers[command].IsEnabled) return;

                #region Check all known commands
                    switch (command)
                    {
                        case TetrisCommand.ROTATE:
                            Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/rotate.wav", UriKind.Relative));
                            _tetris.Rotate();
                            break;
                        case TetrisCommand.DOWN:
                            _tetris.MoveDown(this);
                            break;
                        case TetrisCommand.LEFT:
                            Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/move.wav", UriKind.Relative));
                            _tetris.MoveLeft();
                            break;
                        case TetrisCommand.RIGHT:
                            Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/move.wav", UriKind.Relative));
                            _tetris.MoveRight();
                            break;
                        case TetrisCommand.PAUSE:
                            _tetris.PauseGame();
                            break;
                        default:
                            break;
                    }
                #endregion

                //If the current command has a timer and it was executed, start the timer
                if (_timers.ContainsKey(command)) _timers[command].Start();
            }
        }

        public void KeyUp(TetrisCommand command)
        {
            if (_timers.ContainsKey(command)) _timers[command].Stop();

            _tetris.ResetSoftDrop();
        }

        #region Tetris object's events
            //Stop all timers
            private void Tetris_RowsCompleting(int[] rows)
            {
                foreach (var __timer in _timers.Values)
                    __timer.Stop();
            }

            private void Tetris_GameOver(int score)
            {
                foreach (var __timer in _timers.Values)
                    __timer.Stop();
            }

            private void Tetris_IsPausedChanged()
            {
                if(_tetris.IsPaused)
                    foreach (var __timer in _timers.Values)
                        __timer.Stop();
            }
        #endregion

        #region InputTicks
            private void InputKeyTickRight(object sender, EventArgs e)
            {
                Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/move.wav", UriKind.Relative));
                _tetris.MoveRight();
            }

            private void InputKeyTickLeft(object sender, EventArgs e)
            {
                Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/move.wav", UriKind.Relative));
                _tetris.MoveLeft();
            }

            private void InputKeyTickDown(object sender, EventArgs e)
            {
                _tetris.MoveDown(this);
            }
        #endregion
    }
}
