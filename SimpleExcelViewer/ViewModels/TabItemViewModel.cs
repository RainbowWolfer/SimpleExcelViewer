using DevExpress.Mvvm;
using SimpleExcelViewer.Models;
using System.IO;

namespace SimpleExcelViewer.ViewModels;

public class TabItemViewModel : BindableBase {
	public string FilePath { get; }
	public string FileName { get; }

	public TableModel TableModel {
		get => GetProperty(() => TableModel);
		set => SetProperty(() => TableModel, value);
	}


	public bool IsLoading {
		get => GetProperty(() => IsLoading);
		set => SetProperty(() => IsLoading, value);
	}


	public TabItemViewModel(string filePath) {
		FilePath = filePath;
		FileName = Path.GetFileName(filePath);
	}

	public async Task LoadAsync() {
		IsLoading = true;
		try {
			await Task.Delay(10);



		} finally {
			IsLoading = false;
		}
	}
}
