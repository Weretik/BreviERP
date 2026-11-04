# Catalog: PPE supplier price import («Імпорт цін»)

The tab «Довідники» → «Імпорт цін» lists PPE supplier price sources with their logic, schedule and discount, and updates supplier prices from the suppliers' files: preview first, then apply. Spec: [CAT-017](../../specs/catalog/017-ppe-supplier-price-import/README.md). Stage 1: Insight XML and СЕМТО PDF uploads. Stage 2: АВ-Центр and Перчатка-центр pages and 3C-Маркет's Google lists, every Monday at 09:00 in the `brevierp-sync` job (Mon–Fri 09:00 Kyiv) and on «Оновити зараз». Stage 3: Insight and Трєле dealer cabinets on every `brevierp-sync` run, Mon–Fri 09:00 (logins from Secret Manager).

## Flow

```mermaid
flowchart LR
  Admin[Admin «Імпорт цін»] --> API[PpeSupplierPricesController]
  API --> List[GetPpePriceSources]
  API --> Settings[UpdatePpePriceSource: RequireAdminRole]
  API --> Import[ImportPpeSupplierFiles: dryRun]
  API --> Refresh[RefreshPpeSupplierPrices: dryRun]
  Job[brevierp-sync: ppe-supplier-prices, Monday] --> Scheduled[PpeScheduledPriceRefresh]
  Import --> Reader[PpeSupplierFileReader: Insight YML / СЕМТО PDF]
  Refresh --> Online[PpeSupplierOnlinePriceReader: pages / 3C CSV / cabinets]
  Scheduled --> Online
  Import --> Runner[PpePriceImportRunner]
  Refresh --> Runner
  Scheduled --> Runner
  Runner --> Planner[PpePriceImportPlanner]
  Runner --> Offers[IPpeOfferVersionWriter]
  Settings --> Store[(PpePriceSources / PpePriceImportRuns)]
  Runner --> Store
```

## Code structure

```text
Catalog.Application/
  Contracts/Pricing/Ppe/SupplierPrices/   # store port, file and online reader ports, run/problem records
  Features/Pricing/Ppe/SupplierPrices/
    Sources/                              # fixed source list, key normalization
    List/                                 # source rows with counts and last run
    Settings/                             # discount and supplier (admins, director)
    Import/                               # file import command, planner, rows
    Refresh/                              # «Оновити зараз» command, Monday schedule
    Running/                              # shared run: plan, save offers, record the run
    Shared/                               # supplier resolution, product specs
Catalog.Infrastructure/Pricing/Ppe/SupplierPrices/
  Records/ Configuration/ Store/          # two catalog tables
  Files/                                  # Insight XmlReader, СЕМТО PdfPig, gzip unpack
  Online/                                 # АВ-Центр / Перчатка-центр page patterns, 3C CSV lists, HTTP client
    Cabinets/                             # Insight and Трєле sign-in, session pages, dealer price, options
Catalog.Api/Controllers/Pricing/Ppe/PpeSupplierPricesController.cs
Host.Seed/Jobs/Sync/PpeSupplierPricesSyncTask.cs  # brevierp-sync step ppe-supplier-prices (Mon–Fri 09:00)
```

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter PpeSupplierPrice`; integration: `--filter "FullyQualifiedName~PpeSupplierFile|FullyQualifiedName~PpePriceSourceStore|FullyQualifiedName~PpeSupplierPricesApi|FullyQualifiedName~PpeSupplierOnline|FullyQualifiedName~PpeScheduledPriceRefresh"`.

## Data

`catalog.PpePriceSources`, `catalog.PpePriceImportRuns` ([data model](../../architecture/data-model/catalog.md#ppe-supplier-price-import)); new prices are approved rows in `PpePricingOfferVersions`.
