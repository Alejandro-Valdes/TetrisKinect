using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Tetris.Model
{
    public class BlockT : Block
    {
        public BlockT(List<Part> grid) : base(grid)
        {
            this.Color = Colors.LimeGreen;
            Parts = new List<Part>() { new Part(this, 1, 0), new Part(this, 0, 1), new Part(this, 1, 1), new Part(this, 2, 1) };
        }
    }
}
