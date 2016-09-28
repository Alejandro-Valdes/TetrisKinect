using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Threading;

namespace Tetris.Model.UI.DisplayBehaviours
{
    public class DisplayFlowFromRight : IDisplayBehaviour
    {
        private OverlayUserControl _this;

        #region Constructor
            public DisplayFlowFromRight(OverlayUserControl control)
            {
                _this = control;
            }
        #endregion
        
        /// <summary>
        /// Flow in the control from the right.
        /// </summary>
        public void Show()
        {
            var __width = _this.ActualWidth; //The controls width before any animations is the correct value because it is only hidden
            _this.Visibility = Visibility.Visible;

            #region Create the animation
                var __animation = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(1000),
                    From = 0,
                    To = __width,
                    FillBehavior = FillBehavior.Stop,
                };
            #endregion

            #region Create the completed event
                var __completed = new EventHandler((obj, args) =>
                {
                    _this.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _this.IsDisplayed = true;
                });
            #endregion

            ExecuteAnimation(__animation, __completed);
        }

        /// <summary>
        /// Flow out the given control to the right.
        /// </summary>
        public void Hide()
        {
            #region Create the animation
                var __animation = new DoubleAnimation()
                {
                    Duration = TimeSpan.FromMilliseconds(1000),
                    From = _this.ActualWidth,
                    To = 0,
                    FillBehavior = FillBehavior.Stop
                };
            #endregion

            #region Create the completed event
                var __completed = new EventHandler((obj, args) =>
                {
                    _this.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _this.Visibility = Visibility.Hidden;
                });
            #endregion

            _this.IsDisplayed = false;

            ExecuteAnimation(__animation, __completed);
        }

        private void ExecuteAnimation(DoubleAnimation animation, EventHandler completed)
        {
            #region Add the animation to the storyboard
            var __storyboard = new Storyboard();
                __storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, _this);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Control.Width)"));
            #endregion

            //Set the Alignment to right during the animation to make the control flowout to the right
            _this.HorizontalAlignment = HorizontalAlignment.Right;

            __storyboard.Completed += completed;
            __storyboard.Begin();
        }
    }
}
