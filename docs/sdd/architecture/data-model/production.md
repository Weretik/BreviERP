# Production schema

Code model reviewed 2026-11-04. `Production.Infrastructure.Persistence.ProductionDbContext` owns schema `production`; migration history is `production.__EFMigrationsHistory`. Migration `InitialProduction` creates the tables ([PRD-001](../../specs/production/001-workshop-references/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Employees | Generated int id. FullName (120), NameKey (120, lower case), Position (80), IsActive, CreatedAt, UpdatedAt, UpdatedByUserId (Identity user id as a value). Unique NameKey among active rows (filtered index `"IsActive"`) |
| FabricNorms | Generated int id. Unique ModelCode (32, Catalog model code as a value), OneColor, TwoColorsMain, TwoColorsAdditional, ThreeColorsMain, ThreeColorsAdditional1, ThreeColorsAdditional2 (numeric(7,3) metres, nullable), UpdatedAt, UpdatedByUserId |

The model code and user id are plain values; there are no foreign keys to `catalog` or `identity`.
