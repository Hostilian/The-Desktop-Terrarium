@echo off
echo Cleaning up slop...
del /F /Q format_log.txt git_status_log.txt git_status_whitespace_log.txt install_log.txt whitespace_log.txt run_fix.bat run_whitespace_fix.bat
rmdir /S /Q TestResults test-results .planning
del /F /Q docs\AGENTS.md docs\CODE_QUALITY_IMPROVEMENTS.md docs\EXAM_COMPLIANCE.md docs\FINAL_VERDICT.md docs\PROJECT_COMPLETE_ALL_STAGES.md docs\STAGE_A_CONCEPT_UX_VISUAL_IDENTITY.md docs\STAGE_C_EXAM_COMPLIANCE.md docs\STAGE_D_CI_CD_PIPELINE.md docs\VERIFICATION_CHECKLIST.md
echo Cleanup finished.
