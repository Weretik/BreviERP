# Google field-to-target map

**Source snapshot:** 2026-10-26, all five exact tabs, `A1:T1000`, headers only. [Evidence and workbook](verified-baseline.md). These are mappings, not evidence that target tables or populated source rows exist.

The database is normalized by ownership/lifecycle, not one table per source tab. All **62** headers below have a destination and conversion rule. Source spellings are preserved; target identifiers are proposed. Crm owns the quote/issuer structures; live product/reference/Identity identities remain with their existing modules.

## КП — 13 fields

| Source header | Proposed destination | Rule |
| --- | --- | --- |
| ID КП | SourceMappings.SourceId → Quotes.PublicId | External source key; never relational PK |
| Номер КП | Quotes.Number | Preserve source number; collision goes to review |
| Дата создания | Quotes.SourceCreatedAt / CreatedAt | UTC conversion with raw timestamp provenance |
| Менеджер | Quotes.ResponsibleUserId | Explicit Identity mapping; actor cannot be forged |
| Статус | Quotes.Status + version event | draft/ready/sent; unknown value quarantined |
| Клиент | Customer reference + RecipientSnapshot.Name | Resolve identity, retain quoted name |
| Контактное лицо | Contact reference + RecipientSnapshot.ContactName | Snapshot survives contact edits |
| Телефон | RecipientSnapshot.Phone | Keep source display; normalized live channel separately |
| E-mail | RecipientSnapshot.Email | Validate for new sending; preserve invalid history as flagged |
| Внутренняя заметка | QuoteVersion.InternalNote | Private, excluded from PDF/email |
| Дата последнего изменения | Quotes.UpdatedAt / version audit | Source timestamp is not target concurrency revision |
| Дата отправки | Imported delivery event.SourceSentAt | Historical evidence only; never queues send |
| Ссылка на PDF | QuoteDocumentArtifact source reference | Controlled bytes retrieval, private hash; missing-file state |

## Позиции КП — 13 fields

| Source header | Proposed destination | Rule |
| --- | --- | --- |
| ID позиции | SourceMappings → logical item PublicId | Stable identity across immutable versions |
| ID КП | QuoteVersion.QuoteId | Resolve parent first; orphan goes to staging |
| Порядковый номер | QuoteVersionItem.SortOrder | Unique ordered item within version |
| Тип | QuoteVersionItem.Kind | Typed supported kind; unknown quarantined |
| ID товара / модели | Catalog Article/Product ref or custom version ref | Model alone is not a sellable sewing Article |
| Название для клиента | QuoteVersionItem.DisplayName | Editable public snapshot, not master rename |
| Ссылка на товар | QuoteVersionItem.ProductLinkSnapshot | Validated display URL; never remote file authority |
| Фото из справочника | QuoteVersionItem.CatalogMediaRef | Validated Catalog provenance, pinned snapshot |
| Заменённое фото для КП | QuoteVersionItem.OverrideMediaRef | Private quote-only file, owner checked |
| Описание для клиента | QuoteVersionItem.PublicDescription | Escaped customer-facing text |
| Комментарий для клиента | QuoteVersionItem.PublicComment | Escaped customer-facing text |
| Внутренняя заметка менеджера | QuoteVersionItem.InternalNote | Private projection only |
| Показывать в PDF | QuoteVersionItem.IncludeInDocument | Strict boolean; exclusion enforced server-side |

## Варианты позиции — 13 fields

