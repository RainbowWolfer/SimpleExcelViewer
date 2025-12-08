using AutoMapper;
using SimpleExcelViewer.Services;

namespace SimpleExcelViewer.Mappers;

internal class Mapper : Profile {
	public Mapper() {
		CreateMap<AppSettingsModel, AppSettingsModel>();
	}
}
