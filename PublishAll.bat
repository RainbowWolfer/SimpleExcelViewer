@echo off
chcp 65001 >nul
echo Starting batch publish for SimpleExcelViewer...

:: Set main output directory
set "OutDir=bin"

:: Clean up old directory (optional)
if exist "%OutDir%" rd /s /q "%OutDir%"

echo.
echo [1/4] Building: Self-Contained - 64-bit
:: Self-Contained: Includes runtime, enables ReadyToRun (improves startup speed), includes native C++ libraries
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x64 -p:SelfContained=true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OutDir%\SelfContained_x64"

echo.
echo [2/4] Building: Self-Contained - 32-bit
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x86 -p:SelfContained=true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%OutDir%\SelfContained_x86"

echo.
echo [3/4] Building: Framework-Dependent - 64-bit
:: Framework-Dependent: Explicitly set SelfContained to false, and disable ReadyToRun and native library extraction
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false -o "%OutDir%\FrameworkDependent_x64"

echo.
echo [4/4] Building: Framework-Dependent - 32-bit
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x86 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false -o "%OutDir%\FrameworkDependent_x86"

echo.
echo =========================================
echo All publishing completed! Files have been output to the %OutDir% directory.
echo =========================================
pause