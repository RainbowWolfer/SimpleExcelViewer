using Mapster;
using SimpleExcelViewer.Services;

namespace SimpleExcelViewer.Mappers;

internal class Mapper : IRegister {
	public void Register(TypeAdapterConfig config) {
		config.NewConfig<AppSettingsModel, AppSettingsModel>();
	}
}
