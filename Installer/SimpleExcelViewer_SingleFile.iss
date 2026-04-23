; SimpleExcelViewer_SingleFile.iss - 安装 SingleFile x64/x86 版

#include "Common.iss"

[Setup]
OutputBaseFilename={#MyAppName2}_SingleFile_v{#MyAppVersion}_Installer

[Files]
Source: "{#MyPublishDir}\SimpleExcelViewer_x64.exe"; DestDir: "{app}"; DestName: "{#MyAppExeName}"; Check: Is64BitInstallMode; Flags: ignoreversion solidbreak
Source: "{#MyPublishDir}\SimpleExcelViewer_x86.exe"; DestDir: "{app}"; DestName: "{#MyAppExeName}"; Check: not Is64BitInstallMode; Flags: ignoreversion