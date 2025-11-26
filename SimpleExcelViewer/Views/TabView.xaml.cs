using DevExpress.Mvvm;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.ViewModelServices;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.ViewModels;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views;

public partial class TabView : UserControl {
	public TabView() {
		InitializeComponent();
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

		try {
			await Parameter.LoadAsync();

			UserControlService.Object.MainFastGridControl.Focus();

			DispatcherService.Invoke(AppHelper.ReleaseRAM);
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
		}
	}

}
