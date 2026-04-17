using FastWpfGrid;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using RW.Common.WPF.Models;
using SimpleExcelViewer.Enums;
using SimpleExcelViewer.Interfaces;
using SimpleExcelViewer.Services;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace SimpleExcelViewer.Models;

public class TableModel : FastGridModelBase, IDisposable {
	public ITableData Data { get; }

	private List<int> _visibleColumnMap = [];
	public IReadOnlyList<int> CurrentColumnMap => _visibleColumnMap;

	private bool isTransposed = false;
	private ColumnHeaderType columnHeaderType;

	public bool IsTransposed {
		get => isTransposed;
		set {
			if (isTransposed != value) {
				isTransposed = value;
				RefreshDimensions();
				InvalidateAll();
			}
		}
	}

	public ColumnHeaderType ColumnHeaderType {
		get => columnHeaderType;
		set {
			if (columnHeaderType != value) {
				columnHeaderType = value;
				InvalidateAll();
			}
		}
	}

	private readonly List<CompiledRegexItem> _compiledRegexes = [];

	private class CompiledRegexItem(Regex pattern, Color backgroundColor, Color foregroundColor) {
		public Regex Pattern { get; } = pattern;
		public Color? BackgroundColor { get; } = backgroundColor;
		public Color? ForegroundColor { get; } = foregroundColor;
	}

	public TableModel(ITableData data) : base(data.ColumnCount, data.RowCount) {
		Data = data;

		ResetColumns();
	}

	private void RefreshDimensions() {
		int visibleDataColumnCount = _visibleColumnMap.Count;
		int dataRowCount = Data.RowCount;

		if (IsTransposed) {
			UpdateRowCount(visibleDataColumnCount);
			UpdateColumnCount(dataRowCount);
		} else {
			UpdateRowCount(dataRowCount);
			UpdateColumnCount(visibleDataColumnCount);
		}
	}

	public void ResetColumns() {
		_visibleColumnMap = [.. Enumerable.Range(0, Data.ColumnCount)];
		RefreshDimensions();
		InvalidateAll();
	}

	public void UpdateColumns(IEnumerable<int>? orderedVisibleIndices) {
		if (orderedVisibleIndices is null) {
			return;
		}

		_visibleColumnMap = [.. orderedVisibleIndices];

		RefreshDimensions();
		AutoColumnWidth();
		InvalidateAll();
	}

	public SelectionRect? SelectionRect {
		get => GetProperty(() => SelectionRect);
		set => SetProperty(() => SelectionRect, value);
	}

	public override void SelectionChanged(SelectionRect selectionRect) {
		base.SelectionChanged(selectionRect);
		SelectionRect = selectionRect;
	}


	public override string GetColumnHeaderText(int column) {
		if (IsTransposed) {
			return $"Row {column + 1}";
		} else {
			int realDataColumnIndex = _visibleColumnMap[column];
			return GetColumnName(realDataColumnIndex);
		}
	}

	public override string GetRowHeaderText(int row) {
		if (IsTransposed) {
			int realDataColumnIndex = _visibleColumnMap[row];
			return GetColumnName(realDataColumnIndex);
		} else {
			return (row + 1).ToString();
		}
	}

	private string GetColumnName(int index) {
		string name = Data.GetColumnName(index);
		string result = ColumnHeaderType switch {
			ColumnHeaderType.Name => $"{name}",
			ColumnHeaderType.Name_Index => $"{name} ({index})",
			ColumnHeaderType.Name_Number => $"{name} ({index + 1})",
			ColumnHeaderType.Name_Alphabet => $"{name} ({(index + 1).IndexToColumn()})",
			_ => $"{name}",
		};
		return result;
	}

	public override string GetCellText(int row, int column) {
		if (IsTransposed) {
			int realDataColumnIndex = _visibleColumnMap[row];
			int realDataRowIndex = column;
			return Data.GetCell(realDataRowIndex, realDataColumnIndex).SafeToString();
		} else {
			int realDataColumnIndex = _visibleColumnMap[column];
			int realDataRowIndex = row;
			return Data.GetCell(realDataRowIndex, realDataColumnIndex).SafeToString();
		}
	}

	public override IContextMenuCreator? GetContextMenu(IFastGridView grid, SelectionRect selectionRect, FastGridCellAddress cell) {
		IEnumerable<ContextMenuModelItem> items;
		if (cell.IsColumnHeader) {
			if (isTransposed) {
				items = RowHeaderContextMenuItems();
			} else {
				items = ColumnHeaderContextMenuItems();
			}
		} else if (cell.IsRowHeader) {
			if (isTransposed) {
				items = ColumnHeaderContextMenuItems();
			} else {
				items = RowHeaderContextMenuItems();
			}
		} else if (cell.IsGridHeader) {
			items = GridHeaderContextMenuItems();
		} else if (cell.IsCell) {
			items = CellContextMenuItems();
		} else {
			return null;
		}

		if (items.IsEmpty()) {
			return null;
		}

		ContextMenuModelEx contextMenu = [.. items];
		return contextMenu;
	}

	private IEnumerable<ContextMenuModelItem> ColumnHeaderContextMenuItems() {
		yield return new ContextMenuModelItem("Copy all column names", "", () => {
			List<string> names = [];
			for (int i = 0; i < Data.ColumnCount; i++) {
				string name = Data.GetColumnName(i);
				names.Add(name);
			}
			string allNames = string.Join(", ", names);
			allNames.CopyToClipboard();
		});
		yield return new ContextMenuModelItem("Copy selected column name(s)", "", () => {
			if (SelectionRect is null) {
				return;
			}
			List<string> names = [];

			for (int i = SelectionRect.RectFrom.Column; i <= SelectionRect.RectTo.Column; i++) {
				string name = Data.GetColumnName(i);
				names.Add(name);
			}
			string allNames = string.Join(", ", names);
			allNames.CopyToClipboard();
		}) {
			IsEnabled = SelectionRect != null,
		};
	}

	private IEnumerable<ContextMenuModelItem> RowHeaderContextMenuItems() {
		yield break;
	}

	private IEnumerable<ContextMenuModelItem> GridHeaderContextMenuItems() {
		yield break;
	}

	private IEnumerable<ContextMenuModelItem> CellContextMenuItems() {
		yield break;
	}


	public override IFastGridCell GetCell(IFastGridView view, int row, int column) {
		string cellText = GetCellText(row, column);

		FastGridCellImpl cell = new() {
			Alignment = RenderTextAlignment.Right,
		};

		cell.AddTextBlock(cellText);

		if (_compiledRegexes.Count == 0) {
			return cell;
		}

		foreach (CompiledRegexItem cachedItem in _compiledRegexes) {
			if (cachedItem.Pattern.IsMatch(cellText)) {
				cell.BackgroundColor = cachedItem.BackgroundColor;
				break;
			}
		}

		return cell;
	}

	public void UpdateRegexConfig(RegexConfigModel? regexConfig) {
		_compiledRegexes.Clear();

		if (regexConfig != null && regexConfig.Items != null) {
			foreach (RegexConfigModelItem item in regexConfig.Items) {
				if (item.Regex.IsBlank()) {
					continue;
				}

				try {
					Regex compiledRegex = new(item.Regex, RegexOptions.Compiled);

					CompiledRegexItem cachedItem = new(
						compiledRegex,
						item.BackColor.ToMediaColor(),
						item.TextColor.ToMediaColor()
					);
					_compiledRegexes.Add(cachedItem);
				} catch (ArgumentException) {
				}
			}
		}

		InvalidateAll();
	}


	public void Dispose() {
		Data.Dispose();
	}

}
