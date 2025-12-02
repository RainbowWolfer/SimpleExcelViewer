using System;
using System.Windows.Media;

namespace FastWpfGrid;

public partial class FastGridControl {
	private Color _cellFontColor = Colors.Black;
	private Color _headerBackground = Color.FromRgb(246, 247, 249);
	private Color _headerCurrentBackground = Color.FromRgb(190, 207, 220);
	private Color _selectedColor = Color.FromRgb(51, 153, 255);
	private Color _selectedTextColor = Colors.White;
	private Color _limitedSelectedColor = Color.FromRgb(51, 220, 220);
	private Color _limitedSelectedTextColor = Colors.White;
	private Color _mouseOverRowColor = Color.FromRgb(235, 235, 255); // Colors.LemonChiffon; // Colors .Beige;
	private string _cellFontName = "Arial";
	private int _cellFontSize;
	private Color _gridLineColor = Colors.LightGray;
	private int _cellPaddingHorizontal = 2;
	private int _cellPaddingVertical = 1;
	private int _blockPadding = 2;
	private int? _minColumnWidthOverride;

	private Color[] _alternatingColors =
	[
		Colors.White,
		//Colors.White,
		//Color.FromRgb(235, 235, 235),
		//Colors.White,
		//Colors.White,
		//Color.FromRgb(235, 245, 255)
	];

	private Color _activeRegionFrameColor = Color.FromRgb(0xAA, 0xAA, 0xFF);
	private Color _activeRegionHoverFillColor = Color.FromRgb(0xAA, 0xFF, 0xFF);

	public int ColumnResizeThreshold { get; set; } = 2;

	public string CellFontName {
		get => _cellFontName;
		set {
			_cellFontName = value;
			RecalculateDefaultCellSize();
			RenderChanged();
		}
	}

	public int? MinColumnWidthOverride {
		get => _minColumnWidthOverride;
		set {
			_minColumnWidthOverride = value;
			RecalculateDefaultCellSize();
			InvalidateAll();
		}
	}

	public int MinColumnWidth => _minColumnWidthOverride ?? _columnSizes.DefaultSize;

	public int CellFontSize {
		get => _cellFontSize;
		set {
			_cellFontSize = value;
			RecalculateDefaultCellSize();
			InvalidateAll();
		}
	}

	public int RowHeightReserve {
		get => _rowHeightReserve;
		set {
			_rowHeightReserve = value;
			RecalculateDefaultCellSize();
			RenderGrid();
		}
	}

	public Color CellFontColor {
		get => _cellFontColor;
		set {
			_cellFontColor = value;
			RenderGrid();
		}
	}

	public Color SelectedColor {
		get => _selectedColor;
		set {
			_selectedColor = value;
			RenderGrid();
		}
	}

	public Color SelectedTextColor {
		get => _selectedTextColor;
		set {
			_selectedTextColor = value;
			RenderGrid();
		}
	}

	public Color LimitedSelectedColor {
		get => _limitedSelectedColor;
		set {
			_limitedSelectedColor = value;
			RenderGrid();
		}
	}

	public Color LimitedSelectedTextColor {
		get => _limitedSelectedTextColor;
		set {
			_limitedSelectedTextColor = value;
			RenderGrid();
		}
	}

	public Color MouseOverRowColor {
		get => _mouseOverRowColor; set => _mouseOverRowColor = value;
	}

	public Color GridLineColor {
		get => _gridLineColor;
		set {
			_gridLineColor = value;
			RenderChanged();
		}
	}

	public Color[] AlternatingColors {
		get => _alternatingColors;
		set {
			if (value.Length < 1) {
				throw new Exception("Invalid value");
			}

			_alternatingColors = value;
			RenderChanged();
		}
	}

	public int CellPaddingHorizontal {
		get => _cellPaddingHorizontal;
		set {
			_cellPaddingHorizontal = value;
			RenderChanged();
		}
	}

	public int CellPaddingVertical {
		get => _cellPaddingVertical;
		set {
			_cellPaddingVertical = value;
			RenderChanged();
		}
	}

	public int BlockPadding {
		get => _blockPadding;
		set {
			_blockPadding = value;
			RenderChanged();
		}
	}

	public Color HeaderBackground {
		get => _headerBackground;
		set {
			_headerBackground = value;
			RenderChanged();
		}
	}

	public Color HeaderCurrentBackground {
		get => _headerCurrentBackground;
		set {
			_headerCurrentBackground = value;
			RenderChanged();
		}
	}

	public Color ActiveRegionFrameColor {
		get => _activeRegionFrameColor;
		set {
			_activeRegionFrameColor = value;
			RenderChanged();
		}
	}

	public Color ActiveRegionHoverFillColor {
		get => _activeRegionHoverFillColor;
		set => _activeRegionHoverFillColor = value;
	}

	public int WideColumnsLimit { get; set; } = 250;
}
