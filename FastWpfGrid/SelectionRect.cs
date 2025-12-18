using System;

namespace FastWpfGrid;


public class SelectionRect : /*IEnumerable<SimpleCellAddress>, */ICloneable {
	public SimpleCellAddress RectFrom { get; set; }
	public SimpleCellAddress RectTo { get; set; }

	public SelectionRect(SimpleCellAddress rectFrom, SimpleCellAddress rectTo) {
		RectFrom = rectFrom;
		RectTo = rectTo;
	}

	public SelectionRect(SimpleCellAddress singleCell) {
		RectFrom = new SimpleCellAddress(singleCell.Column, singleCell.Row);
		RectTo = new SimpleCellAddress(singleCell.Column, singleCell.Row);
	}

	public bool Contains(int col, int row) {
		if (row >= RectFrom.Row && row <= RectTo.Row
			&& col >= RectFrom.Column && col <= RectTo.Column) {
			return true;
		}
		return false;
	}

	public int GetCellsCount() {
		return Math.Abs(RectTo.Row - RectFrom.Row + 1) * Math.Abs(RectTo.Column - RectFrom.Column + 1);
	}

	public SimpleCellAddress GetFirstCell() {
		return new SimpleCellAddress(RectFrom.Column, RectFrom.Row);
	}

	public override string ToString() {
		return $"Rect From {RectFrom} > {RectTo}";
	}

	//public IEnumerator<SimpleCellAddress> GetEnumerator() {
	//	for (int c = RectFrom.Column; c <= RectTo.Column; c++) {
	//		for (int r = RectFrom.Row; r <= RectTo.Row; r++) {
	//			yield return new SimpleCellAddress(c, r);
	//		}
	//	}
	//}

	//IEnumerator IEnumerable.GetEnumerator() {
	//	return GetEnumerator();
	//}

	public object Clone() {
		return new SelectionRect(RectFrom.Copy(), RectTo.Copy());
	}

	public SelectionRect Copy() => (SelectionRect)Clone();
}
