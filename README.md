# SafePick SA

**Digital Learner Transport Verification & Pickup Management System**

> A **Guarantees Custody, Single-Node Expert System** for verifiable learner
> transport in South Africa.

---

## What This Is

SafePick SA establishes a **verifiable chain of custody** for every learner
using private transport, every school day. It is not a digitized register.
It is an accountability system: it knows what *should* happen, detects what
*didn't*, and never lets an unconfirmed pickup silently disappear.

## Architectural Position

- **Guarantees Custody** — the system models expectations, events, and gaps.
  Every missing pickup becomes an owned, resolvable custody gap.
- **Single-Node Expert System** — full domain rigor, deliberately scoped
  deployment. No brokers, no cloud, no distribution. Every guarantee is
  enforced by the domain model, not compensated for by infrastructure.

## The Custody Model (in one sentence)

> Custody is the responsibility of an accountable party — the School or the
> Driver — for a learner, during a transport window, with a verifiable
> handover event.

Expectations are **derived** (not stored). Events are **immutable**
(corrections are compensating events). Gaps are **first-class** (with a
five-state lifecycle).

## Stack

| Layer          | Technology                          |
| -------------- | ----------------------------------- |
| Backend        | .NET 10, C# 14                      |
| Architecture   | Clean Architecture, CQRS-lite       |
| Persistence    | PostgreSQL 16, EF Core 8            |
| Frontend       | Angular 17 (Standalone, responsive) |
| Notifications  | SMTP via MailKit (channel-abstract) |
| PDF            | QuestPDF                            |
| Local Infra    | Docker Compose                      |

## Repository Layout

- `src/SafePick.Domain` — domain model and business invariants
- `src/SafePick.Application` — use cases and application workflows
- `src/SafePick.Infrastructure` — persistence and external integrations
- `src/SafePick.Api` — ASP.NET Core API host
- `src/safepick-web` — Angular web application
- `docker` — local PostgreSQL setup
- `docs` — architecture, domain, and decision records

## Configuration

The API does not include usable JWT signing or operator bootstrap credentials.
Set these values through environment variables or a deployment secret store
before starting the API:

| Environment variable | Requirement |
| --- | --- |
| `Jwt__SigningKey` | At least 32 UTF-8 bytes |
| `Seed__OperatorPassword` | At least 12 characters |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |

For local development, use .NET User Secrets or environment variables rather
than committing credentials. `appsettings.Development.json` is excluded by
`.gitignore`; do not force-add it to Git.
