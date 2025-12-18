using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FastWpfGrid;

public partial class FastGridControl {
	private void RenderGrid() {
		//Cursor = Cursors.Wait;
		if (_drawBuffer == null) {
			ClearInvalidation();
			return;
		}
		using (_drawBuffer.GetBitmapContext()) {
			int colsToRender = VisibleColumnCount;
			int rowsToRender = VisibleRowCount;

			if (_invalidatedCells.Count > 250 ||
				Math.Abs(Hscroll.Value - Hscroll.Maximum) < 0.01 ||
				Math.Abs(Vscroll.Value - Vscroll.Maximum) < 0.01) {
				_isInvalidatedAll = true;
			}

			if (!_isInvalidated || _isInvalidatedAll) {
				_drawBuffer.Clear(BackgroundColor);
			}

			if (ShouldDrawGridHeader()) {
				RenderGridHeader();
			}

			// render frozen rows
			for (int row = 0; row < _rowSizes.FrozenCount; row++) {
				for (int col = FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount;
					col < FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount + colsToRender;
					col++
				) {
					//if (!ShouldDrawCell(row, col)) {
					//	continue;
					//}

					RenderCell(row, col);
				}
			}

			// render frozen columns
			for (int row = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount;
				row < FirstVisibleRowScrollIndex + _rowSizes.FrozenCount + rowsToRender;
				row++
			) {
				for (int col = 0; col < _columnSizes.FrozenCount; col++) {
					if (!ShouldDrawCell(row, col)) {
						continue;
					}

					RenderCell(row, col);
				}
			}

			// render cells
			for (int row = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount; row < FirstVisibleRowScrollIndex + _rowSizes.FrozenCount + rowsToRender; row++) {
				for (int col = FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount; col < FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount + colsToRender; col++) {
					if (row < 0 || col < 0 || row >= _realRowCount || col >= _realColumnCount) {
						continue;
					}

					if (!ShouldDrawCell(row, col)) {
						continue;
					}

					RenderCell(row, col);
				}
			}

			// render frozen row headers
			for (int row = 0; row < _rowSizes.FrozenCount; row++) {
				if (!ShouldDrawRowHeader(row)) {
					continue;
				}

				RenderRowHeader(row);
			}

			// render row headers
			for (int row = FirstVisibleRowScrollIndex + _rowSizes.FrozenCount; row < FirstVisibleRowScrollIndex + _rowSizes.FrozenCount + rowsToRender; row++) {
				if (row < 0 || row >= _realRowCount) {
					continue;
				}

				if (!ShouldDrawRowHeader(row)) {
					continue;
				}

				RenderRowHeader(row);
			}

			// render frozen column headers
			for (int col = 0; col < _columnSizes.FrozenCount; col++) {
				if (!ShouldDrawColumnHeader(col)) {
					continue;
				}

				RenderColumnHeader(col);
			}


			// render column headers
			for (int col = FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount; col < FirstVisibleColumnScrollIndex + _columnSizes.FrozenCount + colsToRender; col++) {
				if (col < 0 || col >= _realColumnCount) {
					continue;
				}

				if (!ShouldDrawColumnHeader(col)) {
					continue;
				}

				RenderColumnHeader(col);
			}
		}
		if (_isInvalidatedAll) {
			//Debug.WriteLine("Render full grid: {0} ms", Math.Round((DateTime.Now - start).TotalMilliseconds));
		}
		ClearInvalidation();
		//Cursor = null;


		FinishInvalidating?.Invoke();
		Model?.OnInvalidatingFinished();


	}

	private void RenderGridHeader() {
		if (Model == null) {
			return;
		}

		IFastGridCell cell = Model.GetGridHeader(this);
		IntRect rect = GetGridHeaderRect();
		RenderCell(
			cell: cell,
			rect: rect,
			selectedTextColor: null,
			borderColor: null,
			bgColor: HeaderBackground,
			cellAddr: FastGridCellAddress.GridHeader,
			alignment: cell.Alignment
		);
	}

