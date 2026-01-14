@echo off
echo ========================================================
echo   Terrarium Desktop - FORCE CLEAN BUILD
echo ========================================================

echo 1. Killing any stuck processes...
taskkill /F /IM Terrarium.Desktop.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1

echo 2. Deleting old publish folder...
rmdir /S /Q publish
if exist publish (
    echo ERROR: Could not delete publish folder. It might be locked.
    echo Please restart your computer and try again.
    pause
    exit /b 1
)

echo 3. Building Project...
dotnet publish Terrarium.Desktop\Terrarium.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

if %ERRORLEVEL% NEQ 0 (
    echo Build Failed!
    pause
    exit /b 1
)

echo 4. Copying Assets...
if exist widgets xcopy widgets publish\widgets /E /I /Y >nul
if exist Unciv xcopy Unciv publish\external\Unciv /E /I /Y >nul
if exist The-Powder-Toy xcopy The-Powder-Toy publish\external\The-Powder-Toy /E /I /Y >nul

echo.
echo ========================================================
echo   BUILD SUCCESSFUL!
echo   Run: publish\Terrarium.Desktop.exe
echo ========================================================
pause
