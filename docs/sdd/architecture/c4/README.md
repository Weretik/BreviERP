# C4 diagrams

**Checked against code:** 2026-11-16 (`src/`, `.github/workflows/deploy-cloudrun.yml`, `Host.Api/appsettings.json`, project references, brevi-web `apps/`).

Three levels of the [C4 model](https://c4model.com): who uses Brevi and what it talks to (context), which deployable parts it has (containers), and which modules are inside the API (components). The diagrams are Mermaid and render on GitHub. Why each part looks the way it does is in the [ADRs](../decisions/README.md).

Update a diagram in the same change that adds or removes a module, a host, a deployable app, an external system, or a connection between them.

## Level 1: System context

```mermaid
flowchart TB
    client["<b>Client</b><br/>[Person]<br/>Company that orders workwear or PPE"]
    staff["<b>Staff</b><br/>[Person]<br/>Director, managers, accountant,<br/>workshop chief, SEO"]

    brevi["<b>Brevi ERP</b><br/>[Software system]<br/>Pricing, КП, CRM, production, warehouse,<br/>accounting, public site"]

    keepin["<b>KeepinCRM</b><br/>[External]<br/>Clients and suppliers"]
    sheets["<b>Google Sheets</b><br/>[External]<br/>«финансы», «Лиды Brevi»"]
    suppliers["<b>Supplier sites</b><br/>[External]<br/>PPE price lists"]
    telegram["<b>Telegram</b><br/>[External]<br/>Existing bot and staff group"]
    gmail["<b>Gmail</b><br/>[External]<br/>Staff send КП (send-only)"]
    openai["<b>OpenAI</b><br/>[External]<br/>Reads invoices"]
    cdn["<b>CDN brevi-media.cdn.express</b><br/>[External, adm.tools]<br/>Photos"]

    client -->|"Reads the site, sends a request, phones"| brevi
    staff -->|"Works in the admin"| brevi
    brevi -->|"Imports clients and suppliers"| keepin
    brevi -->|"Imports invoices and leads"| sheets
    brevi -->|"Imports prices"| suppliers
    brevi -->|"Reminders, orders, site requests"| telegram
    brevi -->|"Sends КП"| gmail
    brevi -->|"Extracts invoice data"| openai
    brevi -->|"Stores photos"| cdn
```

## Level 2: Containers

```mermaid
flowchart TB
    client["<b>Client</b><br/>[Person]"]
    staff["<b>Staff</b><br/>[Person]"]

    subgraph cf["Cloudflare"]
        site["<b>Public site «Строчка»</b><br/>[Next.js on Workers]<br/>brevi.com.ua, server-rendered, SEO"]
        admin["<b>Admin «Цех»</b><br/>[React SPA on Cloudflare Pages]<br/>office.brevi.com.ua, noindex, cookie session"]
        proxy["<b>API proxy</b><br/>[Cloudflare Worker]<br/>api.brevi.com.ua"]
    end

    subgraph gcp["Google Cloud, project brevistoreapi (europe-central2)"]
        api["<b>Host.Api</b><br/>[ASP.NET Core, Cloud Run brevierp-api]<br/>Minimal API of all modules"]
        seed["<b>Host.Seed</b><br/>[.NET console, Cloud Run Jobs]<br/>brevierp-seed: migrations;<br/>brevierp-tasks: background tasks"]
        tick["<b>brevierp-tick</b><br/>[Cloud Scheduler]<br/>Every 5 minutes, OIDC"]
        db[("<b>PostgreSQL</b><br/>[Database]<br/>One schema per module")]
        bucket["<b>Cloud Storage</b><br/>Accounting files"]
        secrets["<b>Secret Manager</b>"]
        observability["<b>Cloud Logging + Cloud Trace</b><br/>JSON logs with trace id, OTLP traces"]
    end

    ext["<b>External systems</b><br/>KeepinCRM, Google Sheets, supplier sites,<br/>Telegram, Gmail, OpenAI, CDN"]

    client --> site
    staff --> admin
    site -->|"Reads products, pages; sends requests (HTTPS, OpenAPI)"| proxy
    admin -->|"JSON + cookie + CSRF"| proxy
    proxy --> api
    tick -->|"POST /internal/scheduler/tick"| api
    api -->|"Starts brevierp-tasks execution"| seed
    api --> db
    seed --> db
    api --> bucket
    api --> ext
    seed --> ext
    api -.-> secrets
    api -.->|"Console JSON logs; traces (OTLP/HTTP, ADR-0017)"| observability
    seed -.-> secrets
```

Deploy: a merge to `master` of `Weretik/BreviERP` starts GitHub Actions, which signs in without a key, runs `brevierp-seed` and then deploys `brevierp-api` ([ADR-0004](../decisions/0004-keyless-cloud-run-delivery.md), [ADR-0018](../decisions/0018-deploy-on-merge-from-main-repository.md)). The old OpenCart site keeps `brevi.com.ua` until the cutover; it is not drawn.

## Level 3: Components of the backend

```mermaid
flowchart LR
    host["<b>Host.Api / Host.Seed</b><br/>composition only:<br/>Add&lt;Module&gt;Module()"]

    subgraph sales["Selling"]
        catalog["<b>Catalog</b><br/>products, Sewing and PPE pricing"]
        crm["<b>Crm</b><br/>clients, КП, leads, tasks,<br/>site requests"]
        content["<b>Content</b><br/>site pages, categories, articles, SEO"]
    end

    subgraph make["Making"]
        production["<b>Production</b><br/>workshop orders, work log, payroll"]
        warehouse["<b>Warehouse</b><br/>fabric stock and issues"]
        reference["<b>Reference</b><br/>fabrics, accessories, operations,<br/>suppliers, pricing parameters"]
    end

    subgraph money["Money"]
        accounting["<b>Accounting</b><br/>accounts, operations, reports"]
        purchasing["<b>Purchasing</b><br/>purchase invoices"]
    end

    subgraph platform["Platform"]
        identity["<b>Identity</b><br/>staff, roles, «Права доступу»"]
        media["<b>Media</b><br/>photo library on the CDN"]
        ai["<b>Ai</b><br/>OpenAI settings and extraction"]
        reminders["<b>Reminders</b><br/>Telegram reminders"]
        scheduling["<b>Scheduling</b><br/>background tasks, tick"]
        activitylog["<b>ActivityLog</b><br/>«Журнал дій»: who did what"]
    end

    bb["<b>BuildingBlocks</b><br/>Result mapping, Mediator behaviors,<br/>outbox, read cache (HybridCache, in memory),<br/>shared primitives"]
    ext["<b>Outside systems</b><br/>KeepinCRM, Google Sheets, Gmail,<br/>OpenAI, Telegram, CDN adm.tools,<br/>Dropbox, supplier sites, Cloud Run"]

    host --> sales & make & money & platform
    crm --> catalog & reference & media & reminders
    production --> catalog & reference & media & reminders & crm
    warehouse --> production & reference
    purchasing --> ai & accounting & reference
    content --> catalog & reference & media
    catalog --> reference & media
    accounting --> reference & crm
    identity --> media & activitylog
    crm & production & warehouse & reference & catalog & media -. "activity.recorded (outbox)" .-> activitylog
    ext ==>|"ACL Integrations/KeepinCrm, GoogleSheets"| crm
    crm ==>|"ACL Integrations/Gmail, Pdf"| ext
    ext ==>|"ACL Integrations/KeepinCrm"| reference
    ext ==>|"ACL Integrations/PpeSupplierSites, PpeSupplierFiles"| catalog
    ext ==>|"ACL Integrations/GoogleSheets, Dropbox"| purchasing
    ai ==>|"ACL Integrations/OpenAi"| ext
    media ==>|"ACL Integrations/AdmTools"| ext
    reminders ==>|"ACL Integrations/Telegram; Crm and Production post through ITelegramBot"| ext
    scheduling ==>|"Outbox run; ACL Integrations/CloudRun starts brevierp-tasks"| ext
    sales & make & money & platform --> bb
```

This diagram is the context map. A thin arrow from A to B means A's Infrastructure references `B.Contracts` (checked from the `.csproj` files on 2026-11-18); Crm, Production and Warehouse also use `Identity.Contracts` for staff names. The arrows form no cycle ([ADR-0019](../decisions/0019-module-graph-without-cycles.md)): when an upstream module needs something of a downstream one, it declares a port in its own `Contracts` and the downstream module implements it, so the arrow still points downstream. Catalog implements Reference's pricing ports and Media's photo usage, Crm implements Media's photo usage, Production implements Crm's `IQuoteOrderLinks`, Warehouse implements Production's `IStockFacts`, and Identity implements ActivityLog's `IActivityActorNames` (the names in «Журнал дій»). No module references another module's Application, Domain or Infrastructure, and there are no foreign keys between schemas ([ADR-0001](../decisions/0001-modular-monolith.md), [ADR-0006](../decisions/0006-module-contracts.md)). The exact references are in the [module map](../overview/modules.md). Thick arrows cross the system edge: an anti-corruption layer in the module's `Infrastructure/Integrations/<System>` translates the outside model, and messages out go through the outbox, sent by the scheduler tick. The architecture test `Modules_do_not_depend_on_each_other_in_a_cycle` fails on any new cycle.

## Dynamic: an action reaches «Журнал дій»

How a staff member's change becomes an entry ([ADR-0016](../decisions/0016-domain-events-to-activity-log.md), [ACT-001](../../specs/activity-log/001-staff-activity-log/README.md)).

```mermaid
sequenceDiagram
    actor M as Manager
    participant A as Admin «Цех»
    participant E as Endpoint + handler (source module)
    participant G as Aggregate (Domain)
    participant S as SaveChanges + DomainEventOutboxInterceptor
    participant O as OutboxMessages (source schema)
    participant D as Outbox dispatcher (after the command, then the tick)
    participant H as RecordActivityOutboxHandler (ActivityLog)
    participant L as activity_log.ActivityEntries

    M->>A: changes КП №125
    A->>E: PUT /api/crm/quotes/{id}
    E->>G: quote.Save(...)
    G-->>G: Raise(QuoteChanged)
    E->>S: SaveChangesAsync
    S->>S: module describer → ActivityRecorded + actor
    S->>O: INSERT data + activity.recorded (one transaction)
    E-->>A: 200
    D->>O: takes the unsent message
    D->>H: payload
    H->>L: INSERT once by EventId
    A->>L: «Адміністрування → Журнал дій» (GET /api/activity-log/entries)
```
