namespace SimpleExcelViewer.Interfaces;

public interface ITableData : IDisposable {
	int RowCount { get; }
	int ColumnCount { get; }

	string GetColumnName(int index);

	object[] GetRow(int index);
	object GetCell(int row, int column);
}