namespace Purchasing.Application.Features.Documents.Shared.Recognition;

/// <summary>What the AI is asked to read (user decision 2026-11-05: number, date, supplier, VAT and positions only).</summary>
public static class PurchaseRecognitionPrompt
{
    public const string SchemaName = "purchase_invoice";

    public const string Instructions = """
        You read one Ukrainian supplier invoice, order or similar purchase document and return only facts printed in it.
        - documentType: the title of the document as printed, e.g. «Рахунок на оплату», «Рахунок-фактура», «Замовлення покупця».
        - documentNumber: the number of the invoice or order as printed. documentDate: its date as YYYY-MM-DD.
        - supplierName: the seller (Постачальник / Виконавець / Продавець), not the buyer.
        - vat: "with_vat" only when the document clearly shows VAT (ПДВ) as a non-zero amount or a rate; otherwise
          "without_vat" (also when unsure).
        - pricesIncludeVat: true when the unit prices and line sums in the table already include VAT; false otherwise.
        - items: every position of the table in order, with name, quantity, unit (null if none), price and sum exactly as
          printed in the table. When the table has no price column (only a sum per line), put "" as price.
        - vatAmount: the VAT total for "with_vat", otherwise null. totalPayable: the final amount to pay (Всього до сплати).
        Numbers are strings with a dot as decimal separator, without spaces, currency or thousand separators
        (e.g. "12345.60"). Never invent or compute values that are not printed; copy them.
        """;

    public const string SchemaJson = """
        {
          "type": "object",
          "properties": {
            "documentType": { "type": "string" },
            "documentNumber": { "type": "string" },
            "documentDate": { "type": ["string", "null"], "description": "YYYY-MM-DD" },
            "supplierName": { "type": "string" },
            "vat": { "type": "string", "enum": ["with_vat", "without_vat"] },
            "pricesIncludeVat": { "type": "boolean" },
            "items": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" },
                  "quantity": { "type": "string" },
                  "unit": { "type": ["string", "null"] },
                  "price": { "type": "string" },
                  "sum": { "type": "string" }
                },
                "required": ["name", "quantity", "unit", "price", "sum"],
                "additionalProperties": false
              }
            },
            "vatAmount": { "type": ["string", "null"] },
            "totalPayable": { "type": "string" }
          },
          "required": ["documentType", "documentNumber", "documentDate", "supplierName", "vat", "pricesIncludeVat", "items",
            "vatAmount", "totalPayable"],
          "additionalProperties": false
        }
        """;
}
