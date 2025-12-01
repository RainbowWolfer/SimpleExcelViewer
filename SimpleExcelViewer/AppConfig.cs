using RW.Common.Models;
using System.Diagnostics;
using System.Reflection;

namespace SimpleExcelViewer;

public static class AppConfig {
	public const string AppName = "SimpleExcelViewer";

	public static bool IsRelease {
		get {
#if RELEASE
			return true;
#else
			return false;
#endif
		}
	}

	public static Guid SessionID { get; }
	public static DateTime AppStartTime { get; }
	public static VersionStruct Version { get; }

	static AppConfig() {
		SessionID = Guid.NewGuid();
		AppStartTime = DateTime.Now;

		string location = Assembly.GetEntryAssembly()!.Location;
		FileVersionInfo info = FileVersionInfo.GetVersionInfo(location);
		Version = new VersionStruct(info.FileMajorPart, info.ProductMinorPart, info.FileBuildPart, info.FilePrivatePart);

		Debug.WriteLine(location);
	}




	public static IReadOnlyCollection<string> ValidExtensionsSet { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
		".csv",
		//"",
		//"",
	};

	public static string ValidFileFilterString { get; } = string.Join("|", [
		"CSV File (*.csv)|*.csv",
		"Excel File (*.xlsx;*.xls)|*.xlsx;*.xls",
		"All File (*.*)|*.*",
	]);

}
