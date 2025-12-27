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
			if (TableModel != null) {
				TableModel.IsTransposed = IsTransposed;
			}
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

	public bool IsInterrupted {
		get => GetProperty(() => IsInterrupted);
		set => SetProperty(() => IsInterrupted, value);
	}

	public string WarningMessage {
		get => GetProperty(() => WarningMessage);
		set => SetProperty(() => WarningMessage, value);
	}

	public bool IsTransposed {
		get => GetProperty(() => IsTransposed);
		set {
			SetProperty(() => IsTransposed, value);
			if (TableModel != null) {
				TableModel.IsTransposed = IsTransposed;
			}
		}
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

			StatusReport.SetStatus("Reading");
			StatusReport.SetProgress(null);

			//await Task.Delay(500000000);

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
			});

			StatusReport.SetStatus("Done");
			//long v = data.EstimateMemoryUsage();

			if (!IsInterrupted) {
				token.ThrowIfCancellationRequested();
			}

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

	public void Interrupt() {
		IsInterrupted = true;
		cts?.Cancel();
		cts?.Dispose();
		cts = null;
		WarningMessage = "Partial data: Loading was interrupted.";
	}

	public void Dispose() {
		TableModel = null;
		cts?.Cancel();
		cts?.Dispose();
		cts = null;
	}
}
