using RW.Base.WPF.ViewModelServices;

namespace SimpleExcelViewer.ViewModelServices;

public class MessageBoxServiceEx : MessageBoxService, IMessageBoxServiceEx {
	public MessageBoxServiceEx() {
		MessageTitle = AppConfig.AppName;
	}
}
