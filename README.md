# Cat Collector

ASP.NET Core Web API that imports cat images from [TheCatAPI](https://thecatapi.com/), stores them locally, and exposes endpoints for retrieval and filtering. Includes a lightweight Blazor UI.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containerized setup)

## Quick Start with Docker Compose

```bash
docker compose build
docker compose up
```

This starts the API and a SQL Server instance. The database is created and migrated automatically on startup.

- **Swagger**: http://localhost:8080/swagger
- **Blazor UI**: http://localhost:8080/ui

To tear down everything including data:
```bash
docker compose down -v
```

## Running Without Docker

You need a SQL Server instance. Update the connection string in `appsettings.json`, then:

```bash
dotnet restore
dotnet run --project StealAllTheCats
```

Migrations are applied automatically at startup. Or manually:
```bash
dotnet ef database update -p StealAllTheCats.Database -s StealAllTheCats
```

- **Swagger**: http://localhost:5221/swagger
- **Blazor UI**: http://localhost:5221/ui

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/cat-imports` | Starts a background import of 25 cats. Returns a job ID immediately. |
| GET | `/api/cat-imports/{id}` | Returns import job status (queued, running, completed, failed) with counters. |
| GET | `/api/cats` | Returns paged cats. Query params: `page`, `pageSize`, `tag`. |
| GET | `/api/cats/{id}` | Returns a single cat by database ID, including tags. |
| GET | `/api/cats/{id}/image` | Returns the stored cat image bytes. |

## Running Tests

```bash
dotnet test
```

To run a specific test class:
```bash
dotnet test --filter "FullyQualifiedName~CatImportServiceTests"
```

## EF Core Migrations

```bash
dotnet ef migrations add MigrationName -p StealAllTheCats.Database -s StealAllTheCats
dotnet ef database update -p StealAllTheCats.Database -s StealAllTheCats
```

## Architecture

Four projects in the solution:

- **StealAllTheCats** — API controllers, services, background job infrastructure, Blazor UI
- **StealAllTheCats.Database** — EF Core DbContext, entity models, generic repository
- **StealAllTheCats.Common** — Shared configuration binding
- **StealAllTheCats.Tests** — xUnit + Moq unit tests

### Background Jobs

Import jobs use a `Channel<int>` queue with a `BackgroundService` worker. When `POST /api/cat-imports` is called, a job record is created in the database (status: queued), its ID is pushed onto the channel, and the response returns immediately. The worker picks up the job, sets it to running, fetches 25 cats with breed data from TheCatAPI, downloads each image, extracts temperament tags, and saves everything to the database. Duplicate cats (by CatId) are skipped, not overwritten.

### Image Storage

Images are stored as `byte[]` in the database (`CatEntity.ImageData` column) and served via `GET /api/cats/{id}/image`.

**Trade-offs:**
- Storing in the database keeps the deployment self-contained — no volume mounts or file path management needed. For 25 cat images (~5-15 MB total), the overhead is negligible.
- For a production service with thousands of images, filesystem or blob storage (S3, Azure Blob) would be more appropriate. The database approach was chosen here for simplicity and portability in a demo/review context.
- The original TheCatAPI URL is also stored in `ImageUrl` as a fallback reference.

## License

MIT — see [LICENSE](LICENSE) for details.
