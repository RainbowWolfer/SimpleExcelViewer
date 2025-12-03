using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using SimpleExcelViewer.Events;
using System.Windows;
using System.Windows.Input;

namespace SimpleExcelViewer.Views;

public partial class MainWindow : Window {
	public MainWindow() {
		InitializeComponent();
	}
}


internal class MainWindowViewModel(IEventAggregator eventAggregator) : ViewModelBase {

	private DelegateCommand<KeyEventArgs>? previewKeyDownCommand;
	public IDelegateCommand PreviewKeyDownCommand => previewKeyDownCommand ??= new(PreviewKeyDown);
	private void PreviewKeyDown(KeyEventArgs args) {
		eventAggregator.GetEvent<MainWindowPreviewKeyDownEvent>().Publish(args);
	}
}