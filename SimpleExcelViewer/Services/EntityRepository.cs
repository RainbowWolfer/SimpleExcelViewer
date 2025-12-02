using DevExpress.Mvvm;
using LiteDB;
using System.Diagnostics.CodeAnalysis;

namespace SimpleExcelViewer.Services;

public abstract class EntityRepository : BindableBase, IDisposable {
	public abstract string FilePath { get; }

	protected LiteDatabase? Database { get; private set; }

	[MemberNotNull(nameof(Database))]
	public void Initialize() {
		Database = new LiteDatabase(FilePath);
	}

	public virtual void Dispose() {
		Database?.Dispose();
	}

}
