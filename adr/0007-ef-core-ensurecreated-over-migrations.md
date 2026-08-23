# 0007 — EF Core EnsureCreated over migrations

Date: 2026-08-23

## Status

Accepted (final)

## Context

The scaffold needs a working database schema on first run, in every environment (local, Aspire, Docker), without manual steps. EF Core offers two paths: `Database.EnsureCreated()` (create the schema directly from the model) or migrations (versioned, incremental schema changes).

Migrations earn their cost when a database with real data must survive schema changes across releases. This application is an assignment project and **will not become a production application**, so that scenario does not apply.

## Decision

Call `Database.EnsureCreated()` at API startup, as the **final** schema-management approach for this project. Migrations and the `dotnet ef` tooling are deliberately left out.

## Consequences

- Zero-step first run everywhere; no migration tooling to install or document.
- Schema changes during development are handled by deleting the SQLite file (local: `app_data/`, Docker: `docker compose down -v`) and letting the API recreate it — acceptable because all data is disposable.
- If the project's fate ever changes and it heads to production after all, this decision must be superseded by a new ADR introducing migrations; `EnsureCreated`-created databases have no migration history, so that would mean a fresh baseline, not an in-place upgrade.
