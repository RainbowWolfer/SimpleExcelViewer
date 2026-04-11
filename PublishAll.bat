::[Bat To Exe Converter]
::
::YAwzoRdxOk+EWAnk
::fBw5plQjdG8=
::YAwzuBVtJxjWCl3EqQJgSA==
::ZR4luwNxJguZRRnk
::Yhs/ulQjdF+5
::cxAkpRVqdFKZSDk=
::cBs/ulQjdF+5
::ZR41oxFsdFKZSDk=
::eBoioBt6dFKZSDk=
::cRo6pxp7LAbNWATEpCI=
::egkzugNsPRvcWATEpCI=
::dAsiuh18IRvcCxnZtBJQ
::cRYluBh/LU+EWAnk
::YxY4rhs+aU+JeA==
::cxY6rQJ7JhzQF1fEqQJQ
::ZQ05rAF9IBncCkqN+0xwdVs0
::ZQ05rAF9IAHYFVzEqQJQ
::eg0/rx1wNQPfEVWB+kM9LVsJDGQ=
::fBEirQZwNQPfEVWB+kM9LVsJDGQ=
::cRolqwZ3JBvQF1fEqQJQ
::dhA7uBVwLU+EWDk=
::YQ03rBFzNR3SWATElA==
::dhAmsQZ3MwfNWATElA==
::ZQ0/vhVqMQ3MEVWAtB9wSA==
::Zg8zqx1/OA3MEVWAtB9wSA==
::dhA7pRFwIByZRRnk
::Zh4grVQjdCuDJGqL4VAzLSdGSRSNL1eJD7gM5O3e9+mCrnsUUfU6arPrz7aCKfMby0noO5M10xo=
::YB416Ek+ZG8=
::
::
::978f952a14a936cc963da21a135fa983
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