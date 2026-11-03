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
| `AdmToolsStorage` | Photo library CDN storage (Media module); login and password come from secrets | Login, password: yes |
| `MediaUpload` | Maximum photo size and CDN folder for new originals | No |
| `Reminders:Telegram` | Bot token, webhook secret and public webhook URL for [Telegram reminders](../jobs/telegram-reminders.md) | Token and secret: yes |

## Photo library uploads

`MediaUpload` configures the [photo library](../../features/media/library.md). `MaxFileSizeBytes` (15 MiB in `appsettings.json`) is the largest accepted photo; `BaseFolder` (`/library/original`) is the CDN folder for new originals. Both are required: an invalid section fails the first upload with an options validation error. The multipart request itself is capped at 32 MiB by the upload endpoint. It replaces the former `CatalogMediaUpload` section; remove that one from deployment overrides if it was set.

A production deployment can override these values without changing the file:

```text
MediaUpload__MaxFileSizeBytes=15728640
MediaUpload__BaseFolder=/library/original
```

The double underscore in an environment variable name corresponds to `:` in a .NET configuration key.

Example local secret:

```powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<local-only-value>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
```

Never add real passwords, tokens, or production connection strings to `appsettings*.json`, Markdown, source code, or logs. Do not print configuration values while diagnosing startup problems.

## Cookie and CORS note

Refresh authentication uses credential-bearing cookies, so CORS requires an explicit allowlist of origins; wildcard origins are not used. Check refresh over HTTPS or create an intentional local override that is never promoted to a shared environment.
