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

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für PauseScreen.xaml
    /// </summary>
    public partial class PauseScreen : OverlayUserControl
    {
        public PauseScreen()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFadeIn(this);
        }
    }
}
