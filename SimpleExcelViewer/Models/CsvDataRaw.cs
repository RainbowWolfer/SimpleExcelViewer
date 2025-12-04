using RW.Base.WPF.Interfaces;
using SimpleExcelViewer.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SimpleExcelViewer.Models;

public class CsvDataRaw(Encoding encoding) : ITableData {
	private record struct CellSlice(int Start, int Length);

	// 列名缓冲区和切片
	private byte[]? _headerBuffer;
	private CellSlice[]? _headerSlices;

	// 每行存储原始字节
	private readonly List<byte[]> _rowBuffers = new(1024 * 50);

	// 每行的字段切片信息
	private readonly List<CellSlice[]> _rowSlices = new(1024 * 50);

	public int RowCount => _rowBuffers.Count;
	public int ColumnCount => _headerSlices?.Length ?? 0;

	public string GetColumnName(int index) {
		if (_headerBuffer == null || _headerSlices == null) {
			return string.Empty;
		}

		if (index < 0 || index >= _headerSlices.Length) {
			return string.Empty;
		}

		CellSlice slice = _headerSlices[index];
		return encoding.GetString(_headerBuffer, slice.Start, slice.Length);
	}

	public object GetCell(int row, int column) {
		try {
			byte[] buffer = _rowBuffers[row];
			CellSlice slice = _rowSlices[row][column];
			return encoding.GetString(buffer, slice.Start, slice.Length);
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			return "!ERROR!";
		}
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

	private void SetColumnNames(byte[] buffer, CellSlice[] slices) {
		_headerBuffer = buffer;
		_headerSlices = slices;
	}

	private void AddRow(byte[] buffer, CellSlice[] slices) {
		_rowBuffers.Add(buffer);
		_rowSlices.Add(slices);
	}

	public void Dispose() {
		_headerBuffer = null;
		_headerSlices = null;
		foreach (byte[] item in _rowBuffers) {
			Array.Clear(item, 0, item.Length);
		}
		foreach (CellSlice[] item in _rowSlices) {
			Array.Clear(item, 0, item.Length);
		}
		_rowBuffers.Clear();
		_rowSlices.Clear();
	}

	public static CsvDataRaw Read(Stream stream, Encoding encoding, char splitter = ',', IStatusReport? statusReport = null, CancellationToken token = default) {
		using StreamReader reader = new(stream, encoding);
		CsvDataRaw csv = new(encoding);

		string? line;
		bool isHeader = true;

		byte splitter_byte = (byte)splitter;

		int rowCount = 1;

		while ((line = reader.ReadLine()) != null) {
			token.ThrowIfCancellationRequested();
			statusReport?.SetStatus($"Reading line: {rowCount++}");
			// 原始字节
			byte[] buffer = encoding.GetBytes(line);

			// 手动分割，记录每个字段的起始位置和长度
			List<CellSlice> slices = [];
			int start = 0;
			for (int i = 0; i < buffer.Length; i++) {
				if (buffer[i] == splitter_byte) {
					slices.Add(new CellSlice(start, i - start));
					start = i + 1;
				}
			}
			slices.Add(new CellSlice(start, buffer.Length - start));

			if (isHeader) {
				// 列名也存成原始字节 + 切片
				csv.SetColumnNames(buffer, [.. slices]);
				isHeader = false;
			} else {
				csv.AddRow(buffer, [.. slices]);
			}
		}

		return csv;
	}
}
