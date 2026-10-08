# ADR 0003: Scope operational data by school

- **Status:** Accepted

## Context

Schools manage their own learners, transport arrangements, staff, and custody
records. A school must not be treated as an untrusted client-supplied filter.

## Decision

Use the school as the operational tenant boundary. School-scoped users are
bound to a school, and application operations should derive and enforce the
authorized school context from the authenticated user.

## Consequences

Queries and mutations that access school data must consistently enforce the
school boundary. The presence of `SchoolId` fields alone is not proof of
tenant isolation; review endpoint authorization and query scoping before any
real-world deployment.
