using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;

namespace Tetris.Model
{
    public class Tetris : INotifyPropertyChanged
    {
        #region Fields
            private int _level;
            private int _clearedLines;
            private int _score;
            private Block _nextBlock;
            private bool _isPaused;
        #endregion<

        #region Properties
            /// <summary>
            /// Grid used to be a [18,10] array of Parts, but since every Block holds its abssolute position 
            /// and the parts theirs relative to it, the Grid is now a List<Part> which makes it much easier to use for certain operations
            /// </summary>
            public List<Part> Grid { get; set; }
            public Block CurrentBlock { get; set; }
            public bool IsPaused
            {
                get { return _isPaused; }
            }

            private DispatcherTimer GameTimer { get; set; }

            //In the original tetris, the user gets additional points depending on the number of MoveDown steps he triggered manually for each block
            //A user invoked MoveDown() increments this counter, a KeyUp event for the "Down"-Key resets it
            private int SoftDrops { get; set; } 

            public int Level 
            {
                get { return _level; }
                set { _level = value; OnPropertyChanged("Level"); }
            }
            public int ClearedLines
            {
                get { return _clearedLines; }
                set { _clearedLines = value; OnPropertyChanged("ClearedLines"); }
            }
            public int Score
            {
                get { return _score; }
                set { _score = value; OnPropertyChanged("Score"); }
            }
            public Block NextBlock
            {
                get { return _nextBlock; }
                set { _nextBlock = value; OnPropertyChanged("NextBlock"); }
            }
        #endregion

        #region Constructor
            public Tetris()
            {
                Grid = new List<Part>();

                #region Initialize the timer
                    GameTimer = new DispatcherTimer();
                    GameTimer.Interval = TimeSpan.FromMilliseconds(800);
                    GameTimer.Tick += new EventHandler(TetrisTick);
                #endregion

                _isPaused = true;
            }
        #endregion

        #region Methods
            /// <summary>
            /// Start the game
            /// </summary>
            public void StartGame()
            {
                NextBlock = Block.NewBlock(this.Grid);   //Create a "NextBlock" in the beginning
                NewBlock();
                GameTimer.Start();
                _isPaused = false;
            }

            /// <summary>
            /// Pause the game at its current state
            /// </summary>
            public void PauseGame()
            {
                GameTimer.Stop();
                _isPaused = true;
                if (IsPausedChanged != null) { IsPausedChanged(); }
            }

            /// <summary>
            /// Resume the game after a previous pause
            /// </summary>
            public void ResumeGame()
            {
                GameTimer.Start();
                _isPaused = false;
                if (IsPausedChanged != null) { IsPausedChanged(); }
            }

            /// <summary>
            /// Called by the timer to let the Block fall down
            /// </summary>
            private void TetrisTick(object sender, EventArgs e)
            {
                MoveDown(sender);
            }

            /// <summary>
            /// Generates a random new Block as the CurrentBlock and adds its Parts to the Grid.
            /// </summary>
            /// <returns>False in case of the block couldn't be place at the spawning location (which means its Game Over)</returns>
            private bool NewBlock()
            {
                //The NextBlock is now the CurrentBlock
                CurrentBlock = NextBlock;
                
                //Create a random new NextBlock
                NextBlock = Block.NewBlock(this.Grid); 

                Grid.AddRange(CurrentBlock.Parts);

                if (NewBlockAdded != null) { NewBlockAdded(); }

                #region Check for GameOver
                    if (CurrentBlock.HasPositionConflict())
                        return false;
                #endregion

                return true;
            }

