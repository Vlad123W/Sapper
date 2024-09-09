using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace saper1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool bombFilling = false;
        public bool BombFilling { get => bombFilling; set => bombFilling = value; }

        private int ninesOnField;

        public int MinesOnField
        {
            get => ninesOnField;
            set => ninesOnField = value;
        }

        private int openedCells;

        public int OpenedCells
        {
            get { return openedCells; }
            set { openedCells = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {              
                for (int i = 0; i < playField.ColumnDefinitions.Count; i++)
                {
                    for (int j = 0; j < playField.RowDefinitions.Count; j++)
                    {
                        TextBlock block = new()
                        {
                            Text = " ",
                            Foreground = Brushes.LightGreen,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            TextAlignment = TextAlignment.Center,
                            FontSize = 14,
                            Visibility = Visibility.Collapsed
                        };

                        Border border = new()
                        {
                            Height = block.Height,
                            Width = block.Width,
                            Child = block,
                            Style = (Style)FindResource("Playfield")
                        };
                       
                        border.MouseLeftButtonDown += Actions;
                        border.MouseRightButtonDown += PointMine;                       
                        Grid.SetRow(border, i);
                        Grid.SetColumn(border, j);

                        playField.Children.Add(border);
                    }
                }  
            };
        }

        private void PointMine(object sender, MouseButtonEventArgs e)
        {
            Border? border = sender as Border;

            if (border != null && border.Background is SolidColorBrush brush && BombFilling)
            {
                Color currentColor = brush.Color;
                Color targetColor = (Color)ColorConverter.ConvertFromString("#7b828c");

                if (currentColor == targetColor)
                {
                    border.Style = (Style)FindResource("selectedSquare");
                }
                else if(border.Background != Brushes.Transparent)
                {
                    border.Style = (Style)FindResource("Playfield");
                }
            }
        }

        private void Actions(object sender, MouseButtonEventArgs e)
        {
            Border? border = sender as Border;
            if(!BombFilling)
            {
                Random rand = new Random();
                foreach (Border element in playField.Children)
                {
                    if(element.Child is TextBlock block && element.GetHashCode() != border?.GetHashCode())
                    {
                        int randNumber = rand.Next(0, 40);
                        if (randNumber > 0 && randNumber < 10)
                        {
                            block.Text = "M";
                            MinesOnField++;
                        }
                    }
                }

                ShowStartView(4, border!);

                foreach (Border item in playField.Children)
                {
                    CountMinesForAllCells(item);
                }
                OpenedCells++;
                bombFilling = true;
            }
            else
            {
                if(border?.Child is TextBlock block)
                {
                    border.Background = Brushes.Transparent;
                    block.Visibility = Visibility.Visible;

                    if(block.Text == "M")
                    {
                        MessageBox.Show("Game over!");
                        App.Current.Shutdown();
                    }
                    else
                    {
                        int mines = CountNearbyMines(border);

                        if(mines == 0)
                        {
                            ShowCell(border);
                        }

                        if(400 - OpenedCells == MinesOnField)
                        {
                            MessageBox.Show("You win!");
                            App.Current.Shutdown();
                        }

                        OpenedCells++;
                    }
                }
            }
        }

        private void InitializeVariables(ref int rowStart, ref int colStart, ref int rowEnd, ref int colEnd, Border b, int arg)
        {
            rowStart = Grid.GetRow(b) - arg;
            colStart = Grid.GetColumn(b) - arg;
            rowEnd = Grid.GetRow(b) + arg;
            colEnd = Grid.GetColumn(b) + arg;

            if (colStart < 0)
            {
                colStart -= arg;
            }

            if (rowStart < 0)
            {
                rowStart -= arg;
            }

            if (rowEnd > 20)
            {
                rowEnd -= rowEnd + arg;
            }

            if (colEnd > 20)
            {
                colEnd -= colEnd + arg;
            }
        }

        private void ShowCell(Border b) => RevealSafeCells(Grid.GetRow(b), Grid.GetColumn(b));

        private HashSet<(int, int)> visitedCells = new();

        private void RevealSafeCells(int row, int col)
        {
            if (visitedCells.Contains((row, col)))
                return;

            Border? element = playField.Children
                .OfType<Border>()
                .FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);

            if (element == null || element.Child is not TextBlock block)
                return;

            if (block.Text == "M")
                return;

            visitedCells.Add((row, col));

            element.Background = Brushes.Transparent;
            block.Visibility = Visibility.Visible;
            openedCells++;

            if (block.Text == string.Empty) 
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        int newRow = row + i;
                        int newCol = col + j;

                        if (newRow >= 0 && newRow < playField.RowDefinitions.Count &&
                            newCol >= 0 && newCol < playField.ColumnDefinitions.Count)
                        {
                            RevealSafeCells(newRow, newCol);
                        }
                    }
                }
            }
        }

        private int CountNearbyMines(Border clickedBorder)
        {
            int count = 0;

            int rowStart = 0;
            int colStart = 0;
            int rowEnd = 0;
            int colEnd = 0;

            InitializeVariables(ref rowStart, ref colStart, ref rowEnd, ref colEnd, clickedBorder, 1);

            for (int i = rowStart; i <= rowEnd; i++)
            {
                for (int j = colStart;  j <= colEnd; j++)
                {
                    var element = playField.Children
                                  .OfType<Border>()
                                  .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);

                    if(element?.Child is TextBlock block)
                    {
                        if(block.Text == "M")
                        {
                            count++;
                        }
                    }
                }
            }
        
            return count;
        }

        private void ShowStartView(int radius, Border startPositionBorder)
        {
            int rowStart = 0;
            int colStart = 0;
            int rowEnd = 0;
            int colEnd = 0;
            
            InitializeVariables(ref rowStart, ref colStart, ref rowEnd, ref colEnd, startPositionBorder, radius);
            
            for (int i = rowStart; i <= rowEnd; i++)
            {
                for (int j = colStart; j <= colEnd; j++)
                {
                    var element = playField.Children
                                  .OfType<Border>()
                                  .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);

                    if (element?.Child is TextBlock block && block.Text != "M")
                    {
                        int mines = CountNearbyMines(element);
                        block.Text = mines == 0 ? "" : mines.ToString();
                        element.Background = Brushes.Transparent;
                        block.Visibility = Visibility.Visible;
                        OpenedCells++;
                    }
                }
            }
        }

        private void CountMinesForAllCells(Border b)
        {
            int rowStart = 0;
            int colStart = 0;
            int rowEnd = 0;
            int colEnd = 0;

            InitializeVariables(ref rowStart, ref colStart, ref rowEnd, ref colEnd, b, 1);

            for (int i = rowStart; i <= rowEnd; i++)
            {
                for (int j = colStart; j <= colEnd; j++)
                {
                    var element = playField.Children
                                  .OfType<Border>()
                                  .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);

                    if (element?.Child is TextBlock block && block.Text != "M")
                    {
                        int mines = CountNearbyMines(element);
                        block.Text = mines == 0 ? "" : mines.ToString();
                    }
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}