using RW.Base.WPF.ViewModelServices;

namespace SimpleExcelViewer.ViewModelServices;

public class MessageBoxServiceEx : MessageBoxService {
	public MessageBoxServiceEx() {
		MessageTitle = AppConfig.AppName;
	}
}
