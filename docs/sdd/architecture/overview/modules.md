# Modules

## Identity module

Owns users, roles, refresh sessions, authentication, and access policies. The `api/auth/session` routes for login, refresh, logout, and the current user are implemented.

## Catalog module

Owns products and product categories. Product photos keep a Media library photo id and read URLs through `IProductMediaReader` (host adapter); Catalog answers which products use a photo through `GetProductMediaUsagesQuery`.

## Reference module

Owns reference data for suppliers, fabrics, accessories, garment parts, operations on parts, and additional reference data.

## Accounting module

Owns wallets, transactions, and transaction categories. At the audit date, the module's API scenarios are not yet represented by controllers.

## Crm module

The client/contact slice implements scoped customer APIs, exact source identity, revision/idempotency and Admin merge in the existing four layers. See the [feature map](../../features/crm/clients-and-quotes.md). Quotes, custom sewing, document delivery and historical migration remain planned in CRM-001.

## Media module

Owns the photo library in schema `media`: folders and photos on the Brevi CDN (adm.tools adapter in Infrastructure). Admin APIs live under `/api/media` for Manager and Admin. Other modules store only the photo id ([MED-001](../../specs/media/001-media-library/README.md)).

## Reminders module

Owns deadline reminder tasks, their periods and action history, Telegram chats and extra non-working days in schema `reminders`. Admin APIs live under `/api/reminders`; the Telegram webhook is in the same module, and the daily send runs as the `Host.Seed --reminders-dispatch=true` mode. See the [feature map](../../features/reminders/telegram-reminders.md).

## Shared BuildingBlocks

Shared domain primitives, Application behaviors, domain event and result handling, and technical adapters.
