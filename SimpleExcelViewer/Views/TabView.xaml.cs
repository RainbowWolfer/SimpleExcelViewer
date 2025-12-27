using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using SimpleExcelViewer.Services;
using SimpleExcelViewer.ViewModels;
using SimpleExcelViewer.ViewModelServices;
using SimpleExcelViewer.Views.Dialogs;
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

internal class TabViewModel(IAppSettingsService appSettingsService) : ViewModelBase {

	private IMessageBoxServiceEx MessageBoxService => GetService<IMessageBoxServiceEx>();
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<TabView> UserControlService => GetService<ITypedUIObjectService>(nameof(UserControlService)).As<TabView>();

	public IDialogServiceEx ManageColumnsDialogService => GetService<IDialogServiceEx>(nameof(ManageColumnsDialogService));

	public new TabItemViewModel Parameter {
		get => GetProperty(() => Parameter);
		private set => SetProperty(() => Parameter, value);
	}

	public MainViewModel MainViewModel {
		get => GetProperty(() => MainViewModel);
		private set => SetProperty(() => MainViewModel, value);
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


	private DelegateCommand? manageColumnsCommand;
	public IDelegateCommand ManageColumnsCommand => manageColumnsCommand ??= new(ManageColumns, CanManageColumns);
	private void ManageColumns() {
		if (CanManageColumns()) {
			ManageColumnsDialogParameter parameter = new(Parameter.TableModel!);
			if (ManageColumnsDialogService.ShowOKCancel(this, parameter)) {
				parameter.TableModel.UpdateColumns(parameter.Result);
			}
		}
	}
	private bool CanManageColumns() => Parameter?.TableModel is not null;


	private DelegateCommand? autoColumnsWidthCommand;
	public IDelegateCommand AutoColumnsWidthCommand => autoColumnsWidthCommand ??= new(AutoColumnsWidth, CanAutoColumnsWidth);
	private void AutoColumnsWidth() {
		if (CanAutoColumnsWidth()) {
			Parameter?.TableModel?.AutoColumnWidth();
		}
	}
	private bool CanAutoColumnsWidth() => Parameter?.TableModel is not null;



	private DelegateCommand? interruptAndShowCommand;
	public IDelegateCommand InterruptAndShowCommand => interruptAndShowCommand ??= new(InterruptAndShow, CanInterruptAndShow);
	private void InterruptAndShow() {
		if (CanInterruptAndShow()) {
			Parameter.Interrupt();
		}
	}
	private bool CanInterruptAndShow() => true;



	private DelegateCommand? cancelLoadingCommand;
	public IDelegateCommand CancelLoadingCommand => cancelLoadingCommand ??= new(CancelLoading, CanCancelLoading);
	private void CancelLoading() {
		if (CanCancelLoading()) {
			if (appSettingsService.Model.ConfirmOnCancelLoading
				&& !MessageBoxService.ShowOkCancelQuestion($"Are you sure to cancel loading of ({Parameter.FileName}) ？")
			) {
				return;
			}
			MainViewModel.CloseItem(Parameter);
		}
	}
	private bool CanCancelLoading() => Parameter != null && Parameter.IsLoading;

}
