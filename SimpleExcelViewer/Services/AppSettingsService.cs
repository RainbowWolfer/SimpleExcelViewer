using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;
using SimpleExcelViewer.Configs;

namespace SimpleExcelViewer.Services;

public interface IAppSettingsService : ISettingsServiceBase<AppSettingsModel> {

}

internal class AppSettingsService(
	AppFolderConfig appFolderConfig
) : JsonSettingsServiceBase<AppSettingsModel>, ISingletonDependency, IAppSettingsService {
	public override string FilePath => appFolderConfig.AppSettingsFilePath;
	public override AppSettingsModel GetDefaultModel() => new();
}

[JsonObject]
public class AppSettingsModel {
	public bool AllowMultipleInstances { get; set; } = true;
}