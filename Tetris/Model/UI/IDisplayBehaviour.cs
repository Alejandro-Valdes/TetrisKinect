using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.Model.UI
{
    //Use of the strategy pattern in order to use multiple display animations for UserControls
    public interface IDisplayBehaviour
    {
        void Show();
        void Hide();
    }
}
