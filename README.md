# Order Management System

Full-stack order management application: .NET 10 Web API + EF Core (SQLite) backend, Angular 22 (standalone components) frontend, orchestrated locally by .NET Aspire and deployable with Docker Compose.

### Prerequisites
- .NET 10 SDK
- Node.js 20+ (for Angular)
- Angular CLI 22+ (npm install -g @angular/cli@22)

## Structure

```
├── adr/                               # Architecture Decision Records
├── Backend/OrderManagement.Api        # .NET 10 Web API + EF Core (SQLite)
├── Frontend/                          # Angular 22 SPA (standalone components)
├── Aspire/OrderManagement.AppHost     # Aspire orchestrator (API + Angular dev server)
├── Aspire/OrderManagement.ServiceDefaults
├── docker-compose.yml                 # Production-style deployment (API + nginx)
├── .env                               # All configurable settings (copy from .env.example)
└── OrderManagement.slnx
```

## Configuration

All settings live in `.env` at the repository root (see `.env.example`):

| Variable | Purpose |
|---|---|
| `DB_CONNECTION_STRING` | EF Core SQLite connection string |
| `API_PORT` | Port the API listens on / compose host port |
| `API_URL` | Base URL the Angular app calls (empty = same origin) |
| `FRONTEND_PORT` | Host port for the frontend |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Optional OTLP endpoint for telemetry export; empty = disabled (Aspire injects its own) |

The backend loads `.env` via `DotNetEnv` in `Program.cs`; the frontend generates `src/environments/environment.ts` from `.env` via `scripts/set-env.js` (runs automatically on `npm start` / `npm run build`).


### Aspire ServiceDefaults
The `Aspire/OrderManagement.ServiceDefaults` project defines default settings for the application, which can be overridden by the `.env` file. This allows for a flexible configuration that can adapt to different environments (development, staging, production).

It is a centralized place to define default values for various settings used throughout the application. It currently defines OpenAPI settings, CORS policies, and other application-specific configurations. The ServiceDefaults project is referenced by the Aspire AppHost and the backend API, ensuring that both parts of the application can access these default settings.

## Run locally with Aspire


```bash
cd Aspire/OrderManagement.AppHost
dotnet build
dotnet run
```

The Aspire dashboard opens with both the API and the Angular dev server. Aspire injects the resolved API endpoint into the frontend as `API_URL`.

## Run pieces individually

```bash
dotnet build 
# API (reads port + connection string from .env)
cd Backend/OrderManagement.Api && dotnet run --no-launch-profile

# Frontend
cd Frontend
npm i 
npm start
```

## Run with Docker Compose

```bash
docker compose up --build
```

- Frontend: http://localhost:4200 (nginx, proxies `/api` to the API container)
- API: http://localhost:5080
- SQLite data persists in the `sqlite-data` volume (mounted at `/data`).
