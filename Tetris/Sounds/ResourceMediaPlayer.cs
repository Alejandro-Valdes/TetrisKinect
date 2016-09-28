using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.IO;
using System.Windows;
using System.Threading;
using System.ComponentModel;

namespace Tetris.Sounds
{
    public class ResourceMediaPlayer : MediaPlayer, IDisposable
    {
        #region Fields/Properties 
            protected internal string TempDirectory;
            public bool IsRepeating;
        #endregion

        #region Constructor
            public ResourceMediaPlayer()
            {
                base.MediaEnded += new EventHandler(SoundPlayer_MediaEnded);
            }
        #endregion

        #region Methods
            /// <summary>
            /// Deletes all temporary created sound files in the user's TempDirectory.
            /// Unfortunately its not possible to use the deconstructor for this task, as you HAVE TO close the MediaPlayer first.
            /// A more detailed approach written by Sebastian Krysmanski on codeproject can be found here: http://www.codeproject.com/KB/audio-video/wpfaudioplayer.aspx
            /// </summary>
            public void Dispose()
            {
                if (TempDirectory != null)
                {
                    try
                    {
                        this.Close();
                        //Delete all files first
                        foreach (var __f in Directory.GetFiles(TempDirectory))
                            File.Delete(__f);
                        Directory.Delete(TempDirectory, true);
                    }
                    catch(Exception) { }
                }
            }

            /// <summary>
            /// Plays an audio file that is built into the assembly as a resource.
            /// </summary>
            /// <param name="uri">The complete URI to the file. (Assembly;component/Folder/file.extension)</param>
            protected internal virtual void PlayResourceFile(Uri uri)
            {
                Uri __uri = WriteResourceFileToTemp(uri);    
                this.Open(__uri);
                this.Play();
            }

            /// <summary>
            /// The MediaPlayer cannot play sound files which are built in as a resource.
            /// This method writes a certain file to the users AppData temp
            /// </summary>
            /// <param name="uri">The complete URI to the file. (Assembly;component/Folder/file.extension)</param>
            /// <returns>A path to the file in the users temp directory.</returns>
            protected internal Uri WriteResourceFileToTemp(Uri uri)
            {
                try
                {
                    #region Create a new temp folder if there isn't one for this session of the application
                        if (string.IsNullOrEmpty(TempDirectory))
                        {
                            TempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                            Directory.CreateDirectory(TempDirectory);
                        }
                    #endregion

                    #region Write the ressource file to the file system if it isn't there already
                        var __file = Path.Combine(TempDirectory, Path.GetFileName(uri.ToString()));
                        if(!File.Exists(__file))
                        {
                            var __stream = Application.GetResourceStream(uri).Stream;
                            var __fileStream = File.Create(__file);
                            __stream.CopyTo(__fileStream);
                            __fileStream.Close();
                        }
                    #endregion

                    return new Uri(__file, UriKind.Absolute);
                }
                catch (Exception) { return null; }
            }

            protected internal void WriteStreamToFile(Stream stream, string file)
            {
                #region Create the stream
                    long __length = stream.Length;
                    byte[] data = new byte[__length];
                    stream.Read(data, 0, (int)__length);

                    FileStream fs = new FileStream(file, FileMode.Create);
                #endregion

                #region Write the file
                    fs.Write(data, 0, (int)__length);
                    fs.Flush();
                    fs.Close();
                #endregion
            }

            /// <summary>
            /// Occurs when the Media has been played to the end. Depending on IsRepeating, the player will start playing the file again.
            /// </summary>
            private void SoundPlayer_MediaEnded(object sender, EventArgs e)
            {
                if (IsRepeating)
                    this.Position = new TimeSpan(0); //Rewind the position
            }
        #endregion
    }
}
