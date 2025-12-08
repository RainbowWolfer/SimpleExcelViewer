using DevExpress.Mvvm;
using Newtonsoft.Json;
using RW.Base.WPF.Interfaces;
using SimpleExcelViewer.Configs;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace SimpleExcelViewer.Services;

public interface IRecentFilesService {
	ICollectionView CollectionView { get; }
	bool IsEmpty { get; }
	void Update(string filePath);
	void ClearAllRecords();
}

internal class RecentFilesService : BindableBase, IRecentFilesService, IAppInitializeAsync, IAppClosedHandler {
	public string FilePath => appFolderConfig.RecentFilesConfigFilePath;

	private readonly AppFolderConfig appFolderConfig;
	private readonly FileSystemWatcher watcher;

	// Collection synchronization lock
	private readonly object list_lock = new();

	// Debounce timer for watcher events
	private readonly DispatcherTimer watcherDebounceTimer;

	// Cross-process mutex for file read/write
	private static readonly Mutex fileMutex = new(false, @$"Global\{AppConfig.AppName}_{(Assembly.GetEntryAssembly()?.FullName ?? "default").GetHashCode():X}_RecentFilesMutex");

	public ObservableCollection<RecentFileItemModel> List { get; } = [];
	public ICollectionView CollectionView { get; }

	public bool IsEmpty => List.Count == 0;

	int IPriority.Priority => 0;
	string IAppInitializeAsync.Description => "Loading Recent Files Config";

	int IAppClosedHandler.Priority => 0;
	string IAppClosedHandler.Description => "Closing Recent Files Config";

	public RecentFilesService(AppFolderConfig appFolderConfig, IApplication application) {
		this.appFolderConfig = appFolderConfig;

		// Enable WPF collection synchronization with explicit lock and callback
		BindingOperations.EnableCollectionSynchronization(List, list_lock, CollectionAccessCallback);

		List.CollectionChanged += List_CollectionChanged;

		CollectionView = CollectionViewSource.GetDefaultView(List);
		CollectionView.SortDescriptions.Add(
			new SortDescription(nameof(RecentFileItemModel.DateTime), ListSortDirection.Descending)
		);

		// Setup watcher with minimal notifications
		watcher = new FileSystemWatcher() {
			Path = Path.GetDirectoryName(FilePath)!,
			Filter = Path.GetFileName(FilePath)!,
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
		};
		watcher.Changed += Watcher_Changed;
		watcher.Renamed += Watcher_Changed;
		watcher.Created += Watcher_Changed;
		watcher.Deleted += Watcher_Changed;
		watcher.EnableRaisingEvents = true;

		// Debounce rapid watcher events (e.g., multiple change notifications)
		watcherDebounceTimer = new DispatcherTimer {
			Interval = TimeSpan.FromMilliseconds(200)
		};
		watcherDebounceTimer.Tick += (s, e) => {
			watcherDebounceTimer.Stop();
			// Re-load on UI thread for safe binding updates
			application.Dispatcher.Invoke(LoadFromFileSafe);
		};
	}

	void IAppClosedHandler.AppClosed(AppCloseParameter parameter) {
		try {
			SaveToFileSafe();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		} finally {
			watcher?.Dispose();
		}
	}

	private void List_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		RaisePropertyChanged(() => IsEmpty);
	}

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {
		//await Task.CompletedTask;
		await LoadFromFileSafe();
	}

	// Callback for WPF to access the collection under lock
	private void CollectionAccessCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess) {
		lock (list_lock) {
			accessMethod();
		}
	}

	// Debounced watcher event
	private void Watcher_Changed(object sender, FileSystemEventArgs e) {
		// Reset debounce timer
		watcherDebounceTimer.Stop();
		watcherDebounceTimer.Start();
	}

	// Thread-safe, mutex-protected read with small retry to avoid transient sharing violations
	private async Task LoadFromFileSafe() {
		fileMutex.WaitOne();
		try {
			// Retry in case another process hasn't finished closing handle
			const int maxRetries = 5;
			const int delayMs = 100;

			for (int attempt = 0; attempt < maxRetries; attempt++) {
				try {
					if (!File.Exists(FilePath)) {
						File.WriteAllText(FilePath, "[]");
					}

					string json = File.ReadAllText(FilePath);
					IReadOnlyList<RecentFileItemModel>? list = JsonConvert.DeserializeObject<IReadOnlyList<RecentFileItemModel>>(json);

					lock (list_lock) {
						List.Clear();
						foreach (RecentFileItemModel item in list ?? []) {
							List.Add(item);
						}
					}
					return;
				} catch (IOException) {
					//Thread.Sleep(delayMs);
					await Task.Delay(delayMs);
				} catch (JsonException je) {
					// If partial write caused invalid JSON, wait and retry
					Debug.WriteLine(je);
					await Task.Delay(delayMs);
					//Thread.Sleep(delayMs);
				}
			}
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		} finally {
			fileMutex.ReleaseMutex();
		}
	}

	// Thread-safe, mutex-protected write
	private void SaveToFileSafe() {
		fileMutex.WaitOne();
		try {
			List<RecentFileItemModel> snapshot;
			lock (list_lock) {
				snapshot = [.. List];
			}
			string json = JsonConvert.SerializeObject(snapshot, Formatting.Indented);

			// Atomic replace to reduce partial write risk: write to temp then move
			string tempPath = FilePath + ".tmp";
			File.WriteAllText(tempPath, json);
			File.Copy(tempPath, FilePath, overwrite: true);
			File.Delete(tempPath);
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		} finally {
			fileMutex.ReleaseMutex();
		}
	}

	public void Update(string filePath) {
		// Update list under lock
		lock (list_lock) {
			RecentFileItemModel? existing = List.FirstOrDefault(x => x.FilePath == filePath);
			if (existing != null) {
				RecentFileItemModel updated = existing with { DateTime = DateTime.Now };
				int index = List.IndexOf(existing);
				if (index >= 0) {
					List[index] = updated;
				}
			} else {
				List.Add(new RecentFileItemModel(filePath, DateTime.Now));
			}

			// Keep only latest 20
			while (List.Count > 20) {
				RecentFileItemModel? oldest = List.OrderBy(x => x.DateTime).FirstOrDefault();
				if (oldest is null) {
					break;
				}

				List.Remove(oldest);
			}
		}

		SaveToFileSafe();
	}

	public void ClearAllRecords() {
		lock (list_lock) {
			List.Clear();
		}
		SaveToFileSafe();
	}
}

public record class RecentFileItemModel(string FilePath, DateTime DateTime);
