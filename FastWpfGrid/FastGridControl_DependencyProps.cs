using RW.Common;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FastWpfGrid;

public partial class FastGridControl {
	#region Events

	public event TypedEventHandler<FastGridControl, FastGridCellAddress> CellDoubleClick;

	#endregion



	public bool EnableVerticalScroll {
		get => (bool)GetValue(EnableVerticalScrollProperty);
		set => SetValue(EnableVerticalScrollProperty, value);
	}

	public static readonly DependencyProperty EnableVerticalScrollProperty = DependencyProperty.Register(
		nameof(EnableVerticalScroll),
		typeof(bool),
		typeof(FastGridControl),
		new PropertyMetadata(true)
	);



	public bool EnableHorizontalScroll {
		get => (bool)GetValue(EnableHorizontalScrollProperty);
		set => SetValue(EnableHorizontalScrollProperty, value);
	}

	public static readonly DependencyProperty EnableHorizontalScrollProperty = DependencyProperty.Register(
		nameof(EnableHorizontalScroll),
		typeof(bool),
		typeof(FastGridControl),
		new PropertyMetadata(true)
	);





	/// <summary>
	/// Prevents the inline editor from being used if the control is in a read-only state.
	/// </summary>
	public bool IsReadOnly {
		get => (bool)GetValue(IsReadOnlyProperty);
		set => SetValue(IsReadOnlyProperty, value);
	}

	public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
		nameof(IsReadOnly),
		typeof(bool),
		typeof(FastGridControl),
		new PropertyMetadata(false)
	);





	public Thickness InnerPadding {
		get => (Thickness)GetValue(InnerPaddingProperty);
		set => SetValue(InnerPaddingProperty, value);
	}

	public static readonly DependencyProperty InnerPaddingProperty = DependencyProperty.Register(
		nameof(InnerPadding),
		typeof(Thickness),
		typeof(FastGridControl),
		new PropertyMetadata(new Thickness(-1, -2, -1, -1))
	);




	public Cursor ColumnReorderDragCursor {
		get => (Cursor)GetValue(ColumnReorderDragCursorProperty);
		set => SetValue(ColumnReorderDragCursorProperty, value);
	}

	public static readonly DependencyProperty ColumnReorderDragCursorProperty = DependencyProperty.Register(
		nameof(ColumnReorderDragCursor),
		typeof(Cursor),
		typeof(FastGridControl),
		new PropertyMetadata(Cursors.UpArrow)
	);



	//public bool EnableColumnDragReorder {
	//	get => (bool)GetValue(EnableColumnDragReorderProperty);
	//	set => SetValue(EnableColumnDragReorderProperty, value);
	//}

	//public static readonly DependencyProperty EnableColumnDragReorderProperty = DependencyProperty.Register(
	//	nameof(EnableColumnDragReorder),
	//	typeof(bool),
	//	typeof(FastGridControl),
	//	new PropertyMetadata(true));



	public object CornerContent {
		get => GetValue(CornerContentProperty);
		set => SetValue(CornerContentProperty, value);
	}

	public static readonly DependencyProperty CornerContentProperty = DependencyProperty.Register(
		nameof(CornerContent),
		typeof(object),
		typeof(FastGridControl),
		new PropertyMetadata(null)
	);



	#region Arrange



	public int HeaderHeight {
		get => (int)GetValue(HeaderHeightProperty);
		set => SetValue(HeaderHeightProperty, value);
	}

	public static readonly DependencyProperty HeaderHeightProperty = DependencyProperty.Register(
		nameof(HeaderHeight),
		typeof(int),
		typeof(FastGridControl),
		new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHeaderHeightChanged, CoerceHeaderHeight)
	);

	private static object CoerceHeaderHeight(DependencyObject d, object baseValue) {
		if (baseValue is int height) {
			return CalculateActualHeight(height, (Visual)d);
		}
		return baseValue;
	}

	private static void OnHeaderHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is FastGridControl view) {
			view.SetScrollbarMargin();
			view.InvalidateAll();
		}
	}




	public int HeaderWidth {
		get => (int)GetValue(HeaderWidthProperty);
		set => SetValue(HeaderWidthProperty, value);
	}

	public static readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(
		nameof(HeaderWidth),
		typeof(int),
		typeof(FastGridControl),
		new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHeaderWidthChanged, CoerceHeaderWidth)
	);

	private static object CoerceHeaderWidth(DependencyObject d, object baseValue) {
		if (baseValue is int width) {
			return CalculateActualWidth(width, (Visual)d);
		}
		return baseValue;
	}

	private static void OnHeaderWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is FastGridControl view) {
			view.SetScrollbarMargin();
			view.InvalidateAll();
		}
	}




	#endregion

	public ICommand CellDoubleClickCommand {
		get => (ICommand)GetValue(CellDoubleClickCommandProperty);
		set => SetValue(CellDoubleClickCommandProperty, value);
	}

	public static readonly DependencyProperty CellDoubleClickCommandProperty = DependencyProperty.Register(
		nameof(CellDoubleClickCommand),
		typeof(ICommand),
		typeof(FastGridControl),
		new PropertyMetadata(null)
	);



	#region property Model

	public IFastGridModel Model {
		get => (IFastGridModel)this.GetValue(ModelProperty);
		set => this.SetValue(ModelProperty, value);
	}

	public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
		"Model", typeof(IFastGridModel), typeof(FastGridControl), new PropertyMetadata(null, OnModelPropertyChanged));

	private static void OnModelPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
		((FastGridControl)dependencyObject).OnModelPropertyChanged();
	}

	#endregion

	#region property UseClearType

	public bool UseClearType {
		get => (bool)this.GetValue(UseClearTypeProperty);
		set => this.SetValue(UseClearTypeProperty, value);
	}

	public static readonly DependencyProperty UseClearTypeProperty = DependencyProperty.Register(
		"UseClearType", typeof(bool), typeof(FastGridControl), new PropertyMetadata(true, OnUseClearTypePropertyChanged));

	private static void OnUseClearTypePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
		((FastGridControl)dependencyObject).OnUseClearTypePropertyChanged();
	}

	#endregion

	#region property AllowFlexibleRows

	public bool AllowFlexibleRows {
		get => (bool)this.GetValue(AllowFlexibleRowsProperty);
		set => this.SetValue(AllowFlexibleRowsProperty, value);
	}

	public static readonly DependencyProperty AllowFlexibleRowsProperty = DependencyProperty.Register(
		"AllowFlexibleRows", typeof(bool), typeof(FastGridControl), new PropertyMetadata(false, OnAllowFlexibleRowsPropertyChanged));

	private static void OnAllowFlexibleRowsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
		((FastGridControl)dependencyObject).OnAllowFlexibleRowsPropertyChanged();
	}

	#endregion


	public bool EnableMouseHoverCellBackground {
		get => (bool)GetValue(EnableMouseHoverCellBackgroundProperty);
		set => SetValue(EnableMouseHoverCellBackgroundProperty, value);
	}

	public static readonly DependencyProperty EnableMouseHoverCellBackgroundProperty = DependencyProperty.Register(
		nameof(EnableMouseHoverCellBackground),
		typeof(bool),
		typeof(FastGridControl),
		new PropertyMetadata(false)
	);


}