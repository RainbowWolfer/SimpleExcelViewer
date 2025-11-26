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
				"CSV 文件 (*.csv)|*.csv",
				"Excel 文件 (*.xlsx;*.xls)|*.xlsx;*.xls",
				"所有文件 (*.*)|*.*",
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


}