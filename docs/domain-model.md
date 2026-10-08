# Domain model

The domain layer is organized around identity, transport, and custody. It
models the parties responsible for a handover separately from the events
that record it and the expectations used to detect a missing confirmation.

## Identity

- **User** represents an account and its role.
- **Invitation** grants time-limited, single-use access to a non-operator
  account.
- **Invitation token** is represented as a value object; persistence and token
  generation are infrastructure concerns.

The documented authority model seeds one Operator. Operators invite school
administrators, school administrators invite school staff, and drivers
self-register before school approval. Parents are represented as contacts,
not login accounts. See [ADR 0014](adr/0014-authority-chain-and-provisioning-model.md).

## Transport

- **School** is the school context for transport activity.
- **Learner** belongs to a school and may have a parent contact.
- **Driver** represents a transport provider and its approval state.
- **Vehicle** represents transport capacity and its details.
- **Assignment** links a learner to a driver/vehicle for an effective period.
- **Transport window** defines the expected handover period for a school.

These entities and their rules are implemented under
`src/SafePick.Domain/Transport`.

## Custody

- **Custody event** records an observed custody-related action.
- **Custody expectation** is derived from transport arrangements; it is not
  the source of truth for an event.
- **Custody gap** tracks an expected pickup that has not been accounted for.
- **Reconciliation run** evaluates expectations against recorded events.

A gap progresses through `Open`, `Acknowledged`, `Investigating`, `Resolved`,
and `Closed`. A late custody event may resolve an open gap automatically;
otherwise an authorized user supplies a resolution before closure. Domain
transitions are enforced by the aggregate. See
[ADR 0007](adr/0007-guarantees-custody-single-node-expert-system.md).

## Events and time

Domain events describe notable state changes and are dispatched by the
application/infrastructure pipeline. Corrections are recorded as
compensating events instead of editing historical custody records.
Server-provided timestamps are used for custody actions; clients should not
be treated as the authority for event time.
