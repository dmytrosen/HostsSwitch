using HostSwitch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HostSwitch.Controls.Common
{
    public class AppModeToVisible : IValueConverter
    {
        public EAppMode EnableAppMode { get; set; } = EAppMode.Initializing;
        public bool Collapse { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (EAppMode)value == EnableAppMode 
                ? Visibility.Visible
                : Collapse
                    ? Visibility.Collapsed
                    : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
