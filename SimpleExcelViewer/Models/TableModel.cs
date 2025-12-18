using FastWpfGrid;
using RW.Common.Helpers;
using SimpleExcelViewer.Interfaces;

namespace SimpleExcelViewer.Models;

public class TableModel(ITableData data) : FastGridModelBase(data.ColumnCount, data.RowCount), IDisposable {
	public ITableData Data { get; } = data;

	//public override int RightAlignBlockCount => 1;

	public override IFastGridCell GetCell(IFastGridView view, int row, int column) {
		IFastGridCell cell = base.GetCell(view, row, column);
		return cell;
	}

	public override string GetColumnHeaderText(int column) {
		return Data.GetColumnName(column);
	}

	public override string GetRowHeaderText(int row) {
		return base.GetRowHeaderText(row);
	}

	public override string GetCellText(int row, int column) {
		return Data.GetCell(row, column).SafeToString();
	}

	public void Dispose() {
		Data.Dispose();
	}
}
