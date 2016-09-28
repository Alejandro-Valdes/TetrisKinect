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

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für InfoControl.xaml
    /// </summary>
    public partial class InfoControl : UserControl
    {
        #region Fields
            public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(int), typeof(InfoControl), new UIPropertyMetadata(0));
            public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof(int), typeof(InfoControl), new UIPropertyMetadata(0));
            public static readonly DependencyProperty ClearedLinesProperty = DependencyProperty.Register("ClearedLines", typeof(int), typeof(InfoControl), new UIPropertyMetadata(0));
            public static readonly DependencyProperty NextBlockProperty = DependencyProperty.Register("NextBlock", typeof(Model.Block), typeof(InfoControl), new PropertyMetadata(new PropertyChangedCallback(NextBlockChanged)));    
        #endregion

        #region Properties
            public int Score
            {
                get { return (int)GetValue(ScoreProperty); }
                set { SetValue(ScoreProperty, value); }
            }

            public int Level
            {
                get { return (int)GetValue(LevelProperty); }
                set { SetValue(LevelProperty, value); }
            }

            public int ClearedLines
            {
                get { return (int)GetValue(ClearedLinesProperty); }
                set { SetValue(ClearedLinesProperty, value); }
            }

            public Model.Block NextBlock
            {
                get { return (Model.Block)GetValue(NextBlockProperty); }
                set { SetValue(NextBlockProperty, value); }
            }
        #endregion

        public InfoControl()
        {
            InitializeComponent();

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
        }

        #region Methods/Events
            private static void NextBlockChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                var __this = sender as Controls.InfoControl;

                if (__this != null && __this.NextBlock != null)
                {
                    __this.ClearGrid();
                    #region Visualize the NextBlock in the "mini grid"
                        foreach (Part p in __this.NextBlock.Parts)
                        {
                            var __uiPart = __this.grid.Children.Cast<Control>().Where(e => Grid.GetRow(e) == p.PosY - p.ParentBlock.PosY && Grid.GetColumn(e) == p.PosX - p.ParentBlock.PosX).Single();
                            __uiPart.Background = new SolidColorBrush(p.ParentBlock.Color);
                        }
                    #endregion
                }
            }

            private void ClearGrid()
            {
                var __controls = grid.Children.Cast<Control>().ToList();
                foreach (var c in __controls)
                {
                    c.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            /// <summary>
            /// Make sure the mini grid displaying the next block maintains a 1:1 ratio
            /// </summary>
            private void GroupBox_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                var __height = e.NewSize.Height - e.NewSize.Height * 0.2;
                var __width = e.NewSize.Width - e.NewSize.Width * 0.2;

                #region The smaller value determines the size
                    if (__height > __width)
                    {
                        grid.Height = __width;
                        grid.Width = __width;
                    }
                    else
                    {
                        grid.Height = __height;
                        grid.Width = __height;
                    }
                #endregion
            }

            /// <summary>
            /// Set the font size of the GroupBox headers dynamically
            /// </summary>
            private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
            {
                grpScore.FontSize = this.ActualHeight / 20;
                grpLevel.FontSize = this.ActualHeight / 20;
                grpLines.FontSize = this.ActualHeight / 20;
                grpNext.FontSize = this.ActualHeight / 20;
            }
        #endregion
    }
}
