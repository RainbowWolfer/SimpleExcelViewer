using DevExpress.Mvvm;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using SimpleExcelViewer.Services;
using SimpleExcelViewer.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class ConfigHighlightDialog : UserControl {
	public ConfigHighlightDialog() {
		InitializeComponent();
	}
}


internal class ConfigHighlightDialogViewModel(IRegexConfigService regexConfigService) : DialogViewModelOkCancel<object> {

	public ObservableCollection<RegexConfigItem> RegexConfigItems { get; } = [];


	protected override void OnInitialized() {
		base.OnInitialized();
		DialogTitle = "Config Highlight";

		IEnumerable<RegexConfigItem> items = regexConfigService.GetRegexConfigItems();
		foreach (RegexConfigItem item in items) {
			RegexConfigItems.Add(item);
		}

		RegexConfigItems.CollectionChanged += RegexConfigItems_CollectionChanged;
	}

	private void RegexConfigItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		RaisePropertyChanged(() => RegexConfigItems);
	}

	protected override bool Validate(out string message) {
		for (int i = 0; i < RegexConfigItems.Count; i++) {
			RegexConfigItem item = RegexConfigItems[i];
			if (!IsValidRegexPattern(item.Regex)) {
				message = $"Regex {i + 1} has error.";
				return false;
			}
		}
		return base.Validate(out message);
	}

	protected override bool OnConfirmed() {
		regexConfigService.SaveRegexConfigItems(RegexConfigItems);
		return base.OnConfirmed();
	}

	public static bool IsValidRegexPattern(string pattern) {
		if (pattern.IsBlank()) {
			return true;
		}

		try {
			// 尝试实例化 Regex 对象。如果 pattern 语法有误，底层会直接抛出 ArgumentException
			Regex tempRegex = new(pattern);
			return true;
		} catch (Exception) {
			// 捕获到 ArgumentException 说明正则语法错误（比如括号不匹配、转义错误等）
			return false;
		}
	}


	private DelegateCommand? addCommand;
	public IDelegateCommand AddCommand => addCommand ??= new(Add);
	private void Add() {
		RegexConfigItem item = new() {
			EnableBackColor = true,
			EnableTextColor = true,
		};

		item.TextColor.Color = ColorHelper.GetContrastColor(item.BackColor.Color);

		RegexConfigItems.Add(item);
	}



	private DelegateCommand<RegexConfigItem>? removeCommand;
	public IDelegateCommand RemoveCommand => removeCommand ??= new(Remove, CanRemove);
	private void Remove(RegexConfigItem item) {
		if (CanRemove(item)) {
			RegexConfigItems.Remove(item);
		}
	}
	private bool CanRemove(RegexConfigItem item) => item != null;


}