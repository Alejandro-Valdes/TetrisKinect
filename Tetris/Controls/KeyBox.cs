using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Tetris.Controls
{
    public class KeyBox : TextBox
    {
        #region Fields/Properties
            public static readonly DependencyProperty KeySettingProperty = DependencyProperty.Register("KeySetting", typeof(Model.KeySetting), typeof(KeyBox), new UIPropertyMetadata(new PropertyChangedCallback(KeySetting_Changed)));

            public Model.KeySetting KeySetting
            {
                get { return (Model.KeySetting)GetValue(KeySettingProperty); }
                set { SetValue(KeySettingProperty, value); }
            }
        #endregion


        #region Constructor
            public KeyBox()
            {
                this.GotFocus += new RoutedEventHandler(KeyBox_GotFocus);
                this.LostFocus += new RoutedEventHandler(KeyBox_LostFocus);
                this.KeyDown += new System.Windows.Input.KeyEventHandler(KeyBox_KeyDown);
                #region Set properties to specify the look of the control
                    this.IsReadOnly = true;
                    this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                #endregion
            }
        #endregion

        private static void KeySetting_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = (KeyBox)sender;
            __this.FontStyle = FontStyles.Normal;
            __this.Foreground = new SolidColorBrush(Colors.Black);
            __this.Text = __this.KeySetting.Key.ToString();
        }

        private void KeyBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.FontStyle = FontStyles.Italic;
            this.Foreground = new SolidColorBrush(Colors.Gray);
            this.Text = "Press the new key for this command.";
        }

        private void KeyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.FontStyle = FontStyles.Normal;
            this.Foreground = new SolidColorBrush(Colors.Black);
            this.Text = KeySetting.Key.ToString();
        }

        private void KeyBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Changing the Key seems not to trigger the changed event, also every Property involved executes PropertyChanged...
            KeySetting = new Model.KeySetting(e.Key, KeySetting.Command);
        }
    }
}
