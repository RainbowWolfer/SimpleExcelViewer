namespace FastWpfGrid;

public struct FastGridCellAddress(int? row, int? col, bool isGridHeader = false) {
	public static readonly FastGridCellAddress Empty = new();
	public static readonly FastGridCellAddress GridHeader = new(null, null, true);

	public readonly int? Row = row;
	public readonly int? Column = col;
	public bool IsGridHeader = isGridHeader;

	public readonly bool Equals(FastGridCellAddress other) => Row == other.Row && Column == other.Column;

	public override readonly bool Equals(object obj) {
		if (obj is null) {
			return false;
		}

		return obj is FastGridCellAddress address && Equals(address);
	}

	public override readonly int GetHashCode() {
		unchecked {
			return (Row.GetHashCode() * 397) ^ Column.GetHashCode();
		}
	}

	public readonly FastGridCellAddress ChangeRow(int? row) {
		return new FastGridCellAddress(row, Column, IsGridHeader);
	}

	public readonly FastGridCellAddress ChangeColumn(int? col) {
		return new FastGridCellAddress(Row, col, IsGridHeader);
	}

	public readonly bool IsCell => Row.HasValue && Column.HasValue;

	public readonly bool IsRowHeader => Row.HasValue && !Column.HasValue;

	public readonly bool IsColumnHeader => Column.HasValue && !Row.HasValue;

	public readonly bool IsEmpty => Row == null && Column == null && !IsGridHeader;

	public readonly bool TestCell(int row, int col) {
		return row == Row && col == Column;
	}

	public static bool operator ==(FastGridCellAddress a, FastGridCellAddress b) {
		return a.Row == b.Row && a.Column == b.Column && a.IsGridHeader == b.IsGridHeader;
	}

	public static bool operator !=(FastGridCellAddress a, FastGridCellAddress b) {
		return !(a == b);
	}
}
