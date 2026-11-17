# Database rules

**Applies to:** every module with a `DbContext`, `Host.Api`, `Host.Seed` (migrations, seed, sync and task jobs)
**Checked by:** architecture tests, integration tests on PostgreSQL (`BREVIERP_*_TEST_CONNECTION`), migration review, code audit

How code reads and writes the database: queries, indexes and search, transactions, concurrency, and how migrations reach production. KedrStore and BreviERP use this text verbatim, followed by their own specifics. The rules assume EF Core with PostgreSQL (Npgsql) and use only PostgreSQL built-ins and the .NET platform: no third-party search engines or locking libraries.

Each rule says **what** to do, **where** in the code it applies, and **how to check it**. An audit reports a violation as a finding with the file and line; a new feature follows the rule from its first commit.

## 1. Queries

### 1.1 Reads do not track

- **What.** A query whose result is only returned or mapped uses `AsNoTracking()` or a `Select(...)` projection to a DTO. Tracking is only for entities the same use case changes and saves.
- **Where.** Query handlers (`Features/<Feature>/Queries/`), readers in Infrastructure, `IRead<Module>DbContext` users, read repositories. A "read repository" type must not track: give it `AsNoTracking` or read through the read context.
- **Check.** Every `ToListAsync`/`FirstOrDefaultAsync`/`ToDictionaryAsync` in a query path is preceded by `AsNoTracking()` or ends in a `Select` to a non-entity type.

### 1.2 Project, do not load whole entities

- **What.** A list or page selects only the columns it returns (`Select(x => new RowDto(...))`). Loading entities and mapping in memory is for commands that change them.
- **Where.** Every list, search, export, and projection rebuild.
- **Check.** No query handler maps an entity list with `.Select` after `ToListAsync`.

### 1.3 No N+1

- **What.** Never run a query or `SaveChanges` inside a loop over rows. Load what the loop needs in one query before it (`Where(x => ids.Contains(x.Id))` → `ToDictionaryAsync`), change objects in memory, and save once after it. For related rows of a list use a projection with the child fields, or one `Include` of a single collection; two or more collection `Include`s in one query use `AsSplitQuery()` to avoid a cartesian explosion.
- **Where.** Import and sync jobs (1C, CSV, CDN), batch commands, scheduler ticks, mappers that call a repository.
- **Check.** A `foreach`/`for` body that contains `await ...Async(` on a `DbContext`, repository, or `SaveChanges`. A loop over at most ten fixed items (for example scheduler task definitions) is allowed and needs no fix.

### 1.4 Filter, sort, and page in SQL

- **What.** `Where`, `OrderBy`, `Skip`/`Take`, `Count` run in the database. Do not load a whole table and filter, search, or page in C#. Every list endpoint has paging; an unbounded endpoint (export, offline sync) is documented as such and uses a projection.
- **Where.** Admin lists, public catalog pages, "state" readers that build a model of the whole catalog.
- **Check.** `ToListAsync()` / `ToDictionaryAsync()` with no `Where` on a table that grows with the business (products, orders, customers, files), followed by LINQ-to-objects filtering.
- **When a whole-table model is really needed** (for example a public catalog tree), cache it with the platform `HybridCache` (or `IMemoryCache`) for a short time and invalidate it from the commands that change it. Never rebuild it on every HTTP request.

### 1.5 Other EF traps

- `Contains` over an id list is fine (Npgsql sends one array parameter); do not pass a list of all ids when the filter keeps every row.
- Change-tracker cost grows with tracked entities: a long import that tracks tens of thousands of rows saves in batches with `ChangeTracker.Clear()` between batches, or uses `ExecuteUpdate`/`ExecuteDelete` for set-based changes.
- Use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for a bulk change that needs no domain rule. When a domain rule must run, load and change the aggregates.
- `EnableRetryOnFailure` (transient connection retries) is optional; when enabled, every explicit transaction runs inside `Database.CreateExecutionStrategy().ExecuteAsync(...)`.

## 2. Indexes and search

### 2.1 Search: every word, part of a word, on an index

