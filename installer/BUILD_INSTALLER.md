# WiX Installer Instructions

To build the MSI installer for Desktop Terrarium, you need the **WiX Toolset** (v3.11 or later).

## 1. Install WiX Toolset
Download from: https://wixtoolset.org/releases/

## 2. Generate UUIDs
In `installer/Product.wxs`, replace the 3 placeholder GUIDs (`PUT-GUID-HERE...`) with real GUIDs.
You can generate them in PowerShell:
```powershell
[guid]::NewGuid()
```

## 3. Build the Installer

Run the following commands from the project root:

```powershell
# 1. Install WiX Toolset (if not installed)
dotnet tool install --global wix

# 2. Build the MSI
wix build installer/Product.wxs -d Terrarium.Desktop.TargetDir=src/Terrarium.Desktop/bin/Release/net8.0-windows/ -o DesktopTerrarium.msi
```

## 4. Test Installation
Run `DesktopTerrarium.msi` to install the application to `C:\Program Files (x86)\Desktop Terrarium`.

---
**Note:** For a complete production installer, use the `heat` tool to automatically harvest all DLLs and dependencies from the build output directory instead of listing them manually.
