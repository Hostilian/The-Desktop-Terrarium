import os
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def find_reports(root: Path):
    return sorted(root.rglob("coverage.cobertura.xml"))


def parse_cobertura(path: Path):
    tree = ET.parse(path)
    root = tree.getroot()

    # Cobertura root often has line-rate, but we compute from totals where possible.
    lines_valid = 0
    lines_covered = 0

    # Some generators include <coverage lines-valid=".." lines-covered=".."/>
    if root.tag.lower().endswith("coverage"):
        lv = root.attrib.get("lines-valid")
        lc = root.attrib.get("lines-covered")
        if lv is not None and lc is not None:
            try:
                return int(lc), int(lv)
            except ValueError:
                pass

    # Fall back: sum per-class line hits
    for line in root.findall(".//line"):
        hits = line.attrib.get("hits")
        if hits is None:
            continue
        try:
            hit_count = int(hits)
        except ValueError:
            continue
        lines_valid += 1
        if hit_count > 0:
            lines_covered += 1

    return lines_covered, lines_valid


def append_step_summary(markdown: str):
    summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if not summary_path:
        return
    try:
        with open(summary_path, "a", encoding="utf-8") as f:
            f.write(markdown)
            if not markdown.endswith("\n"):
                f.write("\n")
    except OSError:
        # Don't fail CI if summary writing fails
        pass


def main() -> int:
    root = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else Path("TestResults").resolve()

    reports = find_reports(root)
    if not reports:
        print(f"No coverage reports found under: {root}")
        append_step_summary("## Code coverage\n\nNo coverage report found.\n")
        return 0

    total_covered = 0
    total_valid = 0

    for report in reports:
        covered, valid = parse_cobertura(report)
        total_covered += covered
        total_valid += valid

    percent = (total_covered / total_valid * 100.0) if total_valid else 0.0

    print(f"Coverage: {percent:.2f}% ({total_covered}/{total_valid} lines)")

    md = (
        "## Code coverage\n\n"
        f"- Line coverage: **{percent:.2f}%** ({total_covered}/{total_valid})\n"
        f"- Reports found: {len(reports)}\n"
    )
    append_step_summary(md)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
