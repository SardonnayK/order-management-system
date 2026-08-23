# 0005 — SQLite file placement and persistence

Date: 2026-08-23

## Status

Accepted

## Context

SQLite stores the database in a plain file, so two questions matter: where the file lives in each environment, and how it survives container restarts. Two constraints surfaced during scaffolding:

1. The repository lives on a case-insensitive Windows filesystem (WSL `/mnt/c` mount), where a runtime `data/` folder and the EF `Data/` source folder are **the same directory** — an early cleanup of the runtime folder deleted `Data/AppDbContext.cs`.
2. SQLite does not create missing directories, and a database written inside a container's filesystem is lost when the container is recreated.

## Decision

- Local runs: the connection string in `.env` is `Data Source=app_data/app.db`. The folder is named `app_data/` specifically so it can never collide with the `Data/` source folder on case-insensitive filesystems. It is gitignored and dockerignored.
- `Program.cs` parses the connection string with `SqliteConnectionStringBuilder` and creates the containing directory at startup, so a fresh checkout runs without manual steps.
- Docker: compose overrides the connection string to `Data Source=/data/app.db` and mounts the named volume `sqlite-data` at `/data`, so data persists across container restarts and rebuilds.

## Consequences

- No name collisions between runtime data and source folders on Windows/macOS.
- `docker compose down` keeps the data (`docker compose down -v` deletes it explicitly).
- The database file location is entirely configuration-driven; moving it means editing `.env` (or the compose override), not code.
