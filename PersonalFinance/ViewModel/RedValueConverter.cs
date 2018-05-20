using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Lomont.PersonalFinance.ViewModel
{
    // convert <0 values to red color, else null
    public class RedValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var dg = values[0] as DataGridCell;
            var dr = values[1] as DataRow;
            if (dr == null)
                return Brushes.Transparent;

            foreach (var v in dr.ItemArray)
            {
                //if (Double.TryParse(v as String, out var vv) && vv < 0)
                //    return Brushes.Red;
            }

            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
