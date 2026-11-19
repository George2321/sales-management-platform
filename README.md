# Sales Management Platform

## Run and clone locally with Docker instructions:

Install Docker with Compose and start Docker. From the repository root, run:

```sh
docker compose up --build
```

Open [http://localhost:8080](http://localhost:8080). The same address serves Angular and the API. You can check API startup at [http://localhost:8080/api/health](http://localhost:8080/api/health).

The API initializes `/data/sales.db` inside the container and adds demo data on first run. Restarting the same container keeps its database. `docker compose down` removes the container and its database. The next run generates a fresh demo dataset.

## Local development setup instructions:

Install the .NET 10 SDK and Node.js 22 with npm. Any editor works.

Run the following commands from the repository root. Install the UI dependencies first.

```sh
npm --prefix UI ci
```

Start the API with hot reload in one terminal.

```sh
dotnet watch --project API/SalesManagementPlatform.Api.csproj run --launch-profile http
```

Start Angular in a second terminal.

```sh
npm --prefix UI start
```

Open [http://localhost:4200](http://localhost:4200). Angular forwards `/api` requests to the API on port 5058. The native API stores its database at `API/sales.db`.

## Database setup

The API creates any missing tables and indexes from `Data/Database/Schema.sql` at startup. The local Development profile and Docker Compose also enable `Database:SeedDemoData`. On an empty database, the seed adds eight fictional customers and ten orders dated in the current UTC month. An existing database with customers keeps its records and dates on subsequent starts.
