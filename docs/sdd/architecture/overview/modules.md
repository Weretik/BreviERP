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

The module exists in the solution with four layers, but domain scenarios are not implemented yet. Do not treat it as a contract for completed CRM functionality.

## Shared BuildingBlocks

Shared domain primitives, Application behaviors, domain event and result handling, and technical adapters.
