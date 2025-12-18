using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FastWpfGrid;

internal class ColumnReorderAdorner : Adorner {
	/// <summary>
	/// Gets or Sets the pen which can be used for the render process.
	/// </summary>
	public Pen Pen { get; set; } = new Pen(Brushes.Gray, 2);

	private readonly AdornerLayer m_AdornerLayer;
	public FastGridCellAddress ModelCell { get; }

	public int StartColumnIndex { get; }
	public int ColumnIndex { get; }

	public FastGridControl FastGridControl { get; }

	public int TargetIndex { get; private set; }

	public ColumnReorderAdorner(FastGridControl fastGridControl, FastGridCellAddress startCell, FastGridCellAddress cell) : base(fastGridControl) {
		FastGridControl = fastGridControl;
		ModelCell = FastGridControl.RealToModel(cell);
		StartColumnIndex = FastGridControl.RealToModel(startCell).Column.Value;
		if (FastGridControl.IsModelCellInValidRange(ModelCell)) {
			ColumnIndex = cell.Column == null ? -1 : cell.Column.Value;
		} else {
			ColumnIndex = -1;
		}
		TargetIndex = ColumnIndex;
		AllowDrop = false;
		SnapsToDevicePixels = true;
		m_AdornerLayer = AdornerLayer.GetAdornerLayer(fastGridControl);
		m_AdornerLayer.Add(this);
	}

	public async void Detach() {
		await Task.Delay(10);//prevent flickering render
		m_AdornerLayer.Remove(this);
	}

	protected override void OnRender(DrawingContext drawingContext) {
		if (ColumnIndex == -1) {
			return;
		}
		Rect rect = FastGridControl.GetColumnHeaderRect(ColumnIndex).ToRect();

		Point pt = Mouse.GetPosition(FastGridControl.Image);
		pt.X /= DpiDetector.DpiXKoef;
		pt.Y /= DpiDetector.DpiYKoef;

		rect.X /= DpiDetector.DpiXKoef;
		rect.Y /= DpiDetector.DpiYKoef;

		rect.Width /= DpiDetector.DpiXKoef;
		rect.Height /= DpiDetector.DpiYKoef;

		double left;
		if (pt.X > rect.X + (rect.Width / 2)) {
			left = rect.X + rect.Width;
			TargetIndex = ColumnIndex + 1;
		} else {
			left = rect.X;
			TargetIndex = ColumnIndex;
		}

		if (TargetIndex == StartColumnIndex || StartColumnIndex - TargetIndex == -1) {
			return;
		}
		Point point1 = new(left, 0);
		Point point2 = new(left, rect.Height);

		drawingContext.DrawLine(Pen, point1, point2);
		DrawTriangle(drawingContext, point1, 90);
		DrawTriangle(drawingContext, point2, 180 + 90);
	}

	public static int GetTargetIndex(FastGridControl control, Point mousePosition, int currentTarget) {
		Rect rect = control.GetColumnHeaderRect(currentTarget).ToRect();
		if (mousePosition.X > rect.X + (rect.Width / 2)) {
			return currentTarget + 1;
		} else {
			return currentTarget;
		}
	}

	private void DrawTriangle(DrawingContext drawingContext, Point origin, double rotation) {
		drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
		drawingContext.PushTransform(new RotateTransform(rotation));

		drawingContext.DrawGeometry(Pen.Brush, null, m_Triangle);

		drawingContext.Pop();
		drawingContext.Pop();
	}

	static ColumnReorderAdorner() {
		// Create the pen and triangle in a static constructor and freeze them to improve performance.
		const int triangleSize = 5;

		LineSegment firstLine = new(new Point(0, -triangleSize), false);
		firstLine.Freeze();
		LineSegment secondLine = new(new Point(0, triangleSize), false);
		secondLine.Freeze();

		PathFigure figure = new() { StartPoint = new Point(triangleSize, 0) };
		figure.Segments.Add(firstLine);
		figure.Segments.Add(secondLine);
		figure.Freeze();

		m_Triangle = new PathGeometry();
		m_Triangle.Figures.Add(figure);
		m_Triangle.Freeze();
	}

	private static readonly PathGeometry m_Triangle;
}
