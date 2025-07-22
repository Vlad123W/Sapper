using saper1.Entities;
using saper1.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace saper1.Services
{
    public class GameLogicController : IGameLogicController
    {
        private readonly Brush _openedCellBrush;

        public GameLogicController(Brush openedCellBrush)
        {
            _openedCellBrush = openedCellBrush;
        }

        public bool CheckWin(int gridSquare, int mineCount, int visited) => gridSquare - visited == mineCount;

        public void RevealAllMines(List<Cell> mineMap)
        {
            mineMap.ForEach(cell =>
            {
                if (cell.Border.Child is TextBlock t)
                {
                    cell.Border.Background = _openedCellBrush;
                    t.Visibility = Visibility.Visible;
                }
            });
        }
    }
}
