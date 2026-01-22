@echo off
echo ========================================================
echo   Building Desktop Terrarium MSI Installer
echo ========================================================
echo.

echo 1. Installing WiX Toolset (if needed)...
dotnet tool install --global wix

echo 2. Ensuring Release Build exists...
if not exist "src\Terrarium.Desktop\bin\Release\net8.0-windows\Terrarium.Desktop.exe" (
    echo Build artifacts not found. Building project...
    dotnet build src/DesktopTerrarium.sln -c Release
)

echo 3. Building MSI...
echo Note: This requires WiX 4. If you have WiX 3, use candle/light manually.
echo.
wix build installer\Product.wxs -d Terrarium.Desktop.TargetDir=src\Terrarium.Desktop\bin\Release\net8.0-windows\ -o DesktopTerrarium.msi

if %ERRORLEVEL% equ 0 (
    echo.
    echo SUCCESS: DesktopTerrarium.msi created!
) else (
    echo.
    echo ERROR: Build failed. Check output above.
)
pause
