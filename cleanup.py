import os
import shutil

root = r"c:\Users\Hostilian\The-Desktop-Terrarium"
docs = os.path.join(root, "docs")

to_delete = [
    "format_log.txt",
    "git_status_log.txt",
    "git_status_whitespace_log.txt",
    "install_log.txt",
    "whitespace_log.txt",
    "run_fix.bat",
    "run_whitespace_fix.bat",
    "cleanup_slop.bat"
]

folders_to_delete = [
    ".planning",
    "TestResults",
    "test-results"
]

docs_to_delete = [
    "AGENTS.md",
    "CODE_QUALITY_IMPROVEMENTS.md",
    "EXAM_COMPLIANCE.md",
    "FINAL_VERDICT.md",
    "PROJECT_COMPLETE_ALL_STAGES.md",
    "STAGE_A_CONCEPT_UX_VISUAL_IDENTITY.md",
    "STAGE_C_EXAM_COMPLIANCE.md",
    "STAGE_D_CI_CD_PIPELINE.md",
    "VERIFICATION_CHECKLIST.md"
]

for f in to_delete:
    p = os.path.join(root, f)
    if os.path.exists(p):
        print(f"Deleting file: {p}")
        try:
            os.remove(p)
        except Exception as e:
            print(f"Error deleting {p}: {e}")

for d in folders_to_delete:
    p = os.path.join(root, d)
    if os.path.exists(p):
        print(f"Deleting folder: {p}")
        try:
            shutil.rmtree(p)
        except Exception as e:
            print(f"Error deleting {p}: {e}")

for f in docs_to_delete:
    p = os.path.join(docs, f)
    if os.path.exists(p):
        print(f"Deleting doc: {p}")
        try:
            os.remove(p)
        except Exception as e:
            print(f"Error deleting {p}: {e}")

print("Cleanup script finished.")
