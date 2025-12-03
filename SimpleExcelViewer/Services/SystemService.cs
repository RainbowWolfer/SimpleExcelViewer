using Microsoft.Win32;
using RW.Base.WPF.DependencyInjections;
using RW.Common.Helpers;
using System.IO;
using System.Reflection;

namespace SimpleExcelViewer.Services;

internal class SystemService : ISingletonDependency {

	private const string ProgId = "SimpleExcelViewer"; // 默认的 csv 文件类型标识
	private const string MenuKeyName = "OpenWithSimpleExcelViewer"; // 自定义菜单项名称
	private const string MenuText = "Open With SimpleExcelViewer"; // 显示在右键菜单里的文字

	/// <summary>
	/// 注册右键菜单：重复点击可修复路径
	/// </summary>
	public bool RegisterCsvContextMenu() {
		string? exePath = Assembly.GetEntryAssembly().Location;

		if (exePath is null) {
			return false;
		}

		// 确保 .csv 文件有正确的 ProgId
		using RegistryKey csvKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.csv");
		string? progId = csvKey.GetValue("") as string;
		if (progId.IsBlank()) {
			csvKey.SetValue("", ProgId);
			progId = ProgId;
		}

		// 创建右键菜单项
		string shellPath = $@"Software\Classes\{progId}\shell\{MenuKeyName}";
		using RegistryKey shellKey = Registry.CurrentUser.CreateSubKey(shellPath);
		shellKey.SetValue("", MenuText);

		using RegistryKey cmdKey = shellKey.CreateSubKey("command");
		string? oldPath = cmdKey.GetValue("") as string;

		// 如果路径不存在或不同，更新为当前 exe 路径
		if (string.IsNullOrEmpty(oldPath) || !File.Exists(GetExeFromCommand(oldPath))) {
			cmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
		}

		return true;
	}

	/// <summary>
	/// 取消注册右键菜单
	/// </summary>
	public void UnregisterCsvContextMenu() {
		string shellPath = $@"Software\Classes\{ProgId}\shell\{MenuKeyName}";
		Registry.CurrentUser.DeleteSubKeyTree(shellPath, false);
	}

	/// <summary>
	/// 从命令字符串中提取 exe 路径
	/// </summary>
	private string? GetExeFromCommand(string? command) {
		if (command.IsBlank()) {
			return null;
		}

		// 通常格式为 "C:\path\app.exe" "%1"
		int firstQuote = command.IndexOf('"');
		int secondQuote = command.IndexOf('"', firstQuote + 1);
		if (firstQuote >= 0 && secondQuote > firstQuote) {
			return command.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
		}
		return null;
	}

}
