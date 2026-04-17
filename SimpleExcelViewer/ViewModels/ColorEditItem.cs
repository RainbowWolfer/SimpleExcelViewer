using DevExpress.Mvvm;
using System.Windows.Media;

namespace SimpleExcelViewer.ViewModels;

public class ColorEditItem : BindableBase {

	public byte R {
		get => GetProperty(() => R);
		set {
			SetProperty(() => R, value);
			RaisePropertyChanged(() => Color);
		}
	}

	public byte G {
		get => GetProperty(() => G);
		set {
			SetProperty(() => G, value);
			RaisePropertyChanged(() => Color);
		}
	}

	public byte B {
		get => GetProperty(() => B);
		set {
			SetProperty(() => B, value);
			RaisePropertyChanged(() => Color);
		}
	}


	public Color Color {
		get => new() { R = R, G = G, B = B, A = 255, };
		set {
			R = value.R;
			G = value.G;
			B = value.B;
			RaisePropertyChanged(() => Color);
		}
	}
}
