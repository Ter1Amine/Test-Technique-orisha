# Reception - Technical Test

Application for tracking the receipt of commands. A command contains palettes, a palette contains cartons, and a carton contains products. Each level has a receipt status computed from its products:

| Status | Displayed as | Rule |
| --- | --- | --- |
| `NotReceived` | Non reçu | No product received (or empty) |
| `PartiallyReceived` | Partiellement reçu | Some products received |
| `Received` | Reçu | All products received |

## Project structure

```
backend/    ASP.NET Core Web API (.NET 9, EF Core, SQL Server LocalDB)
  src/
    Reception.Api/             Controllers, Program.cs, configuration
    Reception.Application/     Services (business logic, validation, DTO mapping)
    Reception.Core/            DTOs and enums
    Reception.Domain/          Entities and repository interfaces
    Reception.Infrastructure/  EF Core DbContext, configurations, migrations, repositories
  tests/
    Reception.Tests/
frontend/   React 19 + TypeScript + Vite
  src/
    api.ts           API client
    App.tsx          Command list with collapsible palettes / cartons / products
    CommandForm.tsx  Command creation form
```

## Prerequisites

- [.NET SDK 9.0.311+](https://dotnet.microsoft.com/download) (see `backend/global.json`)
- SQL Server LocalDB (installed with Visual Studio or SQL Server Express)
- EF Core CLI: `dotnet tool install --global dotnet-ef`
- Node.js 20+ and npm

## Getting started

### 1. Database

Create the LocalDB instance used by the connection string, then apply the migrations:

```powershell
sqllocaldb create test-orisha -s
cd backend
dotnet ef database update --project src/Reception.Infrastructure --startup-project src/Reception.Api
```

The connection string is `ConnectionStrings:ReceptionDatabase` in `backend/src/Reception.Api/appsettings.json`.

### 2. Backend

```powershell
cd backend
dotnet run --project src/Reception.Api
```

- API: http://localhost:5123
- Swagger UI (Development): http://localhost:5123/swagger

CORS origins are configured in `Cors:AllowedOrigins` (default `http://localhost:5173`).

### 3. Frontend

```powershell
cd frontend
npm install
npm run dev
```

- App: http://localhost:5173
- The API URL is set by `VITE_API_URL` in `frontend/.env.development` (default `http://localhost:5123`).

## API

| Method | Route | Description |
| --- | --- | --- |
| GET | `/api/commands` | List all commands with their full hierarchy |
| GET | `/api/command/{commandId}` | Get a command by ID |
| POST | `/api/commands` | Create a command (always created as `NotReceived`) |
| PUT | `/api/{commandId}/receipt?isReceived=` | Set every product of the command as received / not received |
| PUT | `/api/{commandId}/palettes/{paletteId}/receipt?isReceived=` | Same, for a palette |
| PUT | `/api/{commandId}/cartons/{cartonId}/receipt?isReceived=` | Same, for a carton |
| PUT | `/api/{commandId}/products/{productId}/receipt?isReceived=` | Same, for a product |

### Create a command

```json
POST /api/commands
{
  "commandId": "CMD-001",
  "palettes": [
    {
      "paletteId": "PAL-001",
      "cartons": [
        {
          "cartonId": "CAR-001",
          "products": [
            { "refId": "PRD-001", "name": "T-shirt", "color": "Blue", "size": "M", "quantity": 10 }
          ]
        }
      ]
    }
  ]
}
```

Validation rules (checked on both frontend and backend):

- All IDs and product fields are required; quantity must be greater than 0.
- Each command / palette / carton must contain at least one child.
- IDs must be unique within the request and must not already exist in the database.

Returns `201 Created` with the command, or `400 Bad Request` with `{ "errors": [...] }`.

### Design choice: creating the whole command in one request

The form captures the entire command (palettes, cartons and products) and sends it in a **single** `POST /api/commands` call, instead of creating each level separately (command, then palettes, then cartons, then products).

This keeps the frontend and the database in sync and avoids partial or inconsistent data:

- **Atomic save**: the whole hierarchy is saved with one `SaveChanges`, i.e. one transaction. Either everything is saved or nothing is. There can be no command without palettes, or palette without cartons, left behind by a failure halfway through.
- **No partial state on errors**: if a network call, a validation or an ID conflict fails, nothing is written. The user fixes the form and submits again, with no orphan rows to clean up and no "half-created" command.
- **Validation of the whole**: the backend checks the full hierarchy at once (required fields, at least one child per level, IDs unique within the request and not already in the database) before writing anything.
- **Consistent status**: a command's status is computed from its products. Creating everything together guarantees the command is immediately `NotReceived` with a correct count (`0/N`), never in a temporary state such as an empty command shown as "not received" while products are still being added.
- **Single source of truth**: the API returns the created command, and the frontend adds that response to the list instead of rebuilding it locally, so the display always matches what is stored.
- **Simpler and more reliable**: one request instead of N sequential calls means no ordering issues between calls, no concurrent requests competing, and fewer round trips.

The same principle applies to receipt updates: each `PUT .../receipt` returns the full updated command, which replaces the one in the frontend state, so the statuses and counts of every level (product → carton → palette → command) stay in sync.

## Features

- List of commands with status badge and received count (e.g. `3/5`).
- Collapsible details: command → palettes → cartons → products.
- Checkboxes on products (and "check all" on cartons / palettes / commands): statuses and counts update live in the frontend, then "Enregistrer" sends the changes to the API ("Annuler" discards them).
- "Ajouter une commande" form to create a command with its full hierarchy.

## Tests

```powershell
cd backend
dotnet test
```
