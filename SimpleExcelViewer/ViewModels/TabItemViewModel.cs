using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using SimpleExcelViewer.Interfaces;
using SimpleExcelViewer.Models;
using SimpleExcelViewer.Views;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.ViewModels;

internal class TabItemViewModel : BindableBase {
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

	public TabView View { get; } = new();

	public TabItemViewModel(MainViewModel mainViewModel, string filePath) {
		FilePath = filePath;
		FileName = Path.GetFileName(filePath);

		ViewModelExtensions.SetParameter(View, this);
		ViewModelExtensions.SetParentViewModel(View, mainViewModel);
	}

	public async Task LoadAsync() {
		if (IsLoading) {
			return;
		}
		if (!File.Exists(FilePath)) {
			return;
		}

		IsLoading = true;
		try {
			ITableData data = await Task.Run(() => {
				using FileStream fileStream = new(
					FilePath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite,
					1024 * 10,
					FileOptions.SequentialScan
				);
				//return CsvDataBuffer.Read(fileStream, Encoding.UTF8);
				return CsvDataRaw.Read(fileStream, Encoding.UTF8);
				//return CsvData.Read(fileStream, Encoding.UTF8, [',']);
				//return CsvDataTableReader.Read(fileStream, Encoding.UTF8, [',']);
			});

			//long v = data.EstimateMemoryUsage();

			TableModel = new TableModel(data);
		} finally {
			IsLoading = false;
		}
	}
}
