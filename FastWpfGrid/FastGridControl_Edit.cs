using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FastWpfGrid;

public partial class FastGridControl {
	private FastGridCellAddress _inplaceEditorCell;

	//temp cache
	private EditingControlConfig editingControl;

	public void HideInlineEditor(bool saveCellValue = true) {
		if (editingControl == null) {
			return;
		}
		using (InvalidationContext ctx = CreateInvalidationContext()) {
			if (saveCellValue && _inplaceEditorCell.IsCell && editingControl.ValueChanged) {

				editingControl.OnFinish?.Invoke(editingControl);

				InvalidateCell(_inplaceEditorCell);
			}
			_inplaceEditorCell = new FastGridCellAddress();

			editingControl.Clear();

			if (EditBorder.Child is IDisposable disposable) {
				disposable.Dispose();
			}
			EditBorder.Child = null;
			EditBorder.Visibility = Visibility.Collapsed;

			editingControl.Dispose();
		}
		Keyboard.Focus(Image);
	}

	public bool ShowInlineEditor(FastGridCellAddress cell, string textValueOverride = null) {
		if (IsReadOnly) {
			return false;
		}

		if (!cell.IsCell) {
			return false;
		}

		editingControl = Model?.RequestCellEditor(this, cell.Row.Value, cell.Column.Value, textValueOverride);
		if (editingControl == null) {
			return false;
		}

		_inplaceEditorCell = cell;

		EditBorder.Child = editingControl.Control;
		EditBorder.Visibility = Visibility.Visible;
		AdjustInlineEditorPosition();

		editingControl.OnStart(textValueOverride);

		editingControl.ValueChanged = !string.IsNullOrEmpty(textValueOverride);

		return true;
	}

	public void AdjustInlineEditorPosition() {
		if (!_inplaceEditorCell.IsCell) {
			return;
		}

		bool visible = _rowSizes.IsVisible(_inplaceEditorCell.Row.Value, FirstVisibleRowScrollIndex, GridScrollAreaHeight)
					&& _columnSizes.IsVisible(_inplaceEditorCell.Column.Value, FirstVisibleColumnScrollIndex, GridScrollAreaWidth);
		EditBorder.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
		IntRect rect = GetCellRect(_inplaceEditorCell.Row.Value, _inplaceEditorCell.Column.Value);

		EditBorder.Margin = new Thickness {
			Left = rect.Left / DpiDetector.DpiXKoef,
			Top = rect.Top / DpiDetector.DpiYKoef,
			Right = ImageGrid.ActualWidth - (rect.Right / DpiDetector.DpiXKoef),
			Bottom = ImageGrid.ActualHeight - (rect.Bottom / DpiDetector.DpiYKoef),
		};
	}

}

public abstract class EditingControlConfig : IDisposable {
	public int Column { get; }
	public int Row { get; }
	public Control Control { get; }

	public bool ValueChanged { get; set; }

	public IFastGridView View { get; }

	public Action<EditingControlConfig> OnFinish;

	public EditingControlConfig(IFastGridView view, int column, int row, Control control, Action<EditingControlConfig> onFinish) {
		View = view;
		Column = column;
		Row = row;
		Control = control;
		OnFinish = onFinish;
	}

	public abstract void OnStart(string textValueOverride);

	public abstract void Clear();

	public virtual void Dispose() {

	}
}

public abstract class EditingTextBoxConfigBase : EditingControlConfig {
	public TextBox Box { get; }
	protected EditingTextBoxConfigBase(IFastGridView view, int column, int row, TextBox control, Action<EditingControlConfig> onFinish) : base(view, column, row, control, onFinish) {
		Box = control;
		Box.KeyDown += Box_KeyDown;
		Box.TextChanged += Box_TextChanged;
		Box.LostFocus += Box_LostFocus;
	}

	~EditingTextBoxConfigBase() {
		Box.KeyDown -= Box_KeyDown;
		Box.TextChanged -= Box_TextChanged;
		Box.LostFocus -= Box_LostFocus;
	}

	private void Box_LostFocus(object sender, RoutedEventArgs e) {
		View.HideInlineEditor();
	}

	private void Box_TextChanged(object sender, TextChangedEventArgs e) {
		ValueChanged = true;
	}

	private void Box_KeyDown(object sender, KeyEventArgs e) {
		if (e.Key == Key.Escape) {
			View.HideInlineEditor(false);
			e.Handled = true;
		}
		if (e.Key == Key.Enter) {
			View.HideInlineEditor();
			View.MoveCurrentCell(Row + 1, Column, e);
		}

		View.HandleCursorMove(e, true);
		if (e.Handled) {
			View.HideInlineEditor();
		}

		View.InvalidateAll();
	}

	public override void OnStart(string textValueOverride) {
		if (Box.IsFocused) {
			if (textValueOverride == null) {
				Box.SelectAll();
			}
		} else {
			Box.Focus();
			if (textValueOverride == null) {
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, Box.SelectAll);
			}
		}

		if (textValueOverride != null) {
			Box.SelectionStart = textValueOverride.Length;
		}

	}

	public override void Clear() {
		Box.Text = string.Empty;
	}

}