- **What.** A text search box behaves the same everywhere:
  1. Trim the input and split it on whitespace into words.
  2. For each word add one condition `EF.Functions.ILike(column, "%" + Escape(word) + "%", "\\")`; the conditions are joined with AND (each word must match), and one word may match any of the searched columns (OR across columns inside the word).
  3. Escape `%`, `_` and `\` in the word with one shared `LikePattern.Escape` helper per module.
  4. Every searched column has a trigram index: `HasIndex(x => x.Column).HasMethod("gin").HasOperators("gin_trgm_ops")`, with `modelBuilder.HasPostgresExtension("pg_trgm")` in the owning `DbContext`.
  5. Normalize text the same way when you store it and when you search it if the language needs it (apostrophes `'` and `’`, `ё`/`е`). Keep the normalization in one domain function.
- **Never.** `ToLower().Contains(...)`, `ToUpper().Contains(...)`, or `string.Contains` in SQL: the database cannot use an index for them and reads every row. Never search in C# after loading the table.
- **Short words.** A word of one or two characters yields no trigram and makes the index scan everything. Apply it only together with a longer word, or match it as a prefix or exact value (for example an id).
- **Several columns.** When a search spans several text columns of one row, either index each column or store one combined search column (`SearchText = id + " " + name + ...`) maintained by the domain or projection and index that one column.
- **Ranking** ("most similar first") is a feature, not a default: use `pg_trgm` `word_similarity` or full-text search (`tsvector`) only when a specification asks for it.
- **Where.** Every handler or reader with a `Search`, `SearchTerm`, `Query`, or `q` parameter.
- **Check.** Search code follows steps 1 to 4; the searched columns have a `gin_trgm_ops` index in the model snapshot.

### 2.2 Which indexes to add

- **What.** Add an index for each query that filters, joins, or sorts a table that grows: a `WHERE` or `JOIN` column, the `ORDER BY` of a paged list, and every foreign key (EF adds FK indexes itself; hand-written SQL must add them).
- **Column order in a composite index:** first the columns compared with `=`, then one column compared with a range (`>=`, `<`, `BETWEEN`) or used for `ORDER BY`, then a tie-breaker (`Id`) so paging is stable. Example: list of a customer's orders by date → `(CounterpartyId, CreatedAt DESC, Id)`.
- **Partial index** (`HasFilter`) when queries always filter on the same condition (`WHERE "IsDeleted" = false`, `WHERE "ExportToSite"`), and for "at most one active row" uniqueness.
- **Unique index** for every business uniqueness rule (slug, code, idempotency key, number). It is the real guard against duplicates under concurrency (section 4); a code check before insert is only a friendly message.
- **Where.** `Persistence/<Aggregate>/<Entity>Configuration.cs`; the SDD data model lists the indexes of new tables.

### 2.3 Which indexes to remove

