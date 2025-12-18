using System.Windows.Controls;
using System.Windows.Input;

namespace FastWpfGrid;

public class DataGridContextMenuItemArgs {
	public IFastGridModel Model { get; }
	public ContextMenu Owner { get; }
	public MouseButtonEventArgs MouseEventArgs { get; }

	public SelectionRect SelectionRect { get; }

	public DataGridContextMenuItemArgs(IFastGridModel model, ContextMenu owner, MouseButtonEventArgs mouseEventArgs, SelectionRect selectionRect) {
		Model = model;
		Owner = owner;
		MouseEventArgs = mouseEventArgs;
		SelectionRect = selectionRect;
	}
}
