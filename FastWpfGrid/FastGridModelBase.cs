using RW.Common;
using RW.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FastWpfGrid;

public abstract class FastGridModelBase : BindModelBaseEx, IFastGridModel, IFastGridCell, IFastGridCellBlock {
	public event TypedEventHandler<FastGridModelBase, SelectionRect> AfterSelectionChanged;
	public event Action Loaded;

	private readonly List<IFastGridView> _grids = [];

	private int? _requestedRow;
	private int? _requestedColumn;

	private HashSet<int> _frozenRows = [];
	private HashSet<int> _hiddenRows = [];
	private HashSet<int> _frozenColumns = [];
	private HashSet<int> _hiddenColumns = [];


	public int FrozenRowsCount => _frozenRows.Count;
	public int FrozenColumnsCount => _frozenColumns.Count;

	private bool hasLoaded = false;

	private FastGridCellAddress currentSelectedCell = new();

	public virtual bool EnableDragReorderColumn { get; protected set; } = false;

	public int ColumnCount { get; protected set; }

	public int RowCount { get; private set; }

	public FastGridModelBase(int columnCount, int rowCount) {
		ColumnCount = columnCount;
		RowCount = rowCount;

		_hiddenRows = GetHiddenRows();
		_hiddenColumns = GetHiddenColumns();

		_frozenRows = GetFrozenRows();
		_frozenColumns = GetFrozenColumns();

	}

	public virtual void UpdateRowCount(int rowCount) {
		RowCount = rowCount;
		NotifyRefresh(new NotifyRefreshParameter(false));
	}

	public virtual void UpdateColumnCount(int columnCount) {
		ColumnCount = columnCount;
		NotifyRefresh(new NotifyRefreshParameter(false));
	}

	public virtual void UpdateColumnAndRowCount(int columnCount, int rowCount) {
		ColumnCount = columnCount;
		RowCount = rowCount;
		NotifyRefresh(new NotifyRefreshParameter(false));
	}

	public virtual string GetCellText(int row, int column) {
		return string.Format("Row={0}, Column={1}", row + 1, column + 1);
	}

	public virtual IFastGridCell GetCell(IFastGridView view, int row, int column) {
		_requestedRow = row;
		_requestedColumn = column;
		return this;
	}

	public virtual string GetRowHeaderText(int row) {
		return (row + 1).ToString();
	}

	public virtual IFastGridCell GetRowHeader(IFastGridView view, int row) {
		_requestedRow = row;
		_requestedColumn = null;
		return this;
	}

	public virtual IFastGridCell GetColumnHeader(IFastGridView view, int column) {
		_requestedColumn = column;
		_requestedRow = null;
		return this;
	}

	public virtual IFastGridCell GetGridHeader(IFastGridView view) {
		return new FastGridCellImpl();
	}

	public virtual string GetColumnHeaderText(int column) {
		return "Column " + (column + 1).ToString();
	}

	public virtual void AttachView(IFastGridView view) {
		_grids.Add(view);
	}

	public virtual void DetachView(IFastGridView view) {
		_grids.Remove(view);
	}

	public virtual void HandleCommand(IFastGridView view, FastGridCellAddress address, object commandParameter, ref bool handled) {
	}

	public virtual HashSet<int> GetHiddenColumns() {
		return _hiddenColumns;
	}

	public virtual HashSet<int> GetFrozenColumns() {
		return _frozenColumns;
	}

	public virtual HashSet<int> GetHiddenRows() {
		return _hiddenRows;
	}

	public virtual HashSet<int> GetFrozenRows() {
		return _frozenRows;
	}

	public void SetColumnArrange(HashSet<int> hidden, HashSet<int> frozen) {
		_hiddenColumns = hidden;
		_frozenColumns = frozen;
		NotifyColumnArrangeChanged();
	}

	public void SetRowArrange(HashSet<int> hidden, HashSet<int> frozen) {
		_hiddenRows = hidden;
		_frozenRows = frozen;
		NotifyRowArrangeChanged();
	}

	public virtual void SelectCell(int? row, int? column) {
		_grids.ForEach(x => x.SelectCell(row, column));
	}

	public virtual void SelectRect(SelectionRect rect) {
		_grids.ForEach(x => x.SelectRect(rect));
	}

	public virtual void BringIntoView(SimpleCellAddress cell) {
		_grids.ForEach(x => x.BringIntoView(cell));
	}

	public void SetColumnWidth(int width, params int[] columnIndexes) {
		_grids.ForEach(x => x.SetColumnWidth(width, columnIndexes));
	}

	public int GetColumnWidth(int columnIndex) {
		return _grids.FirstOrDefault()?.GetColumnWidth(columnIndex) ?? -1;
	}

	public void AutoColumnWidth() {
		_grids.ForEach(x => x.AutoColumnWidth());
	}

	public void InvalidateAll() {
		_grids.ForEach(x => x.InvalidateAll());
	}

	public void InvalidateCell(int row, int column) {
		_grids.ForEach(x => x.InvalidateModelCell(row, column));
	}

