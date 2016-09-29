using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Tetris.Model.UI
{
    public static class Commands
    {
        public static readonly RoutedCommand StartGame = new RoutedCommand();
        public static readonly RoutedCommand QuitApplication = new RoutedCommand();
        public static readonly RoutedCommand QuitGame = new RoutedCommand();
        public static readonly RoutedCommand EnterScores = new RoutedCommand();
    }
}
