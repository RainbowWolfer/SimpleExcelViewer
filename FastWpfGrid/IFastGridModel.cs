using System.Collections.Generic;
using System.Windows.Input;

namespace FastWpfGrid;

public interface IFastGridModel {

	IContextMenuCreator GetContextMenu(IFastGridView grid, SelectionRect selectionRect, FastGridCellAddress cell);

	int ColumnCount { get; }
	int RowCount { get; }

	bool EnableDragReorderColumn { get; }

	FastGridCellAddress CurrentSelectedCell { get; set; }

	IFastGridCell GetCell(IFastGridView grid, int row, int column);
	IFastGridCell GetRowHeader(IFastGridView view, int row);
	IFastGridCell GetColumnHeader(IFastGridView view, int column);
	IFastGridCell GetGridHeader(IFastGridView view);

	void AttachView(IFastGridView view);
	void DetachView(IFastGridView view);
	void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled);

	HashSet<int> GetHiddenColumns();
	HashSet<int> GetFrozenColumns();
	HashSet<int> GetHiddenRows();
	HashSet<int> GetFrozenRows();

	void HandleSelectionCommand(IFastGridView view, string command);

	void UpdateColumnHeaderHeight(int height);
	void UpdateRowHeaderWidth(int width);

	int? GetDesiredColumnHeaderHeight();

	int GetRealRowIndex(int row);

	void HandleDoubleClickCell(IFastGridView view, FastGridCellAddress cell);

	void OnInvalidatingFinished();

	void OnColumnReorder(int startIndex, int targetIndex);

	void SelectionChanged(SelectionRect selectionRect);
	void OnCellClicked(IFastGridView view, FastGridCellAddress cell);

	EditingControlConfig RequestCellEditor(IFastGridView view, int row, int column, string inputText);

	bool HandleKeyDown(IFastGridView view, KeyEventArgs args);

	//void HandleCellEditorValue(Control control, int row, int column);

}
