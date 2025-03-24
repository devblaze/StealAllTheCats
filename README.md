# 🐱 StealAllTheCats

[![.NET Build](https://github.com/devblaze/StealAllTheCats/actions/workflows/dotnet.yml/badge.svg)](https://github.com/<your-github-username>/<your-repository-name>/actions/workflows/dotnet.yml)
[![Docker Build](https://github.com/devblaze/StealAllTheCats/actions/workflows/docker-image.yml/badge.svg)](https://github.com/<your-github-username>/<your-repository-name>/actions/workflows/docker-image.yml)

> Simple and clear guide to get started quickly on local environment using Docker Compose and .NET 8.0.

## 🔧 Prerequisites
Ensure the following are installed on your computer:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Any IDE (recommended: [JetBrains Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/downloads/))

Verify the installations:
``` bash
docker --version
dotnet --version
```
## 🚀 Getting Started Locally
### 1. Clone the Repository:
Begin by cloning your repo:
``` bash
git clone https://github.com/devblaze/StealAllTheCats.git
cd StealAllTheCats
```
### 2. Starting Local Development Environment using Docker Compose:
Simply use Docker Compose to build and run all necessary services (SQL Server database included):
``` bash
docker compose build
docker compose up
```
Docker Compose will:
- Create a fresh SQL Server DB instance.
- Build and run your ASP.NET application inside Docker.
- **Automatically apply migrations** and create database/schema (`CatsDb`).

### 3. Running Application (direct URLs):
Open your browser or API testing tool to access:

| Application URLs                                               | Environment                           |
|----------------------------------------------------------------|---------------------------------------|
| [Docker Swagger](http://localhost:8080/swagger/index.html)     | Swagger (API Docs Local Development)  |
| [.NET https Launch](https://localhost:7284/swagger/index.html) | Swagger (API Docs Local Development)  |
| [.NET http Launch](http://localhost:5221/swagger/index.html)   | Swagger (API Docs Local Development)  |                                      |

## 🖥 Developing Locally Without Docker:
You can optionally build and run without Docker directly from your IDE or CLI:
- Open solution (`StealAllTheCats.sln`) in Rider/Visual Studio.
- Update project dependencies and restore packages explicitly if needed:
``` bash
dotnet restore
```
- Apply migrations manually on your local DB (first-time setup):
``` bash
dotnet ef database update -p StealAllTheCats -s StealAllTheCats
```
- Run directly from IDE or using CLI:
``` bash
dotnet run --project StealAllTheCats
```
- Now navigate to [https://localhost:8080/swagger](https://localhost:8080/swagger) (or whatever port reported by your OS) explicitly to test API calls.

## ✅ Useful Docker Compose Commands Clearly Shown:
- **Stop and remove containers**, but preserve database data (do NOT remove volume):
``` bash
docker compose down
```
- **Stop and cleanup everything completely** (removes database volumes/data as well, started fresh):
``` bash
docker compose down -v
```
- **Inspect logs clearly:**
``` bash
docker compose logs -f
```
## 🧹 Project Structure Explained Clearly:
``` 
📦 StealAllTheCats
├── 📁 Controllers/                      # Web API Controllers.
├── 📁 Dtos/                             # Data Transfer Objects.
│   ├── 📁 Mappers/                      # DTO mapping logic.
│   ├── 📁 Requets/                      # Request DTOs.
│   ├── 📁 Responses/                    # Response DTOs.
│   ├── 📁 Results/                      # Operation results.
│   └── 📜 Result.cs                     # Generic Result wrapper.
├── 📁 Services/                         # Business logic implementations.
│   ├── 📁 Interfaces/                   # Interfaces for service abstraction.
│   ├── 📜 ApiClient.cs                  # API client implementation.
│   └── 📜 CatService.cs                 # Cat-related operations.
├── 📁 Properties/                       # Launch profiles and settings.
├── 📜 appsettings.json                  # Application settings (DB connections, API keys).
├── 📜 appsettings.Development.json      # Dev-specific configuration.
├── 🐋 Dockerfile                        # Containerization instructions.
├── 📜 Program.cs                        # Application entry point.
└── 📜 StealAllTheCats.csproj            # Project file.
📦 StealAllTheCats.Common
├── 📁 Dtos/                             # Shared Data Transfer Objects.
└── 📜 Configuration.cs                  # Shared configuration helpers.
📦 StealAllTheCats.Database
├── 📁 Migrations/                       # EF Core migration files.
├── 📁 Models/                           # Database entities.
├── 📁 Repositories/                     # Database access logic.
│   ├── 📁 Interfaces/                   # Repository interfaces.
│   └── 📜 GenericRepository.cs          # Generic repository implementation.
└── 📜 ApplicationDbContext.cs           # EF Core database context.
📦 StealAllTheCats.Tests
├── 📁 Services/                         # Tests for business logic services.
│   └── 📁 CatServicesTests/             # Cat service specific tests.
│       ├── 📁 Fixtures/                 # Shared test setup.
│       ├── 📜 FetchCatsTests.cs         # Tests for fetching and storing cats.
│       ├── 📜 GetCatByIdTests.cs        # Tests for retrieving single cats by ID.
└──     └── 📜 GetCatsPaginatedTests.cs  # Tests for cat pagination and filtering.


```
## 🌟 Recommended IDE Configuration and Tools (optional but recommended):
- **JetBrains Rider**:
    - Automatic Docker Compose integration: Open `compose.yaml` directly to interact visually and debug containers.
    - Integrated debugging, migrations, NuGet management.

- **Visual Studio**:
    - Good built-in Docker support, run/debug containers directly from IDE.

## 📖 EF Core Migrations - How Clearly Explained:
Command clearly explained for adding migrations explicitly if you modify entities/models in EF Core yourself:
``` bash
dotnet ef migrations add MigrationName -p StealAllTheCats -s StealAllTheCats
dotnet ef database update -p StealAllTheCats -s StealAllTheCats
```
## 💡 Troubleshooting & Common Issues Explicitly Mentioned:
### Issue: "Cannot connect/login to DB immediately after container starts."
- This issue occurs if migrations run when the SQL container hasn't fully initialized.
- Clear solution: Just wait longer or explicitly restart to retry, Docker compose health-check provided in YAML file ensures readiness anyway.

### Issue: "Database migration conflict/error: Object already exists."
- Ensure you're clearly using `dbContext.Database.Migrate();` only (not `EnsureCreated()`).
- If issues persist (caused by conflicting DB states), explicitly restart completely fresh:
``` bash
docker compose down -v
docker compose build
docker compose up
```

## 📝 License:
This project is licensed under the MIT License - see the LICENSE file for details.