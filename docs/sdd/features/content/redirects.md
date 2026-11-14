# Redirects «Контент → Переадресації»

Permanent (301) redirects of the public site: an old address and the page of the new site it leads to. Addresses are kept as normalized paths, so a full link with a host, capitals or a closing slash matches. Staff add, change and delete redirects; a taken old address or a two-step loop is refused with a Ukrainian message. A one-time transfer takes the addresses of the old site from a prepared file and finds the page for each (article, category, product page or the product's category; `/ru/` addresses go to the Russian page); the preview shows what was not found. When staff change the address of a product page, a category or an article, the old address of both languages gets a redirect by itself. Admin and the Director edit, Manager reads. [SDD](../../specs/content/005-redirects/README.md), [contract](../../contracts/content/content-redirects.openapi.yaml), [tables](../../architecture/data-model/content.md).

The public site reads every redirect with its final page without sign-in from `/api/public/redirects` ([contract](../../contracts/content/content-public-redirects.openapi.yaml)) and answers 301 for an address it does not know. Answers are rebuilt at most once a minute.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/
    Redirects/         # SiteRedirect (+ source), SiteRedirectRules (normalizing, limits)
  Content.Application/Content.Application/
    Features/Redirects/
      Queries/GetSiteRedirects/          # every redirect by the old address
      Commands/SaveSiteRedirect/         # add or change; taken address and loop checks
      Commands/DeleteSiteRedirect/
      Commands/ImportOldSiteRedirects/   # file shape, OldSiteRedirectResolver, transfer handler
      Queries/GetPublicRedirects/        # public list, chains followed
      Shared/                            # responses, mapping, messages, SiteRedirectWriter (address changes), public source kept for a minute
  Content.Infrastructure/Content.Infrastructure/
    Persistence/Redirects/SiteRedirectConfiguration.cs
  Content.Api/Content.Api/
    Endpoints/Redirects/ContentRedirectsEndpoints.cs        # /api/content/redirects, policy access:content-redirects
    Endpoints/PublicRedirects/PublicRedirectsEndpoints.cs   # /api/public/redirects, anonymous, read-only
```

The save handlers of product pages, site categories and articles call `SiteRedirectWriter` when the address changed.

## Tests

```text
tests/UnitTests/Content/SiteRedirectsTests.cs           # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteRedirects"
tests/UnitTests/Content/SiteRedirectsContractTests.cs
```

## Data

Schema `content`: SiteRedirects.
