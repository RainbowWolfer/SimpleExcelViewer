using Newtonsoft.Json;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.Services;
using SimpleExcelViewer.Configs;

namespace SimpleExcelViewer.Services;

public interface IRecentFilesService : ISettingsServiceBase<RecentFilesModel> {

}

internal class RecentFilesService(AppFolderConfig appFolderConfig) : JsonSettingsServiceBase<RecentFilesModel>, IRecentFilesService, IAppInitializeAsync {
	public override string FilePath => appFolderConfig.RecentFilesConfigFilePath;

	string IAppInitializeAsync.Description => "Loading Recent Files Config";
	int IPriority.Priority => 0;

	public override RecentFilesModel GetDefaultModel() => new();

	Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) => LoadSettingsAsync();
}

[JsonObject]
public class RecentFilesModel {

}
