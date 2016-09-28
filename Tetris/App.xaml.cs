using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace Tetris
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public string SerializationPath 
        {
            get
            {
                var __serializationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Sekagra\Tetris";
                if (!Directory.Exists(__serializationPath))
                    Directory.CreateDirectory(__serializationPath);

                return __serializationPath;
            }
        }
    }
}
