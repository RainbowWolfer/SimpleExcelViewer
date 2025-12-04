using SimpleExcelViewer.ViewModels;
using System.IO;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class FileInfoDialog : UserControl {
	public FileInfoDialog() {
		InitializeComponent();
	}
}

public record class FileInfoDialogParameter(FileInfo FileInfo);

internal class FileInfoDialogViewModel : DialogViewModelOk<FileInfoDialogParameter> {


	public FileInfo FileInfo {
		get => GetProperty(() => FileInfo);
		set => SetProperty(() => FileInfo, value);
	}

	protected override void OnInitialized() {
		base.OnInitialized();

		DialogTitle = "File Info";

		FileInfo = Parameter.FileInfo;
	}


}