	private void RenderColumnHeader(int col) {
		IFastGridCell cell = GetColumnHeader(col);

		Color? selectedBgColor = null;
		if (col == CurrentCell.Column) {
			selectedBgColor = HeaderCurrentBackground;
		}

		IntRect rect = GetColumnHeaderRect(col);
		Color? cellBackground = null;
		if (cell != null) {
			cellBackground = cell.BackgroundColor;
		}

		Color? hoverColor = null;
		if (col == _mouseOverColumnHeader) {
			hoverColor = MouseOverRowColor;
		}

		RenderCell(
			cell: cell,
			rect: rect,
			selectedTextColor: null,
			borderColor: null,
			bgColor: hoverColor ?? selectedBgColor ?? cellBackground ?? HeaderBackground,
			cellAddr: new FastGridCellAddress(null, col),
			alignment: cell.Alignment
		);
	}

	private void RenderRowHeader(int row) {
		IFastGridCell cell = GetRowHeader(row);

		Color? selectedBgColor = null;
		if (row == CurrentCell.Row) {
			selectedBgColor = HeaderCurrentBackground;
		}

		IntRect rect = GetRowHeaderRect(row);
		Color? cellBackground = null;
		if (cell != null) {
			cellBackground = cell.BackgroundColor;
		}

		Color? hoverColor = null;
		if (row == _mouseOverRowHeader) {
			hoverColor = MouseOverRowColor;
		}

		RenderCell(
			cell: cell,
			rect: rect,
			selectedTextColor: null,
			borderColor: null,
			bgColor: hoverColor ?? selectedBgColor ?? cellBackground ?? HeaderBackground,
			cellAddr: new FastGridCellAddress(row, null),
			alignment: cell.Alignment
		);
	}

	private void RenderCell(int row, int col) {
		IntRect rect = GetCellRect(row, col);
		IFastGridCell cell = GetCell(row, col);
		if (cell == null) {
			return;
		}
		Color? selectedBgColor = null;
		Color? selectedTextColor = null;
		Color? hoverRowColor = null;
		Color? borderColor = null;
		if (CurrentCell.TestCell(row, col)/* || _selectedCells.Contains(new FastGridCellAddress(row, col))*/
			|| Selection.Contains(col, row)) {
			selectedBgColor = SelectedColor;
			selectedTextColor = SelectedTextColor;
			borderColor = Colors.Red;
		}
		if (EnableMouseHoverCellBackground) {
			if (row == _mouseOverRow) {
				hoverRowColor = MouseOverRowColor;
			}

			if (col == _mouseOverColumn) {
				hoverRowColor = MouseOverRowColor;
			}
		}

		Color? cellBackground = null;
		if (cell != null) {
			cellBackground = cell.BackgroundColor;
		}

		RenderCell(cell,
			rect: rect,
			selectedTextColor: selectedTextColor,
			borderColor: borderColor,
			bgColor: selectedBgColor ?? hoverRowColor ?? cellBackground ?? GetAlternateBackground(row),
			cellAddr: new FastGridCellAddress(row, col),
			alignment: cell.Alignment
		);
	}

	private int GetCellContentHeight(IFastGridCell cell) {
		if (cell == null) {
			return 0;
		}

		GlyphFont font = GetFont(false, false);
		int res = font.TextHeight;
		for (int i = 0; i < cell.BlockCount; i++) {
			IFastGridCellBlock block = cell.GetBlock(i);
			if (block.BlockType != FastGridBlockType.Text) {
				continue;
			}

			string text = block.TextData;
			if (text == null) {
				continue;
			}

			int hi = font.GetTextHeight(text);
			if (hi > res) {
				res = hi;
			}
		}
		return res;
	}

