using SimpleExcelViewer.Interfaces;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.Models;

public class CsvDataBuffer : ITableData {
	private string[] _columnNames = [];

	// 整个文件的统一缓冲区
	private char[] _buffer = [];

	// 每行的字段切片信息：行 -> 列 -> (start, length)
	private readonly List<(int Start, int Length)[]> _rowSlices = [];

	public int RowCount => _rowSlices.Count;
	public int ColumnCount => _columnNames.Length;

	public string GetColumnName(int index) =>
		index >= 0 && index < _columnNames.Length ? _columnNames[index] : string.Empty;

	public object GetCell(int row, int column) {
		(int Start, int Length) = _rowSlices[row][column];
		return new string(_buffer, Start, Length);
	}

	public object[] GetRow(int index) {
		(int Start, int Length)[] slices = _rowSlices[index];
		object[] row = new object[slices.Length];
		for (int i = 0; i < slices.Length; i++) {
			row[i] = new string(_buffer, slices[i].Start, slices[i].Length);
		}
		return row;
	}

	public static CsvDataBuffer Read(Stream stream, Encoding encoding, char splitter = ',') {
		// 一次性读入整个文件
		using StreamReader reader = new(stream, encoding);
		string allText = reader.ReadToEnd();
		CsvDataBuffer csv = new() {
			_buffer = allText.ToCharArray()
		};

		int pos = 0;
		bool isHeader = true;
		while (pos < csv._buffer.Length) {
			// 找到行结束符
			int lineEnd = Array.IndexOf(csv._buffer, '\n', pos);
			if (lineEnd == -1) {
				lineEnd = csv._buffer.Length;
			}

			int lineLength = lineEnd - pos;

			// 解析一行
			List<(int Start, int Length)> slices = [];
			int start = pos;
			for (int i = pos; i < lineEnd; i++) {
				if (csv._buffer[i] == splitter) {
					slices.Add((start, i - start));
					start = i + 1;
				}
			}
			slices.Add((start, lineEnd - start));

			if (isHeader) {
				// 列名直接解码
				csv._columnNames = new string[slices.Count];
				for (int i = 0; i < slices.Count; i++) {
					csv._columnNames[i] = new string(csv._buffer, slices[i].Start, slices[i].Length).Trim();
				}

				isHeader = false;
			} else {
				csv._rowSlices.Add([.. slices]);
			}

			pos = lineEnd + 1; // 跳到下一行
		}

		return csv;
	}
	public void Dispose() {

	}
}
