; SimpleExcelViewer.iss - 安装 bin\Release（AnyCPU 多文件版）

#include "Common.iss"

[Setup]
OutputBaseFilename={#MyAppName2}_v{#MyAppVersion}_Installer

[Files]
; 安装整个 Release 文件夹（exe + 所有 DLL）
Source: "{#MyPublishDir}\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs