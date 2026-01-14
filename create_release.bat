@echo off
echo ========================================================
echo   Terrarium Desktop - Build & Release Wrapper
echo ========================================================
echo.
echo Calling modern build script...
powershell -ExecutionPolicy Bypass -File scripts\build.ps1
echo.
if %ERRORLEVEL% EQU 0 (
    echo ========================================================
    echo   Build SUCCESS!
    echo   Run: publish\Terrarium.Desktop.exe
    echo ========================================================
) else (
    echo ========================================================
    echo   Build FAILED.
    echo ========================================================
)
pause
