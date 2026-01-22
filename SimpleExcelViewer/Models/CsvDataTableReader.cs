using SimpleExcelViewer.Interfaces;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.Models;

public class CsvDataTableReader : ITableData {

	private readonly DataTable _table;

	public CsvDataTableReader(DataTable table) {
		_table = table;
	}

	public int RowCount => _table.Rows.Count;
	public int ColumnCount => _table.Columns.Count;

	public string GetColumnName(int index) {
		return _table.Columns[index].ColumnName;
	}

	public object[] GetRow(int index) {
		DataRow row = _table.Rows[index];
		return row.ItemArray!;
	}

	public object GetCell(int row, int column) => _table.Rows[row][column];
	public static CsvDataTableReader Read(Stream stream, Encoding encoding, char[] splitters, bool convert = false) {
		try {
			using StreamReader reader = new(stream, encoding);
			DataTable dt = new();

			string? line;
			bool isHeader = true;

			while ((line = reader.ReadLine()) != null) {
				string[] values = line.Split(splitters);

				if (isHeader) {
					// 第一行作为列名
					foreach (string col in values) {
						dt.Columns.Add(col.Trim());
					}
					isHeader = false;
				} else {
					DataRow row = dt.NewRow();
					for (int i = 0; i < Math.Min(dt.Columns.Count, values.Length); i++) {
						object cell = values[i];

						if (convert && double.TryParse(values[i], out double d)) {
							cell = d;
						}

						row[i] = cell;
					}
					dt.Rows.Add(row);
				}
			}

			return new CsvDataTableReader(dt);
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			throw;
		}
	}

	public void Dispose() {

	}
}
