using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace FastWpfGrid;

public partial class FastGridControl {
	public int FirstVisibleColumnScrollIndex;
	public int FirstVisibleRowScrollIndex;
	private int _modelRowCount;
	private int _modelColumnCount;

	private int _realRowCount;
	private int _realColumnCount;

	private readonly SeriesSizes _rowSizes = new();
	private readonly SeriesSizes _columnSizes = new();

	public int VisibleRowCount => _rowSizes.GetVisibleScrollCount(FirstVisibleRowScrollIndex, GridScrollAreaHeight);

	public int VisibleColumnCount => _columnSizes.GetVisibleScrollCount(FirstVisibleColumnScrollIndex, GridScrollAreaWidth);

	//public bool IsWide => _realColumnCount > WideColumnsLimit;

	public bool FlexibleRows => /*!IsWide &&*/ AllowFlexibleRows;

	private int GetRowTop(int row) {
		if (row < _rowSizes.FrozenCount) {
			return _rowSizes.GetFrozenPosition(row) + HeaderHeight;
		}

		return _rowSizes.GetSizeSum(FirstVisibleRowScrollIndex, row - _rowSizes.FrozenCount) + HeaderHeight + FrozenHeight;
		//return (row - FirstVisibleRow) * RowHeight + HeaderHeight;
	}

	private int GetColumnLeft(int column) {
		if (column < _columnSizes.FrozenCount) {
			return _columnSizes.GetFrozenPosition(column) + HeaderWidth;
		}

		return _columnSizes.GetSizeSum(FirstVisibleColumnScrollIndex, column - _columnSizes.FrozenCount) + HeaderWidth + FrozenWidth;
		//return (column - FirstVisibleColumn) * ColumnWidth + HeaderWidth;
	}

	private IntRect GetCellRect(int row, int column) {
		return new IntRect(
			new IntPoint(GetColumnLeft(column), GetRowTop(row)),
			new IntSize(_columnSizes.GetSizeByRealIndex(column) + 1, _rowSizes.GetSizeByRealIndex(row) + 1)
		);
	}

	private IntRect GetContentRect(IntRect rect) {
		return rect.GrowSymmetrical(-CellPaddingHorizontal, -CellPaddingVertical);
	}

	private IntRect GetRowHeaderRect(int row) {
		return new IntRect(new IntPoint(0, GetRowTop(row)), new IntSize(HeaderWidth + 1, _rowSizes.GetSizeByRealIndex(row) + 1));
	}

	internal IntRect GetColumnHeaderRect(int column) {
		return new IntRect(
			topLeft: new IntPoint(GetColumnLeft(column), 0),
			size: new IntSize(
				width: _columnSizes.GetSizeByRealIndex(column) + 1,
				height: HeaderHeight + 1
			)
		);
	}

	private IntRect GetColumnHeadersScrollRect() {
		return new IntRect(new IntPoint(HeaderWidth + FrozenWidth, 0), new IntSize(GridScrollAreaWidth, HeaderHeight + 1));
	}

	private IntRect GetRowHeadersScrollRect() {
		return new IntRect(new IntPoint(0, HeaderHeight + FrozenHeight), new IntSize(HeaderWidth + 1, GridScrollAreaHeight));
	}

	private IntRect GetFrozenColumnsRect() {
		return new IntRect(new IntPoint(HeaderWidth, HeaderHeight), new IntSize(_columnSizes.FrozenSize + 1, GridScrollAreaHeight));
	}

	private IntRect GetFrozenRowsRect() {
		return new IntRect(new IntPoint(HeaderWidth, HeaderHeight), new IntSize(GridScrollAreaHeight, _rowSizes.FrozenSize + 1));
	}

	public Rect GetColumnHeaderRectangle(int modelColumnIndex) {
		Rect rect = GetColumnHeaderRect(_columnSizes.ModelToReal(modelColumnIndex)).ToRect();

		Point pt = Image.PointToScreen(rect.TopLeft);
		return new Rect(pt, rect.Size);
	}

