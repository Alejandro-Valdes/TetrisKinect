using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Tetris.Model
{
    public class BlockJ : Block
    {
        public BlockJ(List<Part> grid) : base(grid)
        {
            this.Color = Colors.Red;
            Parts = new List<Part>() { new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 2, 1), new Part(this, 2, 2) };
        }
    }
}
