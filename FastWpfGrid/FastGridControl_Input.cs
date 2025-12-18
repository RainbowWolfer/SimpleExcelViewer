using RW.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FastWpfGrid;

public partial class FastGridControl {
	public static readonly object ToggleAllowFlexibleRowsCommand = new();
	public static readonly object SelectAllCommand = new();
	public static readonly object AdjustColumnSizesCommand = new();

	public class ActiveRegion {
		internal IntRect Rect;
		public object CommandParameter;
		public string Tooltip;
	}

	public event TypedEventHandler<FastGridControl, BeforeColumnReorderEventArgs> BeforeColumnReorder;
	public event TypedEventHandler<FastGridControl, ColumnReorderEventArgs> ColumnReorder;

	public event Action<object, ColumnClickEventArgs> ColumnHeaderClick;
	public event Action<object, RowClickEventArgs> RowHeaderClick;
	public List<ActiveRegion> CurrentCellActiveRegions = [];
	public ActiveRegion CurrentHoverRegion;
	private Point? _mouseCursorPoint;
	private ToolTip _tooltip;
	private object _tooltipTarget;
	private string _tooltipText;
	private DispatcherTimer _tooltipTimer;
	private readonly DispatcherTimer _dragTimer;
	private FastGridCellAddress _dragStartCell;
	private FastGridCellAddress? _rightMouseDownCell;
	private Point startDragPosition;
	private FastGridCellAddress _mouseOverCell;
	private bool _mouseOverCellIsTrimmed;
	private int? _mouseOverRow;
	private int? _mouseOverColumn;
	private int? _mouseOverRowHeader;
	private int? _mouseOverColumnHeader;
	//private readonly FastGridCellAddress _shiftDragStartCell;
	public event EventHandler ScrolledModelRows;
	public event EventHandler ScrolledModelColumns;
	private FastGridCellAddress _showCellEditorIfMouseUp;

	// mouse is scrolled and captured out of control are - force scroll
	private bool _mouseIsBehindBottom;
	private bool _mouseIsBehindTop;
	private bool _mouseIsBehindLeft;
	private bool _mouseIsBehindRight;

	private int? _mouseMoveColumn;
	private int? _mouseMoveRow;

	private int? _resizingColumn;
	private Point? _resizingColumnOrigin;
	private int? _resizingColumnStartSize;
	private int? _lastResizingColumn;
	private DateTime _lastResizingColumnSet;
	private readonly DateTime? _lastDblClickResize;
	private ColumnReorderAdorner columnReorderAdorner;

	internal ColumnReorderAdorner ColumnReorderAdorner {
		get => columnReorderAdorner;
		set {
			value?.InvalidateVisual();
			columnReorderAdorner?.Detach();
			columnReorderAdorner = value;
		}
	}




	private static void SetContextMenuItemsCommandParameters(ContextMenu contextMenu, object parameter) {
		List<MenuItem> all = GetContextMenuChildren(contextMenu.Items);
		foreach (MenuItem item in all) {
			item.CommandParameter = parameter;
		}
	}

	private static List<MenuItem> GetContextMenuChildren(ItemCollection items) {
		List<MenuItem> result = [];
		foreach (MenuItem item in items.OfType<MenuItem>()) {
			result.Add(item);
			if (item.Items.Count > 0) {
				result.AddRange(GetContextMenuChildren(item.Items));
			}
		}
		return result;
	}


	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
		base.OnMouseLeftButtonDown(e);
		_showCellEditorIfMouseUp = FastGridCellAddress.Empty;

		Point pt = e.GetPosition(Image);
		pt.X *= DpiDetector.DpiXKoef;
		pt.Y *= DpiDetector.DpiYKoef;
		FastGridCellAddress cell = GetCellAddress(pt);

		Model?.OnCellClicked(this, cell);

		//if (cell.Column.HasValue && cell.Row != null && _rowSizes.IsIndexFrozen(cell.Row.Value)) {
		//	cell = new FastGridCellAddress(null, cell.Column, false);
		//}

