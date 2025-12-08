using Autofac;
using RW.Base.WPF;
using RW.Base.WPF.Configs;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.ViewModels;
using SimpleExcelViewer.Configs;
using SimpleExcelViewer.Services;
using SimpleExcelViewer.Views;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
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

	private AppSettingsService AppSettingsService { get; }

	protected override bool EnablePipeServerStream => false;

	public App() {
		instance = this;

		// reduce the memory from 100MB to 23MB. but it might reduce render performance.
		//RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
		RenderOptions.ProcessRenderMode = RenderMode.Default;

		AppSettingsService = new AppSettingsService((AppFolderConfig)FolderConfig);
		AppSettingsService.LoadSettings();
	}

	protected override string GetMutexName() {
		if (AppSettingsService.Model.AllowMultipleInstances) {
			return string.Empty;
		} else {
			return base.GetMutexName();
		}
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

	private class _DllLoader() : DllLoader() {
		protected override void AfterInitialized(IReadOnlyDictionary<string, Assembly> pool, IReadOnlyDictionary<string, Type> types) {
			base.AfterInitialized(pool, types);

			Debug.WriteLine(new string('-', 30));

			foreach (KeyValuePair<string, Assembly> entry in pool) {
				Debug.WriteLine($"Assembly: {entry}");
			}

			foreach (KeyValuePair<string, Type> entry in types) {
				Debug.WriteLine($"Type: {entry}");
			}

			Debug.WriteLine(new string('-', 30));
		}
	}

	private class _IoCInitializer(IApplication application) : IoCInitializer(application) {
		private readonly App application = (App)application;

		protected override void InitializeDependencies() {
			base.InitializeDependencies();

			builder.RegisterInstance(application.AppSettingsService).As<IAppSettingsService>();

		}
	}
}
