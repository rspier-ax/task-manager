# Development workflow

Branch format: **`<type>/<short-description>`**

| Type | Use for |
|------|---------|
| `chore` | Scaffold, tooling, config |
| `feature` | User-facing or domain behavior |
| `docs` | Documentation-only changes |
| `test` | Tests without production code changes |
| `bugfix` | Defect fixes |

## Flow

```
Plan (Cursor plan or issue notes)
       ↓
Feature branch (<type>/…)
       ↓
Small PR with tests
       ↓
Merge to main
       ↓
Update living docs only if behavior or boundaries changed
       ↓
New plan for the next slice
```

## Pull requests

- One logical bucket per PR (domain, application, API, frontend, docs).
- Include a short test plan in the PR body.
- Do not commit directly to `main` for feature work.
- **Never stage or commit `_local/`** (private notes). If it appears in `git status` as staged, abort.

## Anti-patterns

- Monolithic PR covering the entire app
- Docs that duplicate code line-by-line
- ADR for every minor styling choice
- Committing secrets, `.env`, or `_local/`
