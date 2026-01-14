Write-Host "Starting restructure..."
if (Test-Path "Unci") { Remove-Item "Unci" -Recurse -Force; Write-Host "Removed Unci" }
if (Test-Path "restructure.bat") { Remove-Item "restructure.bat" -Force; Write-Host "Removed restructure.bat" }

New-Item -ItemType Directory -Path "external" -Force | Out-Null
if (Test-Path "Unciv") { Move-Item "Unciv" "external\" -Force; Write-Host "Moved Unciv" }
if (Test-Path "The-Powder-Toy") { Move-Item "The-Powder-Toy" "external\" -Force; Write-Host "Moved The-Powder-Toy" }

New-Item -ItemType Directory -Path "src" -Force | Out-Null
if (Test-Path "Terrarium.Desktop") { Move-Item "Terrarium.Desktop" "src\" -Force; Write-Host "Moved Terrarium.Desktop" }
if (Test-Path "Terrarium.Logic") { Move-Item "Terrarium.Logic" "src\" -Force; Write-Host "Moved Terrarium.Logic" }
if (Test-Path "Terrarium.Tests") { Move-Item "Terrarium.Tests" "src\" -Force; Write-Host "Moved Terrarium.Tests" }
if (Test-Path "DesktopTerrarium.sln") { Move-Item "DesktopTerrarium.sln" "src\" -Force; Write-Host "Moved DesktopTerrarium.sln" }

New-Item -ItemType Directory -Path "docs" -Force | Out-Null
Get-ChildItem -Path . -Filter "*.md" | Where-Object { $_.Name -ne "README.md" } | Move-Item -Destination "docs\" -Force

New-Item -ItemType Directory -Path "scripts" -Force | Out-Null
Get-ChildItem -Path . -Filter "*.bat" | Where-Object { $_.Name -ne "restructure_v2.bat" } | Move-Item -Destination "scripts\" -Force

Write-Host "Restructure Complete."
