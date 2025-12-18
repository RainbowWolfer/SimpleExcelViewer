using RW.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FastWpfGrid;

/// <summary>
/// Interaction logic for FastGridControl.xaml
/// </summary>
public partial class FastGridControl : UserControl, IFastGridView, IDisposable {
	public delegate void ExceptionHandler(FastGridControl sender, FastWpfGridExceptionType type, Exception exception);

	public event ExceptionHandler ExceptionHandling;

	//private double _lastvscroll;
	private IFastGridModel _model;

	private FastGridCellAddress _currentCell;

	//private int _headerHeight;
	//private int _headerWidth;
	private readonly Dictionary<Tuple<bool, bool>, GlyphFont> _glyphFonts = [];
	private readonly Dictionary<Color, Brush> _solidBrushes = [];
	private int _rowHeightReserve = 5;
	//private Color _headerBackground = Color.FromRgb(0xDD, 0xDD, 0xDD);
	private WriteableBitmap _drawBuffer;

	private bool hasDisposed = false;

	private static readonly Dictionary<string, ImageHolder> _imageCache = [];


	public FastGridModelBase ModelSource {
		get => (FastGridModelBase)GetValue(ModelSourceProperty);
		set => SetValue(ModelSourceProperty, value);
	}

	public static readonly DependencyProperty ModelSourceProperty = DependencyProperty.Register(
		nameof(ModelSource),
		typeof(FastGridModelBase),
		typeof(FastGridControl),
		new PropertyMetadata(null, OnModelSourceChanged)
	);

	public static void OnModelSourceChanged(object sender, DependencyPropertyChangedEventArgs e) {
		FastGridControl ct = (FastGridControl)sender;
		if (ct != null) {
			ct.Model = (FastGridModelBase)e.NewValue;
		}
	}



	public FastGridControl() {
		InitializeComponent();
		//gridCore.Grid = this;
		RecalculateDefaultCellSize();
		_dragTimer = new DispatcherTimer(DispatcherPriority.Normal) {
			IsEnabled = false,
			Interval = TimeSpan.FromSeconds(0.05),
		};
		_dragTimer.Tick += DragTimer_Tick;
		AllowSelectAll = true;
	}

	public bool AllowSelectAll { get; set; }

	internal GlyphFont GetFont(bool isBold, bool isItalic) {
		Tuple<bool, bool> key = Tuple.Create(isBold, isItalic);
		if (!_glyphFonts.ContainsKey(key)) {
			GlyphFont font = LetterGlyphTool.GetFont(new PortableFontDesc(CellFontName, CellFontSize, isBold, isItalic, UseClearType));
			_glyphFonts[key] = font;
		}
		return _glyphFonts[key];
	}

	public void ClearCaches() {
		_glyphFonts.Clear();
	}

	public int GetTextWidth(string text, bool isBold, bool isItalic) {
		return GetFont(isBold, isItalic).GetTextWidth(text);
		//double size = CellFontSize;
		//int totalWidth = 0;
		//var glyphTypeface = GetFont(isBold, isItalic);

		//for (int n = 0; n < text.Length; n++)
		//{
		//    ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
		//    double width = Math.Round(glyphTypeface.AdvanceWidths[glyphIndex] * size);
		//    totalWidth += (int)width;
		//}
		//return totalWidth;
	}

	private void RecalculateDefaultCellSize() {
		ClearCaches();
		int rowHeight = GetFont(false, false).TextHeight + (CellPaddingVertical * 2) + 2 + RowHeightReserve;
		int columnWidth = MinColumnWidthOverride ?? (rowHeight * 4);

		_rowSizes.DefaultSize = rowHeight;
		_columnSizes.DefaultSize = columnWidth;

		RecalculateHeaderHeight();

		InvalidateAll();
	}

	private void RecalculateHeaderHeight() {
		HeaderHeight = _rowSizes.DefaultSize;
	}

	private void RecalculateHeaderWidth() {
		HeaderWidth = GetTextWidth("0000000", false, false);

		if (Model != null) {
			int width = GetCellContentWidth(Model.GetGridHeader(this));
			if (width + (2 * CellPaddingHorizontal) > HeaderWidth) {
				HeaderWidth = width + (2 * CellPaddingHorizontal);
			}
		}
	}

