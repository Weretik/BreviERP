# Gmail for КП (CRM-003 R-15)

Each staff member sends КП from their own Gmail. Brevi asks Google only for `openid`, `email` and `gmail.send`: it can send letters and never reads the mailbox. The refresh token is encrypted with ASP.NET Data Protection (keys in PostgreSQL, `identity.DataProtectionKeys`) and stored in `crm.StaffMailboxes`.

## Configuration

| Key | Where | Value |
| --- | --- | --- |
| `Crm:Gmail:ClientId` | Secret Manager `crm-gmail-client-id` → `Crm__Gmail__ClientId` | OAuth client id of type **Web application** |
| `Crm:Gmail:ClientSecret` | Secret Manager `crm-gmail-client-secret` → `Crm__Gmail__ClientSecret` | its secret |
| `Crm:Gmail:RedirectUri` | `appsettings.Production.json` | `https://api.brevi.com.ua/api/crm/mailboxes/google-callback` |
| `Crm:Gmail:AdminUrl` | `appsettings.Production.json` | `https://office.brevi.com.ua` |

While the client id or secret is empty, Gmail is off: «Підключити Gmail» and «Надіслати» answer «Gmail ще не налаштовано». The deploy workflow attaches the two secrets only when `CRM_GMAIL_ENABLED` is `'true'` in `.github/workflows/deploy-cloudrun.yml`, so a deploy never fails because they are missing.

## One-time setup in Google Cloud (owner)

1. The OAuth app lives in the Google Cloud project **BreviERP Mail** (`brevierp-mail`); enable **Gmail API** there. Its older client «BreviERP — підключення пошти» is a Desktop app and cannot take the web redirect, so stage 3 uses a separate Web client «BreviERP — надсилання КП» (2026-11-04).
2. **Google Auth platform → Audience**: External, publishing status **In production**. In Testing, Google drops the access after 7 days. An unverified app shows «Google hasn't verified this app»; staff press «Advanced → Go to …». That is acceptable for up to 100 staff; verification is needed only for wider use.
3. **Data Access**: add the scope `https://www.googleapis.com/auth/gmail.send` (plus `openid`, `email`).
4. **Clients → Create client → Web application**. Authorized redirect URI: `https://api.brevi.com.ua/api/crm/mailboxes/google-callback`. Copy the client id and secret; do not send them in chat.
5. In Secret Manager of `brevistoreapi` create `crm-gmail-client-id` and `crm-gmail-client-secret` with those values. The Cloud Run runtime service account already has Secret Accessor.
6. Set `CRM_GMAIL_ENABLED: 'true'` in the deploy workflow (a one-line PR), Sync fork, wait for the green deploy. Done 2026-11-04; set it back to `'false'` to switch sending off.

## Use

- «Налаштування КП → Контакти менеджерів у КП»: «Підключити Gmail» per staff member (Director or Admin), or the manager connects their own Gmail from «Надіслати КП». The person signs in to their Gmail on the Google page and allows sending. The row then shows the address; «Відключити» forgets it.
- After a password change or revoking access in the Google account, sending answers «Доступ до Gmail закінчився…»; connect again.
- A network failure while sending says to check «Надіслані» before trying again: Gmail may have sent the letter.

## Local development

Create your own Web client with redirect `http://localhost:5000/api/crm/mailboxes/google-callback` and put the id and secret into user secrets (`dotnet user-secrets set "Crm:Gmail:ClientId" … --project src/Bootstrapper/Host.Api`). Without them Gmail stays off locally.
