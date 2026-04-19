# Financist Backend

Production-oriented backend foundation for **Financist**, a personal finance platform built with **.NET 10**, **ASP.NET Core Web API**, **EF Core**, **PostgreSQL**, **JWT**, and an observability-first setup.

## Architecture Overview

The backend follows a pragmatic Clean Architecture approach:

- `Financist.Domain`
  Rich domain model with entities, value objects, enums, and business invariants.
- `Financist.Application`
  Use-case orchestration, DTOs, service contracts, repository interfaces, and application exceptions.
- `Financist.Infrastructure`
  EF Core persistence, PostgreSQL integration, JWT token generation, password hashing, seeding, telemetry wiring, and document storage.
- `Financist.Api`
  Controllers, middleware, dependency injection, authentication/authorization, versioning, Swagger, health checks, and metrics endpoints.

This keeps domain rules close to the model while leaving infrastructure concerns at the edges.

## Project Structure

```text
backend/
  Financist.sln
  dotnet-tools.json
  Dockerfile
  docker-compose.yml
  src/
    Financist.Api/
    Financist.Application/
    Financist.Domain/
    Financist.Infrastructure/
  tests/
    Financist.UnitTests/
    Financist.IntegrationTests/
```

## Scaffold Commands Used

The solution and projects were scaffolded with the official .NET CLI:

```powershell
New-Item -ItemType Directory -Force backend
Set-Location backend

dotnet new sln -n Financist -f sln

dotnet new webapi -n Financist.Api --use-controllers
dotnet new classlib -n Financist.Application
dotnet new classlib -n Financist.Domain
dotnet new classlib -n Financist.Infrastructure

dotnet new xunit -n Financist.UnitTests
dotnet new xunit -n Financist.IntegrationTests

dotnet sln Financist.sln add src/Financist.Api/Financist.Api.csproj
dotnet sln Financist.sln add src/Financist.Application/Financist.Application.csproj
dotnet sln Financist.sln add src/Financist.Domain/Financist.Domain.csproj
dotnet sln Financist.sln add src/Financist.Infrastructure/Financist.Infrastructure.csproj
dotnet sln Financist.sln add tests/Financist.UnitTests/Financist.UnitTests.csproj
dotnet sln Financist.sln add tests/Financist.IntegrationTests/Financist.IntegrationTests.csproj

dotnet add src/Financist.Api/Financist.Api.csproj reference src/Financist.Application/Financist.Application.csproj
dotnet add src/Financist.Api/Financist.Api.csproj reference src/Financist.Infrastructure/Financist.Infrastructure.csproj
dotnet add src/Financist.Application/Financist.Application.csproj reference src/Financist.Domain/Financist.Domain.csproj
dotnet add src/Financist.Infrastructure/Financist.Infrastructure.csproj reference src/Financist.Application/Financist.Application.csproj
dotnet add src/Financist.Infrastructure/Financist.Infrastructure.csproj reference src/Financist.Domain/Financist.Domain.csproj
```

## Key Design Decisions

- **Pragmatic DDD**
  The domain is not anemic. `Transaction`, `Goal`, `Card`, and `DocumentImport` enforce real invariants and state transitions.
- **No generic repository ceremony**
  Repository interfaces are focused on actual use cases, not abstracted for abstraction’s sake.
- **Application services over overengineered CQRS**
  The first version stays straightforward and readable while still keeping use-case logic out of controllers.
- **Observability from day one**
  Structured logging, traces, metrics, and health checks are part of the foundation, not an afterthought.
- **Future AI extension points**
  Document analysis is represented behind an application contract so OCR/extraction/classification services can be introduced later without rewriting the core layers.

## Domain Model

### Entities

- `User`
- `Transaction`
- `Category`
- `Card`
- `Goal`
- `DocumentImport`

### Value Objects

- `Money`
- `DateRange`

### Enums

- `TransactionType`
- `DocumentImportStatus`

### Sample Domain Rules

