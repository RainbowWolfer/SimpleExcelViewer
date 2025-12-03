using SimpleExcelViewer.ViewModels;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class AppSettingsDialog : UserControl {
	public AppSettingsDialog() {
		InitializeComponent();
	}
}

public record class AppSettingsDialogParameter();

internal class AppSettingsDialogViewModel() : DialogViewModelOkCancel<AppSettingsDialogParameter> {

	protected override void OnInitialized() {
		base.OnInitialized();

		
	}

	protected override bool Validate(out string message) {

		//message = "!!!";
		//return false;

		return base.Validate(out message);
	}
}