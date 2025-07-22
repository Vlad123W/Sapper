using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace saper1.IServices
{
    public interface IThemeManager
    {
        Brush OpenedCellBrush { get; }
        void ApplyTheme(string theme, ResourceDictionary resources);
    }
}
