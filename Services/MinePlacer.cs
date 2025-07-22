using saper1.Entities;
using saper1.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace saper1.Services
{
    internal class MinePlacer : IMinePlacer
    {
        private readonly Random _rand = new();

        public void PlaceMines(int gridSize, int mineProbability, int safeRow, int safeCol, List<Cell> texts)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    if (Math.Abs(i - safeRow) <= 1 && Math.Abs(j - safeCol) <= 1)
                        continue;

                    if (_rand.Next(40) < mineProbability)
                    {
                        var cell = texts.Where(c => c._coordinates!.X == i && c._coordinates.Y == j).FirstOrDefault();
                        cell!.IsMine = true;
                        
                        if(cell.Border.Child is TextBlock block)
                        {
                            block.Text = "M";
                        }
                    }
                }
            }
        }
    }
}
