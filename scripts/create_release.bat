@echo off
setlocal
echo ==========================================
echo    Desktop Terrarium Release Builder
echo ==========================================

:: 0. Navigate to Root
pushd %~dp0\..

:: 1. Check Prerequisites
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK not found. Please install .NET 8.0 SDK.
    pause
    exit /b 1
)

:: 2. Clean and Build
echo.
echo [1/4] Building Terrarium.Desktop (Release)...
dotnet publish Terrarium.Desktop\Terrarium.Desktop.csproj -c Release -o Publish /p:DebugType=None /p:DebugSymbols=false

if %errorlevel% neq 0 (
    echo [ERROR] Build failed.
    popd
    pause
    exit /b 1
)

:: 3. Copy Python Scripts (from widgets/)
echo.
echo [2/4] Copying Python modules...
copy /Y widgets\*.py Publish\ >nul
copy /Y widgets\requirements.txt Publish\ >nul

:: 4. Copy Documentation (from docs/)
echo.
echo [3/4] Copying documentation...
copy /Y docs\*.md Publish\ >nul

:: 5. Success
echo.
echo [4/4] Release created in 'Publish' folder!
echo.
echo ==========================================
echo    BUILD SUCCESSFUL
echo ==========================================
echo.
echo You can now distribute the contents of the 'Publish' folder.
echo To test, go to 'Publish' and run 'Terrarium.Desktop.exe'.
echo.

popd
pause
