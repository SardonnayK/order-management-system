# Build context is the repository root (see docker-compose.yml)
FROM node:24-alpine AS build
WORKDIR /app

COPY Frontend/package.json Frontend/package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY Frontend/ .
# Empty API_URL makes the app call the API on the same origin; nginx proxies /api to the api container.
ENV API_URL=""
RUN npm run build

FROM nginx:alpine AS runtime
COPY Frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html
EXPOSE 80
