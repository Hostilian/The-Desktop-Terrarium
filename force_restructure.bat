@echo off
echo Force Restructure...

if not exist src\Terrarium.Desktop (
    echo Copying Terrarium.Desktop...
    xcopy Terrarium.Desktop src\Terrarium.Desktop /s /e /h /i /y
    if exist src\Terrarium.Desktop\Terrarium.Desktop.csproj (
        echo Copy Success. Removing source...
        rmdir /s /q Terrarium.Desktop
    )
)

if not exist src\Terrarium.Logic (
    echo Copying Terrarium.Logic...
    xcopy Terrarium.Logic src\Terrarium.Logic /s /e /h /i /y
    if exist src\Terrarium.Logic\Terrarium.Logic.csproj (
        echo Copy Success. Removing source...
        rmdir /s /q Terrarium.Logic
    )
)

if not exist src\Terrarium.Tests (
    echo Copying Terrarium.Tests...
    xcopy Terrarium.Tests src\Terrarium.Tests /s /e /h /i /y
    if exist src\Terrarium.Tests\Terrarium.Tests.csproj (
        echo Copy Success. Removing source...
        rmdir /s /q Terrarium.Tests
    )
)

echo Done.
exit /b 0
