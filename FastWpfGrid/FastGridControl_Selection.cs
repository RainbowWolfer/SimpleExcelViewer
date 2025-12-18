using System;
using System.Windows;
using System.Windows.Input;

namespace FastWpfGrid;

public partial class FastGridControl {
	public event EventHandler<SelectionChangedEventArgs> SelectedCellsChanged;

	public ICommand SelectedCellChangedCommand {
		get => (ICommand)GetValue(SelectedCellChangedCommandProperty);
		set => SetValue(SelectedCellChangedCommandProperty, value);
	}

	public static readonly DependencyProperty SelectedCellChangedCommandProperty = DependencyProperty.Register(
		nameof(SelectedCellChangedCommand),
		typeof(ICommand),
		typeof(FastGridControl),
		new PropertyMetadata(null)
	);


	public bool AllowEscapeToDeselect {
		get => (bool)GetValue(AllowEscapeToDeselectProperty);
		set => SetValue(AllowEscapeToDeselectProperty, value);
	}

	public static readonly DependencyProperty AllowEscapeToDeselectProperty = DependencyProperty.Register(
		nameof(AllowEscapeToDeselect),
		typeof(bool),
		typeof(FastGridControl),
		new PropertyMetadata(true)
	);



	public bool CanSelect {
		get => (bool)GetValue(CanSelectProperty);
		set => SetValue(CanSelectProperty, value);
	}

	public static readonly DependencyProperty CanSelectProperty = DependencyProperty.Register(
		nameof(CanSelect),
		typeof(bool),
		typeof(FastGridControl),
		new PropertyMetadata(true, OnCanSelectChanged)
	);

	private static void OnCanSelectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is FastGridControl view) {
			view.Selection.CanSelect = (bool)e.NewValue;
		}
	}

	internal SelectionModel Selection { get; } = new SelectionModel();


	private void SelectRectInternal(FastGridCellAddress start, FastGridCellAddress target) {
		bool isStartInFrozenRows = start.Row == null || _rowSizes.IsIndexFrozen(start.Row.Value);
		int frozenCount = _rowSizes.FrozenCount;

		SelectionRect rect = GetCellRect(start, target);

		if (rect is null) {
			return;
		}

		//handle frozen and normal rows' separation

		if (isStartInFrozenRows) {
			if (rect.RectTo.Row >= frozenCount) {
				rect.RectTo = new SimpleCellAddress(rect.RectTo.Column, frozenCount - 1);
			}
		} else {
			//_rowSizes
			if (rect.RectFrom.Row < frozenCount) {
				int row = FirstVisibleRowScrollIndex + frozenCount - 1;
				target = new FastGridCellAddress(row, target.Column);
				rect = GetCellRect(start, target);
				rect.RectFrom = new SimpleCellAddress(rect.RectFrom.Column, Math.Max(row + 1, frozenCount));
			}
		}

		Selection.SelectRect(rect);
	}

	private void ClearSelectedCells() {
		Selection.Clear();
	}

	private void OnChangeSelectedCells(bool isInvokedByUser) {
		SelectedCellsChanged?.Invoke(this, new SelectionChangedEventArgs { IsInvokedByUser = isInvokedByUser });
		SelectedCellChangedCommand?.Execute(CurrentCell);
		if (Model != null) {
			Model.CurrentSelectedCell = CurrentCell;
			Model.SelectionChanged(GetSelectionRect());
		}
	}

	public void SelectCell(int? row, int? column) {
		FastGridCellAddress cell = new(row, column);
		Selection.Clear();
		if (row == null && column == null) {
			//clear all
		} else if (row == null) {
			//click column header
			Selection.SelectColumn(column.Value, _rowSizes.RealCount);
		} else if (column == null) {
			//click row header
			Selection.SelectRow(row.Value, _columnSizes.RealCount);
		} else {
			Selection.SelectCell(new SimpleCellAddress(cell));
		}

		InvalidateAll();

		SetCurrentCell(cell);
		OnChangeSelectedCells(true);
		ScrollCurrentCellIntoView();
	}

	public void SelectRect(SelectionRect rect) {
		Selection.Clear();
		if (rect != null) {
			Selection.SelectRect(rect);
			SetCurrentCell(rect.GetFirstCell());
		} else {
			SetCurrentCell(FastGridCellAddress.Empty);
		}
		OnChangeSelectedCells(true);
		ScrollCurrentCellIntoView();
		InvalidateAll();
	}

	public void BringIntoView(SimpleCellAddress cell) {
		ScrollIntoView(cell);
		InvalidateAll();
	}

}
