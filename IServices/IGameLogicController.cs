using saper1.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace saper1.IServices
{
    public interface IGameLogicController
    {
        public bool CheckWin(int gridSize, int mineCount, int visited);
        public void RevealAllMines(List<Cell> mineMap);
    }
}
