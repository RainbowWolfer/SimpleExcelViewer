using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModelServices;
using SimpleExcelViewer.Controls;
using SimpleExcelViewer.Events;
using SimpleExcelViewer.Services;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SimpleExcelViewer.Views;

public partial class MainWindow : WindowBase {
	public MainWindow() {
		InitializeComponent();
	}
}

internal class MainWindowViewModel(
	IEventAggregator eventAggregator,
	IApplication application,
	IAppStatusService appStatusService,
	IAppSettingsService appSettingsService
) : ViewModelBase {

	private ICurrentWindowServiceEx CurrentWindowService => GetService<ICurrentWindowServiceEx>();

	private IMessageBoxServiceEx MessageBoxService => GetService<IMessageBoxServiceEx>();

	private WindowState previousWindowState = WindowState.Normal;


	protected override void OnInitializeInRuntime() {
		base.OnInitializeInRuntime();

		appStatusService.OnFullScreenChanged += AppStatusService_OnFullScreenChanged;
	}


	private DelegateCommand<EventArgs>? stateChangedCommand;
	public IDelegateCommand StateChangedCommand => stateChangedCommand ??= new(StateChanged);
	private void StateChanged(EventArgs args) {
		//if (appStatusService.FullScreen) {
		//	appStatusService.FullScreen = false;
		//}
	}

	private void AppStatusService_OnFullScreenChanged(IAppStatusService sender, EventArgs args) {
		Window window = CurrentWindowService.GetWindow();
		if (sender.FullScreen) {
			previousWindowState = window.WindowState;
			window.WindowState = WindowState.Maximized;
			window.WindowStyle = WindowStyle.None;
		} else {
			window.WindowState = previousWindowState;
			window.WindowStyle = WindowStyle.SingleBorderWindow;
		}
	}

	private DelegateCommand<KeyEventArgs>? previewKeyDownCommand;
	public IDelegateCommand PreviewKeyDownCommand => previewKeyDownCommand ??= new(PreviewKeyDown);
	private void PreviewKeyDown(KeyEventArgs args) {

		if (args.Key == Key.F11) {
			args.Handled = true;
			appStatusService.FullScreen = !appStatusService.FullScreen;
			return;
		}


		eventAggregator.GetEvent<MainWindowPreviewKeyDownEvent>().Publish(args);

	}



	private DelegateCommand<EventArgs>? closedCommand;
	public IDelegateCommand ClosedCommand => closedCommand ??= new(Closed);
	private void Closed(EventArgs args) {
		application.Shutdown();
	}



	private DelegateCommand<CancelEventArgs>? closingCommand;
	public IDelegateCommand ClosingCommand => closingCommand ??= new(Closing);
	private void Closing(CancelEventArgs args) {
		if (appSettingsService.Model.ConfirmOnClosingApplication) {
			if (!MessageBoxService.ShowOkCancelQuestion("Are you sure to quit this application?")) {
				args.Cancel = true;
				return;
			}
		}
	}


}