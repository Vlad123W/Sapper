using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using saper1.Data;

namespace saper1.IServices
{
    public interface ISettingsService
    {
        SettingsData _settingsData { get; set; }
        void Load();
        void Save(string difficulty, string theme);
    }
}
