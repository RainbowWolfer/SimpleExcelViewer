using DevExpress.Mvvm;
using LiteDB;
using System.Diagnostics.CodeAnalysis;

namespace SimpleExcelViewer.Services;

public abstract class EntityRepository : EntityRepositoryBase, IDisposable {
	protected LiteDatabase? Database { get; private set; }

	[MemberNotNull(nameof(Database))]
	public void Initialize() {
		Database = GetDatabase();
	}

	public virtual void Dispose() {
		Database?.Dispose();
	}

}

public abstract class EntityRepositoryBase : BindableBase {
	public abstract string FilePath { get; }

	protected virtual LiteDatabase GetDatabase() {
		return new LiteDatabase(FilePath);
	}
}