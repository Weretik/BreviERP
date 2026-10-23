# BreviERP Glossary

This document contains only stable business terms that recur across feature specifications. Detailed rules for a specific capability remain in its `requirements/` directory.

## Product

A catalog item created and maintained by a manager. A product has one type: Sewing or PPE.

## Sewing product

A product made in-house. Its sewing operations, fabrics, accessories, and fabric usage per item are tracked.

## PPE

Personal protective equipment. A supplier and coefficient are defined for it.

## Sewing operation

A separate operation required to make a Sewing product. It has a duration used to calculate labor intensity and output per shift.

## Labor intensity

The total duration of all sewing operations for one product.

## Main fabric

The single primary fabric in a non-empty list of fabrics for a Sewing product. Other fabrics are additional.

## Accessory

A material or component needed to make a Sewing product, with a quantity per item.

## Meters per item

The number of meters of fabric required for one Sewing item.

## Supplier

A counterparty supplying PPE or reference materials. Supplier data is shared reference data and is not duplicated on a product.

## PPE coefficient

A value for a PPE product, supplied either by a shared reference entry or manually for that product only.

## Description, Information, Characteristics

Three kinds of product descriptive data: a multilingual Markdown description, ordered information blocks, and ordered characteristic tables.
