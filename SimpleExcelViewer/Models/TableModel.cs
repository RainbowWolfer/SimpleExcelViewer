using FastWpfGrid;
using RW.Common.Helpers;

namespace SimpleExcelViewer.Models;

public class TableModel : FastGridModelBase {
	public override int ColumnCount { get; }
	public override int RowCount { get; }

	public CsvData Data { get; }

	public override int RightAlignBlockCount => 1;

	public TableModel(CsvData data) {
		Data = data;

		ColumnCount = data.ColumnCount;
		RowCount = data.RowCount;

	}

	public override string GetColumnHeaderText(int column) {
		return Data.ColumnNames.ElementAtOrDefault(column);
	}

	public override string GetRowHeaderText(int row) {
		return base.GetRowHeaderText(row);
	}

	public override string GetCellText(int row, int column) {
		return Data.GetRow(row).ElementAtOrDefault(column).SafeToString();
	}
}
