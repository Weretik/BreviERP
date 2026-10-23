# Product page — descriptive data

## Accepted descriptive data structure

### Description

The user enters two required texts: Ukrainian and Russian, up to `20 000` characters each. Description is stored and sent as Markdown. Paragraphs, headings, numbered and bulleted lists, links, bold text, and italics are allowed. Tables, images, and raw HTML are not allowed: tables are represented only through `Characteristics`, and the frontend renders Markdown with a sanitized renderer.

### Information

Information is an ordered collection of localized `heading → text` blocks. Each block has Ukrainian and Russian headings up to `200` characters and plain-text content up to `2 000` characters. It is not a characteristics table: the frontend displays each block as a separate information section in the selected language.

```text
InformationBlock
├── TitleUk, TitleRu
├── TextUk, TextRu
└── SortOrder
```

### Characteristics

Characteristics is an ordered collection of localized subtables. Each subtable has Ukrainian and Russian headings up to `200` characters and an ordered list of rows with Ukrainian and Russian `parameter → value` pairs. A parameter is limited to `200` characters and a value to `1 000` characters:

```text
Characteristics
├── Таблиця «Матеріали»
│   ├── «Основна тканина» → «...»
│   └── «Щільність»       → «...»
└── Таблиця «Догляд»
    └── «Прання»          → «...»
```

Table and row order are part of the data because the frontend uses them.

```text
CharacteristicTable
├── TitleUk, TitleRu
├── SortOrder
└── Rows[]
    ├── LabelUk, LabelRu
    ├── ValueUk, ValueRu
    └── SortOrder
```

## Frontend format

The API sends structured data, not HTML. Localized values are stored separately for `uk` and `ru`; the exact locale parameter and response DTO shape are defined by the OpenAPI contract.
