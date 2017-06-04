using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace HostSwitch.Controls.Common
{
    public class BoolToErrorColor : IValueConverter
    {
        private static readonly SolidColorBrush SolidColorBrushBlack = new SolidColorBrush(Colors.Black);
        private static readonly SolidColorBrush SolidColorBrushRed = new SolidColorBrush(Colors.Red);

        public bool ReverseBoolean { get; set; } = false;
        public SolidColorBrush TrueColor { get; set; } = SolidColorBrushBlack;
        public SolidColorBrush ErrorColor { get; set; } = SolidColorBrushRed;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value
                ? ReverseBoolean ? ErrorColor : TrueColor
                : ReverseBoolean ? TrueColor : ErrorColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ReverseBoolean
                ? (SolidColorBrush)value == TrueColor
                : (SolidColorBrush)value != TrueColor;
        }
    }
}
