using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using RW.Base.WPF.Interfaces;
using SimpleExcelViewer.Controls;
using SimpleExcelViewer.Events;
using System.Windows.Input;

namespace SimpleExcelViewer.Views;

public partial class MainWindow : WindowBase {
	public MainWindow() {
		InitializeComponent();
	}
}

internal class MainWindowViewModel(IEventAggregator eventAggregator, IApplication application) : ViewModelBase {

	private DelegateCommand<KeyEventArgs>? previewKeyDownCommand;
	public IDelegateCommand PreviewKeyDownCommand => previewKeyDownCommand ??= new(PreviewKeyDown);
	private void PreviewKeyDown(KeyEventArgs args) {
		eventAggregator.GetEvent<MainWindowPreviewKeyDownEvent>().Publish(args);
	}



	private DelegateCommand<EventArgs>? closedCommand;
	public IDelegateCommand ClosedCommand => closedCommand ??= new(Closed);
	private void Closed(EventArgs args) {
		application.Shutdown();
	}

}