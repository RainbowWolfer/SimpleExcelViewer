using RW.Base.WPF;
using RW.Base.WPF.Configs;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModels;
using SimpleExcelViewer.Configs;
using SimpleExcelViewer.Views;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace SimpleExcelViewer;

public partial class App : ApplicationBase {
	private static App? instance;
	public static App Instance => instance!;

	static App() {
		DebugConfig.Print = o => Debug.WriteLine(o);
		DebugConfig.DebuggerBreak = Debugger.Break;
	}

	public App() {
		instance = this;

		// reduce the memory from 100MB to 23MB. but it might reduce render performance.
		//RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
		RenderOptions.ProcessRenderMode = RenderMode.Default;
	}

	protected override Window GetMainWindow() => new MainWindow();

	protected override CultureInfo? GetCultureInfo() {
		// todo
		return null;
	}

	protected override AppManager GetAppManager() => new _AppManager();
	protected override DllLoader GetDllLoader() => new _DllLoader();
	protected override IoCInitializer GetIoCInitializer(IApplication application) => new _IoCInitializer(application);
	protected override FolderConfig GetFolderConfig(IAppManager appManager) => new AppFolderConfig(appManager);

	protected override void ShowFatalDialog(Exception exception) {
		MessageBox.Show(exception.ToString(), "Fatal Error");
	}


	private class _AppManager : AppManager {
		public override string AppName => AppConfig.AppName;
		public override string BuildMode => AppConfig.IsRelease ? "Release" : "Debug";
		public override bool IsRelease => AppConfig.IsRelease;
	}

	private class _DllLoader : DllLoader {

	}

	private class _IoCInitializer(IApplication application) : IoCInitializer(application) {

	}
}