- Transactions must have an amount greater than zero.
- Only `Expense` transactions can be linked to a card.
- Goal target amounts must be greater than zero.
- Goals expose progress calculation.
- Card limits must be greater than zero.
- Document imports enforce valid state transitions.

## Local Development

### Prerequisites

- .NET SDK 10
- PostgreSQL 16+ or 17+
- Docker Desktop (optional, recommended)

### Default Development Configuration

Connection string in `src/Financist.Api/appsettings.json`:

```json
"Host=localhost;Port=5432;Database=financist;Username=postgres;Password=postgres"
```

Default seeded development user:

- Email: `dev@financist.local`
- Password: `Financist123!`

### Run with Docker Compose

From `backend/`:

```powershell
docker compose up --build
```

This starts:

- PostgreSQL on `localhost:5432`
- API on `http://localhost:8080`
- Jaeger UI on `http://localhost:16686`

### Run Locally Without Docker

1. Start PostgreSQL locally.
2. From `backend/`, apply migrations:

```powershell
dotnet dotnet-ef database update --project src/Financist.Infrastructure --startup-project src/Financist.Api
```

3. Start the API:

```powershell
dotnet run --project src/Financist.Api
```

In `Development`, the application also migrates and seeds the database on startup.

## Migrations

The repository includes an initial EF Core migration.

Useful commands:

```powershell
dotnet dotnet-ef migrations add <MigrationName> --project src/Financist.Infrastructure --startup-project src/Financist.Api --output-dir Persistence/Migrations
dotnet dotnet-ef database update --project src/Financist.Infrastructure --startup-project src/Financist.Api
```

Local EF tool manifest:

```powershell
dotnet tool restore
```

## Build and Test

Build the full solution:

```powershell
dotnet build Financist.sln -m:1
```

Run tests:

```powershell
dotnet test Financist.sln -m:1
```

## API Surface

Base path:

- `/api/v1`

Implemented endpoints:

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/register`
- `GET /api/v1/transactions`
- `POST /api/v1/transactions`
- `GET /api/v1/categories`
- `POST /api/v1/categories`
- `GET /api/v1/cards`
- `POST /api/v1/cards`
- `GET /api/v1/goals`
- `POST /api/v1/goals`
- `POST /api/v1/documents/upload`
- `GET /api/v1/dashboard/summary`
- `GET /health`
- `GET /metrics`

Swagger:

- `GET /swagger`

## Authentication

Authentication uses JWT bearer tokens.

- Login accepts email and password.
- Registration accepts full name, email, and password, and returns a JWT access token for the new user.
- Protected endpoints require `Authorization: Bearer <token>`.
- JWT settings can be configured through `appsettings` or environment variables:
  - `Jwt__Issuer`
  - `Jwt__Audience`
  - `Jwt__Key`
  - `Jwt__ExpirationMinutes`

## Observability

### Logging

- `Serilog`
- Structured console logs
- Request logging
- Correlation ID support through `X-Correlation-ID`

### Metrics

- `prometheus-net.AspNetCore`
- Prometheus-compatible HTTP metrics at `/metrics`

### Tracing

- OpenTelemetry tracing for:
  - ASP.NET Core
  - `HttpClient`
  - EF Core
- OTLP exporter compatibility via:
  - `OpenTelemetry__Otlp__Endpoint`

### Health

- Health endpoint at `/health`
- Includes EF Core/PostgreSQL connectivity check

## PostgreSQL Notes

Persistence uses `FinancistDbContext` with explicit Fluent API mappings:

- `users`
- `transactions`
- `categories`
- `cards`
- `goals`
- `document_imports`

Table names, relationships, money columns, and enum conversions are configured explicitly in the infrastructure layer.

## Future AI Integration

The current implementation intentionally keeps AI concerns behind application contracts:

- `IDocumentAnalysisService`

Today this is backed by a null/placeholder implementation that acknowledges future extraction support without polluting the core model. Later iterations can add:

- OCR/document extraction
- categorization suggestions
- transaction enrichment
- financial insights
- conversational assistant capabilities

Those additions can live in infrastructure adapters or new modules while preserving the existing domain and application boundaries.
