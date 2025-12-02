using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModels;
using RW.Base.WPF.ViewModelServices;
using RW.Common.WPF.Helpers;
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


internal class MainViewModel(IAppManager appManager) : ViewModelBase {

	private IOpenFileDialogService OpenFileDialogService => GetService<IOpenFileDialogService>();
	private IMessageBoxServiceEx MessageBoxService => GetService<IMessageBoxServiceEx>();

	public IAppManager AppManager { get; } = appManager;

	public ObservableCollection<TabItemViewModel> TabItems { get; } = [];

	public TabItemViewModel SelectedItem {
		get => GetProperty(() => SelectedItem);
		set => SetProperty(() => SelectedItem, value);
	}


	private DelegateCommand<DragEventArgs>? dropCommand;
	public IDelegateCommand DropCommand => dropCommand ??= new(Drop);
	private void Drop(DragEventArgs args) {
		if (args.Data is DataObject dataObject) {
			if (dataObject.ContainsFileDropList()) {
				StringCollection filePaths = dataObject.GetFileDropList();
				foreach (string? filePath in filePaths) {
					if (File.Exists(filePath)) {
						if (AppConfig.ValidExtensionsSet.Contains(Path.GetExtension(filePath))) {
							TabItemViewModel item = new(this, filePath);
							TabItems.Add(item);
							SelectedItem = item;
						}
					}
				}
			}
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
					if (!AppConfig.ValidExtensionsSet.Contains(Path.GetExtension(filePath))) {
						continue;
					}

					TabItemViewModel item = new(this, filePath);
					TabItems.Add(item);
					SelectedItem = item;
				}
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Opening file failed.", ex);
		}
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

}