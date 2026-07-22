# Demo guide

> Stub — fill in after API and frontend land (later PR).

## Prerequisites

- .NET 8 SDK
- Node.js 20+ (for frontend)
- Seed user (planned): `demo@taskmanager.local` / `Demo123!`

## Walkthrough (planned)

1. Start API (`dotnet run --project src/TaskManager.Api`)
2. Start frontend (`cd frontend && npm run dev`)
3. Open the SPA → log in with seed credentials
4. List tasks → create a task → edit status → delete
5. Call a task endpoint without a token → expect `401`
6. Hit `GET /api/health` without a token → expect success

## Swagger

When the API is running, open `/swagger` for interactive exploration.
