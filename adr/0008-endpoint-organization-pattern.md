# 0008 — Endpoint organization: one extension file per resource under Endpoints/

Date: 2026-08-23

## Status

Accepted

## Context

The API uses ASP.NET Core minimal APIs. The first endpoints were mapped inline in `Program.cs`, which works for a handful of routes but scales badly: `Program.cs` mixes host/configuration wiring with HTTP surface, the route prefix is repeated on every mapping, and adding a resource means growing an already-busy file.

## Decision

Each API resource gets a dedicated static class under `Endpoints/`, exposing a single `Map<Resource>Endpoints()` extension method on `IEndpointRouteBuilder`. Inside, a `MapGroup("/api/<resource>")` route group declares the prefix once and the individual routes are relative to it. `Program.cs` stays limited to host wiring and one `app.Map<Resource>Endpoints();` call per resource.

Current instance: `Endpoints/OrderEndpoints.cs` with `MapOrderEndpoints()` grouping `GET /`, `GET /{id}`, and `POST /` under `/api/orders`.

## Consequences

- `Program.cs` contains only configuration and composition; the HTTP surface of a resource is readable in one file.
- New resources (customers, products, …) follow the same recipe: one file, one extension method, one line in `Program.cs`.
- Cross-cutting route concerns (auth, validation filters, versioned prefixes) can later be applied per group in one place via the `MapGroup` builder.
- Controllers remain deliberately unused; if the API ever needed controller-specific features (e.g. model-binding conventions, `ApiController` behaviors), that would be a new ADR.
