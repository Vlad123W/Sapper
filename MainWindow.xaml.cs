// File: MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.IO;

namespace saper1
{
    public partial class MainWindow : Window
    {
        private int _gridSize;
        private int _mineProbability;

        private bool _gameStarted;
        private bool _isSettingsPanelOpen = false;

        private int _mineCount;
        private int _openedCells;
        private readonly HashSet<(int, int)> _visited = new();
        private readonly HashSet<(int, int)> _flagged = new();

        private Border[,] _cells;
        private TextBlock[,] _texts;
        private readonly Random _rand = new();
        private DispatcherTimer _timer;

        private int _minutes;
        private int _seconds;

        private Brush _openedCellBrush;

        private string _difficulty;
        private string _theme;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            ApplySettings();
            BuildGrid();

            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
            (Resources["MainAnimation"] as Storyboard)?.Begin(Main);
        }

        private void LoadSettings()
        {
            string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "scfg");
            string settingsFilePath = Path.Combine(settingsDirectory, "myconfig.json");

            if (!Directory.Exists(settingsDirectory))
            {
                Directory.CreateDirectory(settingsDirectory);
            }

            _difficulty = "Новачок";
            _theme = "Темна";

            if (File.Exists(settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(settingsFilePath);
                    var settings = JsonConvert.DeserializeObject<SettingsService>(json);
                    _difficulty = settings?.Difficulty ?? _difficulty;
                    _theme = settings?.Theme ?? _theme;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити конфігурацію: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            Loaded += (_, __) =>
            {
                foreach (ComboBoxItem item in difficultyComboBox.Items)
                {
                    if ((item.Content?.ToString() ?? "") == _difficulty)
                    {
                        difficultyComboBox.SelectedItem = item;
                        break;
                    }
                }

                foreach (ComboBoxItem item in themeComboBox.Items)
                {
                    if ((item.Content?.ToString() ?? "") == _theme)
                    {
                        themeComboBox.SelectedItem = item;
                        break;
                    }
                }
            };
        }

        private void ApplySettings()
        {
            switch (_difficulty)
            {
                case "Новачок":
                    _gridSize = 10;
                    _mineProbability = 6;
                    break;
                case "Любитель":
                    _gridSize = 15;
                    _mineProbability = 7;
                    break;
                case "Професіонал":
                    _gridSize = 20;
                    _mineProbability = 8;
                    break;
                default:
                    _gridSize = 10;
                    _mineProbability = 6;
                    break;
            }
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            string themeFile = _theme == "Світла" ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";

            try
            {
                var dict = new ResourceDictionary
                {
                    Source = new Uri(themeFile, UriKind.Relative)
                };

                Resources.MergedDictionaries.Clear();
                Resources.MergedDictionaries.Add(dict);

                _openedCellBrush = _theme == "Світла"
                    ? new SolidColorBrush(Color.FromRgb(220, 220, 220))
                    : new SolidColorBrush(Color.FromRgb(60, 63, 70));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося застосувати тему: {ex.Message}", "Помилка теми", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BuildGrid()
        {
            _cells = new Border[_gridSize, _gridSize];
            _texts = new TextBlock[_gridSize, _gridSize];

            playField.Children.Clear();
            playField.RowDefinitions.Clear();
            playField.ColumnDefinitions.Clear();

            for (int i = 0; i < _gridSize; i++)
            {
                playField.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                playField.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            double fontSize = Math.Max(12, 500.0 / _gridSize * 0.4); 

            for (int row = 0; row < _gridSize; row++)
            {
                for (int col = 0; col < _gridSize; col++)
                {
                    var text = new TextBlock
                    {
                        Text = " ",
                        Visibility = Visibility.Collapsed,
                        Foreground = (Brush)FindResource("TextForeground"), 
                        FontSize = fontSize,
                        TextAlignment = TextAlignment.Center,
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
            for (int i = 0; i < _gridSize; i++)
                for (int j = 0; j < _gridSize; j++)
                {
                    if (Math.Abs(i - safeRow) <= 1 && Math.Abs(j - safeCol) <= 1) continue;
                    if (_rand.Next(40) < _mineProbability)
                    {
                        _texts[i, j].Text = "M";
                        _mineCount++;
                    }
                }
        }

        private void CountAllMines()
        {
            for (int i = 0; i < _gridSize; i++)
                for (int j = 0; j < _gridSize; j++)
                {
                    if (_texts[i, j].Text == "M") continue;
                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int ni = i + dx, nj = j + dy;
                            if (ni >= 0 && ni < _gridSize && nj >= 0 && nj < _gridSize && _texts[ni, nj].Text == "M")
                                count++;
                        }
                    _texts[i, j].Text = count == 0 ? string.Empty : count.ToString();
                }
        }

        private void RevealRecursive(int row, int col)
        {
            if (row < 0 || col < 0 || row >= _gridSize || col >= _gridSize) return;
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
            Refresh();
        }

        private void CheckWin()
        {
            int totalCells = _gridSize * _gridSize;
            int revealed = _visited.Count;
            if (totalCells - revealed == _mineCount)
            {
                _timer.Stop();
                MessageBox.Show("You win!");
                Refresh();   
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

        private void Refresh()
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

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            if(!_gameStarted) return;
            if (MessageBox.Show("Точно?", "Caution", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Refresh();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSettingsPanelOpen) return;

            Storyboard openAnimation = (Storyboard)FindResource("OpenSettingsAnimation");
            Overlay.Visibility = Visibility.Visible;
            openAnimation.Begin(this);
            _isSettingsPanelOpen = true;
        }

        private void closeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSettingsPanelOpen)
            {
                return;
            }

            Storyboard closeAnimation = (Storyboard)FindResource("CloseSettingsAnimation");

            closeAnimation.Completed += (s, _) => {
                Overlay.Visibility = Visibility.Collapsed;
            };

            closeAnimation.Begin(this);
            _isSettingsPanelOpen = false;
        }

        private void confirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _difficulty = (difficultyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Новачок";
                _theme = (themeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Темна";

                SettingsService settings = new()
                {
                    Difficulty = _difficulty,
                    Theme = _theme
                };

                string settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "scfg");
                if (!Directory.Exists(settingsDirectory))
                    Directory.CreateDirectory(settingsDirectory);

                string settingsFilePath = Path.Combine(settingsDirectory, "myconfig.json");
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));

                closeSettingsButton_Click(sender, e);
                _gameStarted = false;
                _visited.Clear();
                _flagged.Clear();
                _openedCells = 0;
                _minutes = 0;
                _seconds = 0;
                _timer?.Stop();
                ApplySettings();
                BuildGrid();

                
                ApplyTheme();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}