	//public int RowHeight
	//{
	//    get { return _rowHeight; }
	//}

	//public int ColumnWidth
	//{
	//    get { return _columnWidth; }
	//}

	private void ScrollChanged() {
		//int rowIndex = _rowSizes.GetScrollIndexOnPosition((int) vscroll.Value);
		//int columnIndex = _columnSizes.GetScrollIndexOnPosition((int) hscroll.Value);

		int rowIndex = (int)Math.Round(Vscroll.Value);
		int columnIndex = (int)Math.Round(Hscroll.Value);

		//FirstVisibleRow = rowIndex;
		//FirstVisibleColumn = columnIndex;
		//RenderGrid();
		ScrollContent(rowIndex, columnIndex);
		AdjustInlineEditorPosition();

		//不需要SelectionMenu
		//AdjustSelectionMenuPosition();
	}


	public Color GetAlternateBackground(int row) {
		return _alternatingColors[row % _alternatingColors.Length];
	}

	private void hscroll_Scroll(object sender, ScrollEventArgs e) {
		ScrollChanged();
	}

	private void vscroll_Scroll(object sender, ScrollEventArgs e) {
		ScrollChanged();
	}

	private void OnModelPropertyChanged() {
		_model?.DetachView(this);

		_model = Model;

		_model?.AttachView(this);

		//SetVerticalScrollValue(0);
		//SetHorizontalScrollValue(0);

		NotifyRefresh();
	}


	public int GridScrollAreaWidth {
		get {
			if (_drawBuffer == null) {
				return 1;
			}

			return _drawBuffer.PixelWidth - HeaderWidth - FrozenWidth;
		}
	}

	public int GridScrollAreaHeight {
		get {
			if (_drawBuffer == null) {
				return 1;
			}

			return _drawBuffer.PixelHeight - HeaderHeight - FrozenHeight;
		}
	}

	//private void AdjustVerticalScrollBarRange()
	//{
	//    //vscroll.Maximum = _rowSizes.GetTotalScrollSizeSum() - GridScrollAreaHeight + _rowSizes.DefaultSize;

	//}

	private void AdjustScrollbars() {
		Hscroll.Minimum = 0;
		//hscroll.Maximum = _columnSizes.GetTotalScrollSizeSum() - GridScrollAreaWidth + _columnSizes.DefaultSize;

		// hscroll.Maximum = _columnSizes.ScrollCount - 1;

		Hscroll.Maximum = Math.Min(
			_columnSizes.ScrollCount - 1,
			_columnSizes.ScrollCount - _columnSizes.GetVisibleScrollCountReversed(_columnSizes.ScrollCount - 1, GridScrollAreaWidth) + 1);
		Hscroll.ViewportSize = VisibleColumnCount; //GridScrollAreaWidth;
		Hscroll.SmallChange = 1; // GridScrollAreaWidth / 10.0;
		Hscroll.LargeChange = 3; // GridScrollAreaWidth / 2.0;

		Hscroll.Visibility = VisibleColumnCount - _columnSizes.ScrollCount >= Hscroll.Maximum ? Visibility.Collapsed : Visibility.Visible;

		Vscroll.Minimum = 0;
		if (FlexibleRows) {
			Vscroll.Maximum = _rowSizes.ScrollCount - 1;
		} else {
			Vscroll.Maximum = _rowSizes.ScrollCount - (GridScrollAreaHeight / (_rowSizes.DefaultSize + 1)) + 1;
		}
		Vscroll.ViewportSize = VisibleRowCount;
		Vscroll.SmallChange = 1;
		Vscroll.LargeChange = 10;

		Vscroll.Visibility = VisibleRowCount - _rowSizes.ScrollCount >= Vscroll.Maximum ? Visibility.Collapsed : Visibility.Visible;

		if (Hscroll.Visibility == Visibility.Visible && Vscroll.Visibility == Visibility.Visible) {
			Grid.SetColumnSpan(ImageGrid, 1);
			Grid.SetRowSpan(ImageGrid, 1);
		} else if (Hscroll.Visibility == Visibility.Visible && Vscroll.Visibility == Visibility.Collapsed) {
			Grid.SetColumnSpan(ImageGrid, 2);
			Grid.SetRowSpan(ImageGrid, 1);
		} else if (Hscroll.Visibility == Visibility.Collapsed && Vscroll.Visibility == Visibility.Visible) {
			Grid.SetColumnSpan(ImageGrid, 1);
			Grid.SetRowSpan(ImageGrid, 2);
		} else {
			Grid.SetColumnSpan(ImageGrid, 2);
			Grid.SetRowSpan(ImageGrid, 2);
		}

	}

