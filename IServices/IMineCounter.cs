using saper1.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saper1.IServices
{
    public interface IMineCounter
    {
        void CountAllMines(int gridSize, List<Cell> _cells);
    }
}
