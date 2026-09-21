@echo off
setlocal enabledelayedexpansion

set "CSPROJ_FILE="
for %%f in (*.csproj) do (
    set "CSPROJ_FILE=%%~ff"
    goto :found
)

:found
if "%CSPROJ_FILE%"=="" (
    echo [ERROR] File .csproj not found in the current directory
    pause
    exit /b
)

echo Found project: %CSPROJ_FILE%, starting build
echo.

dotnet publish "%CSPROJ_FILE%" -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64 -p:WindowsPackageType=None

echo.
echo Build finished
pause