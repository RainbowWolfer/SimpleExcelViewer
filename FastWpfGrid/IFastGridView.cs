using System.Collections.Generic;
using System.Windows.Input;

namespace FastWpfGrid;

public interface IFastGridView {
	/// <summary>
	/// invalidates whole grid
	/// </summary>
	void InvalidateAll();

	/// <summary>
	/// invalidates given cell
	/// </summary>
	/// <param name="row"></param>
	/// <param name="column"></param>
	void InvalidateModelCell(int row, int column);

	/// <summary>
	/// invalidates given row header
	/// </summary>
	/// <param name="row"></param>
	void InvalidateModelRowHeader(int row);

	/// <summary>
	/// invalidates given row (all cells including header)
	/// </summary>
	/// <param name="row"></param>
	void InvalidateModelRow(int row);

	/// <summary>
	/// invalidates given column header
	/// </summary>
	/// <param name="column"></param>
	void InvalidateModelColumnHeader(int column);

	/// <summary>
	/// invalidates given column (all cells including header)
	/// </summary>
	/// <param name="column"></param>
	void InvalidateModelColumn(int column);

	/// <summary>
	/// invalidates grid header (top-left header cell)
	/// </summary>
	void InvalidateGridHeader();

	/// <summary>
	/// forces grid to refresh all data
	/// </summary>
	void NotifyRefresh(NotifyRefreshParameter parameter = null);

	/// <summary>
	/// notifies grid about new rows added to the end
	/// </summary>
	void NotifyAddedRows();

	/// <summary>
	/// notifies grid, that result of GetHiddenColumns() or GetFrozenColumns() is changed
	/// </summary>
	void NotifyColumnArrangeChanged();

	/// <summary>
	/// notifies grid, that result of GetHiddenRows() or GetFrozenRows() is changed
	/// </summary>
	void NotifyRowArrangeChanged();

	/// <summary>
	/// gets whether flexible rows (real rows) are currently used
	/// </summary>
	bool FlexibleRows { get; }

	/// <summary>
	/// gets or sets whereher flexible rows are allows
	/// </summary>
	bool AllowFlexibleRows { get; set; }

	SelectionRect GetSelectionRect();

	///// <summary>
	///// gets summary of active rows
	///// </summary>
	///// <returns></returns>
	//ActiveSeries GetActiveRows();

	///// <summary>
	///// gets summary of active columns
	///// </summary>
	///// <returns></returns>
	//ActiveSeries GetActiveColumns();

	/// <summary>
	/// shows quick access menu to selection
	/// </summary>
	/// <param name="commands"></param>
	void ShowSelectionMenu(IEnumerable<string> commands);

	/// <summary>
	/// handles command
	/// </summary>
	/// <param name="address"></param>
	/// <param name="command"></param>
	void HandleCommand(FastGridCellAddress address, object command);

	/// <summary>
	/// selects all cells in grid
	/// </summary>
	void SelectAll();

	void SetColumnHeaderHeight(int height);

	void SetRowHeaderWidth(int width);

	void SelectCell(int? row, int? column);
	void SelectRect(SelectionRect rect);
	void BringIntoView(SimpleCellAddress cell);

	void SetColumnWidth(int width, params int[] columnIndexes);
	void AutoColumnWidth();
	int GetColumnWidth(int columnIndex);

	void CopyCurrentCells();

	bool MoveCurrentCell(int? row, int? col, KeyEventArgs e = null);
	bool HandleCursorMove(KeyEventArgs e, bool isInTextBox = false);

	bool ShowInlineEditor(FastGridCellAddress cell, string textValueOverride = null);
	void HideInlineEditor(bool saveCellValue = true);
	void AdjustInlineEditorPosition();
}
