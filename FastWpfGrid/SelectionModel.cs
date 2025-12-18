namespace FastWpfGrid;

internal class SelectionModel {
	private SelectionRect rect = null;

	public SelectionRect Rect {
		get {
			if (CanSelect) {
				return rect;
			} else {
				return null;
			}
		}
		private set => rect = value;
	}

	public bool CanSelect { get; set; } = true;

	public SelectionModel() {

	}

	public void SelectCell(SimpleCellAddress cell) {
		Rect = new SelectionRect(cell);
	}

	public void SelectRect(SelectionRect rect) {
		Rect = rect;
	}

	public void SelectRow(int row, int columnCount) {
		Rect = new SelectionRect(
			rectFrom: new SimpleCellAddress(0, row),
			rectTo: new SimpleCellAddress(columnCount - 1, row)
		);
	}

	public void SelectColumn(int column, int rowCount) {
		Rect = new SelectionRect(
			rectFrom: new SimpleCellAddress(column, 0),
			rectTo: new SimpleCellAddress(column, rowCount - 1)
		);
	}

	public bool Contains(int col, int row) {
		if (!CanSelect) {
			return false;
		}
		if (Rect?.Contains(col, row) ?? false) {
			return true;
		}
		return false;
	}

	public void Clear() {
		Rect = null;
	}

	public bool HasSelection() => Rect != null;

}

internal enum LastSelectionType {
	None,
	RowHeader,
	ColumnHeader,
	Cell,
}