- **What.** An index costs on every insert and update. Remove an index that a composite index already covers by its leftmost columns (`(A)` when `(A, B, C)` exists), unless it is the FK index EF requires and you replace it. Remove an index that production never uses.
- **How to find unused ones:** on the production database (read-only, with the owner's permission) run

  ```sql
  SELECT schemaname, relname, indexrelname, idx_scan,
         pg_size_pretty(pg_relation_size(indexrelid)) AS size
  FROM pg_stat_user_indexes
  ORDER BY idx_scan, pg_relation_size(indexrelid) DESC;
  ```

  `idx_scan = 0` over weeks of real traffic marks a candidate. Never remove a primary key or a unique index: they guard data even without reads.

### 2.4 Bulk writes and indexes

- **What.** A job that rewrites a table (projection rebuild, catalog sync) updates only changed rows: `INSERT ... ON CONFLICT (...) DO UPDATE ... WHERE <something changed>` plus `DELETE` of rows that disappeared. Never delete all rows and insert them again on a schedule: every row then rewrites every index (GIN indexes are the most expensive), the table bloats with dead rows, and readers see an empty table between the two steps.
- **Large one-off imports** may drop and recreate a secondary index around the load only inside a planned migration with downtime; normal jobs do not.

### 2.5 Hand-written SQL

Raw SQL in a migration (`migrationBuilder.Sql`) or in code (`ExecuteSqlRaw`, outbox queries) follows the same rules: an index for every FK and filter, parameters instead of string concatenation, and the schema-qualified table name of the owning module.

## 3. Transactions

### 3.1 One command, one SaveChanges

- **What.** A command handler changes its aggregates and calls `SaveChanges` exactly once at the end. EF wraps that call in one transaction: either everything is saved or nothing is.
- **Repositories never save on their own.** A generic repository base (for example Ardalis `RepositoryBase`) whose `AddAsync`, `AddRangeAsync`, `UpdateAsync`, `DeleteAsync`, `DeleteRangeAsync` call `SaveChanges` inside them breaks this rule: a handler with two such calls commits twice and can stop half-way. The module's repository overrides these methods to only mark changes (`Set<T>().Add/Update/Remove...`) and the handler calls `SaveChangesAsync` once. An architecture test forbids the auto-saving methods in handlers, or the override makes them safe.
- **Where.** Every `Commands/` handler and every job step.
- **Check.** A handler or job method with more than one `SaveChangesAsync`, or a repository `Add*/Update*/Delete*` call followed by more writes, or `ExecuteDelete`/`ExecuteUpdate` mixed with `SaveChanges` without an explicit transaction.

### 3.2 When an explicit transaction is needed

Use `await using var tx = await db.Database.BeginTransactionAsync(ct); ... await tx.CommitAsync(ct);` when one operation needs more than one database round trip that must succeed together:

- `ExecuteDelete`/`ExecuteUpdate`/raw SQL together with `SaveChanges` (for example "replace all rows of a projection");
- an id from the first `SaveChanges` is needed to build the second write (better: let EF fix up the relation in one `SaveChanges`);
- a lock must be held across reads and writes (section 4.3).

Keep a transaction short: no HTTP calls, file uploads, or waiting on users inside it.

### 3.3 Long jobs

A sync of thousands of rows is not one transaction for hours: it holds locks and blocks the site. Split it into steps (one root, one batch); each step is atomic and the whole job is safe to run again (idempotent). A step that replaces a set of rows (delete missing + upsert present) is one transaction.

### 3.4 Side effects outside the database

- A call to another system (1C, Telegram, e-mail, CDN, site revalidation) never sits inside a transaction and is never "save, then call, then save again". Write an outbox row in the same `SaveChanges` and send it after the commit (module rules, "Transactional outbox").
- A file uploaded to storage before the database write is deleted again if the write fails (compensation); the database row is written last.

## 4. Concurrency

Two requests or jobs can change the same data at the same time. Pick a protection per case:

| Method | How it works | Use for |
| --- | --- | --- |
| Atomic conditional update | One statement changes the row only if it is still in the expected state: `ExecuteUpdateAsync` on `.Where(x => x.Id == id && x.Status == Pending)`; 0 affected rows means someone else was first | Status changes, claiming a job or message, counters, stock decrement (`SET Stock = Stock - @q WHERE Stock >= @q`) |
| Optimistic locking | The row has a version: PostgreSQL `xmin` (`builder.Property<uint>("xmin").IsRowVersion()`) or a `Version` column with `IsConcurrencyToken()`. EF adds `WHERE xmin = @old` to the update; on `DbUpdateConcurrencyException` return 409 "data changed, reload" or retry the whole use case | Edit forms in the admin, aggregates edited rarely by several people, scheduler rows |
| Pessimistic locking | `SELECT ... FOR UPDATE` inside a transaction makes others wait; `FOR UPDATE SKIP LOCKED` lets workers skip taken rows; `pg_advisory_xact_lock(<key>)` locks a named operation, not a row | Queues and outbox dispatch, jobs that must not run twice at once (projection rebuild, imports), hot rows with frequent conflicts |

Rules:

- **Duplicates:** a unique index guards every "must exist once" rule (idempotency key, slug, number). Code that inserts after a "does it exist?" check catches the unique violation (`PostgresException` with `SqlState == PostgresErrorCodes.UniqueViolation`) and answers as if the first request won: for an idempotent create, return the existing result, not 500.
- **Double submit:** every create endpoint the client may repeat (orders, payments, messages) takes an idempotency key stored with a unique index; where it is required and how a repeat is answered is in the [idempotency rules](idempotency-rules.md).
- **Jobs that rewrite shared data** take `pg_advisory_xact_lock` with a constant key per operation at the start of their transaction, so a scheduled run and a manual run never overlap. A scheduler that launches tasks separately also groups tasks that touch the same data so only one of them runs at a time.
- **Admin forms:** the read DTO carries the row version; the update command sends it back; a mismatch returns 409 with a Ukrainian message.
- **Numbers and positions:** never `MAX(x) + 1` for a business number that must be unique; use a database sequence or the identity id. For display order (`Position`) `MAX + 1` is acceptable when a duplicate only changes order.
- **Check.** A read-check-write without a version, unique index, conditional update, or lock on data that two actors can change; a `catch` that is missing for the unique violation behind an idempotency key; a job that can run twice at once.

## 5. Migrations

### 5.1 How migrations reach production

- **What.** Migrations run as a separate step of the deploy pipeline before the new code starts: a job (a `Host.Jobs --job=migrate` command or `dotnet ef migrations bundle`) that runs once, with one instance and no automatic retry, and the pipeline waits for it and stops the deploy on failure.
- **Never** `Database.Migrate()`/`MigrateAsync()` at API startup: several instances would migrate at once, and a failed migration would stop the API from starting.
- **`dotnet ef migrations script --idempotent`** produces the SQL for review: generate it in CI for a pull request that adds a migration and read it for `DROP`, `ALTER COLUMN TYPE`, table rewrites, and missing indexes.
- **Where.** The deploy workflow, `Host.Jobs`, operations documentation.
- **Check.** No `Migrate`/`MigrateAsync` call outside the migration job; the workflow runs the job before the service deploy and waits for it.

### 5.2 Backward-compatible migrations (expand, then contract)

Between the migration step and the new code there are minutes when the **old** code runs on the **new** schema. Every migration must keep the previous release working:

1. Expand: add new tables, nullable columns or columns with defaults, new indexes; copy data.
2. Deploy code that uses the new shape.
3. Contract in a later release: drop the old column or table.

Renaming or dropping a column, or making it `NOT NULL` without a default, in the same release as the code change is not allowed.

### 5.3 Locks during migrations

- Start the migration session with `SET lock_timeout = '10s'` (and a `statement_timeout` for data steps): a migration that waits for a lock fails fast instead of queuing every site query behind it. The deploy can be re-run.
- Create an index on a large table with `CREATE INDEX CONCURRENTLY` in its own migration that does not run in a transaction (`migrationBuilder.Sql(..., suppressTransaction: true)`); small tables may use a normal index.
- Data back-fills of many rows run in batches, not one `UPDATE` of the whole table.

### 5.4 Other migration rules

- Never edit a migration that production has applied; add a new one.
- Each module has its own schema and migration history table (module rules).
- A migration has a tested `Down` or a written recovery plan in its SDD.
- Seed data is idempotent and runs in the migration job after the migrations.

## Brevi specifics

### Identifiers

The [identifier strategy](identifier-strategy.md) defines how to choose a scalar type, typed domain ID, `PublicId`, and human-readable document number. Record the choice and rationale in the feature specification's `data-model.md` before creating code or a migration.

### Read models

For a list or detail read model that does not need domain behavior, the default approach is `Ardalis.Specification.Specification<TEntity, TResult>` with SQL `Select` into a response DTO. A handler receives `TResult` from the read repository rather than materializing `TEntity` and mapping it in memory. Add only necessary `Include` statements; verify with an integration test that filters, sorting, pagination, and projection translate to SQL.

### DbContexts and schemas

Each module owns its `DbContext`, schema and migration history ([module rules](module-rules.md#database-schema-per-module)): `CatalogDbContext`, `CatalogImportDbContext`, `ReferenceDbContext`, `AppIdentityDbContext`, `CrmDbContext`, `ProductionDbContext`, `WarehouseDbContext`, `PurchasingDbContext`, `AccountingDbContext`, `ContentDbContext`, `MediaDbContext`, `RemindersDbContext`, `SchedulingDbContext`, `AiDbContext`. EF mapping lives in the module's Infrastructure; Domain has no persistence dependencies. A module never references another module's tables.

### Migrations and seed

- Migrations and seed run in the `Host.Seed` Cloud Run Job. `deploy-cloudrun.yml` runs it with `--wait` (one task, no retries) **before** it deploys `Host.Api`; a failed job stops the deploy. `Host.Api` never migrates. This satisfies 5.1.
- Use the EF Core CLI as the repository-pinned local tool (`.config/dotnet-tools.json`, same version as `Microsoft.EntityFrameworkCore` in `Directory.Packages.props`): run `dotnet tool restore` once, then `dotnet ef ...`. Do not install `dotnet-ef` globally; when the EF Core package version is raised, raise the tool version with it.
- CI (`tools/ci/migrations.py`, the `verify` job) fails when a module's model has changes without a migration (`has-pending-model-changes`) and when a module's idempotent script does not apply twice to an empty database. The scripts are attached to the run as `migration-scripts`: read them for `DROP`, `ALTER COLUMN TYPE` and table rewrites before the deploy (5.1, 5.2).
- `DbMigrator` sets `lock_timeout = '10s'` for the migration session (5.3): a migration that meets a busy table fails and the deploy stops; run it again when the site is quiet.
- Verify a new migration against an empty test database and write its rollback and deployment risks in the PR. Change reference and seed data through the existing seed mechanism, not hidden runtime writes. Runbooks: [data operations](../operations/data/migrations-and-seeding.md).

### Search

- `BuildingBlocks.Infrastructure/Database/SearchTextColumn`: `builder.HasSearchText(columns...)` adds a generated stored column `SearchText` = lower case of the columns joined by spaces, with `’` and `ʼ` as `'`, and its `gin_trgm_ops` index; `modelBuilder.AddTrigramSearch()` enables `pg_trgm` in the context. The database fills the column for old and new rows, so a new searched table needs no back-fill and no domain code.
- `BuildingBlocks.Application/SearchWords`: `rows.WhereSearchText(text)` keeps rows whose `SearchText` holds every word; `WhereEveryWord(text, pattern => x => ...)` when a word may also match a number, an amount or a child row; `SearchWords.Matches<T>(pattern)` for a specification in a project without EF Core. Patterns are lower case and escaped.
- A table whose domain already keeps a lower-case `SearchText` (`WorkshopArchiveRecord`, `FinanceCounterparty`) only gets the `gin_trgm_ops` index.
- A ledger whose search also covers its parent's fields (`FabricMovement` and its shelf line) indexes its own text columns; the few parent rows are read and matched in memory and enter the query as a list of ids (`FabricMovementLedger`). A report over several periods reads its sources once for the whole range (`PayrollSheets.ReadRangeAsync`), never per period.
- A short list that is read whole anyway (the «Товари» overview, «Товари на сайті», «План/факт тканини») keeps its search in memory with the same rule: `SearchWords.MatchesEveryWord(text, fields)`. Move it to SQL with a `SearchText` column when the list grows past about 2 000 rows.
- Unit tests on SQLite call `connection.WithSearch()` (`tests/UnitTests/TestSupport/SqliteSearch.cs`) so `lower` handles Cyrillic as PostgreSQL does.
- A migration that adds a search column keeps `pg_trgm` in `Down`: other modules' indexes use it.

### Outbox and jobs

- The transactional outbox (`BuildingBlocks.Infrastructure/Outbox`) claims messages with `FOR UPDATE SKIP LOCKED` and is dispatched by Cloud Scheduler through the protected outbox endpoint (3.4, 4). A partial index on `CreatedAt` where `ProcessedAt IS NULL` serves the claim. Each handler has `OutboxOptions.HandlerTimeout` (20 s); a handler may run twice, so it checks its own record first.
- Crm, Production, Reference, Warehouse, Catalog and Media have outboxes; all six carry `activity.recorded` for «Журнал дій» (ACT-001), written by `DomainEventOutboxInterceptor` in the same transaction as the change. The warehouse writes «Тканина» lines of the order history through it (`FabricHistory`): the note is saved with the stock change, and Production skips a line it already has.
- The scheduler tick runs alone: `SchedulingDbContext.TryLockTickAsync` takes a session `pg_try_advisory_lock`; a tick that finds it taken does nothing.
- Background tasks run through the tasks job of `Host.Seed`, launched by Cloud Scheduler; sync jobs (KeepinCRM, 1C) are steps that are safe to re-run (3.3).

### Concurrency already in place

- «Зарплата»: `PayrollMonth.Version` is a concurrency token. Every change of work, hours, payments and adjustments calls `PayrollMonth.RecordChange` (through `PayrollChanges.ApplyAsync` or `PayrollMonths.MarkChangedAsync`) and saves with `PayrollMonths.SaveAsync`, so a change that races the closing of its month gets 409 instead of landing in a closed month.
- Double submit: a create form makes a `RequestId` when it opens and sends it with «Зберегти». The row stores it under a unique partial index (`"RequestId" IS NOT NULL`); a repeat returns the saved row, also when the first request wins a race (catch `DbUpdateException`, read by `RequestId`). In place for workshop orders and work, payroll payments and adjustments (`PayrollChanges.ApplyOnceAsync`), finance operations and transfers, КП create, «КП для знайомства» and copy (`QuoteNumbering.AddAsync`), lead calls, КП letters and site requests (the site form sends `requestId` with the multipart body). The field is optional, so an older client keeps working. A КП letter is checked before Gmail is called: a repeat after success sends nothing, while two presses that reach the server at the same moment are still both sent, and the second only fails to mark the КП (the version check).
- Crm edit forms (КП, leads, tasks, customers, embroidery settings) carry a version and return 409 on `DbUpdateConcurrencyException`.
- «Нагадування»: `ReminderOccurrence.Version` is a concurrency token that every close, postpone, reopen and reschedule moves. Two «Виконано» presses, or a press and the admin close, cannot both be recorded: the Telegram press that loses reads the period again and answers «Вже закрито: …»; the admin gets 409.
- Workshop order numbers are «MAX + 1» without gaps; a create that loses the race to the unique index takes the next number and saves again, and a double press returns the saved order (`OrderNumbers.SaveNewAsync`), like КП numbers (`QuoteNumbering`).
- A save is answered as «someone was first» only for a race: `catch (DbUpdateException e) when (e.IsRace())` (`BuildingBlocks.Application/SaveRaces`: a concurrency exception or a unique violation, PostgreSQL `23505`). Any other failed save, such as a broken reference or a too long value, reaches the global exception handler as an error. The warehouse stock operations use it.

## Known deviations

From the database audit of 2026-11-15; each row is removed when its fix is merged.

| Rule | Where | Status | Plan |
| --- | --- | --- | --- |
| 4 | Product card (only its composition has a version) and Reference fabrics, accessories and suppliers (no version): a second editor silently overwrites the first | deferred | The audit rated it low while one person edits: add a version (`xmin` and the version in the form) when a second editor appears |
| 2.2 | `activity_log.ActivityEntries`: the «Роль» filter (`ActorRoles LIKE '%|Manager|%'`, on by default) and the page count have no index, and «Журнал дій» filters group the whole table | deferred | Fine for the expected thousands of rows a month. Above about 500 000 rows or a slow page: store roles in a `text[]` with a GIN index (or a trigram index on `ActorRoles`) and keep the filter options in a small table updated by the outbox handler; entries are kept one year (ACT-001 R-12), so the table stays near a year of actions |
| 1 | No `EnableRetryOnFailure`: a dropped database connection fails the request | deferred | Turn it on only when the logs show dropped connections; every explicit transaction must then run inside the execution strategy |
