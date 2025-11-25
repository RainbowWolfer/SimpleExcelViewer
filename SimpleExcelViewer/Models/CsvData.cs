using RW.Common.Helpers;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.Models;

public class CsvData {
	public List<string> ColumnNames { get; private set; } = new(256);
	private readonly List<object[]> _rows = new(1024 * 50);

	public int RowCount => _rows.Count;
	public int ColumnCount => ColumnNames.Count;
	public IReadOnlyList<object[]> Rows => _rows;

	public void SetColumnNames(object[] names) {
		ColumnNames = [.. names.Select(x => x.TrimmedSafeToString())];
	}

	public void AddRow(object[] row) {
		_rows.Add(row);
	}

	public object[] GetRow(int index) => _rows[index];

	public static CsvData Read(Stream stream, Encoding encoding, bool convert, char[] splitters) {
		try {
			using StreamReader reader = new(stream, encoding);
			CsvData csv = new();

			string? line;
			bool isHeader = true;

			while ((line = reader.ReadLine()) != null) {
				object[] values = [.. line.Split(splitters).Cast<object>()];

				if (isHeader) {
					csv.SetColumnNames(values);
					isHeader = false;
				} else {
					if (convert) {
						for (int i = 0; i < values.Length; i++) {
							if (NumberHelper.ConvertDouble(values[i], out double value)) {
								values[i] = value;
							}
						}
					}
					csv.AddRow(values);
				}
			}

			return csv;
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			throw;
		}
	}
}
