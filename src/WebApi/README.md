# Genocs.Fonet.WebApi

Minimal ASP.NET Core API that builds PDFs through `Genocs.Fonet.XsltTransformer`, using the same pipeline as the Host console sample (`XslFoPdfService.Print`).

## Stack

| Concern | Package / tech |
|---------|----------------|
| Host / DI | `Genocs.Core` |
| Endpoints | `Genocs.WebApi` |
| OpenAPI | `Genocs.WebApi.OpenApi` |
| Logging | `Genocs.Logging` (Serilog) |
| Persistence | `Genocs.Persistence.MongoDB` |
| PDF engine | `Genocs.Fonet` + `Genocs.Fonet.XsltTransformer` |

## Endpoint

`POST /api/pdf`

```json
{
  "templateId": "books",
  "model": {
    "documentName": "My catalog",
    "bookList": [
      { "title": "Pinocchio", "author": "Carlo Collodi" }
    ]
  },
  "countryId": null
}
```

Response: `application/pdf` stream. Job id is returned in `X-Pdf-Job-Id`.

Template metadata (id → file name, model type) is stored in MongoDB. Template files, fonts, and assets live on disk / Docker volumes under `pdfStorage.*`.

## Local run

1. Start MongoDB (`docker compose up mongo -d` from the repo root).
2. Ensure `data/templates`, `data/fonts`, and `data/assets` contain the Host sample files (copied at project create time).
3. Run:

```powershell
dotnet run --project src/Genocs.Fonet.WebApi
```

Swagger: http://localhost:5080/swagger

## Docker

From the repository root:

```bash
docker compose up --build
```

Volumes:

| Host path | Container path |
|-----------|----------------|
| `docker/data/templates` | `/app/data/templates` |
| `docker/data/fonts` | `/app/data/fonts` |
| `docker/data/assets` | `/app/data/assets` |

API: http://localhost:5080 — health: `/health`, docs: `/swagger`.
