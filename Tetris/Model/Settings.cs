using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using Tetris.Sounds;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tetris.Model
{
    [Serializable()]
    public class Settings : INotifyPropertyChanged
    {
        #region Fields
            private static Settings _instance;
            private ResourceMediaPlayer _musicPlayer;
            private ParallelSoundPlayer _soundPlayer;
            private ObservableCollection<KeySetting> _keySettings;
            private static string _serializationPath
            {
                get { return Path.Combine(((App)App.Current).SerializationPath, "settings.dat"); }
            }
        #endregion

        #region Properties
            public static Settings Instance
            {
                get 
                {
                    if (_instance == null)
                        _instance = new Settings();

                    return _instance;
                }
            }

            /* Both SoundPlayers are kinda misplaced in this class.
             * Nevertheless I put them in here, because anything else would be too much of an effort.
             */

            /// <summary>
            /// Reference to the SoundPlayer singleton to bind the volume
            /// </summary>
            public ParallelSoundPlayer SoundPlayer
            {
                get { return _soundPlayer; }
                set { _soundPlayer = value; }
            }

            /// <summary>
            /// This SoundPlayer is only for the background music
            /// </summary>
            public ResourceMediaPlayer MusicPlayer
            {
                get { return _musicPlayer; }
                set { _musicPlayer = value; }
            }

            public ObservableCollection<KeySetting> KeySettings
            {
                get { return _keySettings; }
                set { _keySettings = value; OnPropertyChanged("KeySettings"); }
            }
        #endregion
             
        private Settings()
        {
            //Create the instance with the applications default values
            MusicPlayer = new ResourceMediaPlayer();
            MusicPlayer.IsRepeating = true;
            SoundPlayer = ParallelSoundPlayer.Instance;            

            #region Deserialize the container object for KeySettings and SoundVolume
                if (File.Exists(_serializationPath))
                {
                    var __formatter = new BinaryFormatter();
                    var __stream = new FileStream(_serializationPath, FileMode.Open);
                    var __container = (SerializeContainer)__formatter.Deserialize(__stream);
                    __stream.Close();

                    //Assign the deserialized values
                    _keySettings = __container.KeySettings;
                    MusicPlayer.IsMuted = __container.IsMuted;
                    MusicPlayer.Volume = __container.SoundVolume;
                    SoundPlayer.Volume = __container.SoundVolume;
                }
                else
                {
                    MusicPlayer.Volume = 0.5;
                    SoundPlayer.Volume = 0.5;
                    KeySettings = new ObservableCollection<KeySetting>();
                    KeySettings.Add(new KeySetting(Key.Down, TetrisCommand.DOWN));
                    KeySettings.Add(new KeySetting(Key.Left, TetrisCommand.LEFT));
                    KeySettings.Add(new KeySetting(Key.Right, TetrisCommand.RIGHT));
                    KeySettings.Add(new KeySetting(Key.Up, TetrisCommand.ROTATE));
                    KeySettings.Add(new KeySetting(Key.Escape, TetrisCommand.PAUSE));
                }
            #endregion
        }

        /// <summary>
        /// Serializes the complete key settings to reload them on a later program start.
        /// </summary>
        public void Serialize()
        {
            #region Create and fill the container class
                var __container = new SerializeContainer()
                {
                    KeySettings = _keySettings,
                    SoundVolume = SoundPlayer.Volume,
                    IsMuted = MusicPlayer.IsMuted
                };
            #endregion

            try
            {
                var __stream = new FileStream(_serializationPath, FileMode.Create);
                var __formatter = new BinaryFormatter();
                __formatter.Serialize(__stream, __container);
                __stream.Close();
            }
            catch (Exception) { }
        }

        #region OnPropertyChanged Event
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// Wraps the PropertyChanged-Event.
            /// </summary>
            /// <param name="property">The name of the property that changed.</param>
            private void OnPropertyChanged(string property)
            {
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(property));
            }
        #endregion

        [Serializable()]
        private class SerializeContainer
        {
            public ObservableCollection<KeySetting> KeySettings;
            public double SoundVolume;
            public bool IsMuted;
        }
    }
}
