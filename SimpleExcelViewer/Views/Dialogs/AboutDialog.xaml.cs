using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using SimpleExcelViewer.ViewModels;
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

internal class AboutDialogViewModel(IAppManager appManager) : DialogViewModelOk<AboutDialogParameter> {
	public IAppManager AppManager { get; } = appManager;

	protected override void OnInitialized() {
		base.OnInitialized();
		DialogTitle = "About";
	}

}