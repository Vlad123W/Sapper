using saper1.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace saper1.Entities
{
    public class Cell
    {
        public Coordinates? Coordinates { get; private set; }
       
        public Border Border { get; set; } = new();

        public bool IsMine { get; set; } = false;
        public bool IsOpen { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        
        public int AdjacentMines { get; set; } = 0;

        public Cell(Coordinates? coordinates)
        {
            Coordinates = coordinates;
        }
    }
}
