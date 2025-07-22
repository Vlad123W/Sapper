using saper1.Data;
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
    public class GridBuilder : IGridBuilder
    {
        public void BuildGrid(Grid targetGrid, int gridSize, Style cellStyle, Style flaggedStyle, Brush textColor, double fontSize, List<Cell> cells)
        {
            targetGrid.Children.Clear();
            targetGrid.RowDefinitions.Clear();
            targetGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < gridSize; i++)
            {
                targetGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                targetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    var text = new TextBlock
                    {
                        Text = " ",
                        Visibility = Visibility.Collapsed,
                        Foreground = textColor,
                        FontSize = fontSize,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var cell = new Border
                    {
                        Name = $"Cell_{row}_{col}",
                        Style = cellStyle,
                        Child = text
                    };

                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    targetGrid.Children.Add(cell);
                    
                    cells.Add(new Cell(new() { X = (byte)row, Y = (byte)col}) { Border = cell });
                }
            }
        }
    }
}
