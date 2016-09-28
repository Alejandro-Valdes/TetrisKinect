using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Tetris.Model
{
    public class BlockL : Block
    {
        public BlockL(List<Part> grid) : base(grid)
        {
            this.Color = Colors.Green;
            Parts = new List<Part>() { new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 2, 1), new Part(this, 0, 2) };
        }
    }
}
