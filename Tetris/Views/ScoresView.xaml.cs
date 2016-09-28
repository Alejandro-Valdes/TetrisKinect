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

namespace Tetris.Views
{
    /// <summary>
    /// Interaktionslogik für ScoresView.xaml
    /// </summary>
    public partial class ScoresView : OverlayUserControl
    {
        public ScoresView()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFlowFromRight(this);
        }

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        } 
        
        public new void Show()
        {
            base.Show();

            dgrHighscores.ItemsSource = null;
            dgrHighscores.ItemsSource = Highscores.Instance.Scores.OrderByDescending(s => s.Points);
        }

        private void dgrHighscores_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (dgrHighscores.ActualHeight > 23)
                dgrHighscores.RowHeight = (dgrHighscores.ActualHeight - 33) / 10;
        }
    }
}
