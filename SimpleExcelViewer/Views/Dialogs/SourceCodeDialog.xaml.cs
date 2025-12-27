using SimpleExcelViewer.ViewModels;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;


public partial class SourceCodeDialog : UserControl {
	public SourceCodeDialog() {
		InitializeComponent();
	}
}

internal class SourceCodeDialogViewModel() : DialogViewModelOk<object> {
	protected override void OnInitialized() {
		base.OnInitialized();

		DialogTitle = "Source Code";

	}
}