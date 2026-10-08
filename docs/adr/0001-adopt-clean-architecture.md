# ADR 0001: Adopt a layered architecture

- **Status:** Accepted

## Context

Transport and custody rules need to remain testable without depending on HTTP,
PostgreSQL, or user-interface implementation details.

## Decision

Separate the backend into Domain, Application, Infrastructure, and API
projects. Domain owns business rules; Application coordinates use cases;
Infrastructure implements external concerns; API composes the services and
exposes HTTP endpoints.

## Consequences

The domain remains independent of infrastructure frameworks. New dependencies
should point inward toward the domain, not from the domain toward outer layers.
The extra project boundaries require explicit interfaces and dependency
injection.