		ActiveRegion currentRegion = CurrentCellActiveRegions.FirstOrDefault(x => x.Rect.Contains(pt));
		if (currentRegion != null) {
			HandleCommand(cell, currentRegion.CommandParameter);
			return;
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		int? resizingColumn = GetResizingColumn(pt);
		if (resizingColumn != null) {
			Cursor = Cursors.SizeWE;
			_resizingColumn = resizingColumn;
			_resizingColumnOrigin = pt;
			if (_resizingColumn.Value < 0) {
				_resizingColumnStartSize = HeaderWidth;
			} else {
				_resizingColumnStartSize = _columnSizes.GetSizeByRealIndex(_resizingColumn.Value);
			}
			CaptureMouse();
		}

		bool isHeaderClickHandled = false;
		if (_resizingColumn == null && cell.IsColumnHeader) {
			isHeaderClickHandled = OnModelColumnClick(_columnSizes.RealToModel(cell.Column.Value));
		}
		if (cell.IsRowHeader) {
			isHeaderClickHandled = OnModelRowClick(_rowSizes.RealToModel(cell.Row.Value));
		}

		if (!isHeaderClickHandled && ((_resizingColumn == null && cell.IsColumnHeader) || cell.IsRowHeader)
			&& (_lastDblClickResize == null || DateTime.Now - _lastDblClickResize.Value > TimeSpan.FromSeconds(1))) {
			HideInlineEditor();

			LastSelectionType type = cell.IsColumnHeader ? LastSelectionType.ColumnHeader : LastSelectionType.RowHeader;

			if (ShiftPressed) {
				if (cell.IsColumnHeader) {
					//column header
					SelectionRect rect = GetCellRectColumns(cell, CurrentCell);
					if (rect != null) {
						Selection.SelectRect(rect);
					}
				} else {
					//row header
					SelectionRect rect = GetCellRectRows(CurrentCell, cell);
					if (rect != null) {
						Selection.SelectRect(rect);
					}
				}
				InvalidateAll();
			} else {
				if (cell.IsColumnHeader) {
					//column header
					Selection.SelectColumn(cell.Column.Value, _rowSizes.RealCount);
				} else {
					//row header
					Selection.SelectRow(cell.Row.Value, _columnSizes.RealCount);
				}

				SetCurrentCell(cell);
				InvalidateAll();

				_dragStartCell = cell;
				startDragPosition = pt;
				_dragTimer.IsEnabled = true;
				CaptureMouse();
			}
			OnChangeSelectedCells(true);
		}

		if (cell.IsCell) {
			bool isStartInFrozen = CurrentCell.Row != null && _rowSizes.IsIndexFrozen(CurrentCell.Row.Value);
			bool isTargetInFrozen = cell.Row != null && _rowSizes.IsIndexFrozen(cell.Row.Value);
			if (ShiftPressed && CurrentCell.IsCell) {

				SelectRectInternal(CurrentCell, cell);
				//SelectionRect rect = GetCellRect(CurrentCell, cell);
				//Selection.SelectRect(rect, LastSelectionType.Cell);

				InvalidateAll();

			} else {

				//_selectedCells.ToList().ForEach(InvalidateCell);
				//ClearSelectedCells();
				if (CurrentCell == cell) {
					_showCellEditorIfMouseUp = CurrentCell;
				} else {
					HideInlineEditor();
					SetCurrentCell(cell);
				}
				//AddSelectedCell(cell);
				Selection.SelectCell(new SimpleCellAddress(cell));

				InvalidateAll();

				_dragStartCell = cell;
				startDragPosition = pt;
				_dragTimer.IsEnabled = true;
				CaptureMouse();
			}
			OnChangeSelectedCells(true);
		}

		//if (cell.IsCell) ShowTextEditor(
		//    GetCellRect(cell.Row.Value, cell.Column.Value),
		//    Model.GetCell(cell.Row.Value, cell.Column.Value).GetEditText());
	}

	protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
		base.OnMouseDoubleClick(e);
		if (e.ChangedButton != MouseButton.Left) {
			return;
		}
		//if (e.ChangedButton == MouseButton.Left && (_lastResizingColumn.HasValue && (DateTime.Now - _lastResizingColumnSet) < TimeSpan.FromSeconds(1))) {
		//	return;
		//	_lastDblClickResize = DateTime.Now;
		//	var col = _lastResizingColumn.Value;

		//	_columnSizes.RemoveSizeOverride(col);

		//	if (_model == null) {
		//		return;
		//	}

		//	var rowCount = _isTransposed ? _modelColumnCount : _modelRowCount;
		//	var colCount = _isTransposed ? _modelRowCount : _modelColumnCount;
		//	{
		//		var cell = _isTransposed ? _model.GetRowHeader(this, col) : _model.GetColumnHeader(this, col);
		//		_columnSizes.PutSizeOverride(col, GetCellContentWidth(cell) + 2 * CellPaddingHorizontal);
		//	}
		//	var visRows = VisibleRowCount;
		//	var row0 = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount;
		//	for (var row = row0; row < Math.Min(row0 + visRows, rowCount); row++) {
		//		var cell = _isTransposed ? _model.GetCell(this, col, row) : _model.GetCell(this, row, col);
		//		_columnSizes.PutSizeOverride(col, GetCellContentWidth(cell, _columnSizes.MaxSize) + 2 * CellPaddingHorizontal);
		//	}

		//	_columnSizes.BuildIndex();
		//	AdjustScrollbars();
		//	SetScrollbarMargin();
		//	FixScrollPosition();
		//	InvalidateAll();
		//} else {
		//}
		Point pt = e.GetPosition(Image);
		pt.X *= DpiDetector.DpiXKoef;
		pt.Y *= DpiDetector.DpiYKoef;
		FastGridCellAddress cell = GetCellAddress(pt);

