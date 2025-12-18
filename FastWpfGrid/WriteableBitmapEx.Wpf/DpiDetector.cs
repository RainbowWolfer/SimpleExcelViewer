using System.Drawing;

namespace System.Windows.Media.Imaging;

internal static class DpiDetector
{
	private static double? _dpiXKoef;

	public static double DpiXKoef
	{
		get
		{
			if (_dpiXKoef == null)
			{
				using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
				_dpiXKoef = graphics.DpiX / 96.0;
			}
			return _dpiXKoef ?? 1;
		}
	}

	private static double? _dpiYKoef;

	public static double DpiYKoef
	{
		get
		{
			if (_dpiYKoef == null)
			{
				using Graphics graphics = Graphics.FromHwnd(IntPtr.Zero);
				_dpiYKoef = graphics.DpiY / 96.0;
			}
			return _dpiYKoef ?? 1;
		}
	}
}
