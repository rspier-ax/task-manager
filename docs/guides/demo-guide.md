# Demo guide

## Prerequisites

- .NET 8 SDK
- Seed user: `demo@taskmanager.local` / `Demo123!`
- Optional later: Node.js 20+ for the React SPA

## API walkthrough (Swagger)

1. Start the API:

```bash
dotnet run --project src/TaskManager.Api
```

2. Open Swagger at `http://localhost:5080/swagger` (or the URL printed in the console).
3. Call `POST /api/auth/login` with:

```json
{ "email": "demo@taskmanager.local", "password": "Demo123!" }
```

4. Copy the `token` from the response. Click **Authorize**, paste the token (Swagger adds `Bearer `), and confirm.
5. Call `GET /api/tasks` — expect the seeded sample tasks.
6. `POST /api/tasks` to create one, then `PUT` / `DELETE` as needed.
7. Clear the token (or use a private window) and call `GET /api/tasks` again — expect `401`.
8. Call `GET /api/health` without a token — expect `{ "status": "ok" }`.

## Frontend (later)

When the SPA lands: `cd frontend && npm run dev`, log in with the same seed credentials, and exercise the same CRUD path in the UI.
