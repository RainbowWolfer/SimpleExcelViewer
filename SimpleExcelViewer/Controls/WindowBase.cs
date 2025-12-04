using System.Windows;
using System.Windows.Media.Imaging;

namespace SimpleExcelViewer.Controls;

public class WindowBase : Window {
	public WindowBase() {
		Icon = new BitmapImage(new Uri("pack://application:,,,/Icon.png"));
	}
}