	private int GetCellContentWidth(IFastGridCell cell, int? maxSize = null) {
		if (cell == null) {
			return 0;
		}

		int count = cell.BlockCount;

		int width = 0;
		width += AutoSizeExtraPadding;
		for (int i = 0; i < count; i++) {
			IFastGridCellBlock block = cell.GetBlock(i);
			if (block == null) {
				continue;
			}

			if (i > 0) {
				width += BlockPadding;
			}

			switch (block.BlockType) {
				case FastGridBlockType.Text:
					string text = block.TextData;
					GlyphFont font = GetFont(block.IsBold, block.IsItalic);
					width += font.GetTextWidth(text, maxSize);
					break;
				case FastGridBlockType.Image:
					width += block.ImageWidth;
					break;
			}

		}
		return width;
	}

	private int RenderBlock(int index, int leftPos, int rightPos, Color? selectedTextColor, Color bgColor, IntRect rectContent, IFastGridCellBlock block, FastGridCellAddress cellAddr, RenderTextAlignment alignment, bool isHoverCell) {
		bool renderBlock = true;
		if (block.MouseHoverBehaviour == MouseHoverBehaviours.HideWhenMouseOut && !isHoverCell) {
			renderBlock = false;
		}

		int width = 0, top = 0, height = 0;

		switch (block.BlockType) {
			case FastGridBlockType.Text:
				GlyphFont font = GetFont(block.IsBold, block.IsItalic);
				int textHeight = font.GetTextHeight(block.TextData);
				width = font.GetTextWidth(block.TextData, _columnSizes.MaxSize);
				height = textHeight;
				top = rectContent.Top + (int)Math.Round((rectContent.Height / 2.0) - (textHeight / 2.0));
				break;
			case FastGridBlockType.Image:
				top = rectContent.Top + (int)Math.Round((rectContent.Height / 2.0) - (block.ImageHeight / 2.0));
				height = block.ImageHeight;
				width = block.ImageWidth;
				break;
		}

		int CalculateStartPos(int _w) {
			return (alignment switch {
				RenderTextAlignment.Left => leftPos,
				RenderTextAlignment.Center => leftPos + ((rightPos - leftPos - _w) / 2),
				RenderTextAlignment.Right => rightPos - _w,
				_ => throw new NotImplementedException(alignment.ToString()),
			}) + block.OffsetX;
		}

		if (renderBlock && block.CommandParameter != null) {
			IntRect activeRect = new IntRect(new IntPoint(CalculateStartPos(width), top), new IntSize(width, height)).GrowSymmetrical(1, 1);
			ActiveRegion region = new() {
				CommandParameter = block.CommandParameter,
				Rect = activeRect,
				Tooltip = block.ToolTip,
			};
			CurrentCellActiveRegions.Add(region);
			if (_mouseCursorPoint.HasValue && activeRect.Contains(_mouseCursorPoint.Value)) {
				_drawBuffer.FillRectangle(activeRect, ActiveRegionHoverFillColor);
				CurrentHoverRegion = region;
			}

			bool renderRectangle = true;
			if (block.MouseHoverBehaviour == MouseHoverBehaviours.HideButtonWhenMouseOut && !isHoverCell) {
				renderRectangle = false;
			}

			if (renderRectangle) {
				_drawBuffer.DrawRectangle(activeRect, ActiveRegionFrameColor);
			}
		}

		switch (block.BlockType) {
			case FastGridBlockType.Text:
				if (renderBlock) {
					string[] splits = block.TextData?.Split('\n') ?? Array.Empty<string>();
					int _top = top;
					foreach (string item in splits) {
						GlyphFont font = GetFont(block.IsBold, block.IsItalic);
						width = font.GetTextWidth(item, _columnSizes.MaxSize);
						IntPoint textOrigin = new(CalculateStartPos(width), _top);
						_drawBuffer.DrawString(
							x0: textOrigin.X,
							y0: textOrigin.Y,
							cliprect: rectContent,
							fontColor: selectedTextColor ?? block.FontColor ?? CellFontColor,
							bgColor: UseClearType ? bgColor : null,
							font: font,
							text: item
						);
						_top += 15;
					}
				}
				break;
			case FastGridBlockType.Image:
				if (renderBlock) {
					IntPoint imgOrigin = new(CalculateStartPos(block.ImageWidth), top);
					ImageHolder image = GetImage(block.ImageSource, block.ImageWidth, block.ImageHeight);
					_drawBuffer.Blit(
						destPosition: new Point(imgOrigin.X, imgOrigin.Y),
						source: image.Bitmap,
						sourceRect: new Rect(0, 0, block.ImageWidth, block.ImageHeight),
						color: image.KeyColor,
						blendMode: image.BlendMode
					);
				}
				break;
		}

		return width;
	}

