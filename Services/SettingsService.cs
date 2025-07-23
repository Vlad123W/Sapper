using Newtonsoft.Json;
using saper1.Data;
using saper1.IServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saper1.Services
{
    public class SettingsService : ISettingsService
    {
        private const string FileName = "myconfig.json";
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;
        public SettingsData SettingsData { get; set; } = new(){Theme = "Темна", Difficulty = "Любитель"};


        public SettingsService()
        {
            _settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "scfg");
            _settingsFilePath = Path.Combine(_settingsDirectory, FileName);
        }

        public void Load()
        {
            try
            {
                if (!Directory.Exists(_settingsDirectory))
                    Directory.CreateDirectory(_settingsDirectory);

                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    SettingsData = JsonConvert.DeserializeObject<SettingsData>(json)!;
                    
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Не вдалося завантажити конфігурацію: {ex.Message}", "Помилка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        public void Save(SettingsData settingsData)
        {
            try
            {
                if (!Directory.Exists(_settingsDirectory))
                    Directory.CreateDirectory(_settingsDirectory);

                File.WriteAllText(_settingsFilePath, JsonConvert.SerializeObject(settingsData, Formatting.Indented));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
