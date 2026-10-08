# ADR 0008: Model parents as learner contacts

- **Status:** Accepted

## Context

The initial custody workflows require a school to record a parent or
guardian's contact details, but do not require that person to sign in.

## Decision

Represent parent/guardian information as a contact associated with a learner,
not as a SafePick user role or account.

## Consequences

Parents do not receive account access or custody authority through the contact
record. Contact details remain personal information and require appropriate
access controls, minimization, and retention decisions.
