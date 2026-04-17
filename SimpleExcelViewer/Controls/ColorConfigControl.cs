using SimpleExcelViewer.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SimpleExcelViewer.Controls;

public class ColorConfigControl : Control {


	public ColorEditItem ColorEditItem {
		get => (ColorEditItem)GetValue(ColorEditItemProperty);
		set => SetValue(ColorEditItemProperty, value);
	}

	public static readonly DependencyProperty ColorEditItemProperty = DependencyProperty.Register(
		nameof(ColorEditItem),
		typeof(ColorEditItem),
		typeof(ColorConfigControl),
		new PropertyMetadata(null)
	);


}
