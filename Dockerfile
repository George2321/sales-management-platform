# syntax=docker/dockerfile:1

FROM node:22-bookworm-slim AS ui-build
WORKDIR /src/UI
COPY UI/package.json UI/package-lock.json ./
RUN npm ci
COPY UI/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY API/SalesManagementPlatform.Api.csproj API/
COPY Data/SalesManagementPlatform.Data.csproj Data/
COPY Domain/Domain.csproj Domain/
COPY Infrastructure/SalesManagementPlatform.Infrastructure.csproj Infrastructure/
RUN dotnet restore API/SalesManagementPlatform.Api.csproj

COPY API/ API/
COPY Data/ Data/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
COPY --from=ui-build /src/UI/dist/sales-management-ui/browser/ API/wwwroot/
RUN dotnet publish API/SalesManagementPlatform.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN mkdir -p /data && chown app:app /data
COPY --from=api-build /app/publish/ ./
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesManagementPlatform.Api.dll"]
