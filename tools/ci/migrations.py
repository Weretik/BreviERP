"""Migration checks of CI (database rules 5.2, 5.3).

For every DbContext with migrations: no model change may be left without a migration
(`dotnet ef migrations has-pending-model-changes`), and its idempotent SQL script is written to
artifacts/migrations/<Context>.sql for the pull request reviewer, then applied twice to an empty PostgreSQL
database, as the Host.Seed job would meet a repeated deploy.

    python3 tools/ci/migrations.py [--configuration Release] [--no-build]

The PostgreSQL server comes from BREVIERP_CATALOG_TEST_CONNECTION (the CI service); psql must be installed.
"""
import argparse
import os
import re
import subprocess
import sys
from pathlib import Path

STARTUP = "src/Bootstrapper/Host.Seed/Host.Seed.csproj"
SNAPSHOT = re.compile(r"\[DbContext\(typeof\((?:[\w.]+\.)?(\w+)\)\)\]")


def contexts(root):
    """(context, project) of every model snapshot under src/, sorted by context."""
    found = []
    for snapshot in sorted(Path(root, "src").rglob("*ModelSnapshot.cs")):
        if {"obj", "bin"} & set(snapshot.relative_to(root).parts):
            continue
        match = SNAPSHOT.search(snapshot.read_text(encoding="utf-8-sig"))
        if match is None:
            continue
        folder = snapshot.parent
        while not list(folder.glob("*.csproj")):
            folder = folder.parent
        project = next(folder.glob("*.csproj")).relative_to(root).as_posix()
        found.append((match.group(1), project))
    return sorted(found)


def connection_parts(connection):
    """Npgsql «Key=Value;» pairs as lower-case keys."""
    parts = {}
    for pair in connection.split(";"):
        if "=" in pair:
            key, value = pair.split("=", 1)
            parts[key.strip().lower()] = value.strip()
    return parts


def psql(parts, database, *arguments):
    env = dict(os.environ, PGPASSWORD=parts.get("password", ""), PGOPTIONS="--client-min-messages=warning")
    command = ["psql", "-h", parts.get("host", "localhost"), "-p", parts.get("port", "5432"),
               "-U", parts.get("username", "postgres"), "-d", database, "-v", "ON_ERROR_STOP=1", "-q", *arguments]
    subprocess.run(command, check=True, env=env, stdout=subprocess.DEVNULL)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--configuration", default="Debug")
    parser.add_argument("--no-build", action="store_true")
    options = parser.parse_args()
    root = Path(__file__).resolve().parents[2]
    connection = os.environ.get("BREVIERP_CATALOG_TEST_CONNECTION")
    if not connection:
        sys.exit("Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string.")
    parts = connection_parts(connection)
    output = root / "artifacts" / "migrations"
    output.mkdir(parents=True, exist_ok=True)
    build = ["--no-build"] if options.no_build else []
    failed = []
    for context, project in contexts(root):
        ef = ["dotnet", "ef", "--project", project, "--startup-project", STARTUP, "--context", context,
              "--configuration", options.configuration, *build]
        print(f"== {context}", flush=True)
        pending = subprocess.run([ef[0], ef[1], "migrations", "has-pending-model-changes", *ef[2:]], cwd=root)
        if pending.returncode != 0:
            failed.append(f"{context}: the model has changes without a migration")
            continue
        script = output / f"{context}.sql"
        subprocess.run([ef[0], ef[1], "migrations", "script", "--idempotent", "-o", str(script), *ef[2:]],
                       cwd=root, check=True)
        database = f"brevierp_migrations_{context.lower()}"
        psql(parts, "postgres", "-c", f'DROP DATABASE IF EXISTS "{database}"', "-c", f'CREATE DATABASE "{database}"')
        try:
            for _ in range(2):
                psql(parts, database, "-f", str(script))
        except subprocess.CalledProcessError:
            failed.append(f"{context}: the idempotent script does not apply twice to an empty database")
        finally:
            psql(parts, "postgres", "-c", f'DROP DATABASE IF EXISTS "{database}"')
    if failed:
        sys.exit("\n".join(failed))
    print(f"Migration scripts: {output}")


if __name__ == "__main__":
    main()
