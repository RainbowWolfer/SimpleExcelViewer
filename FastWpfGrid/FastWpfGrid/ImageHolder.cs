using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FastWpfGrid;

public class ImageHolder {
	public WriteableBitmap Bitmap;
	public BitmapImage Image;
	public WriteableBitmapExtensions.BlendMode BlendMode;
	public Color KeyColor = Colors.White;

	public ImageHolder(WriteableBitmap bitmap, BitmapImage image) {
		Bitmap = bitmap;
		Image = image;

		using BitmapContext context = Bitmap.GetBitmapContext();
		int w = Bitmap.PixelWidth;
		int h = Bitmap.PixelHeight;

		for (int x = 0; x < w; x++) {
			for (int y = 0; y < h; y++) {
				Color color = Bitmap.GetPixel(x, y);
				if (color.A != 0xFF) {
					BlendMode = WriteableBitmapExtensions.BlendMode.Alpha;
					return;
				}
			}
		}

		BlendMode = WriteableBitmapExtensions.BlendMode.ColorKeying;
		KeyColor = bitmap.GetPixel(0, 0);
	}
}