	private void RenderCell(IFastGridCell cell, IntRect rect, Color? selectedTextColor, Color? borderColor, Color bgColor, FastGridCellAddress cellAddr, RenderTextAlignment alignment) {
		bool isHoverCell = !cellAddr.IsEmpty && cellAddr == _mouseOverCell;

		if (isHoverCell) {
			_mouseOverCellIsTrimmed = false;
			CurrentCellActiveRegions.Clear();
			CurrentHoverRegion = null;
		}

		if (cell == null) {
			return;
		}

		IntRect rectContent = GetContentRect(rect);
		_drawBuffer.DrawRectangle(rect, GridLineColor);
		_drawBuffer.FillRectangle(rect.GrowSymmetrical(-1, -1), bgColor);
		//if (borderColor != null) {
		//	//
		//	_drawBuffer.DrawRectangle(new IntRect(
		//		new IntPoint(rect.Left + 0, rect.Top + 0),
		//		new IntSize(rect.Width - 2, rect.Height - 2)
		//	), borderColor.Value);
		//}

		int count = cell.BlockCount;
		int rightCount = cell.RightAlignBlockCount;
		int leftCount = count - rightCount;
		int leftPos = rectContent.Left;
		int rightPos = rectContent.Right;

		for (int i = count - 1; i >= count - rightCount; i--) {
			IFastGridCellBlock block = cell.GetBlock(i);
			if (block == null) {
				continue;
			}

			if (i < count - 1) {
				rightPos -= BlockPadding;
			}

			int blockWi = RenderBlock(i, leftPos, rightPos, selectedTextColor, bgColor, rectContent, block, cellAddr, alignment, isHoverCell);
			rightPos -= blockWi;
		}

		int startLeftPos = leftPos;

		for (int i = 0; i < leftCount && leftPos < rightPos; i++) {
			IFastGridCellBlock block = cell.GetBlock(i);
			if (block == null) {
				continue;
			}

			if (i > 0) {
				leftPos += BlockPadding;
			}

			int blockWidth;
			if (block.BlockType == FastGridBlockType.Image) {
				blockWidth = RenderBlock(i, startLeftPos, rightPos, selectedTextColor, bgColor, rectContent, block, cellAddr, block.Alignment, isHoverCell);
				startLeftPos += blockWidth;
			} else {
				blockWidth = RenderBlock(i, startLeftPos, rightPos, selectedTextColor, bgColor, rectContent, block, cellAddr, alignment, isHoverCell);
			}

			leftPos += blockWidth;
		}
		switch (cell.Decoration) {
			case CellDecoration.StrikeOutHorizontal:
				_drawBuffer.DrawLine(rect.Left, rect.Top + (rect.Height / 2), rect.Right, rect.Top + (rect.Height / 2), cell.DecorationColor ?? Colors.Black);
				break;
		}
		if (isHoverCell) {
			_mouseOverCellIsTrimmed = leftPos > rightPos;
		}
	}

