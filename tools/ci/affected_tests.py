#!/usr/bin/env python3
"""Pick the unit and integration tests a pull request needs (run by the CI verify job).

Reads changed paths (one per line) on stdin and prints one line:
  all          shared code changed: run every test
  none         only documentation changed: the always-run tests are enough
  <filter>     a dotnet test --filter for the changed modules and the modules whose projects reference them

Architecture, host and Platform integration tests always run in CI; a push to master runs everything.
Rules: docs/sdd/standards/module-rules.md («Test structure») and docs/sdd/standards/testing-rules.md.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MODULES_DIR = ROOT / "src" / "Modules"
# Modules whose tests are not yet all under tests/<Project>/<Module>/ (stage 9 moves them): a change runs everything.
# Catalog: 42 unit test files at the UnitTests root; Reference: its pricing tests live under IntegrationTests.Pricing.
UNGROUPED = {"Catalog", "Reference"}


def module_names(root: Path = MODULES_DIR) -> list[str]:
    return sorted(p.name for p in root.iterdir() if p.is_dir())


def referenced_by(root: Path = MODULES_DIR) -> dict[str, set[str]]:
    """module -> modules whose projects reference any project of that module (from the .csproj files)."""
    modules = module_names(root)
    users = {m: set() for m in modules}
    for module in modules:
        for project in (root / module).rglob("*.csproj"):
            if "obj" in project.parts:
                continue
            for include in re.findall(r'<ProjectReference\s+Include="([^"]+)"', project.read_text(encoding="utf-8-sig")):
                provider = Path(include.replace("\\", "/")).name.split(".")[0]
                if provider != module and provider in users:
                    users[provider].add(module)
    return users


def affected(paths: list[str], users: dict[str, set[str]]) -> str:
    changed: set[str] = set()
    for path in paths:
        if not path or path.startswith("docs/") or path.endswith(".md"):
            continue
        parts = path.split("/")
        if parts[0] == "src" and len(parts) > 2 and parts[1] == "Modules" and parts[2] in users:
            changed.add(parts[2])
        elif parts[0] == "tests" and len(parts) > 3 and parts[1] in ("UnitTests", "IntegrationTests") and parts[2] in users:
            changed.add(parts[2])
        elif parts[0] == "tests" and len(parts) > 1 and parts[1] == "ArchitectureTests":
            continue
        else:
            return "all"
    if not changed:
        return "none"
    modules = set(changed)
    for module in changed:
        modules |= users[module]
    if modules & UNGROUPED:
        return "all"
    return "|".join(f"FullyQualifiedName~{project}.{module}." for module in sorted(modules)
                    for project in ("UnitTests", "IntegrationTests"))


if __name__ == "__main__":
    print(affected([line.strip() for line in sys.stdin], referenced_by()))
