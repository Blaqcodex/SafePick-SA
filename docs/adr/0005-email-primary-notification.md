# ADR 0005: Use email as the initial notification channel

- **Status:** Accepted

## Context

The application needs to notify users without coupling domain workflows to a
specific third-party messaging platform.

## Decision

Use email as the initial delivery channel behind application/infrastructure
interfaces. Keep notification content and channel delivery outside the domain.

## Consequences

Email delivery can be replaced or supplemented without changing custody
invariants. Provider credentials must be supplied through protected
configuration, delivery failures need operational handling, and email must
not disclose unnecessary learner information.