	public int? GetResizingColumn(Point pt) {
		if (pt.Y > HeaderHeight) {
			return null;
		}

		int frozenWidth = FrozenWidth;
		if ((int)pt.X - HeaderWidth <= frozenWidth + ColumnResizeThreshold) {
			if ((int)pt.X - HeaderWidth >= frozenWidth - ColumnResizeThreshold && (int)pt.X - HeaderWidth <= FrozenWidth - ColumnResizeThreshold) {
				return _columnSizes.FrozenCount - 1;
			}
			int index = _columnSizes.GetFrozenIndexOnPosition((int)pt.X - HeaderWidth);
			int begin = _columnSizes.GetPositionByRealIndex(index) + HeaderWidth;
			int end = begin + _columnSizes.GetSizeByRealIndex(index);
			if (pt.X >= begin - ColumnResizeThreshold && pt.X <= begin + ColumnResizeThreshold) {
				return index - 1;
			}

			if (pt.X >= end - ColumnResizeThreshold && pt.X <= end + ColumnResizeThreshold) {
				return index;
			}
		} else {
			int scrollXStart = _columnSizes.GetPositionByScrollIndex(FirstVisibleColumnScrollIndex);
			int index = _columnSizes.GetScrollIndexOnPosition((int)pt.X - HeaderWidth - frozenWidth + scrollXStart);
			int begin = _columnSizes.GetPositionByScrollIndex(index) + HeaderWidth + frozenWidth - scrollXStart;
			int end = begin + _columnSizes.GetSizeByScrollIndex(index);
			if (pt.X >= begin - ColumnResizeThreshold && pt.X <= begin + ColumnResizeThreshold) {
				return index - 1 + _columnSizes.FrozenCount;
			}

			if (pt.X >= end - ColumnResizeThreshold && pt.X <= end + ColumnResizeThreshold) {
				return index + _columnSizes.FrozenCount;
			}
		}
		return null;
	}

	private static int? GetSeriesIndexOnPosition(double position, int headerSize, SeriesSizes series, int firstVisible, bool ignoreForzen = false) {
		if (position <= headerSize) {
			return null;
		}

		int frozenSize = series.FrozenSize;
		if (!ignoreForzen) {
			if (position <= headerSize + frozenSize) {
				return series.GetFrozenIndexOnPosition((int)Math.Round(position - headerSize));
			}
		}

		return series.GetScrollIndexOnPosition(
			(int)Math.Round(position - headerSize - frozenSize) + series.GetPositionByScrollIndex(firstVisible)
		) + series.FrozenCount;
	}

	public FastGridCellAddress GetCellAddress(Point pt) {
		if (pt.X <= HeaderWidth && pt.Y < HeaderHeight) {
			return FastGridCellAddress.GridHeader;
		}
		if (pt.X >= GridScrollAreaWidth + HeaderWidth + FrozenWidth) {
			return FastGridCellAddress.Empty;
		}
		if (pt.Y >= GridScrollAreaHeight + HeaderHeight + FrozenHeight) {
			return FastGridCellAddress.Empty;
		}

		int? row = GetSeriesIndexOnPosition(pt.Y, HeaderHeight, _rowSizes, FirstVisibleRowScrollIndex);
		int? col = GetSeriesIndexOnPosition(pt.X, HeaderWidth, _columnSizes, FirstVisibleColumnScrollIndex);

		return new FastGridCellAddress(row, col);
	}

	public void ScrollCurrentCellIntoView() {
		ScrollIntoView(CurrentCell);
	}

	public void ScrollModelIntoView(FastGridCellAddress cell) {
		ScrollIntoView(ModelToReal(cell));
	}

