using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Tetris.Model
{
    public class BlockZ : Block
    {
        public BlockZ(List<Part> grid) : base(grid)
        {
            this.Color = Colors.Blue;
            Parts = new List<Part>() { new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 1, 2), new Part(this, 2, 2) };
        }

        /// <summary>
        /// Rotates the Block if its possible. Otherwise it returns false.
        /// </summary>
        /// <returns>True/False whether or not the Block was rotated.</returns>
        public override bool Rotate()
        {
            #region Determine which state the block currently is in and rotate either clock- or counterclockwise
                if (Parts.Where(p => p.PosX - p.ParentBlock.PosX == 1 && p.PosY - p.ParentBlock.PosY == 0).Count() > 0)
                    return base.Rotate();
                else
                    return base.RotateClockwise();
            #endregion
        }
    }
}
