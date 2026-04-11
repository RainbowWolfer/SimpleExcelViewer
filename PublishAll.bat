@echo off
chcp 65001 >nul
echo 开始批量发布 SimpleExcelViewer...

:: 设置主输出目录
set "OutDir=SimpleExcelViewer\bin\Publish\Matrix"

:: 清理旧目录（可选）
if exist "%OutDir%" rd /s /q "%OutDir%"

echo.
echo [1/4] 正在生成: 独立版 (Self-Contained) - 64位
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OutDir%\SelfContained_x64"

echo.
echo [2/4] 正在生成: 独立版 (Self-Contained) - 32位
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x86 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OutDir%\SelfContained_x86"

echo.
echo [3/4] 正在生成: 框架依赖版 (Framework-Dependent) - 64位
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OutDir%\FrameworkDependent_x64"

echo.
echo [4/4] 正在生成: 框架依赖版 (Framework-Dependent) - 32位
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x86 --self-contained false -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OutDir%\FrameworkDependent_x86"

echo.
echo =========================================
echo 全部发布完成！文件已输出到 %OutDir% 目录。
echo =========================================
pause