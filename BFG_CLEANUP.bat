@echo off
REM BFG Repo Cleaner Script
REM Removes large files from Git history

echo ========================================
echo BFG Repo Cleaner - Removing Large Files
echo ========================================
echo.

REM Check if Java is installed
java -version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Java is not installed!
    echo Please install Java from: https://www.java.com/download/
    pause
    exit /b 1
)

echo Java detected. Proceeding...
echo.

REM Step 1: Create a fresh clone for BFG
echo Step 1: Creating mirror clone...
cd ..
if exist "Terrarium-Mirror" rmdir /S /Q "Terrarium-Mirror"
git clone --mirror https://github.com/Hostilian/The-Desktop-Terrarium.git Terrarium-Mirror
cd Terrarium-Mirror

REM Step 2: Run BFG to delete large folders
echo.
echo Step 2: Running BFG to remove large folders...

REM Copy BFG jar to mirror directory
copy ..\The-Desktop-Terrarium\bfg.jar bfg.jar

echo Removing release_build folder...
java -jar bfg.jar --delete-folders release_build

echo Removing publish folder...
java -jar bfg.jar --delete-folders publish

echo Removing publish-integrity folder...
java -jar bfg.jar --delete-folders publish-integrity

REM Step 3: Clean up
echo.
echo Step 3: Running git reflog and gc...
git reflog expire --expire=now --all
git gc --prune=now --aggressive

REM Step 4: Push cleaned repo
echo.
echo Step 4: Force pushing cleaned repository...
echo WARNING: This will overwrite GitHub history!
pause
git push --force

echo.
echo ========================================
echo DONE! Repository cleaned.
echo ========================================
echo.
echo Now go back to your main repo:
echo   cd ..\The-Desktop-Terrarium
echo   git pull --force origin main
echo.
pause
