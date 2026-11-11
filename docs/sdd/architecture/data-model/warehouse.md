# Warehouse schema

Code model reviewed 2026-11-11. `Warehouse.Infrastructure.Persistence.WarehouseDbContext` owns schema `warehouse`; migration history is `warehouse.__EFMigrationsHistory`. Migration `InitialWarehouse` creates the four tables and copies the rows of the former `production` stock tables with their ids, then drops those tables ([WH-001](../../specs/warehouse/001-warehouse-module/README.md); rules from [PRD-004](../../specs/production/004-fabric-stock/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| FabricStocks | Generated int id. FabricId (Reference fabric id as a value), FabricCode (32), FabricName (300), Color (80), ColorKey (80, lower case); unique (FabricCode, ColorKey). Meters numeric(12,2) (may be negative), HasMovements, CreatedAt, UpdatedAt, Version (concurrency), unique LegacyId |
| FabricMovements | Generated int id, StockId → FabricStocks (restrict). Kind (Opening/Receipt/Issue/Adjustment/Reversal), MetersBefore, Delta, MetersAfter, OperationDate, Note, SupplierId (Reference supplier as a value), SupplierName, Document, OrderId, ItemId, OrderNumber, ModelName (snapshot of the target), ReversesMovementId → FabricMovements, IsReversed, CreatedByName, CreatedAt, unique LegacyId |
| FabricIssues | Generated int id, OrderId, ItemId (Production order and item as values, no FK). OrderNumber, ModelName, IssuedOn, Note, CreatedByName, CreatedAt, ReversedAt, ReversedByName, ReverseReason, unique LegacyId |
| FabricIssueLines | Generated int id, IssueId → FabricIssues (cascade), StockId → FabricStocks, MovementId → FabricMovements (unique). Purpose (Planned/Replacement/Additional), PlanFabricCode, PlanColorKey, PlannedMeters, FabricCode, FabricName, Color, Meters, CoveredMeters |
