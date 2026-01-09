import os
import re
import sys
from pathlib import Path


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
    index = docs_dir / "index.html"

    if not index.exists():
        print("ERROR: docs/index.html not found")
        return 2

    html = index.read_text(encoding="utf-8", errors="replace")
    missing = []

    for ref in sorted(set(iter_local_refs(html))):
        # Normalize leading './'
        ref_path = ref[2:] if ref.startswith("./") else ref

        # Root-relative within docs isn't expected; treat as docs-relative.
        ref_path = ref_path.lstrip("/")

        candidate = (docs_dir / ref_path).resolve()
        # Ensure candidate stays under docs/
        try:
            candidate.relative_to(docs_dir.resolve())
        except ValueError:
            missing.append(ref)
            continue

        if not candidate.exists():
            missing.append(ref)

    if missing:
        print("ERROR: docs/index.html contains missing local references:")
        for ref in missing:
            print(f" - {ref}")
        return 3

    print("OK: docs/index.html local references look valid")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
