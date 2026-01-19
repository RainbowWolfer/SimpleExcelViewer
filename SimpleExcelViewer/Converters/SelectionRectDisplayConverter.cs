using FastWpfGrid;
using System.Globalization;
using System.Windows.Data;

namespace SimpleExcelViewer.Converters;

internal class SelectionRectDisplayConverter : IValueConverter {
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is SelectionRect selectionRect) {
			int rowCount = selectionRect.RectTo.Row - selectionRect.RectFrom.Row + 1;
			int columnCount = selectionRect.RectTo.Column - selectionRect.RectFrom.Column + 1;
			return $"({rowCount}, {columnCount})";
		}
		return value;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}
