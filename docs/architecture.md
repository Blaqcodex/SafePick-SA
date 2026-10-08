# Architecture

SafePick is organized as a .NET solution with an Angular web client and a
PostgreSQL database. The backend separates domain rules from application
workflows and infrastructure concerns.

## Components

```text
Angular web client
        |
        | HTTP / JSON
        v
ASP.NET Core API
        |
        +--> Application (MediatR requests, validation, workflows)
        |       |
        |       +--> Domain (entities, value objects, invariants)
        |
        +--> Infrastructure (EF Core, PostgreSQL, email, PDF, time)
```

### Domain

`src/SafePick.Domain` owns business concepts and invariant enforcement. It has
no project references to the application, API, or infrastructure layers.

### Application

`src/SafePick.Application` groups use cases by capability. Commands and queries
are handled through MediatR; FluentValidation validators and pipeline
behaviors provide request validation and cross-cutting handling.

### Infrastructure

`src/SafePick.Infrastructure` implements application interfaces using
Entity Framework Core and PostgreSQL, and contains identity, notification,
PDF, and time services.

### API and web client

`src/SafePick.Api` is the composition root and HTTP boundary. Controllers map
requests to application operations. `src/safepick-web` contains the
standalone Angular user interface, split into authentication, driver,
operator, and administration features.

## Persistence and event handling

PostgreSQL is accessed through EF Core. Domain events are collected by an
application behavior and written to an outbox for background dispatch. The
current behavior saves the business change in the handler and writes outbox
messages in a subsequent save. It therefore does **not** yet guarantee that
the business change and outbox message commit atomically; reliable,
transactional delivery remains a limitation to address before relying on
notifications.

The API does not apply database migrations on startup; schema changes are a
deployment/setup responsibility.

## Deployment scope

The supplied Compose file is intended for local database development, not
production. This repository does not provide a production deployment, managed
database, monitoring plan, backup/restore process, or operational runbook.
See [the threat model](threat-model.md) before considering any deployment.
