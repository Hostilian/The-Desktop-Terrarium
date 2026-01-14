$ErrorActionPreference = "Stop"

function Find-Project {
    param($Path)
    if (Test-Path "src\$Path") { return "src\$Path" }
    if (Test-Path "$Path") { return "$Path" }
    return $null
}

$Sln = Find-Project "DesktopTerrarium.sln"
$MainProj = Find-Project "Terrarium.Desktop\Terrarium.Desktop.csproj"
$TestProj = Find-Project "Terrarium.Tests\Terrarium.Tests.csproj"

if (-not $MainProj) { Write-Error "Main Project not found!"; exit 1 }

if ($Sln) {
    Write-Host "Restoring dependencies ($Sln)..."
    try {
        dotnet restore "$Sln"
    }
    catch {
        Write-Warning "Failed to restore solution. Attempting direct project restore..."
        dotnet restore "$MainProj"
    }
}
else {
    Write-Host "Restoring project ($MainProj)..."
    dotnet restore "$MainProj"
}

if ($TestProj) {
    Write-Host "Running Tests ($TestProj)..."
    dotnet test "$TestProj"
}

Write-Host "Building Release to 'release_build' directory..."
dotnet publish "$MainProj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o release_build

# Copy Assets
Write-Host "Copying Assets..."
if (Test-Path "widgets") {
    Copy-Item "widgets" -Destination "release_build" -Recurse -Force
    Write-Host "Copied widgets."
}

if (Test-Path "external") {
    Copy-Item "external" -Destination "release_build" -Recurse -Force
    Write-Host "Copied external apps."
}
else {
    # Check for root folders if external doesn't exist
    if (Test-Path "Unciv") { Copy-Item "Unciv" -Destination "release_build\external\Unciv" -Recurse -Force; Write-Host "Copied Unciv." }
    if (Test-Path "The-Powder-Toy") { Copy-Item "The-Powder-Toy" -Destination "release_build\external\The-Powder-Toy" -Recurse -Force; Write-Host "Copied The-Powder-Toy." }
}

Write-Host "Build Complete!"
Write-Host "Run 'release_build\Terrarium.Desktop.exe' to start."
