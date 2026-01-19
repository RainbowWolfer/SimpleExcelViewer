using System.ComponentModel;

namespace SimpleExcelViewer.Enums;

public enum ColumnHeaderType {
	[Description("Column Name")] Name,
	[Description("Column Name with Index")] Name_Index,
	[Description("Column Name With Number")] Name_Number,
	[Description("Column Name With Alphabet")] Name_Alphabet,
}
