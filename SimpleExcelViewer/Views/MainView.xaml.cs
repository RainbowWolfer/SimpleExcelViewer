using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.Events;
using SimpleExcelViewer.Services;
using SimpleExcelViewer.Utilities;
using SimpleExcelViewer.ViewModels;
using SimpleExcelViewer.ViewModelServices;
using SimpleExcelViewer.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleExcelViewer.Views;

public partial class MainView : UserControl {
	public MainView() {
		InitializeComponent();
	}
}


internal class MainViewModel(
	IAppManager appManager,
	IRecentFilesService recentFilesService,
	IEventAggregator eventAggregator,
	IApplication application
) : ViewModelBase {

	private IOpenFileDialogService OpenFileDialogService => GetService<IOpenFileDialogService>();
	private IMessageBoxServiceEx MessageBoxService => GetService<IMessageBoxServiceEx>();
	private IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();

	private IDialogServiceEx AppSettingsDialogService => GetService<IDialogServiceEx>(nameof(AppSettingsDialogService));
	private IDialogServiceEx FileInfoDialogService => GetService<IDialogServiceEx>(nameof(FileInfoDialogService));


	public IAppManager AppManager { get; } = appManager;
	public IRecentFilesService RecentFilesService { get; } = recentFilesService;


	public ObservableCollection<TabItemViewModel> TabItems { get; } = [];

	public TabItemViewModel SelectedItem {
		get => GetProperty(() => SelectedItem);
		set => SetProperty(() => SelectedItem, value);
	}

	public AppMemoryCounter AppMemoryCounter { get; } = new();

	protected override void OnInitializeInRuntime() {
		base.OnInitializeInRuntime();

		eventAggregator.GetEvent<MainWindowPreviewKeyDownEvent>().Subscribe(OnMainWindowPreviewKeyDown);
	}

	private void OnMainWindowPreviewKeyDown(KeyEventArgs args) {
		if (KeyboardHelper.ControlPressed) {
			if (args.Key is Key.O) {
				Open();
			} else if (args.Key is Key.V) {
				OpenPastedFilePath();
			} else if (args.Key is Key.W) {
				Close(SelectedItem);
			}
		}
	}

	private DelegateCommand<DragEventArgs>? dropCommand;
	public IDelegateCommand DropCommand => dropCommand ??= new(Drop);
	private void Drop(DragEventArgs args) {
		try {
			if (args.Data is not DataObject dataObject || !dataObject.ContainsFileDropList()) {
				return;
			}

			StringCollection filePaths = dataObject.GetFileDropList();
			foreach (string? filePath in filePaths) {
				HandleFilePath(filePath);
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Opening file failed.", ex);
		}
	}

	private DelegateCommand? openCommand;
	public IDelegateCommand OpenCommand => openCommand ??= new(Open);
	private void Open() {
		try {
			OpenFileDialogService.Filter = AppConfig.ValidFileFilterString;
			OpenFileDialogService.Multiselect = true;
			if (OpenFileDialogService.ShowDialog()) {
				foreach (IFileInfo? fileInfo in OpenFileDialogService.Files) {
					string filePath = fileInfo.GetFullName();
					HandleFilePath(filePath);
				}
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Opening file failed.", ex);
		}
	}


	private DelegateCommand<RecentFileItemModel>? openFileCommand;
	public IDelegateCommand OpenFileCommand => openFileCommand ??= new(OpenFile, CanOpenFile);
	private void OpenFile(RecentFileItemModel item) {
		try {
			if (CanOpenFile(item)) {
				string filePath = item.FilePath;
				if (!File.Exists(filePath)) {
					MessageBoxService.ShowError($"File does not exist.\n{filePath}");
					return;
				}

				HandleFilePath(filePath);
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Opening file failed.", ex);
		}
	}
	private bool CanOpenFile(RecentFileItemModel item) => item.FilePath.IsNotBlank();


	private DelegateCommand? openPastedFilePathCommand;
	public IDelegateCommand OpenPastedFilePathCommand => openPastedFilePathCommand ??= new(OpenPastedFilePath);
	private void OpenPastedFilePath() {
		try {
			if (Clipboard.ContainsFileDropList()) {
				StringCollection fileList = Clipboard.GetFileDropList();
				foreach (string filePath in fileList) {
					HandleFilePath(filePath);
				}
				return;
			}

			string text = Clipboard.GetText();
			if (text.IsNotBlank()) {
				HandleFilePath(text);
				return;
			}

		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Opening file failed.", ex);
		}
	}


	private void HandleFilePath(string filePath) {
		if (!File.Exists(filePath) || !AppConfig.ValidExtensionsSet.Contains(Path.GetExtension(filePath))) {
			return;
		}

		TabItemViewModel item = new(this, filePath);
		TabItems.Add(item);
		SelectedItem = item;

		RecentFilesService.Update(filePath);
	}


	private DelegateCommand<TabItemViewModel>? closeCommand;
	public IDelegateCommand CloseCommand => closeCommand ??= new(Close, CanClose);
	private void Close(TabItemViewModel item) {
		if (CanClose(item)) {
			if (!MessageBoxService.ShowOkCancelQuestion($"Are you sure to close ({item.FileName}) ？")) {
				return;
			}

			item.Dispose();
			TabItems.Remove(item);

			DispatcherService.Invoke(AppHelper.ReleaseRAM);
		}
	}
	private bool CanClose(TabItemViewModel item) => item != null;



	private DelegateCommand<TabItemViewModel>? closeOthersCommand;
	public IDelegateCommand CloseOthersCommand => closeOthersCommand ??= new(CloseOthers, CanCloseOthers);
	private void CloseOthers(TabItemViewModel item) {
		if (CanCloseOthers(item)) {
			if (!MessageBoxService.ShowOkCancelQuestion($"Are you sure to close all others except ({item.FileName}) ？")) {
				return;
			}

			TabItemViewModel[] others = [.. TabItems.Where(x => x != item)];
			foreach (TabItemViewModel _item in others) {
				_item.Dispose();
				TabItems.Remove(_item);
			}
			DispatcherService.Invoke(AppHelper.ReleaseRAM);
		}
	}
	private bool CanCloseOthers(TabItemViewModel item) => item != null && TabItems.Count > 1;


	private DelegateCommand<TabItemViewModel>? closeAllCommand;
	public IDelegateCommand CloseAllCommand => closeAllCommand ??= new(CloseAll, CanCloseAll);
	private void CloseAll(TabItemViewModel item) {
		if (CanCloseAll(item)) {
			if (!MessageBoxService.ShowOkCancelQuestion($"Are you sure to close all files？")) {
				return;
			}

			foreach (TabItemViewModel _item in TabItems) {
				_item.Dispose();
			}
			TabItems.Clear();
			DispatcherService.Invoke(AppHelper.ReleaseRAM);
		}
	}
	private bool CanCloseAll(TabItemViewModel item) => item != null && TabItems.IsNotEmpty();



	private DelegateCommand<TabItemViewModel>? copyFilePathCommand;
	public IDelegateCommand CopyFilePathCommand => copyFilePathCommand ??= new(CopyFilePath, CanCopyFilePath);
	private void CopyFilePath(TabItemViewModel item) {
		if (CanCopyFilePath(item)) {
			item.FilePath.CopyToClipboard();
		}
	}
	private bool CanCopyFilePath(TabItemViewModel item) => item != null;



	private DelegateCommand<TabItemViewModel>? copyFileNameCommand;
	public IDelegateCommand CopyFileNameCommand => copyFileNameCommand ??= new(CopyFileName, CanCopyFileName);
	private void CopyFileName(TabItemViewModel item) {
		if (CanCopyFileName(item)) {
			item.FileName.CopyToClipboard();
		}
	}
	private bool CanCopyFileName(TabItemViewModel item) => item != null;



	private DelegateCommand<TabItemViewModel>? openInFileExplorerCommand;
	public IDelegateCommand OpenInFileExplorerCommand => openInFileExplorerCommand ??= new(OpenInFileExplorer, CanOpenInFileExplorer);
	private void OpenInFileExplorer(TabItemViewModel item) {
		if (CanOpenInFileExplorer(item)) {
			Exception? exception = item.FilePath.ShowInExplorer();
			if (exception != null) {
				MessageBoxService.ShowError("Fail to open in file explorer", exception);
			}
		}
	}
	private bool CanOpenInFileExplorer(TabItemViewModel item) => item != null;



	private DelegateCommand<TabItemViewModel>? viewFileInfoCommand;
	public IDelegateCommand ViewFileInfoCommand => viewFileInfoCommand ??= new(ViewFileInfo, CanViewFileInfo);
	private void ViewFileInfo(TabItemViewModel item) {
		if (CanViewFileInfo(item)) {
			FileInfoDialogParameter parameter = new(item.FileInfo);
			FileInfoDialogService.ShowDialog(this, parameter);
		}
	}
	private bool CanViewFileInfo(TabItemViewModel item) => item != null;



	private AsyncCommand<TabItemViewModel>? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload, CanReload);
	private async Task Reload(TabItemViewModel item) {
		if (CanReload(item)) {
			await item.LoadAsync(DispatcherService);
		}
	}
	private bool CanReload(TabItemViewModel item) => item != null && !item.IsLoading;


	private DelegateCommand<MouseButtonEventArgs>? tabItemMouseDownCommand;
	public IDelegateCommand TabItemMouseDownCommand => tabItemMouseDownCommand ??= new(TabItemMouseDown);
	private void TabItemMouseDown(MouseButtonEventArgs args) {
		if (args.ChangedButton is MouseButton.Middle
			&& args.Source is FrameworkElement frameworkElement
			&& frameworkElement.DataContext is TabItemViewModel tabItemViewModel
		) {
			Close(tabItemViewModel);
			args.Handled = true;
		}
	}




	private DelegateCommand? clearAllRecentFilesCommand;
	public IDelegateCommand ClearAllRecentFilesCommand => clearAllRecentFilesCommand ??= new(ClearAllRecentFiles, CanClearAllRecentFiles);
	private void ClearAllRecentFiles() {
		if (CanClearAllRecentFiles()) {
			if (MessageBoxService.ShowOkCancelQuestion("Are you sure to clear all recent files list？")) {
				RecentFilesService.ClearAllRecords();
			}
		}
	}
	private bool CanClearAllRecentFiles() => !RecentFilesService.IsEmpty;




	private DelegateCommand? openAppSettingsCommand;
	public IDelegateCommand OpenAppSettingsCommand => openAppSettingsCommand ??= new(OpenAppSettings);
	private void OpenAppSettings() {
		if (AppSettingsDialogService.ShowOKCancel(this, new object())) {

		}
	}


	private DelegateCommand? exitCommand;
	public IDelegateCommand ExitCommand => exitCommand ??= new(Exit);
	private void Exit() {
		application.TotalShutdown();
	}



	private DelegateCommand? configHighlightCommand;
	public IDelegateCommand ConfigHighlightCommand => configHighlightCommand ??= new(ConfigHighlight);
	private void ConfigHighlight() {

	}



	private DelegateCommand? openSourceLibrariesCommand;
	public IDelegateCommand OpenSourceLibrariesCommand => openSourceLibrariesCommand ??= new(OpenSourceLibraries);
	private void OpenSourceLibraries() {

	}



	private DelegateCommand? sourceCodeCommand;
	public IDelegateCommand SourceCodeCommand => sourceCodeCommand ??= new(SourceCode);
	private void SourceCode() {

	}


	private DelegateCommand? aboutCommand;
	public IDelegateCommand AboutCommand => aboutCommand ??= new(About);
	private void About() {

	}


}