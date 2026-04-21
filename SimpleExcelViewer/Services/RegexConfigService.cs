using DevExpress.Mvvm.Native;
using Newtonsoft.Json;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Common;
using RW.Common.Models;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.Configs;
using SimpleExcelViewer.ViewModels;
using System.IO;

namespace SimpleExcelViewer.Services;

public interface IRegexConfigService {
	event TypedEventHandler<IRegexConfigService, EventArgs>? RegexConfigChanged;


	RegexConfigModel? RegexConfigModel { get; }

	IEnumerable<RegexConfigItem> GetRegexConfigItems();
	void SaveRegexConfigItems(IEnumerable<RegexConfigItem> items);
}

internal class RegexConfigService(AppFolderConfig folderConfig) : IRegexConfigService, IAppInitializeAsync {
	string IAppInitializeAsync.Description => "Initializing Regex Config Service";
	int IPriority.Priority => 0;

	public event TypedEventHandler<IRegexConfigService, EventArgs>? RegexConfigChanged;

	private RegexConfigModel? model;
	public RegexConfigModel? RegexConfigModel => model;

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {
		Reload();
	}

	private void Reload() {
		try {
			string filePath = folderConfig.RegexConfigFilePath;
			if (File.Exists(filePath)) {
				string json = File.ReadAllText(filePath);
				model = JsonConvert.DeserializeObject<RegexConfigModel>(json);
			}
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
		}
	}

	public IEnumerable<RegexConfigItem> GetRegexConfigItems() {
		if (model is null) {
			yield break;
		}

		foreach (RegexConfigModelItem item in model.Items) {
			RegexConfigItem r = new() {
				Regex = item.Regex,
			};

			r.BackColor.Color = item.BackColor.ToMediaColor();
			r.TextColor.Color = item.TextColor.ToMediaColor();

			r.EnableTextColor = item.EnableTextColor;
			r.EnableBackColor = item.EnableBackColor;

			yield return r;
		}
	}

	public void SaveRegexConfigItems(IEnumerable<RegexConfigItem> items) {
		try {
			string filePath = folderConfig.RegexConfigFilePath;

			RegexConfigModel model = new() {
				Items = [.. items.Select(x => new RegexConfigModelItem() {
					Regex = x.Regex,
					BackColor = x.BackColor.Color.ToSimpleColor(),
					TextColor = x.TextColor.Color.ToSimpleColor(),
					EnableBackColor = x.EnableBackColor,
					EnableTextColor = x.EnableTextColor,
				})],
			};

			this.model = model;

			RegexConfigChanged?.Invoke(this, EventArgs.Empty);

			string json = JsonConvert.SerializeObject(model, Formatting.Indented);
			File.WriteAllText(filePath, json);
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
		}
	}

}

public class RegexConfigModel {
	public required RegexConfigModelItem[] Items { get; init; }
}

public class RegexConfigModelItem {
	public required string Regex { get; init; }
	public required bool EnableBackColor { get; init; }
	public required bool EnableTextColor { get; init; }
	public required SimpleColor BackColor { get; init; }
	public required SimpleColor TextColor { get; init; }
}
