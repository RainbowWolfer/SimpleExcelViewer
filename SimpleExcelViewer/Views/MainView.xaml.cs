using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using SimpleExcelViewer.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views;

public partial class MainView : UserControl {
	public MainView() {
		InitializeComponent();
	}
}


internal class MainViewModel : ViewModelBase {

	private IOpenFileDialogService OpenFileDialogService => GetService<IOpenFileDialogService>();

	public ObservableCollection<TabItemViewModel> TabItems { get; } = [];

	public TabItemViewModel SelectedItem {
		get => GetProperty(() => SelectedItem);
		set => SetProperty(() => SelectedItem, value);
	}

	public MainViewModel() {

	}

	private DelegateCommand? openCommand;
	public IDelegateCommand OpenCommand => openCommand ??= new(Open);
	private void Open() {
		try {
			string filterString = string.Join("|", [
				"CSV File (*.csv)|*.csv",
				"Excel File (*.xlsx;*.xls)|*.xlsx;*.xls",
				"All File (*.*)|*.*",
			]);

			OpenFileDialogService.Filter = filterString;
			if (OpenFileDialogService.ShowDialog()) {
				string filePath = OpenFileDialogService.GetFullFileName();
				TabItemViewModel item = new(this, filePath);
				TabItems.Add(item);
				SelectedItem = item;
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBox.Show(ex.ToString(), "ERROR");
		}
	}



	private DelegateCommand<TabItemViewModel>? closeCommand;
	public IDelegateCommand CloseCommand => closeCommand ??= new(Close, CanClose);
	private void Close(TabItemViewModel item) {
		if (CanClose(item)) {
			if (MessageBox.Show($"Are you sure to close ({item.FileName}) ？", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) {
				TabItems.Remove(item);
			}
		}
	}
	private bool CanClose(TabItemViewModel item) => item != null;



	private DelegateCommand<TabItemViewModel>? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload, CanReload);
	private void Reload(TabItemViewModel item) {
		if (CanReload(item)) {

		}
	}
	private bool CanReload(TabItemViewModel item) => item != null;


}