@echo off
echo ========================================================
echo   Desktop Terrarium - DEMO MODE
echo ========================================================
echo.
echo This script will help you record the 60s demo video.
echo.
echo Steps:
echo 1. Open your screen recording software (OBS, ShareX, etc.)
echo 2. Set it to capture the application window
echo 3. The app will launch in 5 seconds...
echo.
timeout /t 5

echo Launching application...
if exist "src\Terrarium.Desktop\bin\Release\net8.0-windows\Terrarium.Desktop.exe" (
    start src\Terrarium.Desktop\bin\Release\net8.0-windows\Terrarium.Desktop.exe
) else (
    echo ERROR: Application binary not found!
    echo Please run 'dotnet build -c Release' first.
    pause
    exit /b 1
)

echo.
echo App launched!
echo Record the following for the demo:
echo - Show the main simulation running
echo - Switch between 2-3 terrarium types
echo - Use a God Power (e.g. Spawn Sun)
echo - Open Statistics (F3)
echo - Open Settings (F2)
echo.
echo Good luck!
pause
