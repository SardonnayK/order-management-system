# 0004 — .env as single source of configuration

Date: 2026-08-23

## Status

Accepted

## Context

The assignment requires that both the API and the Angular frontend read configurable settings (API URL, SQLite path, ports) from a `.env` file, explicitly not hardcoded in `appsettings.json` or `environment.ts`. Sensitive and environment-specific settings should be changeable without touching source code. Two complications:

1. A browser SPA cannot read a server-side `.env` at runtime.
2. Hosts (Aspire, Docker Compose, launchSettings) inject their own environment variables, which must not be silently overwritten by `.env` values — e.g. Aspire injects `OTEL_EXPORTER_OTLP_ENDPOINT` and `API_URL` at run time.

## Decision

A single `.env` at the repository root (committed as `.env.example`, the real file gitignored) defines `DB_CONNECTION_STRING`, `API_PORT`, `API_URL`, `FRONTEND_PORT`, and `OTEL_EXPORTER_OTLP_ENDPOINT`.

- **Backend:** `Program.cs` loads it with `DotNetEnv.Env.TraversePath().NoClobber().Load()`. `NoClobber` makes host-supplied environment variables win over `.env` — DotNetEnv's default would have clobbered Aspire's injected OTLP endpoint and broken dashboard telemetry.
- **Frontend:** `scripts/set-env.js` (run automatically by npm `prestart`/`prebuild` hooks) generates `src/environments/environment.ts` from `.env` using Node's built-in `process.loadEnvFile()`, which has the same precedence: a real `API_URL` env var (Aspire, Docker build) wins over the `.env` value. The generated file is gitignored.
- **Compose:** `env_file: .env` plus `${VAR:-default}` interpolation for ports; container-specific values (`DB_CONNECTION_STRING` pointing at the volume, `ASPNETCORE_URLS`) are overridden per service.

## Consequences

- One place to configure the whole system; `.env.example` documents everything required to run it.
- Uniform precedence everywhere: real environment > `.env` > defaults.
- `environment.ts` must be generated before an Angular build; the npm hooks make this automatic, but running `ng build` directly on a clean checkout fails until `npm run build`/`npm start` has run once.
