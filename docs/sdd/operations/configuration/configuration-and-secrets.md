# Configuration and secrets

## Sources

`Host.Api` uses the standard ASP.NET Core configuration stack. In Development, the project has a User Secrets ID; use User Secrets for sensitive local values. Deployment configuration should pass sensitive values through an approved secret mechanism or environment variables.

Environment variable keys use double underscores, for example `ConnectionStrings__Default` for `ConnectionStrings:Default`.

## Important settings

| Setting | Purpose | Secret |
| --- | --- | --- |
| `ConnectionStrings:Default` | PostgreSQL connection for Identity and module contexts | Yes |
| `ADMIN_DEFAULT_PASSWORD` | Password used only if a bootstrap administrator must be created | Yes |
| `Cors:AllowedOrigins` / `AllowedOriginsCsv` | Browser origins allowed to call the API | No, but deployment-specific |
| `Identity:SessionCookies` | Cookie names, paths, lifetimes, `Secure` and SameSite rules | No |
| `Identity:SessionSecurity` | Access and refresh token lifetime limits | No |
| `AdmToolsStorage` | Catalog media storage settings | Depends on the provider |
| `CatalogMediaUpload` | Maximum upload size and base path for original Product media | No |

## Catalog media uploads

`CatalogMediaUpload` is a required configuration section. The code has no defaults: `MaxFileSizeBytes` and `BaseFolder` are set in `appsettings.json` or deployment configuration. The API applies one defined limit both to the multipart request body and the upload command.

Before startup, the Host verifies that `MaxFileSizeBytes` is greater than zero and `BaseFolder` is not empty. Invalid configuration blocks startup rather than the first upload request.

A production deployment can override these values without changing the file:

```text
CatalogMediaUpload__MaxFileSizeBytes=104857600
CatalogMediaUpload__BaseFolder=/products/original
```

The double underscore in an environment variable name corresponds to `:` in a .NET configuration key.

Example local secret:

```powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-only-value>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
```

Never add real passwords, tokens, or production connection strings to `appsettings*.json`, Markdown, source code, or logs. Do not print configuration values while diagnosing startup problems.

## Cookie and CORS note

Refresh authentication uses credential-bearing cookies, so CORS requires an explicit allowlist of origins; wildcard origins are not used. Check refresh over HTTPS or create an intentional local override that is never promoted to a shared environment.
