using DevExpress.Mvvm;
using FreeSql;
using System.Diagnostics.CodeAnalysis;

//using LiteDB;

namespace SimpleExcelViewer.Services;

public abstract class EntityRepository : BindableBase, IDisposable {
	public abstract string FilePath { get; }

	protected IFreeSql? Orm { get; private set; }
	//protected LiteDatabase? Database { get; private set; }

	//[MemberNotNull(nameof(Database))]
	[MemberNotNull(nameof(Orm))]
	public void Initialize() {
		//Database = new LiteDatabase(FilePath);
		Orm = new FreeSqlBuilder()
			.UseConnectionString(DataType.Sqlite, $"Data Source={FilePath}")
			.UseAutoSyncStructure(true) // 自动建表
			.Build();
	}

	public virtual void Dispose() {
		//Database?.Dispose();
		Orm?.Dispose();
	}

}