	public void ScrollIntoView(FastGridCellAddress cell) {
		if (cell.Row.HasValue) {
			if (cell.Row.Value >= _rowSizes.FrozenCount) {
				int newRow = _rowSizes.ScrollInView(FirstVisibleRowScrollIndex, cell.Row.Value - _rowSizes.FrozenCount, GridScrollAreaHeight);
				ScrollContent(newRow, FirstVisibleColumnScrollIndex);
			}
		}

		if (cell.Column.HasValue) {
			if (cell.Column.Value >= _columnSizes.FrozenCount) {
				int newColumn = _columnSizes.ScrollInView(FirstVisibleColumnScrollIndex, cell.Column.Value - _columnSizes.FrozenCount, GridScrollAreaWidth);
				ScrollContent(FirstVisibleRowScrollIndex, newColumn);
			}
		}

		AdjustInlineEditorPosition();
		AdjustScrollBarPositions();
	}

	public FastGridCellAddress CurrentCell {
		get {
			if (CanSelect) {
				return _currentCell;
			} else {
				return FastGridCellAddress.Empty;
			}
		}
		set => MoveCurrentCell(value.Row, value.Column);
	}

	public int? CurrentRow {
		get => CurrentCell.IsCell ? CurrentCell.Row : null;
		set => CurrentCell = CurrentCell.ChangeRow(value);
	}

	public int? CurrentColumn {
		get => CurrentCell.IsCell ? CurrentCell.Column : null;
		set => CurrentCell = CurrentCell.ChangeColumn(value);
	}

	public void NotifyColumnArrangeChanged() {
		UpdateSeriesCounts();
		FixCurrentCellAndSetSelectionToCurrentCell();
		AdjustScrollbars();
		SetScrollbarMargin();
		FixScrollPosition();
		InvalidateAll();
	}

	public void NotifyRowArrangeChanged() {
		UpdateSeriesCounts();
		FixCurrentCellAndSetSelectionToCurrentCell();
		AdjustScrollbars();
		SetScrollbarMargin();
		FixScrollPosition();
		InvalidateAll();
	}

	private void UpdateSeriesCounts() {
		_rowSizes.Count = _modelRowCount;
		_columnSizes.Count = _modelColumnCount;

		if (_model != null) {
			_rowSizes.SetExtraordinaryIndexes(_model.GetHiddenRows(), _model.GetFrozenRows());
			_columnSizes.SetExtraordinaryIndexes(_model.GetHiddenColumns(), _model.GetFrozenColumns());
		}

		_realRowCount = _rowSizes.RealCount;
		_realColumnCount = _columnSizes.RealCount;
	}

	//public int HeaderHeight
	//{
	//	get => _headerHeight;
	//	set
	//	{
	//		_headerHeight = value;
	//		SetScrollbarMargin();
	//	}
	//}

	//public int HeaderWidth
	//{
	//	get => _headerWidth;
	//	set
	//	{
	//		_headerWidth = value;
	//		SetScrollbarMargin();
	//	}
	//}

	private void SetScrollbarMargin() {
		Vscroll.Margin = new Thickness {
			//Top = (HeaderHeight + FrozenHeight) / DpiDetector.DpiYKoef
		};
		Hscroll.Margin = new Thickness {
			//Left = (HeaderWidth + FrozenWidth) / DpiDetector.DpiXKoef
		};
	}

	public int FrozenWidth => _columnSizes.FrozenSize;

	public int FrozenHeight => _rowSizes.FrozenSize;

	private IntRect GetScrollRect() {
		return new IntRect(new IntPoint(HeaderWidth + FrozenWidth, HeaderHeight + FrozenHeight), new IntSize(GridScrollAreaWidth, GridScrollAreaHeight));
	}

	private IntRect GetGridHeaderRect() {
		return new IntRect(new IntPoint(0, 0), new IntSize(HeaderWidth + 1, HeaderHeight + 1));
	}

