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

namespace Tetris.Views
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsView : OverlayUserControl
    {
        #region Dependency Properties
            public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(Model.Settings), typeof(SettingsView), new UIPropertyMetadata(null));

            public Model.Settings Settings
            {
                get { return (Model.Settings)GetValue(SettingsProperty); }
                set { SetValue(SettingsProperty, value); }
            }
        #endregion

        public SettingsView()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFlowFromRight(this);
        }

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Settings.Serialize();
        }

        /// <summary>
        /// Calculates new fontsize values. The solution via Viewbox failed here.
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grpSound.FontSize = this.ActualHeight / 38;
            grpControls.FontSize = this.ActualHeight / 38;
        }
    }
}
