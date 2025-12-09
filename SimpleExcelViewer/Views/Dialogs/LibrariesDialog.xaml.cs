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
		yield return new LibraryRecord("LiteDB", "v5.0.21", "https://www.nuget.org/packages/LiteDB/");
		yield return new LibraryRecord("Fody", "v6.9.3", "https://www.nuget.org/packages/Fody");
		yield return new LibraryRecord("Costura.Fody", "v6.0.0", "https://www.nuget.org/packages/Costura.Fody");
		yield return new LibraryRecord("FastWpfGrid", "Modified", "https://github.com/janproch/fastwpfgrid");
		yield return new LibraryRecord("Autofac", "v9.0.0", "https://www.nuget.org/packages/Autofac");
		yield return new LibraryRecord("AutoMapper", "v10.1.1", "https://www.nuget.org/packages/AutoMapper");
		yield return new LibraryRecord("DevExpressMvvm", "v24.1.6", "https://www.nuget.org/packages/DevExpressMvvm");
		yield return new LibraryRecord("Newtonsoft.Json", "v13.0.4", "https://www.nuget.org/packages/Newtonsoft.Json");
	}
}


public record class LibraryRecord(string Name, string Version, string Link);