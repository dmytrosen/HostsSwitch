﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HostSwitch.Controls.Common
{
    public class BoolToVisibleOrHiddenOrCollapsed : IValueConverter
    {
        public bool Collapse { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value
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
