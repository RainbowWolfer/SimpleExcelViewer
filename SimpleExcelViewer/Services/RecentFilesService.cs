using LiteDB;
using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using SimpleExcelViewer.Configs;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace SimpleExcelViewer.Services;

public interface IRecentFilesService {
	ICollectionView CollectionView { get; }
	bool IsEmpty { get; }
	void Update(string filePath);
	void ClearAllRecords();
}

internal class RecentFilesService : EntityRepository, IRecentFilesService, IAppInitializeAsync {
	public override string FilePath => appFolderConfig.RecentFilesConfigFilePath;

	private ILiteCollection<RecentFileItemModel>? collection;
	private readonly AppFolderConfig appFolderConfig;

	public ObservableCollection<RecentFileItemModel> List { get; } = [];
	public ICollectionView CollectionView { get; }

	public bool IsEmpty => List.IsEmpty();

	string IAppInitializeAsync.Description => "Loading Recent Files Config";
	int IPriority.Priority => 0;

	public RecentFilesService(AppFolderConfig appFolderConfig) {
		this.appFolderConfig = appFolderConfig;

		List.CollectionChanged += List_CollectionChanged;

		CollectionView = CollectionViewSource.GetDefaultView(List);
		CollectionView.SortDescriptions.Add(
			new SortDescription(nameof(RecentFileItemModel.DateTime), ListSortDirection.Descending)
		);
	}

	private void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
		RaisePropertyChanged(() => IsEmpty);
	}

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {
		await Task.CompletedTask;
		Initialize();

		collection = Database.GetCollection<RecentFileItemModel>("RecentFiles");

		IEnumerable<RecentFileItemModel> list = collection.Query().ToEnumerable();
		foreach (RecentFileItemModel item in list) {
			List.Add(item);
		}
	}

	public void Update(string filePath) {
		//string _filePath = FileHelper.NormalizePathSafe(filePath);
		if (collection == null) {
			return;
		}

		RecentFileItemModel existing = collection.FindOne(x => x.FilePath == filePath);
		if (existing != null) {
			RecentFileItemModel updated = existing with { DateTime = DateTime.Now };
			collection.Update(updated);

			int index = List.IndexOf(existing);
			if (index >= 0) {
				List[index] = updated;
			}
		} else {
			RecentFileItemModel model = new(ObjectId.NewObjectId(), filePath, DateTime.Now);
			collection.Insert(model);
			List.Add(model);
		}

		while (List.Count > 20) {
			RecentFileItemModel? oldest = List.OrderBy(x => x.DateTime).FirstOrDefault();
			if (oldest is null) {
				break;
			}
			List.Remove(oldest);
			collection.Delete(oldest.Id);
		}

		Database?.Checkpoint();
	}

	public void ClearAllRecords() {
		_ = collection?.DeleteAll();
		List.Clear();
		Database?.Checkpoint();
	}
}

public record class RecentFileItemModel([BsonId] ObjectId Id, string FilePath, DateTime DateTime);

