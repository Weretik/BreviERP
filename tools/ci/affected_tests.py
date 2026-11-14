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
JOBS_DIR = ROOT / "src" / "Jobs"
# Operational job projects (src/Jobs) are tested under tests/<Project>/Jobs/. Catalog and Reference database tests
# seed their data through these jobs (TestSupport/SewingPricingDatabaseFixture), so a job change runs them too.
JOBS = "Jobs"
JOB_TEST_USERS = {"Catalog", "Reference"}


def module_names(root: Path = MODULES_DIR) -> list[str]:
    return sorted(p.name for p in root.iterdir() if p.is_dir())


def referenced_by(root: Path = MODULES_DIR, jobs: Path = JOBS_DIR) -> dict[str, set[str]]:
    """module -> modules (and Jobs) whose projects reference any project of that module (from the .csproj files)."""
    modules = module_names(root)
    users = {m: set() for m in modules}
    users[JOBS] = set(JOB_TEST_USERS)
    for project in jobs.rglob("*.csproj") if jobs.exists() else []:
        if "obj" in project.parts:
            continue
        for include in re.findall(r'<ProjectReference\s+Include="([^"]+)"', project.read_text(encoding="utf-8-sig")):
            provider = Path(include.replace("\\", "/")).name.split(".")[0]
            if provider in users and provider != JOBS:
                users[provider].add(JOBS)
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
        elif parts[0] == "src" and len(parts) > 2 and parts[1] == JOBS and JOBS in users:
            changed.add(JOBS)
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
    if JOBS in users and modules & JOB_TEST_USERS:
        modules.add(JOBS)
    return "|".join(f"FullyQualifiedName~{project}.{module}." for module in sorted(modules)
                    for project in ("UnitTests", "IntegrationTests"))


if __name__ == "__main__":
    print(affected([line.strip() for line in sys.stdin], referenced_by()))
