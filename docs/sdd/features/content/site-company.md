# Company facts «Контент → Контакти та компанія»

The company facts of the public site: the company phone and email, the address and the map query, the working hours, the client managers of «Контакти» (name, photo, phones, email), the payment details and the Instagram, YouTube and Facebook links. One record saved as a whole with a revision check; the migration seeds what the site showed before. Admin and the Director edit, Manager reads. [SDD](../../specs/content/009-site-company/README.md), [contract](../../contracts/content/content-company.openapi.yaml), [tables](../../architecture/data-model/content.md).

The public site reads them without sign-in from `/api/public/company` ([contract](../../contracts/content/content-public-company.openapi.yaml)), with every phone also in the `tel:` form, rebuilt at most once a minute; without the facts the site keeps its own.

## Code structure

```text
src/Modules/Content/
  Content.Domain/Content.Domain/SiteCompany/SiteCompanyInfo.cs         # entity, nested records, phone and IBAN formats
  Content.Application/Content.Application/
    Features/SiteCompany/
      Queries/GetSiteCompany/, Commands/SaveSiteCompany/                # admin read and save
      Queries/GetPublicSiteCompany/                                     # public read kept a minute
      Shared/                                                           # responses, messages, mapping
  Content.Infrastructure/.../Persistence/Configurations/Site/SiteCompanyInfoConfiguration.cs   # JSON text columns
  Content.Infrastructure/.../Migrations/*_AddSiteCompany.cs             # table and the seed of the current facts
  Content.Api/Content.Api/
    Endpoints/SiteCompany/SiteCompanyEndpoints.cs                                # /api/content/company, policy access:content-company
    Endpoints/PublicSiteCompany/PublicSiteCompanyEndpoints.cs                    # /api/public/company, anonymous
```

## Tests

```text
tests/UnitTests/Content/SiteCompanyTests.cs     # dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SiteCompany"
tests/UnitTests/Content/SiteCompanyContractTests.cs
```

## Data

Schema `content`: SiteCompany (one row).