	private void AdjustScrollBarPositions() {
		//hscroll.Value = _columnSizes.GetPositionByScrollIndex(FirstVisibleColumnScrollIndex); //FirstVisibleColumn* ColumnWidth;
		//vscroll.Value = _rowSizes.GetPositionByScrollIndex(FirstVisibleRowScrollIndex);
		Hscroll.Value = FirstVisibleColumnScrollIndex;
		Vscroll.Value = FirstVisibleRowScrollIndex;
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
		base.OnRenderSizeChanged(sizeInfo);
		AdjustScrollbars();
	}

	public void NotifyRefresh(NotifyRefreshParameter parameter = null) {
		parameter ??= new NotifyRefreshParameter(recountColumnWidths: true);

		Cursor = Cursors.Wait;
		_modelRowCount = 0;
		_modelColumnCount = 0;
		int? columnHeight = null;
		if (_model != null) {
			_modelRowCount = _model.RowCount;
			_modelColumnCount = _model.ColumnCount;
			columnHeight = _model.GetDesiredColumnHeaderHeight();
		}

		UpdateSeriesCounts();
		if (HeaderHeight == 0) {
			RecalculateHeaderHeight();
		}
		if (HeaderWidth < MinColumnWidth) {
			RecalculateHeaderWidth();
		}

		if (columnHeight != null) {
			SetColumnHeaderHeight(columnHeight.Value);
		}
		FixCurrentCellAndSetSelectionToCurrentCell();

		if (parameter.RecountColumnWidths) {
			RecountColumnWidths();
		}
		RecountRowHeights();

		AdjustScrollbars();
		SetScrollbarMargin();
		FixScrollPosition();
		InvalidateAll();

		Cursor = null;
	}

	private void FixCurrentCellAndSetSelectionToCurrentCell() {
		int? col = CurrentCell.Column;
		int? row = CurrentCell.Row;

		if (col.HasValue) {
			if (col >= _modelColumnCount) {
				col = _modelColumnCount - 1;
			}

			if (col < 0) {
				col = null;
			}
		}

		if (row.HasValue) {
			if (row >= _modelRowCount) {
				row = _modelRowCount - 1;
			}

			if (row < 0) {
				row = null;
			}
		}

		ClearSelectedCells();
		_currentCell = new FastGridCellAddress(row, col);
		if (CurrentCell.IsCell) {
			//AddSelectedCell(_currentCell);
			Selection.SelectCell(new SimpleCellAddress(CurrentCell));
		}

		OnChangeSelectedCells(false);
	}

	public void NotifyAddedRows() {
		NotifyRefresh();
	}

	public Brush GetSolidBrush(Color color) {
		if (!_solidBrushes.ContainsKey(color)) {
			_solidBrushes[color] = new SolidColorBrush(color);
		}
		return _solidBrushes[color];
	}


	private IFastGridCell GetModelRowHeader(int row) {
		if (_model == null) {
			return null;
		}

		if (row < 0 || row >= _modelRowCount) {
			return null;
		}

		return _model.GetRowHeader(this, row);
	}

	private IFastGridCell GetModelColumnHeader(int col) {
		if (_model == null) {
			return null;
		}

		if (col < 0 || col >= _modelColumnCount) {
			return null;
		}

		return _model.GetColumnHeader(this, col);
	}

	private IFastGridCell GetModelCell(int row, int col) {
		if (_model == null) {
			return null;
		}

		if (row < 0 || row >= _modelRowCount) {
			return null;
		}

		if (col < 0 || col >= _modelColumnCount) {
			return null;
		}

		return _model.GetCell(this, row, col);
	}

	private IFastGridCell GetColumnHeader(int col) {
		return GetModelColumnHeader(_columnSizes.RealToModel(col));
	}

	private IFastGridCell GetRowHeader(int row) {
		return GetModelRowHeader(_rowSizes.RealToModel(row));
	}

