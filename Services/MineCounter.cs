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
            var mineCells = _cells.Where(c => c.IsMine).ToList();

            foreach (var mine in mineCells)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;

                        int nx = mine.Coordinates.X + dx;
                        int ny = mine.Coordinates.Y + dy;

                        if (nx < 0 || ny < 0 || nx >= gridSize || ny >= gridSize)
                            continue;

                        var neighbor = _cells.FirstOrDefault(x => x.Coordinates.X == nx && x.Coordinates.Y == ny);

                        if (neighbor == null || neighbor.IsMine)
                            continue;

                        neighbor.AdjacentMines++;
                    }
                }
            }
        }

    }
}
