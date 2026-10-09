---
name: brevi-erp-database-change
description: "Use when a BreviERP feature changes EF Core schema, mappings, migrations, seed data, transactions, or database rollout behavior."
---

# BreviERP Database Change

Проєктуй і перевіряй data change як окремий ризиковий delivery slice.

## Workflow

1. Прочитай feature data model/design, identifier strategy, database/testing rules і `docs/sdd/operations/data/migrations-and-seeding.md`.
2. Зафіксуй owning DbContext, current schema/data baseline, typed/public ID decision, integrity, compatibility, rollout і rollback/recovery.
3. EF mapping та migrations належать Infrastructure owning module. Не редагуй applied migrations.
4. Створи нову migration з правильними project/startup/context arguments; переглянь Up/Down, indexes, FKs, constraints і destructive operations.
5. Перевір empty baseline та required upgrade path на disposable PostgreSQL. Seeders мають бути idempotent і визначати existing-data behavior.
6. Запускай погоджені migrations/seeders через `IDatabaseMigrator`/`ISeeder` та `src/Bootstrapper/Host.Seed/Host.Seed.csproj`; перевір logs і failure exit.
7. Запиши exact commands/results, rollout order і residual risks у feature evidence.

## Safety boundary

Не запускай Host.Seed, migration, seeder або SQL проти shared, staging чи production database без явного дозволу користувача та точно підтвердженої target configuration. Не виводь secrets.

## Example

`Use $brevi-erp-database-change to add the approved Accounting transaction index and verify it on a disposable database.`
