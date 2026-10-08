# ADR 0010: Give custody gaps an explicit lifecycle

- **Status:** Accepted

## Context

A missing pickup confirmation must remain visible and owned until it has
been investigated and accounted for.

## Decision

Model a custody gap as a domain entity with forward transitions:
`Open` → `Acknowledged` → `Investigating` → `Resolved` → `Closed`.
A late custody event may resolve an open gap.

## Consequences

The system can distinguish an unseen gap from one being investigated or
resolved. Domain methods reject invalid transitions; user authorization and
operational follow-up remain necessary.
