@echo off
echo Terrarium Desktop Build Script
echo ==============================

:main_menu
echo.
echo Choose an action:
echo 1. Build Project
echo 2. Run Tests
echo 3. Clean Build
echo 4. Publish Application
echo 5. Run Application
echo 6. Exit
set /p action="Enter choice (1-6): "

if "%action%"=="1" goto build_menu
if "%action%"=="2" goto run_tests
if "%action%"=="3" goto clean_build
if "%action%"=="4" goto publish_menu
if "%action%"=="5" goto run_app
if "%action%"=="6" goto exit_script

echo Invalid choice. Please try again.
goto main_menu

:build_menu
echo.
echo Choose build configuration:
echo 1. Debug
echo 2. Release
set /p choice="Enter choice (1 or 2): "

if "%choice%"=="1" (
    set config=Debug
    echo Building in Debug mode...
) else if "%choice%"=="2" (
    set config=Release
    echo Building in Release mode...
) else (
    echo Invalid choice. Returning to main menu.
    goto main_menu
)

echo.
echo Restoring packages...
dotnet restore DesktopTerrarium.sln

echo.
echo Building solution...
dotnet build DesktopTerrarium.sln --configuration %config%

if %errorlevel% neq 0 (
    echo.
    echo Build failed! Press any key to continue.
    pause
    goto main_menu
)

echo.
echo Build successful!
goto main_menu

:run_tests
echo.
echo Running tests...
dotnet test Terrarium.Tests/Terrarium.Tests.csproj --verbosity normal

if %errorlevel% neq 0 (
    echo.
    echo Some tests failed! Press any key to continue.
    pause
) else (
    echo.
    echo All tests passed!
)
goto main_menu

:clean_build
echo.
echo Cleaning build artifacts...
dotnet clean DesktopTerrarium.sln
if exist "publish" rmdir /s /q publish
if exist "TestResults" rmdir /s /q TestResults
echo Clean complete!
goto main_menu

:publish_menu
echo.
echo Choose publish target:
echo 1. Windows x64
echo 2. Windows ARM64
echo 3. Both platforms
set /p pub_choice="Enter choice (1-3): "

if "%pub_choice%"=="1" (
    echo Publishing for Windows x64...
    dotnet publish Terrarium.Desktop/Terrarium.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output publish/win-x64
) else if "%pub_choice%"=="2" (
    echo Publishing for Windows ARM64...
    dotnet publish Terrarium.Desktop/Terrarium.Desktop.csproj --configuration Release --runtime win-arm64 --self-contained true --output publish/win-arm64
) else if "%pub_choice%"=="3" (
    echo Publishing for both platforms...
    dotnet publish Terrarium.Desktop/Terrarium.Desktop.csproj --configuration Release --runtime win-x64 --self-contained true --output publish/win-x64
    dotnet publish Terrarium.Desktop/Terrarium.Desktop.csproj --configuration Release --runtime win-arm64 --self-contained true --output publish/win-arm64
) else (
    echo Invalid choice. Returning to main menu.
    goto main_menu
)

if %errorlevel% neq 0 (
    echo.
    echo Publish failed! Press any key to continue.
    pause
) else (
    echo.
    echo Publish complete!
)
goto main_menu

:run_app
echo.
echo Running Terrarium Desktop...
dotnet run --project Terrarium.Desktop/Terrarium.Desktop.csproj
goto main_menu

:exit_script
echo.
echo Goodbye!
exit /b 0
