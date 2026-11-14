# Own Profile

**Module:** identity.

«Мій кабінет» in Admin: the signed-in staff member sees their name, email, role and photo. Every staff role changes their own name and photo; administrators (the director also holds Admin) also change their own phone, email and password (owner decision 2026-11-03). Other people's name, email and phone are changed on «Користувачі». The same photo shows in the header menu and on «Користувачі»; without a photo Admin shows the first letter of the name on a color.

## Rules

- Policy `OwnProfile` (Accountant, Manager, WorkshopChief, Seo, Admin) on `/api/profile`; phone, email and password also need `RequireAdminRole` (`403` for others).
- Phone: optional, 7 to 15 digits with an optional leading `+`, spaces, dashes or brackets (`AspNetUsers.PhoneNumber`, no new column). It also shows on «Користувачі».
- Email and password changes need the current password; a wrong one returns `400` on `currentPassword` and counts towards lockout. A taken email returns `409`. Other sessions end; this browser gets a fresh cookie.
- Photo: JPEG, PNG or WebP, at most 2 MB, at least 128×128 px, at most 24 megapixels. The server applies EXIF orientation, crops the centre square, and stores a 256×256 WebP under a new name, then deletes the previous file. Deleting a staff account deletes its photo.
- Photos live on the Brevi CDN through the Media module storage, folder `/avatars` (not in «Фото»). The database keeps only the key (`AspNetUsers.AvatarPath`); the URL is built at read time. Without storage configured the upload answers `503`.

## Code structure

```text
src/Modules/Identity/
├── Identity.Api/Identity.Api/
│   ├── Profile/                                    # ProfileController (/api/profile), request and response records
│   └── Avatars/AvatarUploadResults.cs              # multipart limit, 503 mapping
├── Identity.Application/Identity.Application/
│   ├── Features/Profile/{Commands,Queries}/        # GetProfile, ChangeOwnName, ChangeOwnPhone, ChangeOwnEmail,
│   │                                               # ChangeOwnPassword, UploadOwnAvatar, RemoveOwnAvatar
│   ├── Features/Avatars/Shared/AvatarRules.cs      # size limits
│   └── Abstractions/IProfileService.cs, IAvatarStorage.cs
└── Identity.Infrastructure/Identity.Infrastructure/
    ├── Integrations/AspNetIdentity/Profile/ProfileService.cs
    └── Integrations/Avatars/                       # SkiaSharp processing, replace/remove
src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Adapters/Media/CdnAvatarStorage.cs  # Media.Contracts ICdnStorage
```

## Tests

`tests/IntegrationTests/Identity/ProfileApiTests.cs`: a manager changes name and photo but gets `403` on email and password; a small or non-image photo is refused; an administrator changes password and email only with the current password and stays signed in. Focused run: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~ProfileApiTests`.

## Data

`identity."AspNetUsers"."AvatarPath"` (`varchar(200)`, null), migration `AddUserAvatarPath`.

## Related sources

[OpenAPI](../../contracts/identity/profile.openapi.yaml), [roles and policies](../../architecture/security/authorization/roles-and-policies.md), [staff accounts](staff-accounts.md).
