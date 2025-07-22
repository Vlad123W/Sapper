using saper1.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace saper1.IServices
{
    public interface IMinePlacer
    {
        public void PlaceMines(int gridSize, int mineProbability, int safeRow, int safeCol, List<Cell> texts);
    }
}