	internal FastGridCellAddress RealToModel(FastGridCellAddress address) {
		return new FastGridCellAddress(
			address.Row.HasValue ? _rowSizes.RealToModel(address.Row.Value) : null,
			address.Column.HasValue ? _columnSizes.RealToModel(address.Column.Value) : null,
			address.IsGridHeader
		);
	}

	private FastGridCellAddress ModelToReal(FastGridCellAddress address) {
		return new FastGridCellAddress(
			address.Row.HasValue ? _rowSizes.ModelToReal(address.Row.Value) : null,
			address.Column.HasValue ? _columnSizes.ModelToReal(address.Column.Value) : null,
			address.IsGridHeader
			);
	}


	private void OnAllowFlexibleRowsPropertyChanged() {
		RecountRowHeights();
		RecountColumnWidths();
		AdjustScrollbars();
		AdjustScrollBarPositions();
		AdjustInlineEditorPosition();
		InvalidateAll();
	}

	//private ActiveSeries GetActiveRealRows() {
	//	var res = new ActiveSeries();
	//	var visibleRows = VisibleRowCount;
	//	for (var i = FirstVisibleRowScrollIndex; i < FirstVisibleRowScrollIndex + visibleRows; i++) {
	//		var model = _rowSizes.RealToModel(i + _rowSizes.FrozenCount);
	//		res.ScrollVisible.Add(model);
	//	}
	//	for (var i = 0; i < _rowSizes.FrozenCount; i++) {
	//		var model = _rowSizes.RealToModel(i);
	//		res.Frozen.Add(model);
	//	}
	//	foreach (var cell in _selectedCells) {
	//		if (!cell.Row.HasValue) {
	//			continue;
	//		}

	//		var model = _rowSizes.RealToModel(cell.Row.Value);
	//		res.Selected.Add(model);
	//	}
	//	return res;
	//}

	//private ActiveSeries GetActiveRealColumns() {
	//	var res = new ActiveSeries();
	//	var visibleCols = VisibleColumnCount;
	//	for (var i = FirstVisibleColumnScrollIndex; i < FirstVisibleColumnScrollIndex + visibleCols; i++) {
	//		var model = _columnSizes.RealToModel(i + _columnSizes.FrozenCount);
	//		res.ScrollVisible.Add(model);
	//	}
	//	for (var i = 0; i < _columnSizes.FrozenCount; i++) {
	//		var model = _columnSizes.RealToModel(i);
	//		res.Frozen.Add(model);
	//	}
	//	foreach (var cell in _selectedCells) {
	//		if (!cell.Column.HasValue) {
	//			continue;
	//		}

	//		var model = _columnSizes.RealToModel(cell.Column.Value);
	//		res.Selected.Add(model);
	//	}
	//	return res;
	//}

	//public ActiveSeries GetActiveRows() {
	//	return IsTransposed ? GetActiveRealColumns() : GetActiveRealRows();
	//}

	//public ActiveSeries GetActiveColumns() {
	//	return IsTransposed ? GetActiveRealRows() : GetActiveRealColumns();
	//}

	private void SetExtraordinaryRealColumns() {
		if (_model != null) {
			_columnSizes.SetExtraordinaryIndexes(_model.GetHiddenColumns(), _model.GetFrozenColumns());
		}
	}

