using Newtonsoft.Json;
using saper1.Entities;
using saper1.IServices;
using saper1.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;
using Application = System.Windows.Application;

namespace saper1
{
    public partial class MainWindow : Window
    {
        private readonly ISettingsService _settingsService;
        private readonly IThemeManager _themeManager;
        private readonly IGameTimer _gameTimer;
        private readonly IMinePlacer _minePlacer;
        private readonly IMineCounter _mineCounter;
        private readonly IGameLogicController _gameLogic;
        private readonly IGridBuilder _gridBuilder;

        private int _gridSize;
        private int GridSquare => _gridSize * _gridSize;

        private int _mineProbability;

        private readonly Dictionary<string, int[]> difficultyConfig = new()
        {
            { "Новачок", new int[] { 10, 6 } },
            { "Любитель", new int[]{ 15, 7 } },
            { "Професіонал", new int[]{ 20, 8 } }
        };
        
        private List<Cell> _cells = [];
        private List<Cell> _mineMap;

        private bool _isSettingsPanelOpen = false;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService = new SettingsService();
            _themeManager = new ThemeManager();
            _gameTimer = new GameTimer();
            _minePlacer = new MinePlacer();
            _mineCounter = new MineCounter();
            _gridBuilder = new GridBuilder();
            _gameLogic = new GameLogicController(_themeManager.OpenedCellBrush);

            _mineMap = _cells.Where(c => c.IsMine).ToList();

            _settingsService.Load();
            ApplySettings();
            BuildGrid();

            _gameTimer.TimeChanged += (min, sec) =>
            {
                Time.Text = string.Format("{0:00}:{1:00}", min, sec);
            };

            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
            (Resources["MainAnimation"] as Storyboard)?.Begin(Main);
        }

        private void ApplySettings()
        {
            _gridSize = difficultyConfig[_settingsService._settingsData.Difficulty][0];
            _mineProbability = difficultyConfig[_settingsService._settingsData.Difficulty][1];
            
            difficultyComboBox.SelectedItem = difficultyComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _settingsService._settingsData.Difficulty);
            
            themeComboBox.SelectedItem = themeComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == _settingsService._settingsData.Theme);
            
            _themeManager.ApplyTheme(_settingsService._settingsData.Theme, Resources);
        }

        private void BuildGrid()
        {
            float fontSize = (float)Math.Max(12, 500.0 / _gridSize * 0.4);

            _gridBuilder.BuildGrid(
                playField,
                _gridSize,
                (Style)FindResource("Playfield"),
                (Style)FindResource("selectedSquare"),
                (Brush)FindResource("TextForeground"),
                fontSize,
                _cells);

               _cells.ForEach(cell =>
               {
                   cell.Border.MouseLeftButtonDown += Cell_LeftClick;
                   cell.Border.MouseRightButtonDown += Cell_RightClick;
               });
        }
        
        private bool _gameStarted;

        private void Cell_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border cell) return;
            int row = Grid.GetRow(cell), col = Grid.GetColumn(cell);

            var properCell = _cells.FirstOrDefault(x => x.Coordinates.X == row && x.Coordinates.Y == col)!;
            if (properCell.IsOpen || properCell.IsFlagged) return;

            if (!_gameStarted)
            {
                _minePlacer.PlaceMines(_gridSize, _mineProbability, row, col, _cells);
                _mineCounter.CountAllMines(_gridSize, _cells);
                _gameTimer.Reset();
                _gameTimer.Start();
                RevealRecursive(row, col);
                _gameStarted = true;
                return;
            }

            var potentionalCell = _cells.FirstOrDefault(x => x.Coordinates!.X == row && x.Coordinates.Y == col);
            bool IsMine = potentionalCell!.IsMine;
            
            if (IsMine)
            {
                if(potentionalCell.Border.Child is TextBlock block)
                {
                    block.Visibility = Visibility.Visible;
                    block.Background = Brushes.Red;
                    potentionalCell.Border.Background = _themeManager.OpenedCellBrush;
                    potentionalCell.IsOpen = true;
                }
                _gameLogic.RevealAllMines(_mineMap);
                _gameTimer.Stop();
                MessageBox.Show("Game over!");
                Refresh();
            }
            else
            {
                RevealRecursive(row, col);
                if (_gameLogic.CheckWin(GridSquare, _mineMap.Count, _cells.Where(x => x.IsOpen).ToList().Count))
                {
                    _gameTimer.Stop();
                    MessageBox.Show("You win!");
                    Refresh();
                }
            }
        }

        private void Cell_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border cell) return;
            int row = Grid.GetRow(cell), col = Grid.GetColumn(cell);
            if (!_gameStarted || cell.Background == _themeManager.OpenedCellBrush) return;

            var current = _cells.FirstOrDefault(x => x.Coordinates.X == row && x.Coordinates.Y == col);
            if (current!.IsFlagged)
            {
                cell.Style = (Style)FindResource("Playfield");
                current.IsFlagged = false;
            }
            else
            {
                cell.Style = (Style)FindResource("selectedSquare");
                current.IsFlagged = true;
            }
        }

        private void RevealRecursive(int row, int col)
        {
            if (row < 0 || col < 0 || row >= _gridSize || col >= _gridSize) return;

            var cell = _cells.FirstOrDefault(x => x.Coordinates?.X == row && x.Coordinates?.Y == col);
            if (cell == null || cell.IsOpen || cell.IsFlagged) return;

            cell.IsOpen = true;

            if (cell.Border.Child is TextBlock text)
            {
                cell.Border.Background = _themeManager.OpenedCellBrush;
                text.Visibility = Visibility.Visible;
                text.Text = cell.AdjacentMines == 0 ? " " : cell.AdjacentMines.ToString();
            }

            if (FindResource("RevealCellAnimation") is Storyboard revealAnim)
            {
                Storyboard.SetTarget(revealAnim, cell.Border);
                revealAnim.Begin();
            }

            if (cell.AdjacentMines == 0)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            RevealRecursive(row + dx, col + dy);
                        }
                    }
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void ExitPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); } catch { }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_gameStarted) return;
            if (MessageBox.Show("Точно?", "Caution", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Refresh();
        }

        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSettingsPanelOpen) return;
            if (FindResource("OpenSettingsAnimation") is Storyboard openAnimation)
            {
                Overlay.Visibility = Visibility.Visible;
                openAnimation.Begin(this);
                _isSettingsPanelOpen = true;
            }
        }
        
        private void Refresh()
        {
            _gameStarted = false;
            _gameTimer.Reset();
            _mineMap.Clear();
            _cells.Clear();
            BuildGrid();
        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSettingsPanelOpen) return;
            if (FindResource("CloseSettingsAnimation") is Storyboard closeAnimation)
            {
                closeAnimation.Completed += (s, _) => Overlay.Visibility = Visibility.Collapsed;
                closeAnimation.Begin(this);
                _isSettingsPanelOpen = false;
            }
        }

        private void ConfirmSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsService.Save(
                                new()
                                {
                                  Difficulty = (difficultyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()!,
                                  Theme = (themeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()!
                                });

                ApplySettings();
                CloseSettingsButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
