@echo off
echo Creating directories...
mkdir src 2>nul
mkdir widgets 2>nul
mkdir docs 2>nul
mkdir scripts 2>nul

echo Moving Source Code...
move /Y Terrarium.Desktop src\
move /Y Terrarium.Logic src\
move /Y Terrarium.Tests src\
move /Y DesktopTerrarium.sln src\
move /Y Unciv src\
move /Y "The-Powder-Toy" src\

echo Moving Widgets...
move /Y *.py widgets\
move /Y requirements.txt widgets\

echo Moving Documentation...
move /Y *.md docs\

echo Moving Scripts...
move /Y build.bat scripts\
move /Y gsd.bat scripts\
move /Y gsd.ps1 scripts\
move /Y run_simulator.bat scripts\
move /Y create_release.bat scripts\

echo Done!
pause
