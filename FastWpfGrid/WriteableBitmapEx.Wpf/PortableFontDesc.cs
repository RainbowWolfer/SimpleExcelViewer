namespace System.Windows.Media.Imaging;

internal class PortableFontDesc
{
	public readonly string FontName;
	public readonly int EmSize;
	public readonly bool IsBold;
	public readonly bool IsItalic;
	public readonly bool IsClearType;

	public PortableFontDesc(string name = "Arial", int emsize = 12, bool isbold = false, bool isitalic = false, bool cleartype = false)
	{
		FontName = name;
		EmSize = emsize;
		IsBold = isbold;
		IsItalic = isitalic;
		IsClearType = cleartype;
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return FontName.GetHashCode() ^ EmSize.GetHashCode() ^ IsBold.GetHashCode() ^ IsItalic.GetHashCode() ^ IsClearType.GetHashCode();
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is not PortableFontDesc other)
		{
			return false;
		}

		return FontName == other.FontName && EmSize == other.EmSize && IsBold == other.IsBold && IsItalic == other.IsItalic && IsClearType == other.IsClearType;
	}
}
