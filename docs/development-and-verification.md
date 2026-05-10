# Development and Verification

## Repository Layout

The local workspace currently contains separate Git repositories:

```text
Financist/
  Financist.Api/
  Financist.Web/
```

This `docs` folder lives in `Financist.Api` because the PDF transaction import
pipeline is owned by the backend and exposed through the API. The frontend repo
has a smaller companion doc at:

```text
../Financist.Web/docs/document-import-ui.md
```

## Local Backend Commands

From `Financist.Api`:

```powershell
dotnet build src\Financist.Api\Financist.Api.csproj --no-restore -m:1 /p:UseSharedCompilation=false
dotnet test tests\Financist.UnitTests\Financist.UnitTests.csproj --no-restore -m:1 /p:UseSharedCompilation=false
dotnet test tests\Financist.IntegrationTests\Financist.IntegrationTests.csproj --no-restore -m:1 /p:UseSharedCompilation=false
```

Use the full solution when touching shared infrastructure:

```powershell
dotnet test Financist.sln -m:1
```

## Local Frontend Commands

From `Financist.Web`:

```powershell
npm run build
npm start -- --host 127.0.0.1 --port 4200
```

Default local URL:

```text
http://127.0.0.1:4200/
```

## Docker Compose

The compose file is in `Financist.Api`.

From `Financist.Api`:

```powershell
docker compose up --build
```

Services:

- API: `http://localhost:8080`
- PostgreSQL: `localhost:5432`
- Jaeger UI: `http://localhost:16686`
- pgAdmin: `http://localhost:5050`

Required environment:

```text
DEEPSEEK_API_KEY
```

The API compose service maps:

```text
Storage__DocumentsPath=/app/storage/documents
```

## Migrations

The candidate pipeline added this migration:

```text
src/Financist.Infrastructure/Persistence/Migrations/20260429213012_AddDocumentTransactionCandidates.cs
```

When adding or changing persistence shape:

```powershell
dotnet dotnet-ef migrations add <MigrationName> --project src/Financist.Infrastructure --startup-project src/Financist.Api --output-dir Persistence/Migrations
dotnet dotnet-ef database update --project src/Financist.Infrastructure --startup-project src/Financist.Api
```

In `Development`, the API also applies migrations on startup.

## Regression Checklist

Use this checklist after changing PDF import, candidate parsing, or the document
review UI.

Backend:

- Upload a readable PDF and confirm the document reaches `Completed`.
- Confirm chunks are created for RAG.
- Confirm candidates are created with `PendingReview`.
- Confirm re-uploading the same statement does not duplicate existing
  fingerprints.
- Confirm installment `02/10` and `03/10` produce different fingerprints.
- Confirm two identical purchases in the same PDF can both be represented by
  occurrence-aware fingerprints.
- Confirm importing creates exactly one `Transaction`.
- Confirm importing changes candidate status to `Imported`.
- Confirm rejecting changes candidate status to `Rejected`.
- Confirm imported/rejected candidates cannot be imported or rejected again.

Frontend:

- Upload a PDF from the documents page.
- Select the imported document.
- Confirm the candidate table appears.
- Edit a pending candidate and import it.
- Confirm the edited values are sent to the API.
- Confirm the transactions list refreshes after import.
- Reject a pending candidate.
- Confirm imported and rejected rows remain visible as history.

## Known Docker Notes

On Windows, Docker build may fail because of Docker Desktop/buildx file access,
for example access denied to a buildx lock file. When the source code builds
locally and the error points to Docker internals, rebuild from an elevated or
approved shell and retry:

```powershell
docker compose build --no-cache
docker compose up --build
```

The compose file currently uses a `version` key. Docker may warn that it is
obsolete; this warning is not expected to stop the API.

The API can also log a missing `libgssapi_krb5.so.2` warning from PostgreSQL
native auth libraries in the container. During the last verification this did
not stop the API from listening on port `8080`.