	public static ImageHolder GetImage(string source, int imageWidth, int imageHeight) {
		lock (_imageCache) {
			if (_imageCache.TryGetValue(source, out ImageHolder value)) {
				return value;
			}
		}
		string packUri;
		if (source.StartsWith("raw::")) {
			packUri = source.Substring(5);
		} else {
			packUri = "pack://application:,,,/" + Assembly.GetEntryAssembly().GetName().Name + ";component/" + source.TrimStart('/');
		}
		BitmapImage bmImage = new();
		bmImage.BeginInit();
		bmImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
		bmImage.UriSource = new Uri(packUri, UriKind.Absolute);
		bmImage.DecodePixelWidth = imageWidth;
		bmImage.DecodePixelHeight = imageHeight;
		bmImage.EndInit();
		WriteableBitmap wbmp = new(bmImage);

		if (wbmp.Format != PixelFormats.Bgra32) {
			wbmp = new WriteableBitmap(new FormatConvertedBitmap(wbmp, PixelFormats.Bgra32, null, 0));
		}

		ImageHolder image = new(wbmp, bmImage);
		lock (_imageCache) {
			_imageCache[source] = image;
		}
		return image;
	}

	private void ScrollContent(int row, int column) {
		if (row == FirstVisibleRowScrollIndex && column == FirstVisibleColumnScrollIndex) {
			return;
		}

		if (row != FirstVisibleRowScrollIndex && !_isInvalidatedAll && column == FirstVisibleColumnScrollIndex
			&& Math.Abs(row - FirstVisibleRowScrollIndex) * 2 < VisibleRowCount) {
			using (InvalidationContext ctx = CreateInvalidationContext()) {
				int scrollY = _rowSizes.GetScroll(FirstVisibleRowScrollIndex, row);
				_rowSizes.InvalidateAfterScroll(FirstVisibleRowScrollIndex, row, InvalidateRow, GridScrollAreaHeight);
				FirstVisibleRowScrollIndex = row;

				_drawBuffer.ScrollY(scrollY, GetScrollRect());
				_drawBuffer.ScrollY(scrollY, GetRowHeadersScrollRect());
				if (_columnSizes.FrozenCount > 0) {
					_drawBuffer.ScrollY(scrollY, GetFrozenColumnsRect());
				}
			}
			// if row heights are changed, invalidate all
			if (CountVisibleRowHeights()) {
				InvalidateAll();
			}
			OnScrolledModelRows();

			return;
		}

		if (column != FirstVisibleColumnScrollIndex && !_isInvalidatedAll && row == FirstVisibleRowScrollIndex
			&& Math.Abs(column - FirstVisibleColumnScrollIndex) * 2 < VisibleColumnCount) {
			using (InvalidationContext ctx = CreateInvalidationContext()) {
				int scrollX = _columnSizes.GetScroll(FirstVisibleColumnScrollIndex, column);
				_columnSizes.InvalidateAfterScroll(FirstVisibleColumnScrollIndex, column, InvalidateColumn, GridScrollAreaWidth);
				FirstVisibleColumnScrollIndex = column;

				_drawBuffer.ScrollX(scrollX, GetScrollRect());
				_drawBuffer.ScrollX(scrollX, GetColumnHeadersScrollRect());
				if (_rowSizes.FrozenCount > 0) {
					_drawBuffer.ScrollX(scrollX, GetFrozenRowsRect());
				}
			}
			OnScrolledModelColumns();

			return;
		}

		bool changedRow = FirstVisibleRowScrollIndex != row;
		bool changedCol = FirstVisibleColumnScrollIndex != column;

		// render all
		using (InvalidationContext ctx = CreateInvalidationContext()) {
			FirstVisibleRowScrollIndex = row;
			FirstVisibleColumnScrollIndex = column;
			CountVisibleRowHeights();
			InvalidateAll();
		}

		if (changedRow) {
			OnScrolledModelRows();
		}
		if (changedCol) {
			OnScrolledModelColumns();
		}
	}
}

public enum RenderTextAlignment {
	Left, Center, Right
}
