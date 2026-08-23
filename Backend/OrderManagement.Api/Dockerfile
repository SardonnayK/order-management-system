# Build context is the repository root (see docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend/OrderManagement.Api/OrderManagement.Api.csproj Backend/OrderManagement.Api/
COPY Aspire/OrderManagement.ServiceDefaults/OrderManagement.ServiceDefaults.csproj Aspire/OrderManagement.ServiceDefaults/
RUN dotnet restore Backend/OrderManagement.Api/OrderManagement.Api.csproj

COPY Backend/ Backend/
COPY Aspire/OrderManagement.ServiceDefaults/ Aspire/OrderManagement.ServiceDefaults/
RUN dotnet publish Backend/OrderManagement.Api/OrderManagement.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderManagement.Api.dll"]
