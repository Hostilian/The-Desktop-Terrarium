# BFG Repo Cleaner - PowerShell Version
# Removes large files from Git history

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BFG Repo Cleaner - Removing Large Files" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if Java is installed
try {
    $null = java -version 2>&1
    Write-Host "✓ Java detected" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: Java is not installed!" -ForegroundColor Red
    Write-Host "Please install Java from: https://www.java.com/download/"
    Read-Host "Press Enter to exit"
    exit 1
}

# Step 1: Download BFG if not present
if (-not (Test-Path "bfg.jar")) {
    Write-Host ""
    Write-Host "Downloading BFG Repo Cleaner..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri "https://repo1.maven.org/maven2/com/madgag/bfg/1.14.0/bfg-1.14.0.jar" -OutFile "bfg.jar"
    Write-Host "✓ Downloaded bfg.jar" -ForegroundColor Green
}

# Step 2: Create mirror clone
Write-Host ""
Write-Host "Step 1: Creating mirror clone..." -ForegroundColor Yellow
Set-Location ..
if (Test-Path "Terrarium-Mirror") {
    Remove-Item -Recurse -Force "Terrarium-Mirror"
}
git clone --mirror https://github.com/Hostilian/The-Desktop-Terrarium.git Terrarium-Mirror
Set-Location Terrarium-Mirror

# Copy BFG
Copy-Item ..\The-Desktop-Terrarium\bfg.jar bfg.jar

# Step 3: Run BFG
Write-Host ""
Write-Host "Step 2: Running BFG to remove large folders..." -ForegroundColor Yellow

Write-Host "  Removing release_build..." -ForegroundColor Gray
java -jar bfg.jar --delete-folders release_build

Write-Host "  Removing publish..." -ForegroundColor Gray
java -jar bfg.jar --delete-folders publish

Write-Host "  Removing publish-integrity..." -ForegroundColor Gray
java -jar bfg.jar --delete-folders publish-integrity

# Step 4: Clean up
Write-Host ""
Write-Host "Step 3: Cleaning repository..." -ForegroundColor Yellow
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# Step 5: Push
Write-Host ""
Write-Host "Step 4: Force pushing to GitHub..." -ForegroundColor Yellow
Write-Host "WARNING: This will overwrite GitHub history!" -ForegroundColor Red
Read-Host "Press Enter to continue or Ctrl+C to cancel"

git push --force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ DONE! Repository cleaned." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Now update your local repo:" -ForegroundColor Yellow
Write-Host "  cd ..\The-Desktop-Terrarium" -ForegroundColor Cyan
Write-Host "  git fetch origin" -ForegroundColor Cyan
Write-Host "  git reset --hard origin/main" -ForegroundColor Cyan
Write-Host ""
Read-Host "Press Enter to exit"
