using System.Windows.Media;

namespace FastWpfGrid;

public enum CellDecoration {
	None,
	StrikeOutHorizontal,
}

public enum TooltipVisibilityMode {
	Always,
	OnlyWhenTrimmed,
}

public interface IFastGridCell {
	Color? BackgroundColor { get; }

	int BlockCount { get; }
	int RightAlignBlockCount { get; }
	IFastGridCellBlock GetBlock(int blockIndex);
	CellDecoration Decoration { get; }
	Color? DecorationColor { get; }

	string GetEditText(int row, int column);

	RenderTextAlignment Alignment { get; }

	string ToolTipText { get; }
	TooltipVisibilityMode ToolTipVisibility { get; }
}
