using DevExpress.Mvvm;
using SimpleExcelViewer.Models;
using System.IO;
using System.Text;

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
		if (IsLoading) {
			return;
		}
		if (!File.Exists(FilePath)) {
			return;
		}

		IsLoading = true;
		try {
			CsvData data = await Task.Run(() => {
				using FileStream fileStream = new(
					FilePath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite,
					1024 * 10,
					FileOptions.SequentialScan
				);
				return CsvData.Read(fileStream, Encoding.UTF8, false, [',']);
			});

			//long v = data.EstimateMemoryUsage();

			TableModel = new TableModel(data);
		} finally {
			IsLoading = false;
		}
	}
}
