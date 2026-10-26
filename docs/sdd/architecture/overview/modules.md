# Modules

## Identity module

Owns users, roles, refresh sessions, authentication, and access policies. The `api/auth/session` routes for login, refresh, logout, and the current user are implemented.

## Catalog module

Owns products, product categories, and catalog media. Publishes category and media APIs; media storage is adapted through Infrastructure.

## Reference module

Owns reference data for suppliers, fabrics, accessories, garment parts, operations on parts, and additional reference data.

## Accounting module

Owns wallets, transactions, and transaction categories. At the audit date, the module's API scenarios are not yet represented by controllers.

## Crm module

The client/contact slice implements scoped customer APIs, exact source identity, revision/idempotency and Admin merge in the existing four layers. See the [feature map](../../features/crm/clients-and-quotes.md). Quotes, custom sewing, document delivery and historical migration remain planned in CRM-001.

## Shared BuildingBlocks

Shared domain primitives, Application behaviors, domain event and result handling, and technical adapters.
