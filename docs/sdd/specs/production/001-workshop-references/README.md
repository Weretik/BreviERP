# PRD-001 «Швейний цех»: module, head of the workshop, references

Status: stage 1 implemented 2026-11-04. Part of the port of the client's separate «Цех» app (brevi-workshop, Next.js on Cloudflare D1) into Brevi. The full 8-stage plan is kept by the owner outside the repository; this file covers stage 1 only.

## Goal

Give the sewing workshop its own module in Brevi and the references every later stage needs: who works in the workshop, how much fabric each model takes, and by which coefficients the workshop pays.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D1 | New bounded context `Production` with its own schema `production` and `production.__EFMigrationsHistory`; four layers like every module. No foreign keys to other schemas. |
| PRD-D2 | New staff role `WorkshopChief` («Начальник цеху»). Policy `CanManageWorkshop` = WorkshopChief + Admin (the director also holds Admin). The role also gets «Мій кабінет» (`OwnProfile`). |
| PRD-D3 | Workers do not sign in. «Працівники цеху» is a list kept by the head of the workshop; a worker who left is archived, not deleted, so old work and pay keep the name. Active names are unique ignoring case and extra spaces. |
| PRD-D4 | Fabric norms are keyed by the Catalog model code (MOD-…) and stored in metres (3 decimals); the old app kept millimetres. Variants: one colour, two colours (main + additional), three colours (main + two additional). The model list comes from Catalog through the host port `IProductionModelReader`. |
| PRD-D5 | Payroll coefficients are **not** copied: the workshop reads them from «Параметри ціноутворення» (Reference) through the host port `IPayrollParameterReader`, so the price and the pay use one value. «Правила зарплати» is read-only and links to that page. Codes: SEAMSTRESS_BONUS, SEAMSTRESS_BONUS_PAYOUT_THRESHOLD, CUTTING_RATE, MASTER_RATE, SHOP_MANAGER_RATE, EMBROIDERY_RATE_1000_STITCHES, SEAMSTRESS_COUNT, MINUTE_COST_SEAMSTRESS. |
| PRD-D6 | Transfer from the old app goes through the import endpoints with a preview (`dryRun=true`) first. A norm list with any wrong row saves nothing. Source: the owner's export of 23.10.2026 (110 norms, 13 workers). |
| PRD-D7 | Admin block «Швейний цех» in brevi-web (pages «Працівники цеху», «Норми витрати тканини», «Правила зарплати»). |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-001-A1 | A worker can be added, renamed, archived and returned; a duplicate active name is 409. |
| PRD-001-A2 | The norm list shows every Catalog sewing model; a norm of a model gone from Catalog is shown with its problem. |
| PRD-001-A3 | A norm for a code that is not a Catalog sewing model is 404; an additional fabric without the main one is 400. |
| PRD-001-A4 | Import preview saves nothing; an import with a wrong row saves nothing and names the row. |
| PRD-001-A5 | «Правила зарплати» shows the eight coefficients with values from «Параметри ціноутворення» and marks only missing or empty ones. |
| PRD-001-A6 | Only WorkshopChief, Admin and Director reach `/api/production`. |

## Next stages (not in this SDD)

2 orders from КП · 3 stages, shipments, files in Media, Telegram · 4 fabric stock · 5 work log and forecast · 6 timesheet and payroll · 7 reports and dashboard · 8 old archive (read-only) and cutover.

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production` — 13 passed (2026-11-04).
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — passed, including `ProductionDependencyTests` and the container restore check.
