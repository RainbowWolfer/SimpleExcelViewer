using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.Configs;
using SimpleExcelViewer.ViewModels;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class AboutDialog : UserControl {
	public AboutDialog() {
		InitializeComponent();
	}

	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
		AppConfig.GithubURL.OpenHyperLink();
	}
}

public record class AboutDialogParameter;

internal class AboutDialogViewModel(AppManagerEx appManager) : DialogViewModelOk<AboutDialogParameter> {
	public AppManagerEx AppManager { get; } = appManager;

	// 绑定属性
	public string AdministratorStatus { get; } = SystemHelper.IsAdministratorSafe ? "True" : "False";
	public string ProcessArchitecture { get; } = RuntimeInformation.ProcessArchitecture.ToString();
	public string DeploymentMode { get; } = CheckDeploymentMode();
	public string FrameworkVersion { get; } = RuntimeInformation.FrameworkDescription;
	public string OSDescription { get; } = RuntimeInformation.OSDescription;

	protected override void OnInitialized() {
		base.OnInitialized();
		DialogTitle = "About";
	}

	/// <summary>
	/// 判断程序的部署模式 (独立部署还是框架依赖)
	/// </summary>
	private static string CheckDeploymentMode() {
		// 获取最底层系统库的加载路径
		string assemblyLocation = typeof(object).Assembly.Location;

		// 如果是在 .NET 5+ 中使用了“产生单文件(Single-File)”选项，Location 可能会返回空字符串
		if (string.IsNullOrEmpty(assemblyLocation)) {
			return "Single-File";
		}

		string appDirectory = AppContext.BaseDirectory;

		// 如果核心库的路径在我们自己软件的目录下，说明是独立部署打包进来的
		if (assemblyLocation.StartsWith(appDirectory, StringComparison.OrdinalIgnoreCase)) {
			return "Self-Contained";
		} else {
			// 否则说明底层库是从系统 C:\Program Files\dotnet 加载的
			return "Framework-Dependent";
		}
	}

}