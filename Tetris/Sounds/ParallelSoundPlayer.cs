using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.IO;
using System.Windows;

namespace Tetris.Sounds
{
    /// <summary>
    /// There used to be an issue with sound effects when only two instances of the SoundPlayer class managed both, music and sound effects. 
    /// Because of the short duration and possibly high frequency of sounds they blocked each other (they were played by a single instance).
    /// At first I thought about just making the class a singleton and let it create a temporary player for every effect. 
    /// That however brought up a problem with binding the music volume.
    /// This and the fact, that i kinda like the SoundPlayer class in its original non-singleton form, I just made this new class which extends the normal SoundPlayer 
    /// and provides the additional "temporary SoundPlayers"-function for not blocking effects.
    /// </summary>
    public class ParallelSoundPlayer : ResourceMediaPlayer
    {
        #region Fields/Properties
            private static ParallelSoundPlayer _instance;
            public static ParallelSoundPlayer Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = new ParallelSoundPlayer();
                    return _instance;
                }
            }

            private List<MediaPlayer> _playerCache;
        #endregion

        #region Constructor
            private ParallelSoundPlayer() : base()
            {
                _playerCache = new List<MediaPlayer>();
            }
        #endregion

        #region Methods
            /// <summary>
            /// Plays an audio file that is build into the assembly as a resource. (This creates a new temporary player for every sound.)
            /// </summary>
            /// <param name="uri">The complete URI to the file. (Assembly;component/Folder/file.extension)</param>
            /// <param name="extension">The file extension (It seems necessary to give the random temp file the correct type in order for the MediaPlayer to use it.)</param>
            protected internal override void PlayResourceFile(Uri uri)
            {
                Uri __uri  = WriteResourceFileToTemp(uri); //Write it to the file system if it hasn't been played already

                #region Create a new MediaPlayer instance for every sound to not block or cut out sound effects
                    var __tempPlayer = new MediaPlayer();
                    __tempPlayer.Volume = this.Volume;
                    __tempPlayer.Open(__uri);
                    
                    //Due to the fact that __tempPlayer is only a local variable it would get deleted by the garbage collector if we dont make sure its reference is held
                    _playerCache.Add(__tempPlayer);
                    #region Remove the object from the list after the media has finished
                        __tempPlayer.MediaEnded += new EventHandler(delegate { _playerCache.Remove(__tempPlayer); });
                    #endregion
                    __tempPlayer.Play();
                #endregion
            }

        #endregion
    }
}
