# ADR 0009: Derive custody expectations

- **Status:** Accepted

## Context

Persisting a separate mutable row for every expected pickup risks divergence
from the assignment and transport-window rules that define the expectation.

## Decision

Derive expected pickups from applicable transport arrangements and windows.
Store observed custody events and reconciliation outcomes as separate facts.

## Consequences

Expectations reflect current domain rules without a second source of truth.
Historical calculations depend on retaining the relevant assignments,
windows, and event data and applying the intended date/time rules consistently.
