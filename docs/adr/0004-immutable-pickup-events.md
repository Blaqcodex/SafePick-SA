# ADR 0004: Preserve custody history

- **Status:** Accepted

## Context

Editing or deleting a pickup record can erase evidence needed to understand
who recorded a handover and how later corrections were made.

## Decision

Treat custody events as historical records. Correct a mistaken event by
recording a compensating correction rather than silently replacing the
original event.

## Consequences

History can explain both the original record and its correction. Storage
permissions and persistence behavior must also protect historical records;
domain-level intent alone does not enforce database immutability.
