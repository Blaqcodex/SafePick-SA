# ADR 0002: Dispatch notifications from domain events

- **Status:** Accepted

## Context

Sending email directly from a domain operation couples business rules to an
external service and makes transient delivery failures difficult to handle.

## Decision

Represent notable changes as domain events and dispatch notification work
outside the domain. Infrastructure includes an outbox writer and dispatcher
for queued work.

## Consequences

Domain rules do not depend on an email provider. Delivery is asynchronous and
must be treated as retryable work; consumers should tolerate redelivery. The
current domain-event behavior writes outbox rows in a follow-up save after
the handler has saved its changes, so atomic persistence of the business
change and outbox message is not guaranteed. Transactional delivery and
operational retry behavior remain implementation work.
