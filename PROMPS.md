

## Initial Git Setup
```
Setup my git folder, I will be building a dotnet + angular minimal application.
Ensure relevant folders are in the .gitignore for this stack.
```
---
```markdown
## Scaffolding the Application

### Task
Scaffold a complete full-stack .NET and Angular application. Use .NET Aspire to orchestrate the local development environment and generate a Docker Compose file for potential production deployment.

### Tech Stack
- **Backend:** .NET 10 Web API
- **Database ORM:** Entity Framework Core (EF Core) using SQLite (file-system database)
- **Frontend:** Angular v22 (Standalone components)
- **Local Orchestration:** .NET Aspire
- **Deployment Orchestration:** Docker Compose
- **Configuration Management:** .env files

### Requirements
1. **Application Structure:** Create a clean directory structure separating the `Backend` (API), `Frontend` (Angular SPA), and `Aspire` (AppHost/ServiceDefaults).
2. **Configuration Setup:** Ensure both the .NET API and the Angular frontend read their configurable settings (like API URLs and the SQLite database file path) from a `.env` file. Do not hardcode these in `appsettings.json` or `environment.ts`.
3. **Database Setup:** Scaffold a basic EF Core DbContext using SQLite. Map the SQLite connection string (e.g., `Data Source=app.db`) to the `.env` file configuration.
4. **Aspire Orchestration:** Configure the Aspire AppHost to natively spin up the Backend API and the Angular frontend server.
5. **Docker Compose:** Generate a `docker-compose.yml` file at the root to run the API and the Frontend (served via an Nginx container). Ensure a volume is mapped for the SQLite `.db` file so data persists between container restarts.

### Verification Steps
Before declaring this task complete, you must verify your work:
1. Run `dotnet build` on the entire solution to ensure there are no compilation errors.
2. Verify that the frontend compiles successfully (e.g., `npm run build` or `ng build`).
3. Ensure the `.env` loading logic is properly implemented in the backend `Program.cs`.
4. Validate that the `docker-compose.yml` syntax is correct, maps the `.env` variables, and includes the volume mount for SQLite.
Read the outputs of these checks and fix any errors before finishing.
```

Handles the boilerplate scaffolding for a full-stack .NET and Angular application, ensuring proper configuration management, database setup, and orchestration for both local development and potential production deployment.

Aspire is used to orchestrate the local development environment, while Docker Compose is set up to show competence in deployment orchestration. The `.env` file is used for configuration management, ensuring that sensitive information and environment-specific settings are not hardcoded into the application.

We use .env for the Angular frontend and the .NET backend to ensure that both parts of the application can be configured easily without changing the source code. The SQLite database is used for simplicity, and its file is persisted using Docker volumes in that use case.

ServiceDefaults in Aspire is used to define default settings for the application, which can be overridden by the `.env` file. This allows for a flexible configuration that can adapt to different environments (development, staging, production).
---