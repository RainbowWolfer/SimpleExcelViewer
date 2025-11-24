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
				TabItems.Add(new TabItemViewModel(filePath));
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBox.Show(ex.ToString(), "ERROR");
		}
	}


}