@echo off
REM NUCLEAR OPTION: Fresh repository with no history of large files

echo ========================================
echo NUCLEAR FIX: Creating Fresh Repository
echo ========================================
echo.
echo This will create a brand new Git history
echo without any large files.
echo.
pause

REM Step 1: Backup current .git
echo Step 1: Backing up old .git folder...
if exist ".git.backup" rmdir /S /Q ".git.backup"
move .git .git.backup

REM Step 2: Initialize fresh repo
echo Step 2: Creating fresh Git repository...
git init
git branch -M main

REM Step 3: Add remote
echo Step 3: Adding GitHub remote...
git remote add origin https://github.com/Hostilian/The-Desktop-Terrarium.git

REM Step 4: Add only source files (no build outputs)
echo Step 4: Adding source files...
git add .gitignore
git add README.md
git add LICENSE
git add global.json
git add .editorconfig
git add clean_build.bat
git add create_release.bat
git add CLEANUP.md

git add .github/
git add .planning/
git add Terrarium.Desktop/
git add Terrarium.Logic/
git add Terrarium.Tests/
git add docs/
git add scripts/
git add src/
git add widgets/

REM Step 5: Commit
echo Step 5: Creating fresh commit...
git commit -m "Fresh start: source code only, no build artifacts"

REM Step 6: Force push
echo Step 6: Force pushing to GitHub...
echo WARNING: This will overwrite GitHub history!
pause
git push origin main --force

echo.
echo ========================================
echo DONE! Your repo is now clean.
echo ========================================
pause
