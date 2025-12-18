using System;

namespace FastWpfGrid;

[Serializable]
public readonly struct SimpleCellAddress : IEquatable<SimpleCellAddress>, ICloneable {
	public int Column { get; }
	public int Row { get; }

	public SimpleCellAddress(int column, int row) {
		Column = column;
		Row = row;
	}

	public SimpleCellAddress(FastGridCellAddress cell) {
		Column = cell.Column.Value;
		Row = cell.Row.Value;
	}

	public override bool Equals(object obj) {
		return obj is SimpleCellAddress address && Equals(address);
	}

	public bool Equals(SimpleCellAddress other) {
		return Column == other.Column && Row == other.Row;
	}

	public override int GetHashCode() {
		int hashCode = 656739706;
		hashCode = (hashCode * -1521134295) + Column.GetHashCode();
		hashCode = (hashCode * -1521134295) + Row.GetHashCode();
		return hashCode;
	}

	public object Clone() {
		return new SimpleCellAddress(Column, Row);
	}

	public SimpleCellAddress Copy() => (SimpleCellAddress)Clone();

	public static bool operator ==(SimpleCellAddress left, SimpleCellAddress right) {
		return left.Equals(right);
	}

	public static bool operator !=(SimpleCellAddress left, SimpleCellAddress right) {
		return !(left == right);
	}

	public static implicit operator FastGridCellAddress(SimpleCellAddress cell) {
		return new FastGridCellAddress(cell.Row, cell.Column);
	}

	public override string ToString() {
		return $"({Column}, {Row})";
	}
}
