# ADR-0003: JWT Bearer authentication

Date: 2026-07-22  
Status: proposed

## Context

The exercise requires user creation, login, and both authorized and non-authorized endpoints. A React SPA will call the API cross-origin.

## Decision

Use JWT Bearer tokens issued on login/register. Protect task CRUD with `[Authorize]`. Leave health (and auth endpoints) public. Scope all task operations to the `sub` / name identifier claim (user id).

## Alternatives considered

- Cookie + identity UI — natural for MVC server-rendered apps, less ideal for a separate SPA demo.
- API keys only — does not satisfy user create/login requirements.

## Consequences

- Straightforward SPA integration via `Authorization` header.
- Need secure password hashing, token lifetime configuration, and CORS for the Vite origin.

## References

- [ARCHITECTURE.md](../architecture/ARCHITECTURE.md)
