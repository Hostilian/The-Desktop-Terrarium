# Repository Cleanup Commands

## 1. Remove tracked files that should be ignored

Run these commands from your repository root:

```bash
# Stop tracking build artifacts (but keep locally)
git rm -r --cached publish/
git rm -r --cached publish-integrity/
git rm -r --cached release_build/
git rm -r --cached __pycache__/
git rm -r --cached TestResults/
git rm --cached "60%"

# Stop tracking .claude if not needed
git rm -r --cached .claude/

# Stop tracking user settings if accidentally committed
git rm --cached terrarium_settings.json

# Stop tracking .vscode if present
git rm -r --cached .vscode/ 2>/dev/null || true
```

## 2. Delete unnecessary local files

```bash
# Delete redundant restructure scripts
del restructure.bat
del restructure.ps1  
del restructure_v2.bat
del force_restructure.bat

# Delete the mysterious "60%" file
del "60%"
```

## 3. Commit the cleanup

```bash
git add .gitignore
git commit -m "Clean up repository: remove build artifacts and redundant files"
git push origin main
```

## 4. Verify cleanup

```bash
git status
```

## Files that SHOULD be in your repo:
- ✅ Source code (`.cs`, `.csproj`)
- ✅ Documentation (`README.md`, `docs/`)
- ✅ Configuration (`global.json`, `.editorconfig`)
- ✅ Build scripts (`clean_build.bat`, `scripts/build.ps1`)
- ✅ CI/CD (`.github/workflows/`)
- ✅ Widgets source (`widgets/`)

## Files that should NOT be in your repo:
- ❌ Build outputs (`bin/`, `obj/`, `publish/`, `release_build/`)
- ❌ Executables (`.exe`, `.dll` from builds)
- ❌ User settings (`terrarium_settings.json`)
- ❌ IDE folders (`.vscode/`, `.vs/`, `.idea/`)
- ❌ Python cache (`__pycache__/`)
- ❌ Test results (`TestResults/`)
