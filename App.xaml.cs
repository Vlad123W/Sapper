using System.Configuration;
using System.Data;
using System.Windows;

namespace saper1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ApplyTheme("Dark"); 
        }

        public void ApplyTheme(string themeName)
        {
            ResourceDictionary newTheme = new ResourceDictionary();

            Application.Current.Resources.MergedDictionaries.Clear();

            if (themeName == "Темна" || themeName == "Dark")
            {
                newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }
            else if (themeName == "Світла" || themeName == "Light")
            {
                newTheme.Source = new Uri("Themes/LightTheme.xaml", UriKind.Relative);
            }
            else
            {
                newTheme.Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative);
            }

            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }
    }

}