	private IFastGridCell GetCell(int row, int col) {
		return GetModelCell(_rowSizes.RealToModel(row), _columnSizes.RealToModel(col));
	}

	private IFastGridCell GetCell(FastGridCellAddress addr) {
		if (addr.IsCell) {
			return GetCell(addr.Row.Value, addr.Column.Value);
		}

		if (addr.IsRowHeader) {
			return GetRowHeader(addr.Row.Value);
		}

		if (addr.IsColumnHeader) {
			return GetColumnHeader(addr.Column.Value);
		}

		if (addr.IsGridHeader && _model != null) {
			return _model.GetGridHeader(this);
		}

		return null;
	}

	private void InvalidateCurrentCell() {
		if (CurrentCell.IsCell) {
			InvalidateCell(CurrentCell);
		}

		if (CurrentCell.Column.HasValue) {
			InvalidateColumnHeader(CurrentCell.Column.Value);
		}

		if (CurrentCell.Row.HasValue) {
			InvalidateRowHeader(CurrentCell.Row.Value);
		}
	}

	private void SetCurrentCell(FastGridCellAddress cell) {
		if (cell.IsRowHeader && CurrentCell.IsCell) {
			cell = new FastGridCellAddress(cell.Row, CurrentCell.Column);
		}

		if (cell.IsColumnHeader && CurrentCell.IsCell) {
			cell = new FastGridCellAddress(CurrentCell.Row, cell.Column);
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		InvalidateCurrentCell();
		_currentCell = cell;
		InvalidateCurrentCell();
	}

	private SelectionRect GetCellRectRows(FastGridCellAddress from, FastGridCellAddress to) {
		if (from.Row == null || to.Row == null) {
			return null;
		}
		bool isFromInFrozen = _rowSizes.IsIndexFrozen(from.Row.Value);
		bool isToInFrozen = _rowSizes.IsIndexFrozen(to.Row.Value);

		int minRow = Math.Min(from.Row.Value, to.Row.Value);
		int maxRow = Math.Max(from.Row.Value, to.Row.Value);

		if (Model != null) {
			if (maxRow >= Model.RowCount) {
				maxRow = Model.RowCount - 1;
			}
		}

		if (isFromInFrozen && !isToInFrozen) {
			maxRow = _rowSizes.FrozenCount - 1;
		} else if (!isFromInFrozen && isToInFrozen) {
			minRow = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount;
		}

		return new SelectionRect(
			new SimpleCellAddress(0, minRow),
			new SimpleCellAddress(_columnSizes.RealCount - 1, maxRow)
		);
	}

	private SelectionRect GetCellRectColumns(FastGridCellAddress a, FastGridCellAddress b) {
		if (a.Column == null || b.Column == null) {
			return null;
		}

		int minCol = Math.Min(a.Column.Value, b.Column.Value);
		int maxCol = Math.Max(a.Column.Value, b.Column.Value);


		if (Model != null) {
			if (maxCol >= Model.ColumnCount) {
				maxCol = Model.ColumnCount - 1;
			}
		}

		return new SelectionRect(
			new SimpleCellAddress(minCol, 0),
			new SimpleCellAddress(maxCol, _rowSizes.RealCount - 1)
		);
	}

	private SelectionRect GetCellRect(FastGridCellAddress a, FastGridCellAddress b) {

		int minRow;
		int maxRow;
		int minCol;
		int maxCol;

		if (a.IsRowHeader) {
			minCol = 0;
			maxCol = _columnSizes.RealCount - 1;
		} else {
			if (a.Column == null || b.Column == null) {
				return null;
			}

			minCol = Math.Min(a.Column.Value, b.Column.Value);
			maxCol = Math.Max(a.Column.Value, b.Column.Value);
		}

		if (a.IsColumnHeader) {
			minRow = 0;
			maxRow = _rowSizes.RealCount - 1;
		} else {
			if (a.Row == null || b.Row == null) {
				return null;
			}

			minRow = Math.Min(a.Row.Value, b.Row.Value);
			maxRow = Math.Max(a.Row.Value, b.Row.Value);
		}

		if (Model != null) {
			if (maxRow >= Model.RowCount) {
				maxRow = Model.RowCount - 1;
			}
			if (maxCol >= Model.ColumnCount) {
				maxCol = Model.ColumnCount - 1;
			}
		}

		return new SelectionRect(
			rectFrom: new SimpleCellAddress(minCol, minRow),
			rectTo: new SimpleCellAddress(maxCol, maxRow)
		);
	}

	///// returns cell range. 
	///// if a is row header, returns full rows.
	///// if a is column header, returns full columns
	//private HashSet<FastGridCellAddress> GetCellRange(FastGridCellAddress a, FastGridCellAddress b) {
	//	HashSet<FastGridCellAddress> res = new HashSet<FastGridCellAddress>();

	//	int minRow;
	//	int maxRow;
	//	int minCol;
	//	int maxCol;

	//	if (a.IsRowHeader) {
	//		minCol = 0;
	//		maxCol = _columnSizes.RealCount - 1;
	//	} else {
	//		if (a.Column == null || b.Column == null) {
	//			return res;
	//		}

	//		minCol = Math.Min(a.Column.Value, b.Column.Value);
	//		maxCol = Math.Max(a.Column.Value, b.Column.Value);
	//	}

	//	if (a.IsColumnHeader) {
	//		minRow = 0;
	//		maxRow = _rowSizes.RealCount;
	//	} else {
	//		if (a.Row == null || b.Row == null) {
	//			return res;
	//		}

	//		minRow = Math.Min(a.Row.Value, b.Row.Value);
	//		maxRow = Math.Max(a.Row.Value, b.Row.Value);
	//	}

	//	if (Model != null) {
	//		if (maxRow >= Model.RowCount) {
	//			maxRow = Model.RowCount - 1;
	//		}
	//		if (maxCol >= Model.ColumnCount) {
	//			maxCol = Model.ColumnCount - 1;
	//		}
	//	}

	//	for (int row = minRow; row <= maxRow; row++) {
	//		for (int col = minCol; col <= maxCol; col++) {
	//			res.Add(new FastGridCellAddress(row, col));
	//		}
	//	}
	//	return res;
	//}


	private void SetHoverRow(int? row) {
		if (row == _mouseOverRow) {
			return;
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		if (_mouseOverRow.HasValue) {
			InvalidateRow(_mouseOverRow.Value);
		}

		_mouseOverRow = row;
		if (_mouseOverRow.HasValue) {
			InvalidateRow(_mouseOverRow.Value);
		}
	}

	private void SetHoverColumn(int? col) {
		if (col == _mouseOverColumn) {
			return;
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		if (_mouseOverColumn.HasValue) {
			InvalidateColumn(_mouseOverColumn.Value);
		}

		_mouseOverColumn = col;
		if (_mouseOverColumn.HasValue) {
			InvalidateColumn(_mouseOverColumn.Value);
		}
	}

	private void SetHoverRowHeader(int? row) {
		if (row == _mouseOverRowHeader) {
			return;
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		if (_mouseOverRowHeader.HasValue) {
			InvalidateRowHeader(_mouseOverRowHeader.Value);
		}

		_mouseOverRowHeader = row;
		if (_mouseOverRowHeader.HasValue) {
			InvalidateRow(_mouseOverRowHeader.Value);
		}
	}

	private void SetHoverColumnHeader(int? column) {
		if (column == _mouseOverColumnHeader) {
			return;
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		if (_mouseOverColumnHeader.HasValue) {
			InvalidateColumnHeader(_mouseOverColumnHeader.Value);
		}

		_mouseOverColumnHeader = column;
		if (_mouseOverColumnHeader.HasValue) {
			InvalidateColumn(_mouseOverColumnHeader.Value);
		}
	}

	private void SetHoverCell(FastGridCellAddress cell) {
		if (cell == _mouseOverCell) {
			return;
		}

		using InvalidationContext ctx = CreateInvalidationContext();
		if (!_mouseOverCell.IsEmpty) {
			InvalidateCell(_mouseOverCell);
		}

		_mouseOverCell = cell.IsEmpty ? FastGridCellAddress.Empty : cell;
		if (!_mouseOverCell.IsEmpty) {
			InvalidateCell(_mouseOverCell);
		}
	}

	private void imageGridResized(object sender, SizeChangedEventArgs e) {
		bool wasEmpty = _drawBuffer == null;
		int width = (int)ImageGrid.ActualWidth - 2;
		int height = (int)ImageGrid.ActualHeight - 2;
		if (width > 0 && height > 0) {
			//To avoid flicker (blank image) while resizing, crop the current buffer and set it as the image source instead of using a new one.
			//This will be shown during the refresh.
			int pixelWidth = (int)Math.Ceiling(width * DpiDetector.DpiXKoef);
			int pixelHeight = (int)Math.Ceiling(height * DpiDetector.DpiYKoef);
			if (_drawBuffer == null) {
				_drawBuffer = BitmapFactory.New(pixelWidth, pixelHeight);
			} else {
				WriteableBitmap oldBuffer = _drawBuffer;
				_drawBuffer = oldBuffer.Crop(0, 0, pixelWidth, pixelHeight);

				//The unmanaged memory when crating new WritableBitmaps doesn't reliably garbage collect and can still cause out of memory exceptions
				//Profiling revealed handles on the object that aren't able to be collected.
				//Freezing the object removes all handles and should help in garbage collection.
				oldBuffer.Freeze();
			}
		} else {
			_drawBuffer = null;
		}
		Image.Source = _drawBuffer;
		Image.Margin = new Thickness(0);
		Image.Width = Math.Max(0, width);
		Image.Height = Math.Max(0, height);

		//var screenPos = imageGrid.PointToScreen(new Point(0, 0));
		//double fracX = screenPos.X - Math.Truncate(screenPos.X);
		//double fracY = screenPos.Y - Math.Truncate(screenPos.Y);
		//double dleft = 1 - fracX;
		//double dtop = 1 - fracY;
		//if (fracX == 0) dleft = 0;
		//if (fracY == 0) dtop = 0;
		//image.Margin = new Thickness(dleft, dtop, imageGrid.ActualWidth - width - dleft - 1, imageGrid.ActualHeight - height - dtop - 25);

		if (wasEmpty && _drawBuffer != null) {
			RecountColumnWidths();
			RecountRowHeights();
		}
		AdjustScrollbars();
		InvalidateAll();

		AdjustInlineEditorPosition();
	}

	public bool MoveCurrentCell(int? row, int? col, KeyEventArgs e = null) {
		if (e != null) {
			e.Handled = true;
		}

		//if (!ShiftPressed) {
		//_selectedCells.ToList().ForEach(InvalidateCell);
		InvalidateAll();
		ClearSelectedCells();
		//}

		InvalidateCurrentCell();

		if (row < 0) {
			row = 0;
		}

		if (row >= _realRowCount) {
			row = _realRowCount - 1;
		}

		if (col < 0) {
			col = 0;
		}

		if (col >= _realColumnCount) {
			col = _realColumnCount - 1;
		}

		_currentCell = new FastGridCellAddress(row, col);
		if (CurrentCell.IsCell) {
			//AddSelectedCell(_currentCell);
			Selection.SelectCell(new SimpleCellAddress(CurrentCell));
		}

		InvalidateCurrentCell();
		ScrollCurrentCellIntoView();
		OnChangeSelectedCells(true);
		return true;
	}

	private void RenderChanged() {
		InvalidateAll();
	}

	private void OnUseClearTypePropertyChanged() {
		ClearCaches();
		RecalculateDefaultCellSize();
		RenderChanged();
	}

	internal bool IsModelCellInValidRange(FastGridCellAddress cell) {
		if (cell.Row.HasValue && (cell.Row.Value < 0 || cell.Row.Value >= _modelRowCount)) {
			return false;
		}

		if (cell.Column.HasValue && (cell.Column.Value < 0 || cell.Column.Value >= _modelColumnCount)) {
			return false;
		}

		return true;
	}

	internal bool IsColumnInValidRange(int column) {
		return column >= 0 && column < _modelColumnCount;
	}

	internal bool IsModelCellInValidRange(SimpleCellAddress cell) {
		if (cell.Row < 0 || cell.Row >= _modelRowCount) {
			return false;
		}

		if (cell.Column < 0 || cell.Column >= _modelColumnCount) {
			return false;
		}

		return true;
	}

	//public SelectionRect GetSelectionRect() {
	//	SelectionRect rect = Selection.Rect?.Copy();
	//	if (rect == null) {
	//		return null;
	//	}
	//	if (rect.RectFrom.Column >= _modelColumnCount) {
	//		rect.RectFrom = new SimpleCellAddress(_modelColumnCount - 1, rect.RectFrom.Row);
	//	}
	//	if (rect.RectTo.Column >= _modelColumnCount) {
	//		rect.RectTo = new SimpleCellAddress(_modelColumnCount - 1, rect.RectTo.Row);
	//	}
	//	if (rect.RectFrom.Row >= _modelRowCount) {
	//		rect.RectFrom = new SimpleCellAddress(rect.RectFrom.Column, _modelRowCount - 1);
	//	}
	//	if (rect.RectTo.Row >= _modelRowCount) {
	//		rect.RectTo = new SimpleCellAddress(rect.RectTo.Column, _modelRowCount - 1);
	//	}

	//	return rect;
	//}

	public SelectionRect GetSelectionRect() {
		SelectionRect rect = Selection.Rect?.Copy();
		if (rect == null) {
			return null;
		}
		if (rect.RectFrom.Column >= _modelColumnCount) {
			return null;
		}
		if (rect.RectTo.Column >= _modelColumnCount) {
			return null;
		}
		if (rect.RectFrom.Row >= _modelRowCount) {
			return null;
		}
		if (rect.RectTo.Row >= _modelRowCount) {
			return null;
		}

		return rect;
	}

	//public HashSet<FastGridCellAddress> GetSelectedModelCells() {
	//	HashSet<FastGridCellAddress> res = new HashSet<FastGridCellAddress>();
	//	foreach (FastGridCellAddress cell in _selectedCells) {
	//		FastGridCellAddress cellModel = RealToModel(cell);
	//		if (cellModel.IsCell && IsModelCellInValidRange(cellModel)) {
	//			res.Add(cellModel);
	//		}
	//	}
	//	return res;
	//}

	public FastGridCellAddress CurrentModelCell {
		get => RealToModel(CurrentCell);
		set => CurrentCell = ModelToReal(value);
	}

	public void ShowSelectionMenu(IEnumerable<string> commands) {
		//if (commands == null) {
		//	MenuSelection.ItemsSource = null;
		//	MenuSelection.Visibility = Visibility.Hidden;
		//} else {
		//	MenuSelection.ItemsSource = commands.Select(x => new SelectionQuickCommand(Model, x)).ToList();
		//	MenuSelection.Visibility = Visibility.Visible;
		//	AdjustSelectionMenuPosition();
		//}
	}

	//private void Root_KeyDown(object sender, KeyEventArgs e) {
	//	if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
	//		CopyCurrentCells();
	//	} else if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
	//		PasteCurrentCells();
	//	}
	//}

	public int PositionToColumnIndex(Point point) {
		point.X *= DpiDetector.DpiXKoef;
		point.Y *= DpiDetector.DpiYKoef;
		FastGridCellAddress cell = GetCellAddress(point);
		return cell.Column == null ? -1 : cell.Column.Value;
	}

	private bool isCopyingString = false;

	public async void CopyCurrentCells() {
		if (isCopyingString) {
			return;
		}
		isCopyingString = true;
		try {
			//TODO : 数据太大全选，string[,]赋值会超内存
			SelectionRect selection = GetSelectionRect();
			if (selection == null) {
				return;
			}
			if (selection.GetCellsCount() > 1) {
				int minRow = selection.RectFrom.Row;
				int maxRow = selection.RectTo.Row;

				int selectionRowCount = maxRow - minRow + 1;
				int minCol = selection.RectFrom.Column;
				int maxCol = selection.RectTo.Column;

				int selectionColCount = maxCol - minCol + 1;

				int id1 = selectionRowCount + 1;
				int id2 = selectionColCount + 1;

				string[,] data = new string[id1, id2];

				StringBuilder sb = new();
				IFastGridModel model = Model;
				for (int c = minCol; c <= maxCol; c++) {
					data[0, c - minCol + 1] = _model.GetColumnHeader(this, c).GetBlock(0).TextData;
				}

				for (int r = minRow; r <= maxRow; r++) {
					data[r - minRow + 1, 0] = _model.GetRowHeader(this, r).GetBlock(0).TextData;
				}

				for (int c = minCol; c <= maxCol; c++) {
					for (int r = minRow; r <= maxRow; r++) {
						IFastGridCell da = model.GetCell(this, r, c);
						data[r - minRow + 1, c - minCol + 1] = da.GetBlock(0).TextData;
					}
				}

				int l0 = data.GetLength(0);
				int l1 = data.GetLength(1);

				for (int i = 0; i < l0; i++) {
					for (int j = 0; j < l1; j++) {
						if (j != 0) {
							sb.Append('\t');
						}

						sb.Append(data[i, j]);
					}

					sb.AppendLine();
				}

				Clipboard.SetText(sb.ToString());
			} else if (selection.GetCellsCount() == 1) {
				SimpleCellAddress first = selection.GetFirstCell();
				string text = Model.GetCell(this, first.Row, first.Column).GetBlock(0).TextData;
				if (text.IsNotBlank()) {
					Clipboard.SetText(text);
				}
			}
		} catch (Exception ex) {
			ExceptionHandling?.Invoke(this, FastWpfGridExceptionType.CopyToClipboard, ex);
		} finally {
			await Task.Delay(100);
			isCopyingString = false;
		}
	}

	public void AutoColumnWidth() {
		UpdateSeriesCounts();
		RecountColumnWidths();
		RecountRowHeights();
		AdjustScrollbars();
		AdjustScrollBarPositions();
		AdjustInlineEditorPosition();
		InvalidateAll();
	}

	public static int CalculateActualWidth(int width, Visual visual) {
		DpiScale dpiScale = VisualTreeHelper.GetDpi(visual);
		double scaleX = dpiScale.DpiScaleX; // The scaling factor for X (width)
		int actualWidth = (int)(width * scaleX);
		return actualWidth;
	}


	public static int CalculateActualHeight(int height, Visual visual) {
		DpiScale dpiScale = VisualTreeHelper.GetDpi(visual);
		double scaleX = dpiScale.DpiScaleY; // The scaling factor for X (width)
		int actualHeight = (int)(height * scaleX);
		return actualHeight;
	}

	public void SetColumnWidth(int width, params int[] columnIndexes) {
		width = CalculateActualWidth(width, this);
		if (columnIndexes == null || columnIndexes.Length == 0) {
			_columnSizes.ResizeNoBuild(width);
		} else {
			foreach (int index in columnIndexes) {
				_columnSizes.ResizeNoBuild(index, width);
			}
		}
		_columnSizes.BuildIndex();
		AdjustScrollbars();
		AdjustScrollBarPositions();
		AdjustInlineEditorPosition();
		InvalidateAll();
	}

	public int GetColumnWidth(int columnIndex) {
		return _columnSizes.GetSizeByModelIndex(columnIndex);
	}

	public void SetColumnHeaderHeight(int height) {
		HeaderHeight = height;
	}

	public void SetRowHeaderWidth(int width) {
		HeaderWidth = width;
	}

	protected virtual void Dispose(bool disposing) {
		if (hasDisposed) {
			return;
		}

		if (disposing) {
			// TODO: dispose managed state (managed objects)

			void Clear() {
				_drawBuffer?.Clear();

				if (_dragTimer != null) {
					_dragTimer.IsEnabled = false;
				}

				if (_tooltipTimer != null) {
					_tooltipTimer.IsEnabled = false;
				}
			}

			if (!CheckAccess()) {
				if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished) {
					Dispatcher.Invoke(() => {
						Clear();
					});
				}
			} else {
				Clear();
			}

		}

		_glyphFonts?.Clear();
		_solidBrushes?.Clear();
		_imageCache?.Clear();

		CurrentCellActiveRegions?.Clear();

		_rowSizes?.Dispose();
		_columnSizes?.Dispose();

		_invalidatedRows?.Clear();
		_invalidatedColumns?.Clear();
		_invalidatedCells?.Clear();
		_invalidatedRowHeaders?.Clear();
		_invalidatedColumnHeaders?.Clear();

		Selection.Clear();

		// TODO: free unmanaged resources (unmanaged objects) and override finalizer
		// TODO: set large fields to null
		hasDisposed = true;
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~FastGridControl() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}


public class NotifyRefreshParameter {
	public bool RecountColumnWidths { get; } = false;

	public NotifyRefreshParameter() { }

	public NotifyRefreshParameter(bool recountColumnWidths) {
		RecountColumnWidths = recountColumnWidths;
	}
}
