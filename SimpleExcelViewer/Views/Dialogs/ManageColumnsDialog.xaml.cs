using DevExpress.Mvvm;
using SimpleExcelViewer.Interfaces;
using SimpleExcelViewer.Models;
using SimpleExcelViewer.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;

namespace SimpleExcelViewer.Views.Dialogs;

public partial class ManageColumnsDialog : UserControl {
	public ManageColumnsDialog() {
		InitializeComponent();
	}
}

public record class ManageColumnsDialogParameter(TableModel TableModel) {
	public IReadOnlyList<int>? Result { get; set; }
}

internal class ManageColumnsDialogViewModel() : DialogViewModelOkCancel<ManageColumnsDialogParameter> {

	public ObservableCollection<ColumnCheckItem> Columns { get; } = [];

	protected override void OnInitialized() {
		base.OnInitialized();

		DialogTitle = "Manage Columns";

		Columns.CollectionChanged += Columns_CollectionChanged;

		ITableData table = Parameter.TableModel.Data;
		IReadOnlyList<int> currentMap = Parameter.TableModel.CurrentColumnMap; // 获取当前列映射状态

		// 1. 先添加当前已显示的列（保持用户调整过的顺序）
		foreach (int index in currentMap) {
			string name = table.GetColumnName(index);
			Columns.Add(new ColumnCheckItem(index, name) { IsChecked = true });
		}

		// 2. 再添加当前隐藏的列（放在末尾）
		HashSet<int> visibleSet = [.. currentMap];
		for (int i = 0; i < table.ColumnCount; i++) {
			if (!visibleSet.Contains(i)) {
				string name = table.GetColumnName(i);
				Columns.Add(new ColumnCheckItem(i, name) { IsChecked = false });
			}
		}
	}

	private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
		RaisePropertyChanged(() => Columns);
	}

	protected override bool OnConfirmed() {
		Parameter.Result = [.. Columns.Where(x => x.IsChecked).Select(x => x.Index)];
		return base.OnConfirmed();
	}



	private DelegateCommand<ColumnCheckItem>? rowButtonCommand;
	public IDelegateCommand RowButtonCommand => rowButtonCommand ??= new(RowButton);
	private void RowButton(ColumnCheckItem item) {
		item.IsChecked = !item.IsChecked;
	}



	private DelegateCommand? resetTogglesCommand;
	public IDelegateCommand ResetTogglesCommand => resetTogglesCommand ??= new(ResetToggles, CanResetToggles);
	private void ResetToggles() {
		if (CanResetToggles()) {
			foreach (ColumnCheckItem column in Columns) {
				column.IsChecked = true;
			}
		}
	}
	private bool CanResetToggles() => true;

	private DelegateCommand? toggleAllCommand;
	public IDelegateCommand ToggleAllCommand => toggleAllCommand ??= new(ToggleAll, CanToggleAll);
	private void ToggleAll() {
		if (CanToggleAll()) {
			foreach (ColumnCheckItem column in Columns) {
				column.IsChecked = !column.IsChecked;
			}
		}
	}
	private bool CanToggleAll() => true;

	private DelegateCommand? resetOrderCommand;
	public IDelegateCommand ResetOrderCommand => resetOrderCommand ??= new(ResetOrder, CanResetOrder);
	private void ResetOrder() {
		if (CanResetOrder()) {
			List<ColumnCheckItem> sortedList = [.. Columns.OrderBy(x => x.Index)];

			Columns.Clear();

			foreach (ColumnCheckItem item in sortedList) {
				Columns.Add(item);
			}
		}
	}
	private bool CanResetOrder() => true;



}

public class ColumnCheckItem(int index, string name) : BindableBase {
	public int Index { get; } = index;
	public string Name { get; } = name;

	public bool IsChecked {
		get => GetProperty(() => IsChecked);
		set => SetProperty(() => IsChecked, value);
	}

}