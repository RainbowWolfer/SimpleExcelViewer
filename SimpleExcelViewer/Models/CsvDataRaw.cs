
using SimpleExcelViewer.Interfaces;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.Models;

public class CsvDataRaw(Encoding encoding) : ITableData {
	private record struct CellSlice(int Start, int Length);

	private List<string> _columnNames = new(256);

	// 每行存储原始字节
	private readonly List<byte[]> _rowBuffers = new(1024 * 50);

	// 每行的字段切片信息
	private readonly List<CellSlice[]> _rowSlices = new(1024 * 50);

	public int RowCount => _rowBuffers.Count;
	public int ColumnCount => _columnNames.Count;

	public string GetColumnName(int index) => index >= 0 && index < _columnNames.Count ? _columnNames[index] : string.Empty;

	public object GetCell(int row, int column) {
		byte[] buffer = _rowBuffers[row];
		CellSlice slice = _rowSlices[row][column];
		return encoding.GetString(buffer, slice.Start, slice.Length);
	}

	public object[] GetRow(int index) {
		byte[] buffer = _rowBuffers[index];
		CellSlice[] slices = _rowSlices[index];
		object[] row = new object[slices.Length];
		for (int i = 0; i < slices.Length; i++) {
			row[i] = encoding.GetString(buffer, slices[i].Start, slices[i].Length);
		}
		return row;
	}

	private void SetColumnNames(string[] names) {
		_columnNames = [.. names];
	}

	private void AddRow(byte[] buffer, CellSlice[] slices) {
		_rowBuffers.Add(buffer);
		_rowSlices.Add(slices);
	}

	public static CsvDataRaw Read(Stream stream, Encoding encoding, char splitter = ',') {
		using StreamReader reader = new(stream, encoding);
		CsvDataRaw csv = new(encoding);

		string? line;
		bool isHeader = true;

		while ((line = reader.ReadLine()) != null) {
			// 原始字节
			byte[] buffer = encoding.GetBytes(line);

			// 手动分割，记录每个字段的起始位置和长度
			List<CellSlice> slices = [];
			int start = 0;
			for (int i = 0; i < buffer.Length; i++) {
				if (buffer[i] == (byte)splitter) {
					slices.Add(new CellSlice(start, i - start));
					start = i + 1;
				}
			}
			slices.Add(new CellSlice(start, buffer.Length - start));

			if (isHeader) {
				// 列名直接解码
				string[] names = new string[slices.Count];
				for (int i = 0; i < slices.Count; i++) {
					names[i] = encoding.GetString(buffer, slices[i].Start, slices[i].Length);
				}

				csv.SetColumnNames(names);
				isHeader = false;
			} else {
				csv.AddRow(buffer, [.. slices]);
			}
		}

		return csv;
	}
}