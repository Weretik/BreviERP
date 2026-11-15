"""Tests of the CI migration checks: python3 -m unittest discover -s tools/ci"""
import tempfile
import unittest
from pathlib import Path

from migrations import connection_parts, contexts


class MigrationContextsTests(unittest.TestCase):
    def test_every_snapshot_gives_its_context_and_the_project_that_holds_it(self):
        with tempfile.TemporaryDirectory() as root:
            module = Path(root, "src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure")
            (module / "Persistence/Migrations").mkdir(parents=True)
            (module / "Crm.Infrastructure.csproj").write_text("<Project />")
            (module / "Persistence/Migrations/CrmDbContextModelSnapshot.cs").write_text(
                "﻿[DbContext(typeof(CrmDbContext))]\npartial class CrmDbContextModelSnapshot {}")
            job = Path(root, "src/Jobs/CatalogImport")
            (job / "Migrations").mkdir(parents=True)
            (job / "CatalogImport.csproj").write_text("<Project />")
            (job / "Migrations/CatalogImportDbContextModelSnapshot.cs").write_text(
                "[DbContext(typeof(CatalogImport.Persistence.CatalogImportDbContext))]")
            stale = Path(root, "src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/obj")
            stale.mkdir()
            (stale / "OldModelSnapshot.cs").write_text("[DbContext(typeof(OldDbContext))]")

            self.assertEqual(contexts(root), [
                ("CatalogImportDbContext", "src/Jobs/CatalogImport/CatalogImport.csproj"),
                ("CrmDbContext", "src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Crm.Infrastructure.csproj"),
            ])

    def test_the_connection_string_keys_are_read_without_case(self):
        self.assertEqual(connection_parts("Host=db;Port=5432;Database=postgres;Username=u;Password=p=1"),
                         {"host": "db", "port": "5432", "database": "postgres", "username": "u", "password": "p=1"})


if __name__ == "__main__":
    unittest.main()
