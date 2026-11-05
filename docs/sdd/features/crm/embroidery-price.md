# «Менеджер сервісу»: embroidery price

**Implemented:** stage 1 of [EMB-001](../../specs/crm/005-embroidery-price/README.md) (2026-11-04). «Ціна вишивки» computes the cost and three prices («Опт», «Стандарт», «Терміново») of an embroidery on a garment or of chevrons, with the repeat price and the cut layout of stabilizer and fabric. Managers get prices only; Director and Admin also get the cost and edit «Налаштування». The embroiderer's rate comes from «Параметри ціноутворення». Stage 2 adds «Збережені»: a saved calculation keeps its form, frozen rates and result, with an optional client, search by name, client or «ВШ-0042»; only Director and Admin delete. «Додати в КП» puts a saved calculation into a КП as one Embroidery row with the chosen level's price in all three tiers. «Імпорт архіву» (Director and Admin) moves the old calculator's saved calculations with their dates, authors and old rates, checking that each gives its old prices. Stage 4: «Макет» — a photo of the design from «Медіа» on a saved calculation, which the workshop sees in the order; the photo cannot be deleted while used.

## Code structure

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/Embroidery/
    Calculation/EmbroideryCalculator.cs   the formula (pure)
    Calculation/EmbroideryCutLayout.cs    blanks across a roll, rows, charge
    Calculation/Rational.cs               exact fractions, half-up rounding
    Calculation/EmbroideryInput.cs, EmbroideryResult.cs
    Settings/EmbroiderySettings.cs        singleton record with Version
    Settings/EmbroiderySettingsContent.cs rates, markups, materials, hoops, extra costs
    Settings/EmbroiderySettingsRules.cs, EmbroiderySettingsDefaults.cs
    Saved/EmbroiderySavedCalculation.cs   saved record, number «ВШ-0042», name rule
    Saved/EmbroideryRateSnapshot.cs       stitch rate + settings content at saving
  Crm.Application/Crm.Application/Embroidery/
    Access/                               EmbroideryActor (Admin sees cost), messages
    StitchRate/IEmbroideryStitchRateReader.cs  pricing parameter port
    Persistence/IEmbroideryStore.cs       store port and settings lookup
    Contracts/, Mapping/, Validation/
    GetForm/ Calculate/ GetSettings/ SaveSettings/
    Calculate/EmbroideryPricing.cs        shared pricing for calculate and save
    SaveCalculation/ GetCalculations/ GetCalculation/ DeleteCalculation/  «Збережені»
    ImportArchive/                        «Імпорт архіву» of the old calculator
    Mockup/                               «Макет»: IEmbroideryMediaReader port, set command, usages for «Медіа»
  Crm.Application/Crm.Application/Quotes/
    AddEmbroidery/                        «Додати в КП» command
    Embroidery/QuoteEmbroideryItems.cs    saved calculation → КП item, kept on save
  Crm.Infrastructure/.../Configurations/Embroidery/  settings row, saved calculations
  Crm.Api/Crm.Api/Embroidery/EmbroideryController.cs  /api/crm/embroidery
src/Bootstrapper/Host.Api/Crm/EmbroideryStitchRateReader.cs  Reference adapter
src/Bootstrapper/Host.Api/Crm/EmbroideryMediaReader.cs        Media adapter for «Макет»
src/Bootstrapper/Host.Api/Production/ProductionEmbroideryReader.cs  saved calculations for the workshop
```

[API contract](../../contracts/crm/embroidery.openapi.yaml), [database ownership](../../architecture/data-model/crm.md).

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.Embroidery"` — specification examples, rounding, missing rates, validators, Manager response.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter EmbroideryPersistenceTests` — settings and calculation over PostgreSQL (`BREVIERP_CRM_TEST_CONNECTION`).
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter EmbroiderySavedPersistenceTests` — saved calculations keep their rates, search, delete rights, «Макет».
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter EmbroideryArchivePersistenceTests` — old archive: check, import once, refuse different prices.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter QuoteEmbroideryPersistenceTests` and `dotnet test tests/UnitTests/UnitTests.csproj --filter QuoteEmbroideryTests` — «Додати в КП».
