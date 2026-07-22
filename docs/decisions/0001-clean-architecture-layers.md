# ADR-0001: Clean Architecture layers

Date: 2026-07-22  
Status: accepted

## Context

The interview exercise requires separation of concerns: API, business logic, and data access, with the business layer independent of the data layer and API. The solution must remain understandable in a short code review.

## Decision

Split the backend into Domain, Application, Infrastructure, and Api projects under Clean Architecture. Domain and Application must not reference EF Core or ASP.NET assemblies. Controllers stay thin; validation and rules live in Application.

## Alternatives considered

- Single ASP.NET project with folders only — faster to start, weaker independence for review and tests.
- Vertical slice / feature folders without a Domain project — fine for larger apps, less explicit for this exercise's wording.

## Consequences

- Clear mapping to the assignment's required layers and unit-test surfaces.
- Slightly more ceremony for a small CRUD app (acceptable for the interview narrative).

## References

- [ARCHITECTURE.md](../architecture/ARCHITECTURE.md)
- [engineering-practice.md](../engineering-practice.md)