            /// <summary>
            /// Wraps NewBlock and calls CheckForCompleteRows() (Stops the timer)
            /// </summary>
            private void FinishRound()
            {
                //Stop the timer
                GameTimer.Stop();
                _isPaused = true;

                if (RoundFinished != null) { RoundFinished(); }

                var __rows = CheckForCompleteRows();

                if (__rows.Count() > 0)
                    if (RowsCompleting != null) { RowsCompleting(__rows); }

                #region Delete all parts in every completed row
                    foreach (var __row in __rows)
                    {
                        DeleteRow(__row);
                    }

                    if(__rows.Count() > 0)
                        if (RowsCompleted != null) { RowsCompleted(__rows); }
                #endregion              

                //Increase the counter for the cleared Lines
                ClearedLines += __rows.Count();

                #region Add points for deleted rows (level+1)*c (c=40, 100, 300, 1200 based on how many rows were cleared)
                    //Dirty code...
                    if (__rows.Count() == 1) Score += (Level + 1) * 40;
                    if (__rows.Count() == 2) Score += (Level + 1) * 100;
                    if (__rows.Count() == 3) Score += (Level + 1) * 300;
                    if (__rows.Count() == 4) Score += (Level + 1) * 1200; 
                #endregion

                #region Add points for softdrops
                    Score += SoftDrops;
                    ResetSoftDrop();
                #endregion

                #region Check if the level has increased
                    if (Level < ClearedLines / 10)
                        NextLevel();
	            #endregion

                #region Create a new block, invoke GameOver if it fails or continue by restarting the timer
                    if (NewBlock())
                    {
                        GameTimer.Start();
                        _isPaused = false;
                    }
                    else
                        EndGame();
                #endregion
            }

            /// <summary>
            /// Searches the Grid for complete rows every round and deletes it
            /// </summary>
            /// <returns>The y-value of the row to be deleted</returns>
            private int[] CheckForCompleteRows()
            {
                #region Get all full rows, (grouped by PosY == 10)
                    var __completeRows = Grid.GroupBy(p => p.PosY)
                                        .Where(r => r.Count() == 10)
                                        .Select(ps => new { Row = ps.First().PosY })
                                        .ToList();
                #endregion

                //Turn it into an int array
                return __completeRows.Select(i => i.Row).OrderBy(r => r).ToArray();
            }

            /// <summary>
            /// Deletes a complete row. It overrides the Parts in the completed row and rearranges the Grid
            /// </summary>
            /// <param name="row">The row in the complete Grid that will be deleted.</param>
            private void DeleteRow(int row)
            {
                //Get all parts which have to be removed
                var __parts = Grid.Where(p => p.PosY == row).ToList();

                //Remove them from the Grid
                Grid.RemoveAll(p => p.PosY == row);

                #region Perform the deletion on every affected block to delete the parts from their blocks and rearrange the remaining ones
                    //Get all affected blocks
                    __parts.GroupBy(p => p.ParentBlock)
                            .Select(ps => ps.First().ParentBlock)
                            .ToList()
                            .ForEach(b => b.DeleteRow(row));
                #endregion

                #region Rearrange the rest
                    //Get all blocks above the just deleted row (parts grouped by their block so every block only moves down once)
                    var __blocks = Grid.Where(p => p.PosY < row)
                                        .GroupBy(p => p.ParentBlock)
                                        .Select(ps => ps.First().ParentBlock).ToList();

                    //Move the blocks (conflicts dont matter because the blocks are already locked)
                    __blocks.ForEach(b => b.MoveDown());
                #endregion
            }

            /// <summary>
            /// Next Level every 10 completed rows. (Increases the speed)
            /// </summary>
            private void NextLevel()
            {
                Level = ClearedLines / 10;
                //Every Level the Block moves down 50 ms faster
                GameTimer.Interval -= TimeSpan.FromMilliseconds(4 * Level + 50.0);
            }

            /// <summary>
            /// End the game regardless of its current state.
            /// This is also called when NewBlock() cant place the new created block due to a conflict.
            /// </summary>
            public void EndGame()
            {
                GameTimer.Stop();
                if (GameOver != null) { GameOver(Score); }
            }

            /// <summary>
            /// In the original tetris, the user gets additional points depending on the number of MoveDown steps he triggered manually for each block
            /// A user invoked MoveDown() increments this counter, a KeyUp event for the "Down"-Key resets it
            /// </summary>
            public void ResetSoftDrop()
            {
                SoftDrops = 0;
            }

            #region Wrap the CurrentBlocks move methods
                /// <summary>
                /// Moves the CurrentBlock down (if it fails, the next round will be started)
                /// </summary>
                public void MoveDown(object sender)
                {
                    if (BlockMoving != null) { BlockMoving(); }

                    var __ret = CurrentBlock.MoveDown();
                    
                    if (BlockMoving != null) { BlockMoved(); }

                    #region Handle the moves result
                        if (!__ret) FinishRound();
                        else
                            if (sender as DispatcherTimer == null) SoftDrops++;
                            //A user invoked MoveDown() is awarded with points
                    #endregion
                }

