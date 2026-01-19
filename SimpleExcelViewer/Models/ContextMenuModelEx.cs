using FastWpfGrid;
using RW.Common.WPF.Models;

namespace SimpleExcelViewer.Models;

public class ContextMenuModelEx : ContextMenuModel, IContextMenuCreator {
	public bool CanShow() => true;


}
