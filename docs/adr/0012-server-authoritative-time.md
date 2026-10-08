# ADR 0012: Use server-authoritative event time

- **Status:** Accepted

## Context

Client clocks can be incorrect or manipulated. Custody records need a
consistent timestamp for ordering and investigation.

## Decision

Use server-side time for recorded custody actions. A client-supplied timestamp
must not override the authoritative time assigned by the API/application.

## Consequences

Event ordering does not rely on a driver's device clock. Offline submissions
still need a clearly defined distinction between occurrence time and receipt
time if that capability is introduced.