	public void InvalidateRowHeader(int row) {
		_grids.ForEach(x => x.InvalidateModelRowHeader(row));
	}

	public void InvalidateColumnHeader(int column) {
		_grids.ForEach(x => x.InvalidateModelColumnHeader(column));
	}

	public void NotifyAddedRows() {
		_grids.ForEach(x => x.NotifyAddedRows());
	}

	public void NotifyRefresh(NotifyRefreshParameter parameter = null) {
		_grids.ForEach(x => x.NotifyRefresh(parameter));
	}

	public void NotifyColumnArrangeChanged() {
		_grids.ForEach(x => x.NotifyColumnArrangeChanged());
	}

	public void NotifyRowArrangeChanged() {
		_grids.ForEach(x => x.NotifyRowArrangeChanged());
	}

	public virtual Color? BackgroundColor => null;

	public virtual int BlockCount => 1;

	public virtual int RightAlignBlockCount => 0;

	public virtual IFastGridCellBlock GetBlock(int blockIndex) {
		return this;
	}

	public virtual void HandleSelectionCommand(IFastGridView view, string command) {
	}

	public virtual string ToolTipText => null;

	public virtual TooltipVisibilityMode ToolTipVisibility => TooltipVisibilityMode.Always;

	public virtual FastGridBlockType BlockType => FastGridBlockType.Text;

	public virtual bool IsItalic => false;

	public virtual bool IsBold => false;

	public virtual Color? FontColor => null;

	public virtual string TextData {
		get {
			if (_requestedColumn == null && _requestedRow == null) {
				return null;
			}

			if (_requestedColumn != null && _requestedRow != null) {
				return GetCellText(_requestedRow.Value, _requestedColumn.Value);
			}

			if (_requestedColumn != null) {
				return GetColumnHeaderText(_requestedColumn.Value);
			}

			if (_requestedRow != null) {
				return GetRowHeaderText(_requestedRow.Value);
			}

			return null;
		}
	}

	public virtual string ImageSource => null;

	public virtual int ImageWidth => 16;

	public virtual int ImageHeight => 16;

	public virtual MouseHoverBehaviours MouseHoverBehaviour => MouseHoverBehaviours.ShowAllWhenMouseOut;

	public virtual object CommandParameter => null;

	public virtual string ToolTip => null;

	public virtual CellDecoration Decoration => CellDecoration.None;

	public virtual Color? DecorationColor => null;

	public virtual int? SelectedRowCountLimit => null;

	public virtual int? SelectedColumnCountLimit => null;

	public virtual RenderTextAlignment Alignment => RenderTextAlignment.Right;

	public FastGridCellAddress CurrentSelectedCell {
		get => currentSelectedCell;
		set {
			FastGridCellAddress previous = currentSelectedCell;
			currentSelectedCell = value;
			if (previous != value) {
				OnSelectedCellChanged(value);
			}
		}
	}

	public virtual string GetEditText(int row, int column) {
		return GetCellText(row, column);
	}

	public virtual void UpdateColumnHeaderHeight(int height) {
		_grids.ForEach(x => x.SetColumnHeaderHeight(height));
	}

	public virtual void UpdateRowHeaderWidth(int width) {
		_grids.ForEach(x => x.SetRowHeaderWidth(width));
	}

	public virtual int? GetDesiredColumnHeaderHeight() {
		return null;
	}

	public virtual IContextMenuCreator GetContextMenu(IFastGridView grid, SelectionRect selectionRect, FastGridCellAddress cell) {
		return null;
	}

	public virtual int GetRealRowIndex(int modelRow) {
		return modelRow - _frozenRows.Count(x => x < modelRow);
	}

	public virtual int GetModelRowIndex(int realRow) {
		return realRow + _frozenRows.Count(x => x < realRow);
	}

	public SelectionRect GetSelectionRect() {
		return _grids.FirstOrDefault()?.GetSelectionRect();
	}

	//public int GetModelRowIndex(int realRow) {
	//	return realRow + _frozenRows.Count(x => x < GetRealRowIndex(realRow));
	//}

	public virtual void OnSelectedCellChanged(FastGridCellAddress current) {

	}

	public virtual void HandleDoubleClickCell(IFastGridView view, FastGridCellAddress cell) {

	}

	public void OnInvalidatingFinished() {
		if (hasLoaded) {
			return;
		}
		hasLoaded = true;
		OnLoaded();
	}

	public virtual void OnLoaded() {
		Loaded?.Invoke();
	}

	public virtual void OnColumnReorder(int startIndex, int targetIndex) {

	}

	public virtual void SelectionChanged(SelectionRect selectionRect) {
		AfterSelectionChanged?.Invoke(this, selectionRect);
	}

	public virtual EditingControlConfig RequestCellEditor(IFastGridView view, int row, int column, string inputText) {
		return null;
	}

	public virtual bool HandleKeyDown(IFastGridView view, KeyEventArgs args) {
		return false;
	}

	public virtual void OnCellClicked(IFastGridView view, FastGridCellAddress cell) {

	}

	public int OffsetX { get; set; } = 0;

}
