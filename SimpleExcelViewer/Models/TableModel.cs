using FastWpfGrid;
using RW.Common.Helpers;
using SimpleExcelViewer.Interfaces;

namespace SimpleExcelViewer.Models;

public class TableModel : FastGridModelBase, IDisposable {
	public ITableData Data { get; }

	//public override int RightAlignBlockCount => 1;
	private List<int> _visibleColumnMap = [];
	public IReadOnlyList<int> CurrentColumnMap => _visibleColumnMap;

	public TableModel(ITableData data) : base(data.ColumnCount, data.RowCount) {
		Data = data;

		ResetColumns();
	}


	public void ResetColumns() {
		_visibleColumnMap = [.. Enumerable.Range(0, Data.ColumnCount)];
		InvalidateAll();
	}

	public void UpdateColumns(IEnumerable<int>? orderedVisibleIndices) {
		if (orderedVisibleIndices is null) {
			return;
		}

		_visibleColumnMap = [.. orderedVisibleIndices];

		UpdateColumnCount(_visibleColumnMap.Count);

		AutoColumnWidth();
		InvalidateAll();
	}

	public SelectionRect? SelectionRect {
		get => GetProperty(() => SelectionRect);
		set => SetProperty(() => SelectionRect, value);
	}

	public override void SelectionChanged(SelectionRect selectionRect) {
		base.SelectionChanged(selectionRect);
		SelectionRect = selectionRect;
	}

	//public override IFastGridCell GetCell(IFastGridView view, int row, int column) {
	//	IFastGridCell cell = base.GetCell(view, row, column);
	//	return cell;
	//	int realColumnIndex = _visibleColumnMap[column];
	//	return base.GetCell(view, row, realColumnIndex);
	//}

	public override string GetColumnHeaderText(int column) {
		//return Data.GetColumnName(column);
		int realColumnIndex = _visibleColumnMap[column];
		return Data.GetColumnName(realColumnIndex);
	}

	public override string GetRowHeaderText(int row) {
		return base.GetRowHeaderText(row);
	}

	public override string GetCellText(int row, int column) {
		//return Data.GetCell(row, column).SafeToString();
		int realColumnIndex = _visibleColumnMap[column];
		return Data.GetCell(row, realColumnIndex).SafeToString();
	}

	public void Dispose() {
		Data.Dispose();
	}
}
