using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tetris.Model
{
    public class Highscores
    {      
        #region Fields/Properties
            private static Highscores _instance;
            private List<Score> _scores;

            private static string _serializationPath
            {
                get { return Path.Combine(((App)App.Current).SerializationPath, "scores.dat"); }
            }
            
            public static Highscores Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = new Highscores();

                    return _instance;
                }
            }

            public List<Score> Scores { get { return _scores;} }
        #endregion

        /// <summary>
        /// Recreate a previously serialized highscore list from the file system, or create a new empty one.
        /// </summary>
        private Highscores() 
        {
            _scores = new List<Score>();

            #region Deserialize the list of highscores if it already exists
                if (File.Exists(_serializationPath))
                {
                    var __formatter = new BinaryFormatter();
                    var __stream = new FileStream(_serializationPath, FileMode.Open);
                    _scores = (List<Score>)__formatter.Deserialize(__stream);
                    __stream.Close();
                }
            #endregion
        }

        #region Methods
            /// <summary>
            /// Adds a new highscore to the list.
            /// </summary>
            /// <param name="points">The value of the new score.</param>
            /// <param name="name">The players name.</param>
            public void Add(int points, string name)
            {
                if(CheckScore(points))
                {
                    #region Remove the lowest score if there are 10
                        if(_scores.Count == 10)
                            _scores.Remove(_scores.OrderBy(s => s.Points).First());
                    #endregion

                    _scores.Add(new Score(points, name, _scores));

                    #region Serialize the complete Highscores-list
                        try
                        {
                            var __stream = new FileStream(_serializationPath, FileMode.Create);
                            var __formatter = new BinaryFormatter();
                            __formatter.Serialize(__stream, _scores);
                            __stream.Close();
                        }
                        catch (Exception) { }
                    #endregion
                }
            }

            /// <summary>
            /// Determines whether the specified score is in the top ten.
            /// </summary>
            /// <param name="points">The score to check.</param>
            /// <returns>True if the score is high enough to be in the top ten.</returns>
            public bool CheckScore(int points)
            {
                if((_scores.Where(s => s.Points >= points).Count() < 10))
                    return true;
                else
                    return false;
            }
        #endregion

        [Serializable()]
        public class Score
        {
            public int Nr 
            { 
                get
                {
                    if (_list != null) return _list.OrderByDescending(s => s.Points).ToList().IndexOf(this)+1;
                    else return 0;
                } 
            }
            public int Points { get; set; }
            public string Player { get; set; }

            private List<Score> _list;

            public Score(int points, string name, List<Score> list)
            {
                Points = points;
                Player = name;
                _list = list;
            }
        }
    }
}
