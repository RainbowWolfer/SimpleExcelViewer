using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using SimpleExcelViewer.Services;
using SimpleExcelViewer.ViewModels;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class AppSettingsDialog : UserControl {
	public AppSettingsDialog() {
		InitializeComponent();
	}
}

public record class AppSettingsDialogParameter();

internal class AppSettingsDialogViewModel(SystemService systemService) : DialogViewModelOkCancel<AppSettingsDialogParameter> {

	protected override void OnInitialized() {
		base.OnInitialized();


	}

	protected override bool Validate(out string message) {

		//message = "!!!";
		//return false;

		return base.Validate(out message);
	}


	private DelegateCommand? registerContextMenuCommand;
	public IDelegateCommand RegisterContextMenuCommand => registerContextMenuCommand ??= new(RegisterContextMenu);
	private void RegisterContextMenu() {
		try {
			systemService.RegisterCsvContextMenu();
			MessageBoxService.ShowInformation("Context menu registered successfully.");
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Failed to register context menu", ex);
		}
	}



	private DelegateCommand? unregisterContextMenuCommand;
	public IDelegateCommand UnregisterContextMenuCommand => unregisterContextMenuCommand ??= new(UnregisterContextMenu);
	private void UnregisterContextMenu() {
		try {
			systemService.UnregisterCsvContextMenu();
			MessageBoxService.ShowInformation("Context menu unregistered successfully.");
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Failed to unregister context menu", ex);
		}
	}

}