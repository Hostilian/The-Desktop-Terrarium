@echo off
echo Starting restructure...
if exist Unci (
    echo Removing Unci...
    rmdir /S /Q Unci
)
if exist restructure.bat (
    echo Removing restructure.bat
    del restructure.bat
)
if not exist external mkdir external
if exist Unciv (
    echo Moving Unciv...
    move Unciv external\
)
if exist "The-Powder-Toy" (
    echo Moving The-Powder-Toy...
    move "The-Powder-Toy" external\
)
if not exist src mkdir src
if exist Terrarium.Desktop (
    echo Moving Terrarium.Desktop...
    move Terrarium.Desktop src\
)
if exist Terrarium.Logic (
    echo Moving Terrarium.Logic...
    move Terrarium.Logic src\
)
if exist Terrarium.Tests (
    echo Moving Terrarium.Tests...
    move Terrarium.Tests src\
)
if exist DesktopTerrarium.sln (
    echo Moving Solution...
    move DesktopTerrarium.sln src\
)
echo Done.
