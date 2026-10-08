# SafePick SA

**Learner transport verification and pickup management**

SafePick SA is an academic project exploring how software can make learner
transport handovers more accountable. It models expected pickups, records
custody events, and gives unresolved handovers an explicit lifecycle instead
of treating missing information as a successful pickup.

> **Project status:** local-development project; not a deployed transport
> service and not ready to manage real learner data. Deployment, operational
> security, privacy review, and production readiness have not been established.

## How it works

1. Schools configure transport windows and assign learners to approved drivers.
2. Drivers record pickup confirmations or report when a learner is not in the
   vehicle.
3. School staff can review custody status and event history.
4. Reconciliation identifies expected pickups without a resolving event.
5. A school can acknowledge, investigate, and resolve each resulting custody
   gap.

The domain distinguishes three concepts:

- **Expectation:** a pickup derived from the learner's active assignment and
  the school's transport window.
- **Custody event:** an attributable record of a handover or related custody
  event.
- **Custody gap:** an unresolved expectation that is tracked through a
  defined lifecycle.

## Project structure

| Path | Responsibility |
| --- | --- |
| `src/SafePick.Domain` | Domain entities, value objects, and invariants |
| `src/SafePick.Application` | Commands, queries, validation, and workflows |
| `src/SafePick.Infrastructure` | PostgreSQL persistence and external services |
| `src/SafePick.Api` | ASP.NET Core API and application composition |
| `src/safepick-web` | Angular web client |
| `tests/SafePick.Domain.Tests` | Domain unit tests |
| `docker` | Local PostgreSQL and pgAdmin setup |
| `docs` | Architecture, domain, privacy, and threat-model notes |

The backend follows a layered architecture. The API handles HTTP concerns,
the application layer coordinates use cases, the domain layer enforces
business rules, and infrastructure provides persistence and integrations.
The frontend is an Angular standalone application.

## Technology

- .NET 10 and C# 14
- PostgreSQL 16 and Entity Framework Core
- Angular 17 and TypeScript
- Docker Compose for local database tooling

## Requirements

- .NET 10 SDK
- Node.js 20 and npm
- Docker Desktop (for the local PostgreSQL setup)

## Local development

Start PostgreSQL from the repository root:

```powershell
docker compose -f docker/docker-compose.yml up -d postgres
```

Configure local API credentials with .NET User Secrets. Run these commands
from the repository root and provide your own random JWT key and operator
password; do not put real credentials in committed files:

```powershell
dotnet user-secrets init --project src/SafePick.Api/SafePick.Api.csproj
dotnet user-secrets set "Jwt:SigningKey" "<random value of at least 32 UTF-8 bytes>" --project src/SafePick.Api/SafePick.Api.csproj
dotnet user-secrets set "Seed:OperatorPassword" "<password of at least 12 characters>" --project src/SafePick.Api/SafePick.Api.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<local PostgreSQL connection string>" --project src/SafePick.Api/SafePick.Api.csproj
```

The API requires `Jwt:SigningKey` and `Seed:OperatorPassword`; it exits at
startup if either is missing or invalid. Database migrations are not applied
by the API at startup. The PostgreSQL credentials in Compose and the
initialization SQL are for isolated local development only. Do not expose
these services or reuse these values in another environment.

## Build and test

From the repository root, build the backend and run the domain tests:

```powershell
dotnet test SafePick.slnx --configuration Release
```

Build the web application for production:

```powershell
Set-Location src/safepick-web
npm ci
npm run build
```

GitHub Actions is configured to run both checks for pushes and pull requests
targeting `main`. The checks still require GitHub Actions to be enabled and
available for the repository account.

## Security and privacy

This project concerns children's transport and contact data. Do not use real
personal information in local development, screenshots, issues, or test
fixtures. See [the threat model](docs/threat-model.md) and
[the privacy notes](docs/popia-strategy.md) for scope and limitations.

Local development settings such as `appsettings.Development.json` and `.env`
files are ignored by Git. Never force-add secret-bearing local files.

## Further documentation

- [Architecture](docs/architecture.md)
- [Domain model](docs/domain-model.md)
- [Use cases](docs/use-cases.md)
- [Glossary](docs/glossary.md)
- [Architecture decision records](docs/adr/)