| Source header | Proposed destination | Rule |
| --- | --- | --- |
| ID варианта | SourceMappings → logical variant PublicId | Stable identity; version child is immutable |
| ID позиции КП | QuoteItemVariant.ItemId | Resolve correct version/item, not current item by name |
| Порядковый номер варианта | QuoteItemVariant.SortOrder | Unique ordered option within item |
| Название варианта для клиента | QuoteItemVariant.DisplayName | Public snapshot |
| Показывать клиенту | QuoteItemVariant.Visible | Enforced in customer projection |
| Цена 1 | QuoteVariantTier[retail].Amount | Decimal server result; preserve source/provenance |
| Цена 2 | QuoteVariantTier[wholesale].Amount | Semantic code; quantity boundaries verified EN-002 |
| Цена 3 | QuoteVariantTier[largeWholesale].Amount | Semantic code; no zero-as-missing sentinel |
| Показывать цену 1 | QuoteVariantTier[retail].Visible | Independent of stored amount |
| Показывать цену 2 | QuoteVariantTier[wholesale].Visible | Independent of stored amount |
| Показывать цену 3 | QuoteVariantTier[largeWholesale].Visible | Independent of stored amount |
| Способ формирования цены | QuoteItemVariant.PriceMethod | Calculated, manualPpe, oneOff or importedLegacy with provenance |
| Причина ручной цены (только СИЗ) | QuotePriceOverride.Reason | Required on new manual PPE overrides; legacy absence flagged |

## Расчётный состав варианта — 12 fields

| Source header | Proposed destination | Rule |
| --- | --- | --- |
| ID строки расчёта | SourceMappings → calculation line identity | Source provenance retained |
| ID варианта | QuoteCalculationLine.VariantId | Resolve immutable variant version |
| Тип | QuoteCalculationLine.Kind | Fabric/accessory/operation/other typed line |
| ID составляющей | QuoteCalculationLine.SourceReference + Version | Cross-module resolved input, no copied master |
| Название | QuoteCalculationLine.NameSnapshot | Immutable historical label |
| Единица измерения | QuoteCalculationLine.UnitSnapshot | Preserve unit, convert only via explicit mapping |
| Исходная норма / количество / время | QuoteCalculationLine.OriginalQuantity | Decimal and unit dimension |
| Изменённая норма / количество / время | QuoteCalculationLine.EffectiveQuantity | Validated change, does not change Catalog norm |
| Цена единицы / стоимость минуты | QuoteCalculationLine.UnitCost | Server-resolved version, private cost projection |
| Итоговая стоимость строки | QuoteCalculationLine.LineTotal | Computed decimal result with rounding provenance |
| Изменено менеджером | QuoteCalculationLine.ChangeMetadata | Server derives flag and actor, do not trust posted boolean |
| Внутренний комментарий к изменению | QuoteCalculationLine.ChangeReason | Private/audited; excluded from customer output |

## Настройки КП — 11 fields

| Source header | Proposed destination | Rule |
| --- | --- | --- |
| Полное название предприятия | IssuerSettingsVersion.LegalName | Seller, not customer; actual value not present in source tab |
| ЄДРПОУ | IssuerSettingsVersion.RegistrationCode | Verified seller value, not invented |
| ИНН / номер плательщика НДС | IssuerSettingsVersion.TaxRegistration | Does not alone define tax arithmetic |
| Юридический адрес | IssuerSettingsVersion.LegalAddress | Versioned seller snapshot |
| IBAN | IssuerSettingsVersion.Iban | Validated verified seller bank account |
| Банк | IssuerSettingsVersion.BankName | Versioned seller bank details |
| МФО | IssuerSettingsVersion.BankCode | Nullable if not applicable under confirmed policy |
| Телефон | IssuerSettingsVersion.Phone | Seller contact |
| E-mail | IssuerSettingsVersion.Email | Seller contact; not sender credential |
| Сайт | IssuerSettingsVersion.Website | Validated display URL |
| Логотип для КП | IssuerSettingsVersion.LogoFileId | Validated bytes/reference pinned to settings version |

## Brevi additions, not populated Sheet columns

Customer/contact PublicIds and kind, normalized search channels, exact external identities, owner access, optimistic revision, requestId, immutable quote/custom version IDs, parent/version links, calculation input/formula versions and blockers, currency/tax provenance, optional requested quantity, archived audit, storage hashes/ACL, outbox/provider attempts and import reconciliation statuses are target additions. They do not imply those values can be imported from these headers.

Source item quantity/time in calculation lines is a component norm; it is not a mandatory sales order quantity. Issuer banking details must not be mapped into customers. Google has no client master records in these tabs. Historical source keys, customer roles, snapshots and custom drafts come from a separately reconciled complete database export, not generated from blank rows.
