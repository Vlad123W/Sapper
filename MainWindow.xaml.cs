// File: MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace saper1
{
    public partial class MainWindow : Window
    {
        private const int GridSize = 20;
        private const int MineProbability = 6;

        private bool _gameStarted;
        private int _mineCount;
        private int _openedCells;
        private readonly HashSet<(int, int)> _visited = new();
        private readonly HashSet<(int, int)> _flagged = new();

        private readonly Border[,] _cells = new Border[GridSize, GridSize];
        private readonly TextBlock[,] _texts = new TextBlock[GridSize, GridSize];
        private readonly Random _rand = new();
        private DispatcherTimer _timer;

        private int _minutes;
        private int _seconds;

        private readonly Brush _openedCellBrush = new SolidColorBrush(Color.FromRgb(60, 63, 70));

        public MainWindow()
        {
            InitializeComponent();
            BuildGrid();

            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
            (Resources["MainAnimation"] as Storyboard)?.Begin(Main);
        }

        private void BuildGrid()
        {
            playField.Children.Clear();
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    var text = new TextBlock
                    {
                        Text = " ", Visibility = Visibility.Collapsed, Foreground = Brushes.LightGreen,
                        FontSize = 14, TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var cell = new Border
                    {
                        Name = $"Cell_{row}_{col}",
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
            int row = Grid.GetRow(cell), col = Grid.GetColumn(cell);
            if (_flagged.Contains((row, col))) return;

            if (!_gameStarted)
            {
                _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                _timer.Tick += Timer_Tick;
                _timer.Start();
                PlaceMines(row, col);
                CountAllMines();
                RevealRecursive(row, col);
                _gameStarted = true;
                return;
            }

            var text = _texts[row, col];
            if (text.Text == "M") EndGame(cell);
            else { RevealRecursive(row, col); CheckWin(); }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _seconds++;
            if (_seconds == 60) { _minutes++; _seconds = 0; }
            Time.Text = string.Format("{0:00}:{1:00}", _minutes, _seconds);
        }

        private void Cell_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border cell) return;
            int row = Grid.GetRow(cell), col = Grid.GetColumn(cell);
            if (!_gameStarted || _cells[row, col].Background == _openedCellBrush) return;

            if (_flagged.Remove((row, col)))
                cell.Style = (Style)FindResource("Playfield");
            else
            {
                _flagged.Add((row, col));
                cell.Style = (Style)FindResource("selectedSquare");
            }
        }

        private void PlaceMines(int safeRow, int safeCol)
        {
            _mineCount = 0;
            for (int i = 0; i < GridSize; i++)
                for (int j = 0; j < GridSize; j++)
                {
                    if (Math.Abs(i - safeRow) <= 1 && Math.Abs(j - safeCol) <= 1) continue;
                    if (_rand.Next(40) < MineProbability)
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
                            if (ni >= 0 && ni < GridSize && nj >= 0 && nj < GridSize && _texts[ni, nj].Text == "M")
                                count++;
                        }
                    _texts[i, j].Text = count == 0 ? string.Empty : count.ToString();
                }
        }

        private void RevealRecursive(int row, int col)
        {
            if (row < 0 || col < 0 || row >= GridSize || col >= GridSize) return;
            if (_visited.Contains((row, col)) || _flagged.Contains((row, col))) return;

            var cell = _cells[row, col];
            var text = _texts[row, col];
            if (text.Text == "M" || cell.Background == _openedCellBrush) return;

            _visited.Add((row, col));
            cell.Background = _openedCellBrush;
            text.Visibility = Visibility.Visible;
            _openedCells++;

            if (FindResource("RevealCellAnimation") is Storyboard sb)
            {
                Storyboard.SetTarget(sb, cell);
                sb.Begin();
            }

            if (string.IsNullOrEmpty(text.Text))
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        if (dx != 0 || dy != 0)
                            RevealRecursive(row + dx, col + dy);
            }
        }

        private void EndGame(Border clicked)
        {
            foreach (var cell in _cells)
            {
                if (cell.Child is TextBlock t && t.Text == "M")
                {
                    cell.Background = _openedCellBrush;
                    t.Visibility = Visibility.Visible;
                }
            }

            _timer.Stop();
            clicked.Background = Brushes.Red;
            MessageBox.Show("Game over!");
            Close();
        }

        private void CheckWin()
        {
            int totalCells = GridSize * GridSize;
            int revealed = _visited.Count;
            if (totalCells - revealed == _mineCount)
            {
                _timer.Stop();
                MessageBox.Show("You win!");
                Application.Current.Shutdown();
            }
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Точно?", "Caution", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _gameStarted = false;
                _mineCount = 0;
                _openedCells = 0;
                _minutes = 0;
                _seconds = 0;
                Time.Text = "00:00";
                _visited.Clear();
                _flagged.Clear();
                _timer.Stop();
                BuildGrid();
            }
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}