	private void RecountColumnWidths(CancellationToken token = default) {//todo : add cancelation
		_columnSizes.Clear();

		SetExtraordinaryRealColumns();

		if (_drawBuffer == null) {
			return;
		}

		//根据控件大小设置最大列宽度
		if (GridScrollAreaWidth > 16) {
			_columnSizes.MaxSize = GridScrollAreaWidth - 16;
		}

		//NOTE: 可以解决控件过小时单元格显示不全的问题
		_columnSizes.MaxSize = int.MaxValue;

		//if (IsWide) {
		//	return;
		//}

		if (_model == null) {
			return;
		}

		int rowCount = _modelRowCount;
		int colCount = _modelColumnCount;

		for (int col = 0; col < colCount; col++) {
			token.ThrowIfCancellationRequested();
			IFastGridCell cell = _model.GetColumnHeader(this, col);
			int width = GetCellContentWidth(cell) + (2 * CellPaddingHorizontal);
			if (width < MinColumnWidth) {
				width = MinColumnWidth;
			}

			_columnSizes.PutSizeOverride(col, width);
		}

		int gap = 4;

		int visRows = VisibleRowCount;
		int row0 = FirstVisibleRowScrollIndex /*+ _rowSizes.FrozenCount*/;
		for (int row = row0; row < Math.Min(row0 + visRows, rowCount); row++) {
			for (int col = 0; col < colCount; col++) {
				token.ThrowIfCancellationRequested();
				IFastGridCell cell = _model.GetCell(this, row, col);
				_columnSizes.PutSizeOverride(col, GetCellContentWidth(cell, _columnSizes.MaxSize) + (2 * CellPaddingHorizontal) + gap);
			}
		}

		_columnSizes.BuildIndex();
	}

	private void SetExtraordinaryRealRows() {
		if (_model != null) {
			_rowSizes.SetExtraordinaryIndexes(_model.GetHiddenRows(), _model.GetFrozenRows());
		}
	}

	private void RecountRowHeights() {
		_rowSizes.Clear();
		SetExtraordinaryRealRows();
		if (_drawBuffer == null) {
			return;
		}

		if (GridScrollAreaHeight > 16) {
			_rowSizes.MaxSize = GridScrollAreaHeight - 16;
		}

		CountVisibleRowHeights();
	}

	private bool CountVisibleRowHeights() {
		if (!FlexibleRows) {
			return false;
		}

		int colCount = _modelColumnCount;
		int rowCount = VisibleRowCount;
		bool changed = false;
		for (int row = FirstVisibleRowScrollIndex; row < FirstVisibleRowScrollIndex + rowCount; row++) {
			int modelRow = _rowSizes.RealToModel(row);
			if (_rowSizes.HasSizeOverride(modelRow)) {
				continue;
			}

			changed = true;
			for (int col = 0; col < colCount; col++) {
				IFastGridCell cell = GetModelCell(row, col);
				_rowSizes.PutSizeOverride(modelRow, GetCellContentHeight(cell) + (2 * CellPaddingVertical) + 2 + RowHeightReserve);
			}
		}
		_rowSizes.BuildIndex();
		//AdjustVerticalScrollBarRange();
		return changed;
	}

	private void FixScrollPosition() {
		if (FirstVisibleRowScrollIndex >= Vscroll.Maximum) {
			FirstVisibleRowScrollIndex = (int)Vscroll.Maximum;
		}

		if (FirstVisibleColumnScrollIndex >= Hscroll.Maximum) {
			FirstVisibleColumnScrollIndex = (int)Hscroll.Maximum;
		}

		ClearSelectedCells();
		if (CurrentCell.Row.HasValue) {
			if (CurrentCell.Row >= _realRowCount) {
				_currentCell = CurrentCell.ChangeRow(_realRowCount > 0 ? _realRowCount - 1 : null);
			}
		}
		if (CurrentCell.Column.HasValue) {
			if (CurrentCell.Column >= _realColumnCount) {
				_currentCell = CurrentCell.ChangeColumn(_realColumnCount > 0 ? _realColumnCount - 1 : null);
			}
		}
		if (CurrentCell.IsCell) {
			//AddSelectedCell(_currentCell);
			Selection.SelectCell(new SimpleCellAddress(CurrentCell));
		}

		AdjustScrollBarPositions();
		OnChangeSelectedCells(false);
	}

	public int FirstVisibleRowModelIndex {
		get {
			return _rowSizes.RealToModel(FirstVisibleRowScrollIndex + _rowSizes.FrozenCount);
		}
	}
}
