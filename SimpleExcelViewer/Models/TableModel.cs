using FastWpfGrid;
using RW.Common.Helpers;
using SimpleExcelViewer.Enums;
using SimpleExcelViewer.Interfaces;

namespace SimpleExcelViewer.Models;

public class TableModel : FastGridModelBase, IDisposable {
	public ITableData Data { get; }

	//public override int RightAlignBlockCount => 1;
	private List<int> _visibleColumnMap = [];
	public IReadOnlyList<int> CurrentColumnMap => _visibleColumnMap;

	private bool isTransposed = false;
	private ColumnHeaderType columnHeaderType;

	public bool IsTransposed {
		get => isTransposed;
		set {
			if (isTransposed != value) {
				isTransposed = value;
				// 状态改变时必须刷新行列总数和视图
				RefreshDimensions();
				InvalidateAll();
			}
		}
	}

	public ColumnHeaderType ColumnHeaderType {
		get => columnHeaderType;
		set {
			if (columnHeaderType != value) {
				columnHeaderType = value;
				InvalidateAll();
			}
		}
	}

	public TableModel(ITableData data) : base(data.ColumnCount, data.RowCount) {
		Data = data;

		ResetColumns();
	}

	private void RefreshDimensions() {
		int visibleDataColumnCount = _visibleColumnMap.Count;
		int dataRowCount = Data.RowCount;

		if (IsTransposed) {
			// 转置后：Grid行数 = 数据可见列数，Grid列数 = 数据行数
			UpdateRowCount(visibleDataColumnCount);
			UpdateColumnCount(dataRowCount);
		} else {
			// 正常：Grid行数 = 数据行数，Grid列数 = 数据可见列数
			UpdateRowCount(dataRowCount);
			UpdateColumnCount(visibleDataColumnCount);
		}
	}

	public void ResetColumns() {
		_visibleColumnMap = [.. Enumerable.Range(0, Data.ColumnCount)];
		RefreshDimensions();
		InvalidateAll();
	}

	public void UpdateColumns(IEnumerable<int>? orderedVisibleIndices) {
		if (orderedVisibleIndices is null) {
			return;
		}

		_visibleColumnMap = [.. orderedVisibleIndices];

		RefreshDimensions();
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
		if (IsTransposed) {
			// 转置后，列标题显示的是原始数据的“行号”
			return $"Row {column + 1}";
		} else {
			// 正常显示列名
			int realDataColumnIndex = _visibleColumnMap[column];
			return GetColumnName(realDataColumnIndex);
		}
	}

	public override string GetRowHeaderText(int row) {
		if (IsTransposed) {
			// 转置后，行标题显示的是原始数据的“列名”
			int realDataColumnIndex = _visibleColumnMap[row];
			return GetColumnName(realDataColumnIndex);
		} else {
			// 正常显示行号
			return (row + 1).ToString();
		}
	}

	private string GetColumnName(int index) {
		string name = Data.GetColumnName(index);
		string result = ColumnHeaderType switch {
			ColumnHeaderType.Name => $"{name}",
			ColumnHeaderType.Name_Index => $"{name} ({index})",
			ColumnHeaderType.Name_Number => $"{name} ({index + 1})",
			ColumnHeaderType.Name_Alphabet => $"{name} ({(index + 1).IndexToColumn()})",
			_ => $"{name}",
		};
		return result;
	}

	public override string GetCellText(int row, int column) {
		if (IsTransposed) {
			// Grid中的 row 其实是数据的列，column 其实是数据的行
			int realDataColumnIndex = _visibleColumnMap[row];
			int realDataRowIndex = column;
			return Data.GetCell(realDataRowIndex, realDataColumnIndex).SafeToString();
		} else {
			// 正常映射
			int realDataColumnIndex = _visibleColumnMap[column];
			int realDataRowIndex = row;
			return Data.GetCell(realDataRowIndex, realDataColumnIndex).SafeToString();
		}
	}

	public void Dispose() {
		Data.Dispose();
	}
}
