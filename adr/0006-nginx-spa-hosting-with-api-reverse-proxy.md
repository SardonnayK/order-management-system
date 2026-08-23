# 0006 — Nginx SPA hosting with /api reverse proxy

Date: 2026-08-23

## Status

Accepted

## Context

In deployment the Angular app is a static bundle that needs a web server, and it must reach the API. Serving them from different origins would require CORS in production and would bake an absolute API URL into the frontend image, making the image environment-specific.

## Decision

The frontend image is a multi-stage build: Node builds the Angular bundle, then nginx serves it.

- The Docker build sets `API_URL=""` so the app calls the API with same-origin relative URLs (`/api/...`).
- nginx reverse-proxies `location /api/` to the API container (`http://api:8080`) on the compose network, and falls back to `index.html` for SPA routes (`try_files`).
- CORS on the API (`AllowAnyOrigin`) exists only for local development, where `ng serve` (port 4200) and the API (port 5080) are different origins.

## Consequences

- No CORS in production; the browser sees one origin.
- The frontend image is environment-agnostic — pointing it at a different API means changing the proxy target, not rebuilding the bundle.
- The nginx config (`Frontend/nginx.conf`) hardcodes the compose service name `api`; deploying outside compose requires adjusting it.
