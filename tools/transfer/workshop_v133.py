#!/usr/bin/env python3
"""Builds the «Швейний цех» transfer files from a D1 export of the old «Цех» (WH-006).

Reads the JSONL tables of the export (one row per line, snake_case columns) and writes, in the format the existing
admin imports take:

* production-orders.json   → «Виробничі завдання → Імпорт» (orders and items; known orders are skipped by legacy id)
* production-progress.json → «Виробничі завдання → Імпорт етапів» (item stages and file links)

Only complete columns are read: the truncated quote snapshots are never used. Deleted orders are left out. Items keep
the order of the export rows within their order: that order is their position, which the stock transfer relies on.

Usage: python3 workshop_v133.py <export-d1-dir> <output-dir>
       python3 workshop_v133.py --database-json <database.json> <output-dir>   (the 23.10 export, for checking)
"""
import json
import sys
from pathlib import Path

STATUSES = {"new": "New", "closed": "Closed", "cancelled": "Cancelled"}
APPLICATIONS = {"embroidery": "Embroidery", "print": "Print"}
STAGES = {
    "fabric": "New", "fabric_ordered": "FabricOrdered", "fabric_available": "FabricAvailable", "cutting": "Cutting",
    "application": "Application", "sewing": "Sewing", "ready": "Ready", "shipped": "Shipped", "complete": "Complete",
}
FILE_KINDS = {"sizes": "Sizes", "application": "Application", "embroidery": "Application", "production": "Extra"}


def read_jsonl(directory: Path, table: str) -> list[dict]:
    path = directory / f"{table}.jsonl"
    if not path.exists():
        return []
    return [json.loads(line) for line in path.read_text(encoding="utf-8").splitlines() if line.strip()]


def read_database_json(path: Path) -> dict[str, list[dict]]:
    data = json.loads(path.read_text(encoding="utf-8"))
    tables = {}
    for name, value in data.items():
        columns = value.get("columns", []) if isinstance(value, dict) else []
        rows = value.get("rows", []) if isinstance(value, dict) else []
        tables[name] = [row if isinstance(row, dict) else dict(zip(columns, row)) for row in rows]
    return tables


def text(value):
    if value is None:
        return None
    value = str(value).strip()
    return value or None


def json_list(value) -> list:
    if not value:
        return []
    try:
        parsed = json.loads(value)
    except (TypeError, json.JSONDecodeError):
        return []
    return parsed if isinstance(parsed, list) else []


def item_row(item: dict) -> dict:
    applications = []
    for application in json_list(item.get("applications_json")):
        kind = APPLICATIONS.get(str(application.get("type", "")).lower())
        if kind is None:
            continue
        applications.append({"kind": kind, "place": text(application.get("place")),
                             "description": text(application.get("description")),
                             "color": text(application.get("color"))})
    cents = item.get("sewing_rate_cents")
    return {
        "modelCode": text(item.get("model_article")),
        "modelName": text(item.get("model_name")) or text(item.get("model_title")) or "",
        "fabricCode": text(item.get("fabric_directory_id")),
        "fabricName": text(item.get("fabric_name")),
        "sewingRate": None if cents is None else round(cents / 100, 2),
        "details": {
            "quantity": int(item.get("quantity") or 0),
            "dueDate": text(item.get("due_date")),
            "mainColor": text(item.get("color")),
            "additionalColors": [c for c in (text(x) for x in json_list(item.get("additional_colors_json"))) if c],
            "sizesText": text(item.get("sizes_text")),
            "technicalChanges": text(item.get("technical_changes")),
            "comment": text(item.get("comment")),
            "applications": applications,
        },
    }


def build(tables: dict[str, list[dict]]):
    orders = [o for o in tables["production_orders"] if not o.get("deleted_at")]
    orders.sort(key=lambda o: o["seq"])
    items_of: dict[str, list[dict]] = {}
    for item in tables["production_items"]:
        items_of.setdefault(item["order_id"], []).append(item)
    files_of: dict[str, list[dict]] = {}
    for file in sorted(tables.get("production_files", []), key=lambda f: (f.get("created_at") or "", f["id"])):
        files_of.setdefault(file["order_id"], []).append(file)
    deliveries = {d["order_id"]: d for d in tables.get("production_telegram_deliveries", [])}

    order_rows, progress_rows = [], []
    for order in orders:
        items = items_of.get(order["id"], [])
        order_rows.append({
            "legacyId": order["id"],
            "number": order["seq"],
            "quoteNumber": text(order.get("quote_number")),
            "clientName": text(order.get("client_name")) or text(order.get("counterparty_name")) or "",
            "managerEmail": text(order.get("manager_email")),
            "managerName": text(order.get("manager_name")) or "",
            "status": STATUSES[order["status"]],
            "orderDate": text(order.get("business_date")) or order["created_at"][:10],
            "comment": text(order.get("comment")),
            "items": [item_row(item) for item in items],
        })
        positions = {item["id"]: index + 1 for index, item in enumerate(items)}
        progress_rows.append({
            "legacyId": order["id"],
            "items": [{"position": positions[item["id"]],
                       "stages": [STAGES[s] for s in json_list(item.get("stages_json")) or [item.get("stage")]
                                  if s in STAGES]}
                      for item in items],
            "files": [row for row in (file_row(file, positions, order) for file in files_of.get(order["id"], []))
                      if row is not None],
            **telegram(order, deliveries),
        })
    return order_rows, progress_rows


def telegram(order: dict, deliveries: dict[str, dict]) -> dict:
    """The old app sent its own Telegram message. A D1 export without the deliveries table marks every order sent, so
    Brevi never sends it again; with the table, its state is taken."""
    if not deliveries:
        return {"telegramSent": True, "telegramSentAt": None}
    delivery = deliveries.get(order["id"])
    sent = delivery is not None and delivery.get("status") == "sent"
    return {"telegramSent": sent, "telegramSentAt": delivery.get("sent_at") if sent else None}


def file_row(file: dict, positions: dict[str, int], order: dict):
    kind = FILE_KINDS.get(str(file.get("kind", "")).lower())
    url = text(file.get("drive_url"))
    if kind is None or url is None:
        return None
    return {"position": positions.get(file.get("item_id")), "kind": kind, "name": text(file.get("name")) or "",
            "contentType": text(file.get("content_type")), "bytes": file.get("bytes"), "url": url,
            "createdAt": file.get("created_at"),
            "createdByName": text(order.get("manager_name")) if file.get("created_by") == order.get("manager_email")
            else text(file.get("created_by"))}


def main(argv: list[str]) -> int:
    if len(argv) == 4 and argv[1] == "--database-json":
        tables, output = read_database_json(Path(argv[2])), Path(argv[3])
    elif len(argv) == 3:
        source = Path(argv[1])
        tables = {name: read_jsonl(source, name)
                  for name in ("production_orders", "production_items", "production_files")}
        output = Path(argv[2])
    else:
        print(__doc__)
        return 2
    orders, progress = build(tables)
    output.mkdir(parents=True, exist_ok=True)
    for name, rows in (("production-orders.json", orders), ("production-progress.json", progress)):
        (output / name).write_text(json.dumps(rows, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"{len(orders)} orders, {sum(len(o['items']) for o in orders)} items, "
          f"{sum(len(p['files']) for p in progress)} file links → {output}")
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))
