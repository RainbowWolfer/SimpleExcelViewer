using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;
using SimpleExcelViewer.Configs;
using SimpleExcelViewer.Enums;

namespace SimpleExcelViewer.Services;

public interface IAppSettingsService : ISettingsServiceBase<AppSettingsModel> {

}

internal class AppSettingsService(
	AppFolderConfig appFolderConfig
) : JsonSettingsServiceBase<AppSettingsModel>, ISingletonDependency, IAppSettingsService {
	public override string FilePath => appFolderConfig.AppSettingsFilePath;
	public override AppSettingsModel GetDefaultModel() => new();
}

/// <summary> map to self </summary>
[JsonObject]
public class AppSettingsModel {
	public bool AllowMultipleInstances { get; set; } = true;



	public AppLanguage AppLanguage { get; set; } = AppLanguage.English;

	public bool ConfirmOnClosingApplication { get; set; } = true;

	public bool ConfirmOnClosingSingleTab { get; set; } = true;

	public bool ConfirmOnClosingOtherTabs { get; set; } = true;

	public bool ConfirmOnClosingAllTabs { get; set; } = true;

}