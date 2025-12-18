using System.Windows.Controls;

namespace FastWpfGrid;

public interface IContextMenuCreator {
	bool CanShow();

	ContextMenu CreateContextMenu();
}
