@echo off
REM Simple fix for large files in Git

echo ====================================
echo Removing Large Files from Repository
echo ====================================

REM Step 1: Delete the actual large files locally
echo.
echo Step 1: Deleting large folders locally...
if exist "release_build" rmdir /S /Q "release_build"
if exist "publish" rmdir /S /Q "publish"  
if exist "publish-integrity" rmdir /S /Q "publish-integrity"
if exist "__pycache__" rmdir /S /Q "__pycache__"
if exist "TestResults" rmdir /S /Q "TestResults"
if exist ".vscode" rmdir /S /Q ".vscode"
if exist ".claude" rmdir /S /Q ".claude"

REM Step 2: Stage the deletions
echo.
echo Step 2: Staging changes...
git add -A

REM Step 3: Commit
echo.
echo Step 3: Committing deletions...
git commit -m "Remove large build artifacts and folders"

REM Step 4: Try to push
echo.
echo Step 4: Attempting push...
git push origin main

echo.
echo ====================================
echo If push still fails with file size error,
echo the large files exist in OLD commits.
echo You'll need to use git filter-repo or BFG.
echo ====================================
pause
