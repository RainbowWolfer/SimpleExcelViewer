using DevExpress.Mvvm;
using RW.Base.WPF.DependencyInjections;
using RW.Common;

namespace SimpleExcelViewer.Services;

public interface IAppStatusService {

	event TypedEventHandler<IAppStatusService, EventArgs>? OnFullScreenChanged;

	bool FullScreen { get; set; }

}

internal class AppStatusService : BindableBase, IAppStatusService, ISingletonDependency {
	public event TypedEventHandler<IAppStatusService, EventArgs>? OnFullScreenChanged;

	public bool FullScreen {
		get => GetProperty(() => FullScreen);
		set {
			SetProperty(() => FullScreen, value);
			OnFullScreenChanged?.Invoke(this, EventArgs.Empty);
		}
	}



	public AppStatusService() {
		FullScreen = false;
	}

}