                /// <summary>
                /// Moves the CurrentBlock left (if it fails nothing happens)
                /// </summary>
                public void MoveLeft() 
                {
                    if (BlockMoving != null) { BlockMoving(); }
                    CurrentBlock.MoveLeft();
                    if (BlockMoving != null) { BlockMoved(); }
                }

                /// <summary>
                /// Moves the CurrentBlock right (if it fails nothing happens)
                /// </summary>
                public void MoveRight() 
                {
                    if (BlockMoving != null) { BlockMoving(); }
                    CurrentBlock.MoveRight();
                    if (BlockMoving != null) { BlockMoved(); }
                }

                /// <summary>
                /// Rotates the CurrentBlock (if it fails nothing happens)
                /// </summary>
                public void Rotate()
                {
                    if (BlockMoving != null) { BlockMoving(); }
                    CurrentBlock.Rotate();
                    if (BlockMoving != null) { BlockMoved(); }
                }
            #endregion

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

            /// <summary>
            /// Output all values and a simple Grid visualization on the console
            /// </summary>
            public void DebugOutput()
            {
                Console.WriteLine("============== Current Tetris State ==============");
                Console.WriteLine("Game State:\t\t" + IsPaused.ToString());
                Console.WriteLine("Level:\t\t\t" + Level.ToString());
                Console.WriteLine("ClearedLines:\t" + ClearedLines.ToString());
                Console.WriteLine("Score:\t\t\t" + Score.ToString());
                Console.WriteLine("Current Block:\t" + CurrentBlock.GetType().Name);
                Console.WriteLine("\t\t\t\t" + CurrentBlock.PosX.ToString());
                Console.WriteLine("\t\t\t\t" + CurrentBlock.PosY.ToString());
                Console.WriteLine("Next Block:\t\t" + NextBlock.GetType().Name);
                Console.WriteLine("--- Current Grid State ---");
                #region Output the grid
                    Console.WriteLine("+-+-+-+-+-+-+-+-+-+-+");
                    for (int i = 0; i < 18; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            Console.Write("|");
                            if (Grid.Where(p => p.PosY == i && p.PosX == j).Count() > 1)
                                Console.Write(Grid.Where(p => p.PosY == i && p.PosX == j).Count().ToString());
                            else if(Grid.Where(p => p.PosY == i && p.PosX == j).Count() == 1)
                                Console.Write("X");
                            else
                                Console.Write(" ");
                        }
                        Console.WriteLine("|");
                    }
                    Console.WriteLine("+-+-+-+-+-+-+-+-+-+-+");
                #endregion
                Console.WriteLine("==================================================");
            }
        #endregion

        #region Events
            //Delegates
            public delegate void NewBlockEventHandler();
            public delegate void BlockMovingEventHandler();
            public delegate void BlockMovedEventHandler();
            public delegate void RowsCompletingEventHandler(int[] rows);
            public delegate void RowsCompletedEventHandler(int[] rows);
            public delegate void GameOverEventHandler(int score);
            public delegate void IsPausedChangedEventHandler();
            public delegate void RoundFinishedEventHandler();

            //Events
            public event NewBlockEventHandler NewBlockAdded;        //The UI basically has to do the same as for BlockMoved, just redraw the CurrentBlock

            public event BlockMovingEventHandler BlockMoving;       //BlockMoving (triggers the UI to delete the Parts of CurrentBlock)
            public event BlockMovedEventHandler BlockMoved;         //BlockMoved  (display the CurrentBlocks parts again)

            public event RowsCompletingEventHandler RowsCompleting;   //RowCompleting (triggers the GUI to display a flash on the Row)
            public event RowsCompletedEventHandler RowsCompleted;     //RowCompleted (clear and redraw everything)

            public event GameOverEventHandler GameOver;             //GameOver() - display the GameOver screen

            public event IsPausedChangedEventHandler IsPausedChanged;

            public event PropertyChangedEventHandler PropertyChanged;

            public event RoundFinishedEventHandler RoundFinished;
        #endregion
    }
}
