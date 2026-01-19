using FastWpfGrid;
using System.Globalization;
using System.Windows.Data;

namespace SimpleExcelViewer.Converters;

internal class SimpleCellAddressDisplayConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is SimpleCellAddress cell) {
			return $"(Row: {cell.Row + 1}, Column: {cell.Column + 1})";
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}
