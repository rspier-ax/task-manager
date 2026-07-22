# ADR-0002: SQLite for demo persistence

Date: 2026-07-22  
Status: proposed

## Context

Reviewers need to clone and run the app with minimal setup. A full SQL Server or Postgres dependency would add friction for a take-home demo.

## Decision

Use EF Core with SQLite for local demo and seeded data. Keep repository abstractions so the store can be swapped later without changing Application or Api contracts.

## Alternatives considered

- SQL Server LocalDB / Docker Postgres — more production-like, heavier for clones.
- In-memory-only store — simpler, but weaker demonstration of a real data layer and migrations mindset.

## Consequences

- Zero external DB install for the happy path.
- Must call out in the presentation that production would typically use SQL Server/Postgres with the same interfaces.

## References

- [ARCHITECTURE.md](../architecture/ARCHITECTURE.md)
