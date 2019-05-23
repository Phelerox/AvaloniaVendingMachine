using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;

namespace VendingMachine {
    public class StockedProductConverter : IMultiValueConverter {
        public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            return new StockedProduct((IProduct)values[0], (uint)values[1]);
        }
    }

    // public class TupleConverter : IMultiValueConverter {
    //     public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    //         Tuple<object, object> tuple = new Tuple<object, object>(values[0], values[1]);
    //         return tuple;
    //     }
    // }

    // public class ProductConverter : IMultiValueConverter {
    //     public object Convert(IList<object> values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    //         if (values != null) {
    //             string valueString = "";
    //             for (int n = 0; n < values.Count; n++) {
    //                 if (valueString.Length > 0) {
    //                     valueString += " ";
    //                 }
    //                 valueString += values[n];
    //             }
    //             return values[0] + " " + values[1];
    //         }
    //         return " ";
    //     }

    //     public IList<object> ConvertBack(object value, IList<Type> targetTypes, object parameter, System.Globalization.CultureInfo culture) {
    //         var values = new List<object>();
    //         if (value != null) {
    //             values.AddRange(value.ToString().Split(' '));
    //             return values;
    //         }
    //         throw new ArgumentException();
    //     }
    // }

    // public class IntegerConverter : IValueConverter {
    //     public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    //         if (value != null) {
    //             return System.Convert.ChangeType(value, targetType);

    //         }
    //         throw new ArgumentException();
    //     }

    //     public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
    //         if (value != null) {
    //             return System.Convert.ChangeType(value, targetType);
    //         }
    //         throw new ArgumentException();
    //     }
    // }
}
