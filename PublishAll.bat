@echo off
chcp 65001 >nul
echo Starting batch publish for SimpleExcelViewer...

:: Use a directory outside the immediate project folder to prevent MSBuild conflicts
set "FinalOutDir=bin"
set "TempDir=bin\Temp"

:: Clean up old directories
if exist "%FinalOutDir%" rd /s /q "%FinalOutDir%"
mkdir "%FinalOutDir%"

echo.
echo [1/4] Building: Self-Contained - 64-bit
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x64 -p:SelfContained=true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%TempDir%"
:: Move and rename the exe
move /y "%TempDir%\SimpleExcelViewer.exe" "%FinalOutDir%\SimpleExcelViewer_x64_Portable.exe"
:: Copy README.md (only needed once)
if exist "%TempDir%\README.md" copy /y "%TempDir%\README.md" "%FinalOutDir%\README.md" >nul
:: Clean up temp folder
rd /s /q "%TempDir%"

echo.
echo [2/4] Building: Self-Contained - 32-bit
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x86 -p:SelfContained=true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%TempDir%"
move /y "%TempDir%\SimpleExcelViewer.exe" "%FinalOutDir%\SimpleExcelViewer_x86_Portable.exe"
rd /s /q "%TempDir%"

echo.
echo [3/4] Building: Framework-Dependent - 64-bit
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false -o "%TempDir%"
move /y "%TempDir%\SimpleExcelViewer.exe" "%FinalOutDir%\SimpleExcelViewer_x64.exe"
rd /s /q "%TempDir%"

echo.
echo [4/4] Building: Framework-Dependent - 32-bit
dotnet publish SimpleExcelViewer\SimpleExcelViewer.csproj -c Release -f net8.0-windows -r win-x86 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false -o "%TempDir%"
move /y "%TempDir%\SimpleExcelViewer.exe" "%FinalOutDir%\SimpleExcelViewer_x86.exe"
rd /s /q "%TempDir%"

echo.
echo =========================================
echo All publishing completed! Files have been output to the %FinalOutDir% directory.
echo =========================================
pause