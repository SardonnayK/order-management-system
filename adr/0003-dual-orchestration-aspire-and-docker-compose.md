# 0003 — Aspire for local development, Docker Compose for deployment

Date: 2026-08-23

## Status

Accepted

## Context

The system has two runnable parts (API and SPA) that developers want to start with one command, with observability while developing. Separately, the assignment calls for a production-style deployment artifact. These are different problems: local orchestration benefits from live dev servers, hot reload, and telemetry; deployment needs reproducible container images.

## Decision

Use **.NET Aspire** (AppHost 13.5.2) to orchestrate the local development environment, and a hand-written **docker-compose.yml** at the root for deployment orchestration — Compose is included deliberately to demonstrate deployment competence, not generated from Aspire.

- The AppHost starts the API as a project resource and the Angular dev server via `AddJavaScriptApp(..., "start")` from `Aspire.Hosting.JavaScript` (the Aspire 13 package; the older `Aspire.Hosting.NodeJs` 9.x is version-mismatched with the 13.x AppHost SDK).
- Aspire assigns the frontend port through the `PORT` env var (`ng serve --port ${PORT:-4200}`) and injects the resolved API endpoint into the frontend as `API_URL`.
- `ServiceDefaults` provides OpenTelemetry, health endpoints, service discovery, and HTTP resilience defaults to the API; the Aspire dashboard receives telemetry via an injected `OTEL_EXPORTER_OTLP_ENDPOINT`.
- Docker Compose builds two images (API, nginx-served SPA) and wires them on one network (see ADR 0006).

## Consequences

- One command per world: `dotnet run` in the AppHost for development with a dashboard; `docker compose up --build` for a production-style run.
- Two orchestration definitions must be kept in sync when resources are added (accepted trade-off; the system is small).
- Local development does not require Docker at all.
