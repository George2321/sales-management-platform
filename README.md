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
npm --prefix Presentation/UI ci
```

### Debug with VS Code

Install Chrome and Microsoft's C# extension for VS Code (`ms-dotnettools.csharp`). Open the repository root in VS Code and run the dependency install command above once after cloning.

In **Run and Debug**, select **Sales Management (.NET + Angular)** and press **F5**. The profile builds and starts the API, which initializes SQLite, then starts Angular and opens Chrome at [http://localhost:4200](http://localhost:4200). You can set breakpoints in both C# and TypeScript.

### Run locally from the terminal

Start the API with hot reload in one terminal.

```sh
dotnet watch --project Presentation/API/SalesManagementPlatform.Api.csproj run --launch-profile http
```

Start Angular in a second terminal.

```sh
npm --prefix Presentation/UI start
```

Open [http://localhost:4200](http://localhost:4200). Angular forwards `/api` requests to the API on port 5058. The native API stores its database at `Presentation/API/sales.db`.

## Run the backend tests

Run the NUnit suite from the repository root with the .NET 10 SDK.

```sh
dotnet test
```

Both groups live in `tests/SalesManagementPlatform.Tests`. To run one group, filter by its NUnit category.

| Folder | Coverage | Command |
| --- | --- | --- |
| `Unit/Domain` | Order rules and calculations in memory | `dotnet test --filter "TestCategory=Unit"` |
| `Integration/Api` | HTTP requests through the API with a temporary SQLite database | `dotnet test --filter "TestCategory=Integration"` |

## Database setup

The API creates any missing tables and indexes from `Infrastructure/Persistence/Database/Schema.sql` at startup. The local Development profile and Docker Compose also enable `Database:SeedDemoData`. On an empty database, the seed adds eight fictional customers and ten orders dated in the current UTC month. Once customers exist, startup skips seeding. Choose an existing customer when creating an order.

If you ran an older schema locally, stop the API and move `Presentation/API/sales.db` aside before restarting. The API will create a fresh database from the current schema.
