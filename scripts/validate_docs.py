import os
import re
import sys
from pathlib import Path


def validate_basic_html_structure(html: str) -> list[str]:
    errors: list[str] = []

    lowered = html.lower()
    if "<!doctype html" not in lowered:
        errors.append("Missing <!DOCTYPE html> declaration")

    required_tags = ["<html", "<head", "<body"]
    for tag in required_tags:
        if tag not in lowered:
            errors.append(f"Missing required tag: {tag}")

    if "</html>" not in lowered:
        errors.append("Missing closing </html> tag")

    if "<title" not in lowered:
        errors.append("Missing <title> tag")

    return errors


def iter_local_refs(html: str):
    # Very small validator: find href/src attributes and validate local file refs.
    # Skips: absolute URLs, anchors, mailto:, data:, javascript:.
    attr_re = re.compile(r"\b(?:href|src)\s*=\s*['\"]([^'\"]+)['\"]", re.IGNORECASE)
    for match in attr_re.finditer(html):
        value = match.group(1).strip()
        if not value:
            continue

        lowered = value.lower()
        if lowered.startswith(("http://", "https://", "mailto:", "data:", "javascript:")):
            continue
        if value.startswith("#"):
            continue

        # Strip query/fragment
        path = value.split("#", 1)[0].split("?", 1)[0]
        if not path:
            continue

        # Treat as local file
        yield path


def main() -> int:
    repo_root = Path(__file__).resolve().parents[1]
    docs_dir = repo_root / "docs"

    if not docs_dir.exists():
        print("ERROR: docs/ directory not found")
        return 2

    html_files = sorted(docs_dir.rglob("*.html"))
    if not html_files:
        print("ERROR: no .html files found under docs/")
        return 2

    docs_root_resolved = docs_dir.resolve()
    any_errors = False

    for html_path in html_files:
        html = html_path.read_text(encoding="utf-8", errors="replace")

        structure_errors = validate_basic_html_structure(html)
        if structure_errors:
            any_errors = True
            rel = html_path.relative_to(docs_dir)
            print(f"ERROR: docs/{rel} basic structure check failed:")
            for err in structure_errors:
                print(f" - {err}")

        missing = []
        for ref in sorted(set(iter_local_refs(html))):
            # Normalize leading './'
            ref_path = ref[2:] if ref.startswith("./") else ref

            # Root-relative within docs isn't expected; treat as docs-relative.
            ref_path = ref_path.lstrip("/")

            candidate = (docs_dir / ref_path).resolve()

            # Ensure candidate stays under docs/
            try:
                candidate.relative_to(docs_root_resolved)
            except ValueError:
                missing.append(ref)
                continue

            if not candidate.exists():
                missing.append(ref)

        if missing:
            any_errors = True
            rel = html_path.relative_to(docs_dir)
            print(f"ERROR: docs/{rel} contains missing local references:")
            for ref in missing:
                print(f" - {ref}")

    if any_errors:
        return 3

    print("OK: docs/*.html local references look valid")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
