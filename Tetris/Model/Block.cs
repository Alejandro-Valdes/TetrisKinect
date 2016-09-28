using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Tetris.Model
{
    public abstract class Block
    {
        #region Fields
            
        #endregion

        #region Properties
            public List<Part> Parts { get; set; }
            public int PosX { get; set; }
            public int PosY { get; set; }
            public Color Color { get; set; }
            public List<Part> Grid { get; set; }
            private bool Locked { get; set; }
        #endregion

        #region Constructor
            /// <summary>
            /// Creates a new random block
            /// </summary>
            /// <param name="grid">List of all Parts</param>
            public Block(List<Part> grid)
            {
                #region Default start locations
                    //The start location has to be x4 y1 for the center of every block (which is 1,1)
                    PosX = 3;
                    PosY = 0;
                #endregion

                Grid = grid;
            }
        #endregion

        #region Methods
            /// <summary>
            /// Creates a random new instance of all Block subclasses implemented
            /// </summary>
            /// <param name="grid">Reference to the grid object, the block will be a part of.</param>
            public static Block NewBlock(List<Part> grid)
            {
                //Get all classes that inherite from block
                var __blockTypes = typeof(Block).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Block))).ToList();

                #region Get a random type from the list
                    Type __randType;
                    if (__blockTypes.Count > 0)
                    {
                        var __rnd = new Random();
                        __randType = __blockTypes[__rnd.Next(0, __blockTypes.Count)];
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                #endregion

                //Create a concrete instance of the determined tyep
                return (Block)Activator.CreateInstance(__randType, grid);
            }

            /// <summary>
            /// Checks if the Block can move on step down and moves it.
            /// </summary>
            /// <returns>True/False whether or not the Block was moved.</returns>
            public bool MoveDown() 
            {
                #region If the Block isn't locked yet check if there is any part that cannot move down
                    if (Locked == false && Parts.Where(p => !p.CheckConflict(p.PosX + 0, p.PosY + 1)).Count() > 0)
                    {
                        Locked = true; //If MoveDown fails the block will be locked
                        return false;
                    }
                #endregion

                //Remove the parts from the Grid
                this.Parts.ForEach(p => Grid.Remove(p));

                //Move the Block
                this.PosY += 1;

                //Readd the Parts
                Grid.AddRange(this.Parts);

                return true; 
            }

            /// <summary>
            /// Checks if the Block can move on step left and moves it.
            /// </summary>
            /// <returns>True/False whether or not the Block was moved.</returns>
            public bool MoveLeft() 
            {
                #region If the Block isn't locked yet check if there is any part that cannot move left
                    if (Locked == false && Parts.Where(p => !p.CheckConflict(p.PosX + -1, p.PosY + 0)).Count() > 0)
                        return false;
                #endregion

                //Remove the parts from the Grid
                this.Parts.ForEach(p => Grid.Remove(p));

                //Move the Block
                this.PosX -= 1;

                //Readd the Parts
                Grid.AddRange(this.Parts);

                return true; 
            }

            /// <summary>
            /// Checks if the Block can move on step right and moves it.
            /// </summary>
            /// <returns>True/False whether or not the Block was moved.</returns>
            public bool MoveRight() 
            {
                #region If the Block isn't locked yet check if there is any part that cannot move right
                    if (Locked == false && Parts.Where(p => !p.CheckConflict(p.PosX + 1, p.PosY + 0)).Count() > 0)
                        return false;
                #endregion
                
                //Remove the parts from the Grid
                this.Parts.ForEach(p => Grid.Remove(p));

                //Move the Block
                this.PosX += 1;

                //Readd the Parts
                Grid.AddRange(this.Parts);

                return true; 
            }

            /// <summary>
            /// Rotates the Block if its possible. Otherwise it returns false.
            /// </summary>
            /// <returns>True/False whether or not the Block was rotated.</returns>
            public virtual bool Rotate()
            {
                return Rotation(90);
            }

            /// <summary>
            /// Rotate the Block clockwise. 
            /// This may be used by some sub classes which cant perform the usual 360° counterclockwise rotation (e.g. S or Z)
            /// </summary>
            /// <returns>True/False whether or not the Block was rotated.</returns>
            protected bool RotateClockwise()
            {
                return Rotation(-90);
            }

            /// <summary>
            /// Rotates the Block based on the given direction
            /// </summary>
            /// <param name="direction">Only 90 or -90 degree values are allowed. This however shoudn't be design problem since the method is private.</param>
            /// <returns>True/False whether or not the Block was rotated.</returns>
            private bool Rotation(int degree)
            {
                //Store all operations in this list and perform them after its safe that there isn't any conflict while rotating
                int[,] __rotations = new int[4, 2];

                #region Set the center for the rotation
                    var __zx = 1;
                    var __zy = 1;
                #endregion

                #region Calculate the Rotation for every part
                    foreach (Part p in Parts)
                    {
                        #region Reduce the coordinates to a 0,0 center
                            //Note: the parts coordinates in the blocks subgrid are private, but can be calculated
                            var __x = p.PosX - p.ParentBlock.PosX - __zx;
                            var __y = p.PosY - p.ParentBlock.PosY - __zy;
                        #endregion

                        #region Rotate based on the degree
                            //Note: clockwise and couterclockwise is mixed up because the grid is upside down
                            double __degree = (Math.PI * degree / 180) * -1; //The Methods in the Math class use radians instead of degrees
                        
                            //Rotate by using a simplyfied formula (considering only -90 or 90 will be calculated)
                            var __t = __x;
                            __x = (int)-(Math.Sin(__degree) * __y);
                            __y = (int)Math.Sin(__degree) * __t;
                        #endregion

                        if (!p.CheckConflict(PosX + __x + __zx, PosY + __y + __zy))
                            return false;

                        #region Add the center coordinates again and store the new values in the array to rotate later
                            //The index in the array is obtained through the parts index in its list
                            __rotations[Parts.IndexOf(p), 0] = __x + __zx;
                            __rotations[Parts.IndexOf(p), 1] = __y + __zy;
                        #endregion
                    }
                #endregion

                //Perform the rotations after every part was checked
                Parts.ForEach(p => p.RearrangePart(__rotations[Parts.IndexOf(p), 0], __rotations[Parts.IndexOf(p), 1]));
                return true;
            }

            /// <summary>
            /// Removes all parts in the row from the block and rearranges the rest. (Every part below moves up. Used while deleting a row.)
            /// </summary>
            /// <param name="part">The row to remove </param>
            public void DeleteRow(int row)
            {
                //Remove the parts
                Parts.RemoveAll(p => p.PosY == row);

                //Rearrange the rest (!! Only do this if there is a part above the just deleted row, otherwise the block wont be moved down later)
                if(Parts.Count(p => p.PosY < row) > 0)
                    Parts.Where(p => p.PosY > row).ToList().ForEach(p => p.RearrangePart(p.PosX - p.ParentBlock.PosX, p.PosY - p.ParentBlock.PosY -1));
            }

            /// <summary>
            /// Check if there are any conflicts with other parts.
            /// This is only to find out when it is game over, since the only time a conflict like this can happen is
            /// when there is already a block in the spawning location of new blocks.
            /// (Only called by NewBlock())
            /// </summary>
            /// <returns>True/False whether or not there are any other blocks in the same location.</returns>
            public bool HasPositionConflict()
            {
                //To find out just call CanMove for all parts with 0, 0
                if (Parts.Where(p => !p.CheckConflict(p.PosX, p.PosY)).Count() > 0)
                    return true;

                return false;
            }
        #endregion
    }
}
