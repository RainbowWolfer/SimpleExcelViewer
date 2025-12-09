using RW.Common.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace SimpleExcelViewer.Controls;

[TemplatePart(Name = PART_RootBorder, Type = typeof(Border))]
internal class HyperLink : Control {
	private const string PART_RootBorder = nameof(PART_RootBorder);


	public string DisplayText {
		get => (string)GetValue(DisplayTextProperty);
		set => SetValue(DisplayTextProperty, value);
	}

	public string Link {
		get => (string)GetValue(LinkProperty);
		set => SetValue(LinkProperty, value);
	}

	public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
		nameof(DisplayText),
		typeof(string),
		typeof(HyperLink),
		new PropertyMetadata(string.Empty, OnDisplayTextChanged)
	);

	public static readonly DependencyProperty LinkProperty = DependencyProperty.Register(
		nameof(Link),
		typeof(string),
		typeof(HyperLink),
		new PropertyMetadata(string.Empty, OnLinkChanged)
	);

	private static void OnDisplayTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is HyperLink control) {
			control.link.Inlines.Clear();
			control.link.Inlines.Add(e.NewValue?.ToString() ?? string.Empty);
		}
	}

	private static void OnLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is HyperLink control) {
			string uri = e.NewValue?.ToString() ?? string.Empty;
			if (uri.IsNotBlank()) {
				control.link.NavigateUri = new Uri(uri);
			} else {
				control.link.NavigateUri = null;
			}
		}
	}

	static HyperLink() {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(HyperLink), new FrameworkPropertyMetadata(typeof(HyperLink)));
	}

	private readonly TextBlock textBlock = new();
	private readonly Hyperlink link = new();

	public HyperLink() {
		link.RequestNavigate += Link_RequestNavigate;
		textBlock.Inlines.Add(link);
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild(PART_RootBorder) is Border border) {
			border.Child = textBlock;
		}

		link.Inlines.Clear();
		link.Inlines.Add(DisplayText);
		if (Link.IsNotBlank()) {
			link.NavigateUri = new Uri(Link);
		}
	}


	private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e) {
		try {
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
		} catch (Exception ex) {
			Debug.WriteLine(ex);
		}
		e.Handled = true;
	}
}
