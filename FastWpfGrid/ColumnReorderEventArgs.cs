using System;

namespace FastWpfGrid;

public class ColumnReorderEventArgs(int startIndex, int targetIndex) : EventArgs {
	public int StartIndex { get; } = startIndex;
	public int TargetIndex { get; } = targetIndex;
}

public class BeforeColumnReorderEventArgs(int startIndex, int targetIndex) : EventArgs {

	public bool Cancel { get; set; } = false;
	public int StartIndex { get; } = startIndex;
	public int TargetIndex { get; } = targetIndex;
}
