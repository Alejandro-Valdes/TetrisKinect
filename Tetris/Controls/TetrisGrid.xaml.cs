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
using Tetris.Model;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für TetrisGrid.xaml
    /// </summary>
    public partial class TetrisGrid : UserControl
    {
        #region Fields
            public static readonly DependencyProperty TetrisProperty = DependencyProperty.Register("Tetris", typeof(Tetris.Model.Tetris), typeof(TetrisGrid), new PropertyMetadata(new PropertyChangedCallback(Tetris_Changed)));
        #endregion

        #region Properties
            public Tetris.Model.Tetris Tetris
            {
                get { return (Tetris.Model.Tetris)GetValue(TetrisProperty); }
                set { SetValue(TetrisProperty, value); }
            }
        #endregion

        public TetrisGrid()
        {
            InitializeComponent();

            #region Populate the grid with parts
                for (int i = 0; i < grid.RowDefinitions.Count(); i++)
			    {
                    for (int j = 0; j < grid.ColumnDefinitions.Count(); j++)
                    {
                        #region Create a new label as "part" and add it to the grid
                            Label __lbl = new Label();
                            __lbl.Background = new SolidColorBrush(Colors.Transparent);
                            __lbl.BorderBrush = new SolidColorBrush(Colors.Transparent);
                            __lbl.BorderThickness = new Thickness(1.0);
                            grid.Children.Add(__lbl);
                            Grid.SetRow(__lbl, i);
                            Grid.SetColumn(__lbl, j);
                        #endregion
                    }
			    }
            #endregion
        }

        #region Methods
            private void Tetris_NewBlockAdded()
            {
                PaintCurrentBlock();
            }

            private void Tetris_BlockMoving()
            {
                RemoveCurrentBlock();
            }

            private void Tetris_BlockMoved()
            {
                PaintCurrentBlock();
            }           

            private void Tetris_RowsCompleting(int[] rows)
            {
                //No Need for this event right now.
                //The DeleteRow method just finishes completly before invoking the CompletedEvent
            }

            private void Tetris_RowsCompleted(int[] rows)
            {
                //Get all parts
                var __rowParts = grid.Children.Cast<Control>().Where(e => rows.Contains(Grid.GetRow(e))).ToList();

                //Use a WPF Storyboard to display an effect on the row (Thread.Sleep() after setting the opacity will just not display anything)
                Storyboard __storyboard = new Storyboard();

                foreach (var p in __rowParts)
                {
                    #region Add the animation for every part in the row to the Storyboard
                        //This also allows to make a more advanced effect like let the row slowly fade away
                        #region Create the animtion
                            DoubleAnimation __animation = new DoubleAnimation() 
                            {
                                Duration = TimeSpan.FromMilliseconds(1000), 
                                From = 1, To = 0, 
                                FillBehavior = FillBehavior.Stop    //Necessary, or the programm won't be able to reset the opacity
                            };
                        #endregion

                        __storyboard.Children.Add(__animation);
                        Storyboard.SetTarget(__animation, p);
                        Storyboard.SetTargetProperty(__animation, new PropertyPath("(Control.Opacity)"));
                    #endregion                   
                }

                #region Start the redraw of all parts after the Storyboard finished
                    __storyboard.Completed += new EventHandler((obj, args) =>
                    {
                        ClearGrid();
                        RedrawGrid();
                    });
                #endregion

                __storyboard.Begin();

                Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/row.wav", UriKind.Relative));

                #region Extremly strange method making the whole thread wait but somehow manages not to delay the animation
                    //This allows to pause the whole Tetris class without putting UI stuff in it, so no new Block is generated until this finishes
                    //The ticks are measured in nanoseconds and have to match the animations Duration property
                    long dtEnd = DateTime.Now.AddTicks(10000000).Ticks;

                    while (DateTime.Now.Ticks < dtEnd)
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object unused) { return null; }, null);
                    }
                #endregion
            }

            private void Tetris_RoundFinished()
            {
                Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/drop.mp3", UriKind.Relative));
            }

            /// <summary>
            /// Draw the CurrentBlock on the grid
            /// Not a good method to get the UIElements, because the complete Grid gets iterated through)
            /// </summary>
            private void PaintCurrentBlock()
            {
                foreach (Part p in Tetris.CurrentBlock.Parts)
                {
                    var __uiPart = grid.Children.Cast<Control>().Where(e => Grid.GetRow(e) == p.PosY && Grid.GetColumn(e) == p.PosX).Single();
                    __uiPart.Background = new SolidColorBrush(p.ParentBlock.Color);
                }
            }

            /// <summary>
            /// Remove the CurrentBlock from the grid
            /// Not a good method to get the UIElements, because the complete Grid gets iterated through)    
            /// </summary>
            private void RemoveCurrentBlock()
            {
                foreach (Part p in Tetris.CurrentBlock.Parts)
                {
                    var __uiPart = grid.Children.Cast<Control>().Where(e => Grid.GetRow(e) == p.PosY && Grid.GetColumn(e) == p.PosX).Single();
                    __uiPart.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            /// <summary>
            /// Clear the complete Grid (reset all colors and the opacity).
            /// (Needed either when the game is restarted or a row is complete).
            /// </summary>
            private void ClearGrid()
            {
                var __controls = grid.Children.Cast<Control>().ToList();
                foreach(var c in __controls)
                {
                    c.Opacity = 1;
                    c.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            /// <summary>
            /// Redraw every part in the Tetris.Grid
            /// </summary>
            private void RedrawGrid()
            {
                #region Redraw every part
                    foreach (var p in Tetris.Grid)
                    {
                        var __uiPart = grid.Children.Cast<Control>().Where(e => Grid.GetRow(e) == p.PosY && Grid.GetColumn(e) == p.PosX).Single();
                        __uiPart.Background = new SolidColorBrush(p.ParentBlock.Color);
                    }
                #endregion
            }
            
            private static void Tetris_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                var __this = sender as Controls.TetrisGrid;

                __this.ClearGrid();
   
                if (__this != null)
                {
                    var __tetris = (Model.Tetris)args.NewValue;
                    #region Register the Tetris-Objects events
                        __tetris.NewBlockAdded += new Model.Tetris.NewBlockEventHandler(__this.Tetris_NewBlockAdded);
                        __tetris.BlockMoved += new Model.Tetris.BlockMovedEventHandler(__this.Tetris_BlockMoved);
                        __tetris.BlockMoving += new Model.Tetris.BlockMovingEventHandler(__this.Tetris_BlockMoving);

                        __tetris.RowsCompleting += new Model.Tetris.RowsCompletingEventHandler(__this.Tetris_RowsCompleting);
                        __tetris.RowsCompleted += new Model.Tetris.RowsCompletedEventHandler(__this.Tetris_RowsCompleted);

                        __tetris.RoundFinished += new Model.Tetris.RoundFinishedEventHandler(__this.Tetris_RoundFinished);
                    #endregion
                }
            }
        #endregion
    }
}
