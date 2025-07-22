using saper1.Entities;
using saper1.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saper1.Services
{
    public class MineCounter : IMineCounter
    {
        public void CountAllMines(int gridSize, List<Cell> _cells)
        {
            List<Cell> mineCells = _cells.Where(c => c.IsMine).ToList();
            

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    var current = _cells.FirstOrDefault(x => x._coordinates!.X == i && x._coordinates.Y == j);
                    
                    bool IsMine = current!.IsMine;
                    
                    if (IsMine)
                    {
                        continue;
                    }

                  
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int ni = i + dx, nj = j + dy;
                            if (ni >= 0 && ni < gridSize && nj >= 0 && nj < gridSize && _cells.Where(x => x._coordinates!.X == ni && x._coordinates.Y == nj).FirstOrDefault()!.IsMine)
                               current!.AdjacentMines++;
                        }
                    }
                }
            }
        }
    }
}
