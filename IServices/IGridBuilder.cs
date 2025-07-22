using saper1.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace saper1.IServices
{
    public interface IGridBuilder
    {
        void BuildGrid(Grid targetGrid, int gridSize, Style cellStyle, Style flaggedStyle, Brush textColor, float fontSize, List<Cell> cells);
    }
}
