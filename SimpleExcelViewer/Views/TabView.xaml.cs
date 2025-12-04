using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleExcelViewer.Views;

public partial class TabView : UserControl {
	public TabView() {
		InitializeComponent();

		FontFamily defaultFontFamily = SystemFonts.MessageFontFamily;
		string fontName = defaultFontFamily.Source;
		//double defaultFontSize = SystemFonts.MessageFontSize;

		MainFastGridControl.FontFamily = defaultFontFamily;
		MainFastGridControl.CellFontName = fontName;

	}
}

internal class TabViewModel : ViewModelBase {

	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<TabView> UserControlService => GetService<ITypedUIObjectService>(nameof(UserControlService)).As<TabView>();

	public new TabItemViewModel Parameter {
		get => GetProperty(() => Parameter);
		private set => SetProperty(() => Parameter, value);
	}

	public MainViewModel MainViewModel {
		get => GetProperty(() => MainViewModel);
		private set => SetProperty(() => MainViewModel, value);
	}

	public TabViewModel() {

	}

	protected override void OnParameterChanged(object parameter) {
		base.OnParameterChanged(parameter);
		Parameter = (TabItemViewModel)parameter;
		Initialize();
	}

	protected override void OnParentViewModelChanged(object parentViewModel) {
		base.OnParentViewModelChanged(parentViewModel);
		MainViewModel = (MainViewModel)parentViewModel;
		Initialize();
	}

	private async void Initialize() {
		if (Parameter is null || MainViewModel is null) {
			return;
		}

		await Parameter.LoadAsync(DispatcherService);

		UserControlService.Object.MainFastGridControl.Focus();

	}

}
