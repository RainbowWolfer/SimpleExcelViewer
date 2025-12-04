using RW.Common.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace SimpleExcelViewer.Converters;

internal class FileSizeConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is long l) {
			return FileHelper.FileSizeToKB(l);
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}
