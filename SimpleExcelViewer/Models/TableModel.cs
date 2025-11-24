using FastWpfGrid;

namespace SimpleExcelViewer.Models;

public class TableModel : FastGridModelBase {
	public override int ColumnCount { get; }
	public override int RowCount { get; }

	public TableModel() {

	}

}
