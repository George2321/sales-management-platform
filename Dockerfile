# syntax=docker/dockerfile:1

FROM node:22-bookworm-slim AS ui-build
WORKDIR /src/Presentation/UI
COPY Presentation/UI/package.json Presentation/UI/package-lock.json ./
RUN npm ci
COPY Presentation/UI/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY Presentation/API/SalesManagementPlatform.Api.csproj Presentation/API/
COPY Application/SalesManagementPlatform.Application.csproj Application/
COPY Domain/SalesManagementPlatform.Domain.csproj Domain/
COPY Infrastructure/SalesManagementPlatform.Infrastructure.csproj Infrastructure/
RUN dotnet restore Presentation/API/SalesManagementPlatform.Api.csproj

COPY Presentation/API/ Presentation/API/
COPY Application/ Application/
COPY Domain/ Domain/
COPY Infrastructure/ Infrastructure/
COPY --from=ui-build /src/Presentation/UI/dist/sales-management-ui/browser/ Presentation/API/wwwroot/
RUN dotnet publish Presentation/API/SalesManagementPlatform.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN mkdir -p /data && chown app:app /data
COPY --from=api-build /app/publish/ ./
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesManagementPlatform.Api.dll"]
