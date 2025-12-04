using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.ViewModels;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.Interfaces;
using SimpleExcelViewer.Models;
using SimpleExcelViewer.Views;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.ViewModels;

internal class TabItemViewModel : BindableBase, IDisposable {
	public string FilePath { get; }
	public string FileName { get; }

	public FileInfo FileInfo { get; }

	public TableModel? TableModel {
		get => GetProperty(() => TableModel);
		set {
			TableModel?.Dispose();
			SetProperty(() => TableModel, value);
		}
	}

	public bool IsLoading {
		get => GetProperty(() => IsLoading);
		set => SetProperty(() => IsLoading, value);
	}

	public string ErrorMessage {
		get => GetProperty(() => ErrorMessage);
		set => SetProperty(() => ErrorMessage, value);
	}

	public StatusReport StatusReport { get; } = new();

	public TabView View { get; } = new();

	private CancellationTokenSource? cts;

	public TabItemViewModel(MainViewModel mainViewModel, string filePath) {
		FilePath = filePath;
		FileName = Path.GetFileName(filePath);
		FileInfo = new FileInfo(filePath);

		ViewModelExtensions.SetParameter(View, this);
		ViewModelExtensions.SetParentViewModel(View, mainViewModel);
	}

	public async Task LoadAsync(IDispatcherService dispatcherService) {
		ErrorMessage = string.Empty;

		if (IsLoading) {
			return;
		}

		if (!File.Exists(FilePath)) {
			ErrorMessage = $"File does not exist.\n{FilePath}";
			return;
		}

		cts = new CancellationTokenSource();
		CancellationToken token = cts.Token;

		IsLoading = true;
		try {
			TableModel = null;
			dispatcherService.Invoke(AppHelper.ReleaseRAM);

			ITableData data = await Task.Run(() => {
				using FileStream fileStream = new(
					FilePath,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite,
					1024 * 10,
					FileOptions.SequentialScan
				);
				return CsvDataRaw.Read(fileStream, Encoding.UTF8, ',', StatusReport, token);
				//return CsvDataBuffer.Read(fileStream, Encoding.UTF8);
				//return CsvData.Read(fileStream, Encoding.UTF8, [',']);
				//return CsvDataTableReader.Read(fileStream, Encoding.UTF8, [',']);
			}, token);

			//long v = data.EstimateMemoryUsage();

			token.ThrowIfCancellationRequested();
			TableModel = new TableModel(data);
		} catch (OperationCanceledException) {
			Debug.WriteLine("Canceled");
		} catch (Exception ex) {
			ErrorMessage = $"File load error\n{ex}";
			DebugLoggerManager.LogHandledException(ex);
		} finally {
			IsLoading = false;
			dispatcherService.Invoke(AppHelper.ReleaseRAM);
			cts?.Dispose();
			cts = null;
		}
	}

	public void Dispose() {
		TableModel = null;
		cts?.Cancel();
		cts?.Dispose();
		cts = null;
	}
}
