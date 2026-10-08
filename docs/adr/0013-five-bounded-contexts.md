# ADR 0013: Organize capabilities into bounded contexts

- **Status:** Accepted

## Context

Identity, transport, custody, reconciliation, and notification concerns have
different rules and change for different reasons.

## Decision

Organize the application and domain around five capabilities:
Identity, Transport, Custody, Reconciliation, and Notification. Keep
cross-cutting concerns such as validation and persistence interfaces in
shared/common areas.

## Consequences

Feature code has clearer ownership and can evolve with fewer accidental
dependencies. This is a modular organization within one deployable
application, not a commitment to microservices or independent databases.
