using SimpleExcelViewer.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.Models;

public class CsvData : ITableData {
	private List<string> _columnNames = new(256);
	private readonly List<string[]> _rows = new(1024 * 50);

	//private readonly Dictionary<Vector2Int, string> _stringPool = [];

	public int RowCount => _rows.Count;
	public int ColumnCount => _columnNames.Count;

	public string GetColumnName(int index) {
		if (index >= 0 && index < _columnNames.Count) {
			return _columnNames[index];
		} else {
			return string.Empty;
		}
	}

	public object GetCell(int row, int column) {
		if (row >= 0 && row < _rows.Count && column >= 0 && column < _rows[row].Length) {
			return _rows[row][column];
		} else {
			return string.Empty;
		}
	}

	public object[] GetRow(int index) => _rows[index];

	private void AddRow(string[] row) {
		_rows.Add(row);
	}

	private void SetColumnNames(string[] names) {
		_columnNames = [.. names];
	}

	public static CsvData Read(Stream stream, Encoding encoding, char[] splitters) {
		try {
			using StreamReader reader = new(stream, encoding);
			CsvData csv = new();

			string? line;
			bool isHeader = true;

			while ((line = reader.ReadLine()) != null) {
				string[] values = line.Split(splitters, StringSplitOptions.None);

				if (isHeader) {
					csv.SetColumnNames(values);
					isHeader = false;
				} else {
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
