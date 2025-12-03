using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.Events;
using SimpleExcelViewer.Services;
using SimpleExcelViewer.ViewModels;
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
	IEventAggregator eventAggregator
) : ViewModelBase {

	private IOpenFileDialogService OpenFileDialogService => GetService<IOpenFileDialogService>();
	private IMessageBoxServiceEx MessageBoxService => GetService<IMessageBoxServiceEx>();

	public IAppManager AppManager { get; } = appManager;
	public IRecentFilesService RecentFilesService { get; } = recentFilesService;


	public ObservableCollection<TabItemViewModel> TabItems { get; } = [];

	public TabItemViewModel SelectedItem {
		get => GetProperty(() => SelectedItem);
		set => SetProperty(() => SelectedItem, value);
	}

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
			if (MessageBoxService.ShowOkCancelQuestion($"Are you sure to close ({item.FileName}) ？")) {
				item.Dispose();
				TabItems.Remove(item);
			}
		}
	}
	private bool CanClose(TabItemViewModel item) => item != null;



	private DelegateCommand<TabItemViewModel>? closeOthersCommand;
	public IDelegateCommand CloseOthersCommand => closeOthersCommand ??= new(CloseOthers, CanCloseOthers);
	private void CloseOthers(TabItemViewModel item) {
		if (CanCloseOthers(item)) {
			if (MessageBoxService.ShowOkCancelQuestion($"Are you sure to close all others except ({item.FileName}) ？")) {
				TabItemViewModel[] others = [.. TabItems.Where(x => x != item)];
				foreach (TabItemViewModel _item in others) {
					_item.Dispose();
					TabItems.Remove(_item);
				}
			}
		}
	}
	private bool CanCloseOthers(TabItemViewModel item) => item != null;


	private DelegateCommand<TabItemViewModel>? closeAllCommand;
	public IDelegateCommand CloseAllCommand => closeAllCommand ??= new(CloseAll, CanCloseAll);
	private void CloseAll(TabItemViewModel item) {
		if (CanCloseAll(item)) {
			foreach (TabItemViewModel _item in TabItems) {
				_item.Dispose();
			}
			TabItems.Clear();
		}
	}
	private bool CanCloseAll(TabItemViewModel item) => item != null;




	private DelegateCommand<TabItemViewModel>? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload, CanReload);
	private void Reload(TabItemViewModel item) {
		if (CanReload(item)) {

		}
	}
	private bool CanReload(TabItemViewModel item) => item != null;


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


}