		FastGridCellAddress cellModel = RealToModel(cell);
		if (cellModel.IsCell && IsModelCellInValidRange(cellModel)) {

			CellDoubleClick?.Invoke(this, cell);
			CellDoubleClickCommand?.Execute(cell);

			Model?.HandleDoubleClickCell(this, cell);

		}
	}

	private bool EnableColumnDragReorder() {
		return Model?.EnableDragReorderColumn ?? false;
	}

	private void DragTimer_Tick(object sender, EventArgs e) {
		bool dragColumnHeaderReorder = _dragStartCell.IsColumnHeader && EnableColumnDragReorder();
		if (dragColumnHeaderReorder) {
			using InvalidationContext ctx = CreateInvalidationContext();
			if (_mouseIsBehindRight) {
				int newColumn = FirstVisibleColumnScrollIndex + 1;
				if (!_columnSizes.IsWholeInView(FirstVisibleColumnScrollIndex, _columnSizes.ScrollCount - 1, GridScrollAreaWidth)) {
					ScrollContent(FirstVisibleRowScrollIndex, newColumn);
					AdjustScrollBarPositions();
				}
			}
			if (_mouseIsBehindLeft) {
				int newColumn = FirstVisibleColumnScrollIndex - 1;
				if (newColumn >= 0) {
					ScrollContent(FirstVisibleRowScrollIndex, newColumn);
					AdjustScrollBarPositions();
				}
			}
		} else {
			bool isStartInFrozenRows = _dragStartCell.Row != null && _rowSizes.IsIndexFrozen(_dragStartCell.Row.Value);
			if (!isStartInFrozenRows) {
				using InvalidationContext ctx = CreateInvalidationContext();
				if (_mouseIsBehindBottom) {
					int newRow = FirstVisibleRowScrollIndex + 1;
					if (!_rowSizes.IsWholeInView(FirstVisibleRowScrollIndex, _rowSizes.ScrollCount - 1, GridScrollAreaHeight)) {
						ScrollContent(newRow, FirstVisibleColumnScrollIndex);
						int row = FirstVisibleRowScrollIndex + VisibleRowCount - 1 + _rowSizes.FrozenCount;
						//SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(row, _mouseMoveColumn ?? _currentCell.Column));
						//SelectionRect rect = GetCellRect();
						//FastGridCellAddress cell = new FastGridCellAddress(row, _mouseMoveColumn ?? _currentCell.Column);
						//SelectRectInternal(_dragStartCell, cell);
						//SetCurrentCell(cell);
						AdjustScrollBarPositions();
					}
				}
				if (_mouseIsBehindTop) {
					int newRow = FirstVisibleRowScrollIndex - 1;
					if (newRow >= 0) {
						ScrollContent(newRow, FirstVisibleColumnScrollIndex);
						int row = newRow + _rowSizes.FrozenCount;
						//SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(row, _mouseMoveColumn ?? _currentCell.Column));
						//FastGridCellAddress cell = new FastGridCellAddress(row, _mouseMoveColumn ?? _currentCell.Column);
						//SelectRectInternal(_dragStartCell, cell);
						//SetCurrentCell(cell);
						AdjustScrollBarPositions();
					}
				}
			}
			using (InvalidationContext ctx = CreateInvalidationContext()) {
				if (_mouseIsBehindRight) {
					int newColumn = FirstVisibleColumnScrollIndex + 1;
					if (!_columnSizes.IsWholeInView(FirstVisibleColumnScrollIndex, _columnSizes.ScrollCount - 1, GridScrollAreaWidth)) {
						ScrollContent(FirstVisibleRowScrollIndex, newColumn);
						int col = FirstVisibleColumnScrollIndex + VisibleColumnCount - 1 + _columnSizes.FrozenCount;
						//SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(_mouseMoveRow ?? _currentCell.Row, col));
						//FastGridCellAddress cell = new FastGridCellAddress(_mouseMoveRow ?? _currentCell.Row, col);
						//SelectRectInternal(_dragStartCell, cell);
						//SetCurrentCell(cell);
						AdjustScrollBarPositions();
					}
				}
				if (_mouseIsBehindLeft) {
					int newColumn = FirstVisibleColumnScrollIndex - 1;
					if (newColumn >= 0) {
						ScrollContent(FirstVisibleRowScrollIndex, newColumn);
						int col = newColumn + _columnSizes.FrozenCount;
						//SetSelectedRectangle(_dragStartCell, new FastGridCellAddress(_mouseMoveRow ?? _currentCell.Row, col));
						//FastGridCellAddress cell = new FastGridCellAddress(_mouseMoveRow ?? _currentCell.Row, col);
						//SelectRectInternal(_dragStartCell, cell);
						//SetCurrentCell(cell);
						AdjustScrollBarPositions();
					}
				}
			}
			InvalidateAll();
		}
	}

	private void EscapeDragColumnReorder() {
		if (EnableColumnDragReorder() && ColumnReorderAdorner != null) {
			startDragPosition = default;
			ColumnReorderAdorner = null;
			_dragStartCell = new FastGridCellAddress();
			Cursor = null;
		}
	}

	protected override void OnLostFocus(RoutedEventArgs e) {
		base.OnLostFocus(e);
		HandleMouseUp();
	}

	protected override void OnLostMouseCapture(MouseEventArgs e) {
		base.OnLostMouseCapture(e);
		HandleMouseUp();
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
		base.OnMouseLeftButtonUp(e);
		HandleMouseUp();
	}

	private void HandleMouseUp() {
		if (!_dragStartCell.IsEmpty) {
			if (ColumnReorderAdorner != null && EnableColumnDragReorder() && ColumnReorderAdorner.TargetIndex != -1) {
				int startIndex = _dragStartCell.Column.Value;
				int targetIndex = ColumnReorderAdorner.TargetIndex;

				ColumnReorder?.Invoke(this, new ColumnReorderEventArgs(startIndex, targetIndex));
				Model?.OnColumnReorder(startIndex, targetIndex);
			}
			_dragStartCell = new FastGridCellAddress();
			_dragTimer.IsEnabled = false;
			ReleaseMouseCapture();
		}
		EscapeDragColumnReorder();
		//bool wasColumnResizing = false;
		if (_resizingColumn.HasValue) {
			_lastResizingColumnSet = DateTime.Now;
			_lastResizingColumn = _resizingColumn;
			_resizingColumn = null;
			_resizingColumnOrigin = null;
			_resizingColumnStartSize = null;
			//wasColumnResizing = true;
			ReleaseMouseCapture();
		}

		Point pt = Mouse.GetPosition(Image);
		pt.X *= DpiDetector.DpiXKoef;
		pt.Y *= DpiDetector.DpiYKoef;
		FastGridCellAddress cell = GetCellAddress(pt);

		if (cell == _showCellEditorIfMouseUp && IsModelCellInValidRange(cell)) {
			ShowInlineEditor(_showCellEditorIfMouseUp);
			_showCellEditorIfMouseUp = FastGridCellAddress.Empty;
		}
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
		base.OnMouseRightButtonDown(e);

		Point pt = e.GetPosition(Image);
		pt.X *= DpiDetector.DpiXKoef;
		pt.Y *= DpiDetector.DpiYKoef;
		FastGridCellAddress cell = GetCellAddress(pt);

		_rightMouseDownCell = cell;

		if (cell.IsRowHeader) {
			if (Selection.Rect != null
				&& Selection.Rect.RectFrom.Row <= cell.Row.Value
				&& Selection.Rect.RectTo.Row >= cell.Row.Value
				&& Selection.Rect.RectTo.Column - Selection.Rect.RectFrom.Column == _columnSizes.RealCount) {
				return;
			}
		}

		if (cell.IsColumnHeader) {
			if (Selection.Rect != null
				&& Selection.Rect.RectFrom.Column <= cell.Column.Value
				&& Selection.Rect.RectTo.Column >= cell.Column.Value
				&& Selection.Rect.RectTo.Row - Selection.Rect.RectFrom.Row == _rowSizes.RealCount) {
				return;
			}
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		if (cell.IsGridHeader) {

		} else if (cell.IsRowHeader) {
			if (Selection.Rect == null ||
				cell.Row < Selection.Rect.RectFrom.Row ||
				cell.Row > Selection.Rect.RectTo.Row ||
				Selection.Rect.RectTo.Column - Selection.Rect.RectFrom.Column + 1 != _modelColumnCount
			) {
				Selection.SelectRow(cell.Row.Value, _columnSizes.Count);
				SetCurrentCell(cell);
				OnChangeSelectedCells(true);
			}
		} else if (cell.IsColumnHeader) {
			if (Selection.Rect == null ||
				cell.Column < Selection.Rect.RectFrom.Column ||
				cell.Column > Selection.Rect.RectTo.Column ||
				Selection.Rect.RectTo.Row - Selection.Rect.RectFrom.Row + 1 != _modelRowCount
			) {
				Selection.SelectColumn(cell.Column.Value, _rowSizes.Count);
				SetCurrentCell(cell);
				OnChangeSelectedCells(true);
			}
		} else if (cell.IsCell) {
			if (Selection.Rect == null || !Selection.Rect.Contains(cell.Column.Value, cell.Row.Value)) {
				Selection.SelectCell(new SimpleCellAddress(cell));
				SetCurrentCell(cell);
				OnChangeSelectedCells(true);
			}
		}
		InvalidateAll();
	}

	protected override void OnMouseRightButtonUp(MouseButtonEventArgs e) {
		base.OnMouseRightButtonUp(e);

		if (Model == null) {
			return;
		}
		if (_rightMouseDownCell is null) {
			return;
		}

		FastGridCellAddress cell = _rightMouseDownCell.Value;
		_rightMouseDownCell = null;

		//if (cell.IsGridHeader) {
		//	return;
		//}

		if (cell.Row != null && (cell.Row.Value >= Model.RowCount || cell.Row.Value < 0)) {
			return;
		}

		if (cell.Column != null && (cell.Column.Value >= Model.ColumnCount || cell.Column.Value < 0)) {
			return;
		}

		//IEnumerable<SimpleCellAddress> selected = GetSelectedModelCells();
		SelectionRect selectionRect = GetSelectionRect();
		if (selectionRect == null) {
			return;
		}

		IContextMenuCreator contextMenuModel = Model.GetContextMenu(this, selectionRect, cell);

		if (contextMenuModel != null && contextMenuModel.CanShow()) {
			ContextMenu menu = contextMenuModel.CreateContextMenu();
			SetContextMenuItemsCommandParameters(menu, new DataGridContextMenuItemArgs(Model, menu, e, selectionRect));
			if (menu.HasItems) {
				menu.IsOpen = true;
			}
		}

	}

	private bool OnModelColumnClick(int column) {
		if (column >= 0 && column < _modelColumnCount) {
			ColumnClickEventArgs args = new() {
				Grid = this,
				Column = column,
			};
			ColumnHeaderClick?.Invoke(this, args);
			return args.Handled;
		}
		return false;
	}

	private bool OnModelRowClick(int row) {
		if (row >= 0 && row < _modelRowCount) {
			RowClickEventArgs args = new() {
				Grid = this,
				Row = row,
			};
			RowHeaderClick?.Invoke(this, args);
			return args.Handled;
			//if (!args.Handled)
			//{
			//    HideInlinEditor();

			//    if (ControlPressed)
			//    {
			//        foreach (var cell in GetCellRange(ModelToReal(new FastGridCellAddress(row, 0)), ModelToReal(new FastGridCellAddress(row, _modelColumnCount - 1))))
			//        {
			//            if (_selectedCells.Contains(cell)) _selectedCells.Remove(cell);
			//            else _selectedCells.Add(cell);
			//            InvalidateCell(cell);
			//        }
			//    }
			//    else if (ShiftPressed)
			//    {
			//        _selectedCells.ToList().ForEach(InvalidateCell);
			//        _selectedCells.Clear();
			//        var currentModel = RealToModel(_currentCell);

			//        foreach (var cell in GetCellRange(ModelToReal(new FastGridCellAddress(currentModel.Row, 0)), ModelToReal(new FastGridCellAddress(row, _modelColumnCount - 1))))
			//        {
			//            _selectedCells.Add(cell);
			//            InvalidateCell(cell);
			//        }
			//    }
			//    else
			//    {
			//        _selectedCells.ToList().ForEach(InvalidateCell);
			//        _selectedCells.Clear();
			//        if (_currentCell.IsCell)
			//        {
			//            var currentModel = RealToModel(_currentCell);
			//            SetCurrentCell(ModelToReal(new FastGridCellAddress(row, currentModel.Column)));
			//            _dragStartCell = ModelToReal(new FastGridCellAddress(row, null));
			//        }
			//        foreach (var cell in GetCellRange(ModelToReal(new FastGridCellAddress(row, 0)), ModelToReal(new FastGridCellAddress(row, _modelColumnCount - 1))))
			//        {
			//            _selectedCells.Add(cell);
			//            InvalidateCell(cell);
			//        }
			//    }
			//}
		}
		return false;
	}

	private void imageMouseWheel(object sender, MouseWheelEventArgs e) {

		//if (e.Delta > 0) {
		//	IncreaseFontSize();
		//} else if (e.Delta < 0) {
		//	DecreaseFontSize();
		//}

		if (ControlPressed) {
			if (EnableHorizontalScroll) {
				if (e.Delta < 0) {
					Hscroll.Value += Hscroll.LargeChange / 2;
				}

				if (e.Delta > 0) {
					Hscroll.Value -= Hscroll.LargeChange / 2;
				}

				ScrollChanged();
			}

		} else {

			if (EnableVerticalScroll) {
				if (e.Delta < 0) {
					Vscroll.Value += Vscroll.LargeChange / 2;
				}

				if (e.Delta > 0) {
					Vscroll.Value -= Vscroll.LargeChange / 2;
				}

				ScrollChanged();
			}

		}
	}

	public double GetVerticalScrollValue() => Vscroll.Value;

	public void SetVerticalScrollValue(double value) {
		Vscroll.Value = value;
		ScrollChanged();
	}

	public double GetHorizontalScrollValue() => Hscroll.Value;

	public void SetHorizontalScrollValue(double value) {
		Hscroll.Value = value;
		ScrollChanged();
	}

	public void IncreaseFontSize() {
		if (CellFontSize < 20) {
			CellFontSize++;
		}
	}

	public void DecreaseFontSize() {
		if (CellFontSize > 6) {
			CellFontSize--;
		}
	}

	private static bool ControlPressed => (Keyboard.Modifiers & ModifierKeys.Control) != 0;

	private static bool ShiftPressed => (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

	public bool HandleCursorMove(KeyEventArgs e, bool isInTextBox = false) {
		int frozenRowCount = _rowSizes.FrozenCount;
		if (e.Key == Key.Up && ControlPressed) {
			return MoveCurrentCell(frozenRowCount, CurrentCell.Column, e);
		}

		if (e.Key == Key.Down && ControlPressed) {
			return MoveCurrentCell(_realRowCount - 1, CurrentCell.Column, e);
		}

		if (e.Key == Key.Left && ControlPressed) {
			return MoveCurrentCell(CurrentCell.Row, 0, e);
		}

		if (e.Key == Key.Right && ControlPressed) {
			return MoveCurrentCell(CurrentCell.Row, _realColumnCount - 1, e);
		}

		if (e.Key == Key.Up) {
			return MoveCurrentCell(CurrentCell.Row - 1, CurrentCell.Column, e);
		}

		if (e.Key == Key.Down) {
			return MoveCurrentCell(CurrentCell.Row + 1, CurrentCell.Column, e);
		}

		if (e.Key == Key.Left && !isInTextBox) {
			return MoveCurrentCell(CurrentCell.Row, CurrentCell.Column - 1, e);
		}

		if (e.Key == Key.Right && !isInTextBox) {
			return MoveCurrentCell(CurrentCell.Row, CurrentCell.Column + 1, e);
		}

		if (e.Key == Key.Home && ControlPressed) {
			return MoveCurrentCell(frozenRowCount, 0, e);
		}

		if (e.Key == Key.End && ControlPressed) {
			return MoveCurrentCell(_realRowCount - 1, _realColumnCount - 1, e);
		}

		if (e.Key == Key.PageDown && ControlPressed) {
			return MoveCurrentCell(_realRowCount - 1, CurrentCell.Column, e);
		}

		if (e.Key == Key.PageUp && ControlPressed) {
			return MoveCurrentCell(frozenRowCount, CurrentCell.Column, e);
		}

		if (e.Key == Key.Home && !isInTextBox) {
			return MoveCurrentCell(CurrentCell.Row, 0, e);
		}

		if (e.Key == Key.End && !isInTextBox) {
			return MoveCurrentCell(CurrentCell.Row, _realColumnCount - 1, e);
		}

		if (e.Key == Key.PageDown) {
			return MoveCurrentCell(CurrentCell.Row + VisibleRowCount, CurrentCell.Column, e);
		}

		if (e.Key == Key.PageUp) {
			return MoveCurrentCell(CurrentCell.Row - VisibleRowCount, CurrentCell.Column, e);
		}

		if (e.Key == Key.Enter && !isInTextBox) {
			return MoveCurrentCell(CurrentCell.Row + 1, CurrentCell.Column, e);
		}

		if (e.Key == Key.Return && !isInTextBox) {
			return MoveCurrentCell(CurrentCell.Row + 1, CurrentCell.Column, e);
		}

		if (e.Key == Key.Tab) {
			return MoveCurrentCell(CurrentCell.Row, CurrentCell.Column + 1, e);
		}

		return false;
	}

	private void imageKeyDown(object sender, KeyEventArgs e) {
		using InvalidationContext ctx = CreateInvalidationContext();
		//if (ShiftPressed) {
		//	if (!_shiftDragStartCell.IsCell) {
		//		_shiftDragStartCell = _currentCell;
		//	}
		//} else {
		//	_shiftDragStartCell = FastGridCellAddress.Empty;
		//}

		bool handled = Model?.HandleKeyDown(this, e) ?? false;
		if (handled) {
			e.Handled = true;
			return;
		}

		bool moved = HandleCursorMove(e);
		//if (ShiftPressed && moved) {
		//	SelectionRect rect = GetCellRect(_shiftDragStartCell, _currentCell);
		//	Selection.SelectRect(rect, LastSelectionType.Cell);
		//	//SetSelectedRectangle(_shiftDragStartCell, _currentCell);
		//}

		if (e.Key == Key.F2 && CurrentCell.IsCell) {
			if (ShowInlineEditor(CurrentCell)) {
				e.Handled = true;
			}
		}

		if (e.Key == Key.A && ControlPressed && AllowSelectAll) {
			SelectAll();
			e.Handled = true;
		}

		if (e.Key == Key.Escape) {
			if (ColumnReorderAdorner != null) {
				//取消拖拽
				EscapeDragColumnReorder();
				e.Handled = true;
			} else if (AllowEscapeToDeselect && Selection.HasSelection()) {
				//取消选择
				Selection.Clear();
				CurrentCell = FastGridCellAddress.Empty;
				OnChangeSelectedCells(true);
				e.Handled = true;
			}
			_dragStartCell = FastGridCellAddress.Empty;
		}

		if (e.Key == Key.C && ControlPressed) {
			CopyCurrentCells();
			e.Handled = true;
		}
	}

	private void imageTextInput(object sender, TextCompositionEventArgs e) {
		return;//不太喜欢这功能

		//if (!CurrentCell.IsCell)
		//{
		//	return;
		//}

		//if (e.Text == null)
		//{
		//	return;
		//}

		//if (e.Text != " " && string.IsNullOrEmpty(e.Text.Trim()))
		//{
		//	return;
		//}

		//if (e.Text.Length == 1 && e.Text[0] < 32)
		//{
		//	return;
		//}

		//ShowInlineEditor(CurrentCell, e.Text);
	}

	private void imageMouseDown(object sender, MouseButtonEventArgs e) {
		Keyboard.Focus(Image);
	}

	protected override void OnMouseMove(MouseEventArgs e) {
		base.OnMouseMove(e);
		//if (e.LeftButton != MouseButtonState.Pressed)
		//{
		//	return;
		//}
		//if (_dragStartCell.Row != null && _rowSizes.IsIndexFrozen(_dragStartCell.Row.Value)) {
		//	return;
		//}
		using (InvalidationContext ctx = CreateInvalidationContext()) {
			Point pt = e.GetPosition(Image);

			int frozenRowsHeight = _rowSizes.GetAllFrozenHeight() + HeaderHeight;

			_mouseIsBehindLeft = pt.X < HeaderWidth;
			_mouseIsBehindRight = pt.X > Image.ActualWidth;
			_mouseIsBehindTop = pt.Y < frozenRowsHeight;
			_mouseIsBehindBottom = pt.Y > Image.ActualHeight;

			pt.X *= DpiDetector.DpiXKoef;
			pt.Y *= DpiDetector.DpiYKoef;
			_mouseCursorPoint = pt;
			FastGridCellAddress cell = GetCellAddress(pt);


			//if (cell.Row != null && _rowSizes.IsIndexFrozen(cell.Row.Value)) {
			//	cell = new FastGridCellAddress(null, cell.Column, false);
			//}

			bool reachMinimumMoveDistance = (startDragPosition - pt).Length > 10;
			bool draggingColumnToReorder = _dragStartCell.IsColumnHeader && EnableColumnDragReorder() && reachMinimumMoveDistance && IsModelCellInValidRange(_dragStartCell);

			_mouseMoveRow = GetSeriesIndexOnPosition(pt.Y, HeaderHeight, _rowSizes, FirstVisibleRowScrollIndex);
			_mouseMoveColumn = GetSeriesIndexOnPosition(pt.X, HeaderWidth, _columnSizes, FirstVisibleColumnScrollIndex);

			//Debug.WriteLine($"{_mouseMoveColumn} - {_mouseMoveRow}");

			bool dragColumnReorderCanceled = false;
			if (draggingColumnToReorder && _dragStartCell.Column != null && cell.Column != null) {
				int startIndex = _dragStartCell.Column.Value;
				int targetIndex = ColumnReorderAdorner.GetTargetIndex(this, pt, cell.Column.Value);

				BeforeColumnReorderEventArgs before = new(startIndex, targetIndex);
				BeforeColumnReorder?.Invoke(this, before);

				if (before.Cancel) {
					dragColumnReorderCanceled = true;
				}
			}

			if (_resizingColumn.HasValue) {
				int newSize = _resizingColumnStartSize.Value + (int)Math.Round(pt.X - _resizingColumnOrigin.Value.X);
				if (newSize < MinColumnWidth) {
					newSize = MinColumnWidth;
				}

				if (_resizingColumn.Value < 0) {
					int maxSize = 300;
					if (newSize > maxSize) {
						newSize = maxSize;
					}

					HeaderWidth = newSize;
				} else {

					if (newSize > GridScrollAreaWidth) {
						newSize = GridScrollAreaWidth;
					}

					_columnSizes.Resize(_resizingColumn.Value, newSize);
				}
				if (_resizingColumn < _columnSizes.FrozenCount) {
					SetScrollbarMargin();
				}
				AdjustScrollbars();
				InvalidateAll();
			} else {
				if (draggingColumnToReorder) {
					if (dragColumnReorderCanceled) {
						Cursor = Cursors.No;
					} else {
						Cursor = ColumnReorderDragCursor;
					}
				} else {
					int? column = GetResizingColumn(pt);
					if (column != null) {
						Cursor = Cursors.SizeWE;
					} else {
						Cursor = null;
					}
				}
			}

			bool isCell = _dragStartCell.IsCell && cell.IsCell;
			bool isRowHeader = _dragStartCell.IsRowHeader && cell.Row.HasValue;
			bool isColumnHeader = _dragStartCell.IsColumnHeader && cell.Column.HasValue;
			if (isCell
				|| isRowHeader
				|| (!draggingColumnToReorder && isColumnHeader)) {
				//Debug.WriteLine(cell);
				//SetSelectedRectangle(_dragStartCell, cell);
				if (isCell) {
					SelectRectInternal(_dragStartCell, cell);
					OnChangeSelectedCells(true);
					InvalidateAll();
				} else if (isRowHeader) {
					SelectionRect rect = GetCellRectRows(_dragStartCell, cell);
					if (rect != null) {
						Selection.SelectRect(rect);
						OnChangeSelectedCells(true);
						InvalidateAll();
					}
				}
				//drag to select multiple columns
			}

			if (draggingColumnToReorder && !dragColumnReorderCanceled) {
				SetCurrentCell(_dragStartCell);
				ColumnReorderAdorner = new ColumnReorderAdorner(this, _dragStartCell, cell);
			}

			SetHoverRow(cell.IsCell ? cell.Row.Value : null);
			SetHoverColumn(cell.IsCell ? cell.Column.Value : null);
			SetHoverRowHeader(cell.IsRowHeader ? cell.Row.Value : null);
			SetHoverColumnHeader(cell.IsColumnHeader ? cell.Column.Value : null);
			SetHoverCell(cell);

			ActiveRegion currentRegion = CurrentCellActiveRegions.FirstOrDefault(x => x.Rect.Contains(pt));
			if (currentRegion != CurrentHoverRegion) {
				InvalidateCell(cell);
			}
		}

		HandleMouseMoveTooltip();
	}

	private void HandleMouseMoveTooltip() {
		if (CurrentHoverRegion != null && CurrentHoverRegion.Tooltip != null) {
			ShowTooltip(CurrentHoverRegion, CurrentHoverRegion.Tooltip);
			return;
		}

		if (CurrentHoverRegion == null) {
			IFastGridCell modelCell = GetCell(_mouseOverCell);
			if (modelCell != null) {
				if (modelCell.ToolTipVisibility == TooltipVisibilityMode.Always || _mouseOverCellIsTrimmed) {
					string tooltip = modelCell.ToolTipText;
					if (tooltip != null) {
						ShowTooltip(_mouseOverCell, tooltip);
						return;
					}
				}
			}
		}

		HideTooltip();
	}

	private void HideTooltip() {
		if (_tooltip != null && _tooltip.IsOpen) {
			_tooltip.IsOpen = false;
		}
		_tooltipTarget = null;
		if (_tooltipTimer != null) {
			_tooltipTimer.IsEnabled = false;
		}
	}

	private void ShowTooltip(object tooltipTarget, string text) {
		if (Equals(tooltipTarget, _tooltipTarget) && _tooltipText == text) {
			return;
		}

		HideTooltip();

		_tooltip ??= new ToolTip();
		if (_tooltipTimer == null) {
			_tooltipTimer = new DispatcherTimer(DispatcherPriority.Normal) {
				Interval = TimeSpan.FromSeconds(0.5)
			};
			_tooltipTimer.Tick += _tooltipTimer_Tick;
		}

		_tooltipText = text;
		_tooltipTarget = tooltipTarget;
		_tooltip.Content = text;
		_tooltipTimer.IsEnabled = true;
	}

	private void _tooltipTimer_Tick(object sender, EventArgs e) {
		_tooltip.IsOpen = true;
		_tooltipTimer.IsEnabled = false;
	}

	protected override void OnMouseLeave(MouseEventArgs e) {
		base.OnMouseLeave(e);
		using InvalidationContext ctx = CreateInvalidationContext();
		SetHoverRow(null);
		SetHoverColumn(null);
		SetHoverRowHeader(null);
		SetHoverColumnHeader(null);
	}

	public void HandleCommand(FastGridCellAddress address, object commandParameter) {
		bool handled = false;
		if (Model != null) {
			FastGridCellAddress addressModel = RealToModel(address);
			Model.HandleCommand(this, addressModel, commandParameter, ref handled);
		}
		if (handled) {
			return;
		}

		if (commandParameter == ToggleAllowFlexibleRowsCommand) {
			AllowFlexibleRows = !AllowFlexibleRows;
		}
		if (commandParameter == SelectAllCommand) {
			DoSelectAll();
		}
		if (commandParameter == AdjustColumnSizesCommand) {
			RecountColumnWidths();
			AdjustScrollbars();
			SetScrollbarMargin();
			FixScrollPosition();
			InvalidateAll();
		}
	}

	public void SelectAll() {
		HandleCommand(FastGridCellAddress.Empty, SelectAllCommand);
	}

	private void DoSelectAll() {

		Selection.SelectRect(new SelectionRect(
			new SimpleCellAddress(0, _rowSizes.FrozenCount),
			new SimpleCellAddress(_columnSizes.RealCount - 1, _rowSizes.RealCount - 1)
		));

		SetCurrentCell(new FastGridCellAddress(_rowSizes.FrozenCount, 0));
		InvalidateAll();

		OnChangeSelectedCells(true);
	}

	//public void SelectAll(int? rowCountLimit, int? columnCountLimit)
	//{
	//    var rows = _rowSizes.RealCount;
	//    var cols = _columnSizes.RealCount;
	//    if (rowCountLimit != null)
	//    {
	//        if (IsTransposed)
	//        {
	//            if (rowCountLimit.Value < cols) cols = rowCountLimit.Value;
	//        }
	//        else
	//        {
	//            if (rowCountLimit.Value < rows) rows = rowCountLimit.Value;
	//        }
	//    }
	//    if (columnCountLimit != null)
	//    {
	//        if (IsTransposed)
	//        {
	//            if (columnCountLimit.Value < rows) rows = columnCountLimit.Value;
	//        }
	//        else
	//        {
	//            if (columnCountLimit.Value < cols) cols = columnCountLimit.Value;
	//        }
	//    }
	//    SetSelectedRectangle(new FastGridCellAddress(0, 0), new FastGridCellAddress(rows - 1, cols - 1));
	//}

	private void imageMouseLeave(object sender, MouseEventArgs e) {
		HideTooltip();
	}

	private void OnScrolledModelRows() {
		ScrolledModelRows?.Invoke(this, EventArgs.Empty);
	}

	private void OnScrolledModelColumns() {
		ScrolledModelColumns?.Invoke(this, EventArgs.Empty);
	}

}
