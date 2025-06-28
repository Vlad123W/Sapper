// File: MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace saper1
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 20;
        private const int MineProbability = 5;

        private bool _gameStarted = false;
        private int _mineCount = 0;
        private int _openedCells = 0;
        private readonly HashSet<(int, int)> _visited = new();
        private readonly HashSet<(int, int)> _flagged = new();

        private Border[,] _cells;
        private TextBlock[,] _texts;

        public MainWindow()
        {
            InitializeComponent();
            BuildGrid();

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };

            if (Resources["MainAnimation"] is Storyboard sb)
                sb.Begin(Main);
        }

        private void BuildGrid()
        {
            _cells = new Border[GridSize, GridSize];
            _texts = new TextBlock[GridSize, GridSize];

            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    var text = new TextBlock
                    {
                        Text = " ",
                        Visibility = Visibility.Collapsed,
                        Foreground = Brushes.LightGreen,
                        FontSize = 14,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var cell = new Border
                    {
                        Style = (Style)FindResource("Playfield"),
                        Child = text
                    };

                    cell.MouseLeftButtonDown += Cell_LeftClick;
                    cell.MouseRightButtonDown += Cell_RightClick;

                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    playField.Children.Add(cell);

                    _cells[row, col] = cell;
                    _texts[row, col] = text;
                }
            }
        }

        private void Cell_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border cell) return;
            int row = Grid.GetRow(cell);
            int col = Grid.GetColumn(cell);
            var text = _texts[row, col];

            if (_flagged.Contains((row, col))) return;

            if (!_gameStarted)
            {
                PlaceMines(row, col);
                CountAllMines();
                RevealRadius(row, col);
                _gameStarted = true;
                return;
            }

            if (text.Text == "M")
            {
                EndGame(cell);
            }
            else
            {
                RevealRecursive(row, col);
                CheckWin();
            }
        }

        private void Cell_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border cell) return;
            int row = Grid.GetRow(cell);
            int col = Grid.GetColumn(cell);

            if (!_gameStarted || _cells[row, col].Background == Brushes.Transparent) return;

            if (_flagged.Contains((row, col)))
            {
                _flagged.Remove((row, col));
                cell.Style = (Style)FindResource("Playfield");
            }
            else
            {
                _flagged.Add((row, col));
                cell.Style = (Style)FindResource("selectedSquare");
            }
        }

        private void PlaceMines(int safeRow, int safeCol)
        {
            var rand = new Random();
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                {
                    if (Math.Abs(i - safeRow) <= 1 && Math.Abs(j - safeCol) <= 1) continue;
                    if (rand.Next(40) < MineProbability)
                    {
                        _texts[i, j].Text = "M";
                        _mineCount++;
                    }
                }
        }

        private void CountAllMines()
        {
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                {
                    if (_texts[i, j].Text == "M") continue;
                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int ni = i + dx, nj = j + dy;
                            if (ni < 0 || nj < 0 || ni >= GridSize || nj >= GridSize || _texts[ni, nj].Text != "M") continue;
                            count++;
                        }
                    _texts[i, j].Text = count == 0 ? string.Empty : count.ToString();
                }
        }

        private void RevealRecursive(int row, int col)
        {
            if (row < 0 || row >= GridSize || col < 0 || col >= GridSize) return;
            if (_visited.Contains((row, col)) || _flagged.Contains((row, col))) return;

            var cell = _cells[row, col];
            var text = _texts[row, col];

            if (text.Text == "M" || cell.Background == Brushes.Transparent) return;

            _visited.Add((row, col));
            cell.Background = Brushes.Transparent;
            text.Visibility = Visibility.Visible;
            _openedCells++;

            if (string.IsNullOrEmpty(text.Text))
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (dx != 0 || dy != 0)
                            RevealRecursive(row + dx, col + dy);
            }
        }

        private void RevealRadius(int startRow, int startCol) => RevealRecursive(startRow, startCol);

        private void EndGame(Border clicked)
        {
            foreach (var cell in _cells)
            {
                if (cell.Child is TextBlock t && t.Text == "M")
                {
                    cell.Background = Brushes.Transparent;
                    t.Visibility = Visibility.Visible;
                }
            }

            clicked.Background = Brushes.Red;
            MessageBox.Show("Game over!");
            Close();
        }

        private void CheckWin()
        {
            if (GridSize * GridSize - _openedCells == _mineCount)
            {
                MessageBox.Show("You win!");
                Application.Current.Shutdown();
            }
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Точно?", "Caution", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _gameStarted = false;
                _mineCount = 0;
                _openedCells = 0;
                _visited.Clear();
                _flagged.Clear();

                playField.Children.Clear();
                BuildGrid();
            }
        }
    }
}
