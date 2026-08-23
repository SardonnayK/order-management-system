# 0009 — Mixed domain and data model

Date: 2026-08-23

## Status

Accepted

## Context

The order domain (Order, Customer, LineItem, OrderStatus) needs to be represented in three places: as EF Core entities persisted to SQLite, as the JSON contract of the API endpoints, and as the domain objects the application logic works with. A layered design would keep these separate — persistence entities, DTOs for the API, and a domain model in between — with mapping code between each layer.

This is a small assignment application with a handful of endpoints and one consumer (the Angular frontend). Three parallel class hierarchies plus mappers would be most of the codebase.

## Decision

Use **one set of classes** (`OrderManagement.Api.Models`) as EF entities, API request/response contracts, and domain objects. This is an explicit trade-off, accepted for now:

- Benefits: no mapping code, one place to change a field, fast to build and read — appropriate for the size of the app.
- Costs we knowingly accept: persistence concerns leak into the domain (relationship/ownership configuration shapes the classes), JSON serialization couples the API contract to the storage shape (renaming a column is an API break and vice versa), and computed get-only properties (`LineTotal`, `Subtotal`, `Total`) must be kept unmapped in EF while still serializing to clients.

Persistence-only concerns that the classes cannot express cleanly live in `AppDbContext.OnModelCreating` (shadow `CustomerId` FK, owned `OrderItems` collection, enum-to-string conversion, unique index on `CustomerId` + `ClientReference`).

## Consequences

- POST `/api/orders` accepts the entity shape directly, so the endpoint must validate and normalize input itself (require a customer, generate `Id`, stamp `CreatedAtUtc`) — there is no DTO boundary to do it.
- Clients can technically send fields that are storage details (e.g. a preset `Id`); the endpoints decide what to honor.
- Computed properties are serialized to clients for free, but any new get-only property must be checked against the EF model (EF ignores getter-only properties by convention; explicit mapping of one would fail).
- **Trigger for revisiting:** the moment the API contract and the storage shape need to diverge — versioned responses, hiding fields per endpoint, denormalizing storage, or a second consumer with different needs — introduce separate DTOs and/or persistence entities via a new ADR that supersedes this one.
