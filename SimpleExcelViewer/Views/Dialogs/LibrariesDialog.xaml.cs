using SimpleExcelViewer.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class LibrariesDialog : UserControl {
	public LibrariesDialog() {
		InitializeComponent();
	}
}

internal class LibrariesDialogViewModel : DialogViewModelOk<object> {

	public ObservableCollection<LibraryRecord> LibraryRecords { get; } = [];

	protected override void OnInitialized() {
		base.OnInitialized();
		DialogTitle = "Open Source Libraries";

		foreach (LibraryRecord item in GetLibraryRecords()) {
			LibraryRecords.Add(item);
		}
	}


	private IEnumerable<LibraryRecord> GetLibraryRecords() {
		yield return new LibraryRecord("PolySharp", "v1.15.0", "https://www.nuget.org/packages/PolySharp");
		yield return new LibraryRecord("gong-wpf-dragdrop", "v4.0.0", "https://www.nuget.org/packages/gong-wpf-dragdrop");
		yield return new LibraryRecord("FastWpfGrid", "Modified", "https://github.com/janproch/fastwpfgrid");
		yield return new LibraryRecord("Autofac", "v9.1.0", "https://www.nuget.org/packages/Autofac");
		yield return new LibraryRecord("MapsterMapper", "v10.0.7", "https://www.nuget.org/packages/Mapster/");
		yield return new LibraryRecord("DevExpressMvvm", "v24.1.6", "https://www.nuget.org/packages/DevExpressMvvm");
		yield return new LibraryRecord("Newtonsoft.Json", "v13.0.7", "https://www.nuget.org/packages/Newtonsoft.Json");
	}
}


public record class LibraryRecord(string Name, string Version, string Link);