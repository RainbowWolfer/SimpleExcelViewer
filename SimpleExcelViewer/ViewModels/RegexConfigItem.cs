using DevExpress.Mvvm;
using RW.Common.Helpers;

namespace SimpleExcelViewer.ViewModels;

public class RegexConfigItem : BindableBase {

	public ColorEditItem BackColor { get; } = new() {
		R = RandomHelper.GetRandomBytes(1)[0],
		G = RandomHelper.GetRandomBytes(1)[0],
		B = RandomHelper.GetRandomBytes(1)[0],
	};

	public ColorEditItem TextColor { get; } = new() {
		R = RandomHelper.GetRandomBytes(1)[0],
		G = RandomHelper.GetRandomBytes(1)[0],
		B = RandomHelper.GetRandomBytes(1)[0],
	};


	public bool EnableBackColor {
		get => GetProperty(() => EnableBackColor);
		set => SetProperty(() => EnableBackColor, value);
	}

	public bool EnableTextColor {
		get => GetProperty(() => EnableTextColor);
		set => SetProperty(() => EnableTextColor, value);
	}

	public string Regex {
		get => GetProperty(() => Regex);
		set => SetProperty(() => Regex, value);
	}



}
