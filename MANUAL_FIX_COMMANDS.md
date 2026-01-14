# ============================================
# MANUAL FIX: Remove Large Files from Git History
# ============================================
# Copy and paste these commands ONE BY ONE into PowerShell

# Step 1: Delete the large folders locally
Remove-Item -Recurse -Force release_build -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force publish -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force publish-integrity -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force __pycache__ -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force TestResults -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .vscode -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .claude -ErrorAction SilentlyContinue
Remove-Item -Force "60%" -ErrorAction SilentlyContinue

# Step 2: Install git-filter-repo (easiest way to purge files)
# Download from: https://github.com/newren/git-filter-repo/blob/main/git-filter-repo
# Or use pip:
pip install git-filter-repo

# Step 3: Backup your repo (just in case)
Copy-Item -Recurse .git .git.backup

# Step 4: Remove the large folders from ALL commits
git filter-repo --path release_build --invert-paths --force
git filter-repo --path publish --invert-paths --force
git filter-repo --path publish-integrity --invert-paths --force

# Step 5: Add back the remote (filter-repo removes it)
git remote add origin https://github.com/Hostilian/The-Desktop-Terrarium.git

# Step 6: Force push the cleaned history
git push origin main --force

# ============================================
# ALTERNATIVE: If git-filter-repo doesn't work
# ============================================

# Use BFG Repo Cleaner (faster, simpler)
# 1. Download BFG from: https://rtyley.github.io/bfg-repo-cleaner/
# 2. Run:
java -jar bfg.jar --delete-folders release_build
java -jar bfg.jar --delete-folders publish
java -jar bfg.jar --delete-folders publish-integrity

# 3. Clean up
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 4. Force push
git push origin main --force

# ============================================
# SIMPLEST: Fresh start (nuclear option)
# ============================================

# 1. Backup old .git
Move-Item .git .git.old

# 2. Create fresh repo
git init
git branch -M main

# 3. Add only source files
git add Terrarium.Desktop/ Terrarium.Logic/ Terrarium.Tests/
git add docs/ scripts/ widgets/ src/
git add .github/ .gitignore README.md LICENSE global.json
git add clean_build.bat CLEANUP.md

# 4. Commit
git commit -m "Fresh start: source code only"

# 5. Set remote and force push
git remote add origin https://github.com/Hostilian/The-Desktop-Terrarium.git
git push origin main --force
