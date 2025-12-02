using RW.Base.WPF.Configs;
using RW.Base.WPF.Interfaces;
using System.IO;

namespace SimpleExcelViewer.Configs;

public class AppFolderConfig(IAppManager appManager) : FolderConfig(appManager) {
	//public string RecentFilesConfigFilePath => Path.Combine(FolderConfig.DataFolder, "RecentFiles.json");

}
