using saper1.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace saper1.Services
{
    public class ThemeManager : IThemeManager
    {
        public Brush OpenedCellBrush { get; private set; } = Brushes.Gray;

        public void ApplyTheme(string theme, ResourceDictionary resources)
        {
            string themeFile = theme == "Світла" ? "Themes/LightTheme.xaml" : "Themes/DarkTheme.xaml";

            try
            {
                var dict = new ResourceDictionary
                {
                    Source = new Uri(themeFile, UriKind.Relative)
                };

                resources.MergedDictionaries.Clear();
                resources.MergedDictionaries.Add(dict);

                OpenedCellBrush = theme == "Світла"
                    ? new SolidColorBrush(Color.FromRgb(220, 220, 220))
                    : new SolidColorBrush(Color.FromRgb(60, 63, 70));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося застосувати тему: {ex.Message}", "Помилка теми", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
