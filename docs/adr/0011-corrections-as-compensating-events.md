# ADR 0011: Record corrections as compensating events

- **Status:** Accepted

## Context

An erroneous custody event should be correctable without making the original
record disappear from the audit history.

## Decision

Record a correction that references the event being corrected and captures
the reason for the change. Preserve the original event as historical data.

## Consequences

Consumers of custody history must account for both original and corrective
events. Corrections need validation and authorization, and downstream
reporting must interpret them consistently.
