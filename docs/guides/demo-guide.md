# Demo guide

## Prerequisites

- .NET 10 SDK
- Node.js 20+ (for the React SPA)
- Seed user: `demo@taskmanager.local` / `Demo123!`

## SPA walkthrough

1. Start the API:

```bash
dotnet run --project src/TaskManager.Api
```

2. Start the frontend:

```bash
cd frontend && npm ci && npm run dev
```

3. Open `http://localhost:5173`.
4. Sign in with the seed credentials (hint shown on the login page).
5. Review seeded tasks → create a task → edit status → delete.
6. Sign out and confirm you are redirected to login.

## API walkthrough (Swagger)

1. With the API running, open `http://localhost:5080/swagger`.
2. Call `POST /api/auth/login` with:

```json
{ "email": "demo@taskmanager.local", "password": "Demo123!" }
```

3. Copy the `token`. Click **Authorize**, paste it, and confirm.
4. Call `GET /api/tasks` — expect the seeded sample tasks.
5. Clear the token and call `GET /api/tasks` again — expect `401`.
6. Call `GET /api/health` without a token — expect `{ "status": "ok" }`.
