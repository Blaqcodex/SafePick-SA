# 0007. Guarantees Custody — Single-Node Expert System

- **Status:** Accepted
- **Date:** 2026-09-17
- **Deciders:** SafePick SA Architecture Team

## Context

SafePick SA exists to establish a verifiable chain of custody for learners
using private transport. The domain is one in which a missing record is not
an inconvenience — it is a safety failure. If a learner is expected at
06:45 and no confirmation ever arrives, the system must not treat that as
"no data." It must treat it as an *open question that requires resolution*.

This forces a foundational choice before any code is written: is SafePick SA
a system that **records** custody, or a system that **guarantees** it?

- A **recording** system is a faithful witness. It stores what it is told,
  when it is told. If a pickup is not reported, that is the reporter's
  problem. The system has no notion of what *should* have happened.

- A **guaranteeing** system is an accountable authority. It knows what
  should happen, detects what did not, and refuses to let an unconfirmed
  event silently disappear. It owns the gap until it is resolved.

The executive summary states that SafePick SA "establishes a verifiable
chain of custody for every learner, every school day." The words
*establishes* and *every* are guarantee-language. A recorder cannot
establish a chain; it can only observe one. A recorder cannot promise
*every* day; only the days someone reported.

The second constraint is deployment. SafePick SA is an academic project
delivered on a single machine, presented from localhost. There is no
cloud, no message broker, no distribution, and no operational team.

These two constraints — guarantee-grade semantics, single-node realization —
appear to conflict. They do not. They are orthogonal.

## Decision

**SafePick SA is a Guarantees Custody, Single-Node Expert System.**

### Semantics: Guarantee-grade

The system models three custody concepts, not one:

1. **Custody Event** — an immutable, attributable record that a learner's
   custody transitioned from one accountable party to another, at a
   specific server-authoritative time, within a specific transport window.

2. **Custody Expectation** — a *derived* statement that, given an active
   assignment and a valid school day, a custody transition was supposed to
   occur. Expectations are computed, never stored as authoritative data.

3. **Custody Gap** — a first-class, owned, lifecycle-bearing representation
   of an expectation that has passed its grace period without a resolving
   event. A gap has an owner, a state machine, and an audit trail.

Every expected pickup must reach exactly one of three terminal states:
**confirmed**, **explained** (e.g., "not in car"), or **gapped**. No
expected pickup is ever allowed to silently vanish.

### Realization: Single-node

The system's guarantees are enforced by the domain model, not by
infrastructure. There is no message broker, no distributed coordinator,
no cloud service compensating for weak design. Specifically:

- **Notification delivery** uses an outbox table and a background hosted
  service, avoiding a message-broker dependency. The current event-dispatch
  behavior writes outbox messages in a follow-up save after the command
  handler saves its changes, so atomicity between those writes is not
  guaranteed yet. Reliable, transactional delivery remains implementation
  work.

- **Reconciliation** is a designed subsystem, invoked at transport-window
  close and upon late sync arrival. It is not a cron job.

- **Offline tolerance** is achieved through idempotent, client-generated
  identifiers on every mutation, and (in a later slice) a Service Worker
  queue in the web client.

- **Immutability** is enforced at the storage layer — no `UPDATE` grant on
  the custody event table — not by convention.

The deployment topology is deliberately scoped: single process, single
database, single machine. This is not a limitation of the architecture;
it is a choice about which parts of the architecture are *exercised*. The
domain semantics are production-grade. The deployment is not.

### Expertise as a standard

"Expert" in this project is defined by design rigor, not by the number of
technologies employed. An expert system is one in which:

- Every invariant is enforced at the correct layer.
- Every failure mode has a defined behavior.
- Every trade-off is documented and defended.
- Every concept in the domain model reflects reality, not convenience.

Complexity is not a goal. Every concept added to the system must earn its
place by solving a problem the domain demands. Concepts removed are as
significant as concepts added.

## Consequences

### Positive

- **Accountability is structural.** Unconfirmed pickups become owned gaps,
  not silent absences. The system can prove it did its job even when
  nothing went wrong.

- **The rarity is in the model, not the machinery.** A carefully designed
  single-node system with a coherent custody model is more defensible than
  a sprawling distributed system with no coherent domain.

- **Localhost forces rigor.** With no infrastructure to compensate for
  weak design, every guarantee must be earned in the domain model itself.

- **The defense story is precise.** The system claims exactly what it
  delivers: guarantee-grade semantics, single-node realization. It never
  over-claims production-readiness, and never under-claims to being a
  mere prototype.

### Negative

- **More domain concepts than a recorder.** The system introduces
  `CustodyExpectation`, `CustodyGap`, and a reconciliation subsystem that
  a recording system would not need. These must be explained and
  defended.

- **Reconciliation is genuinely difficult.** Offline tolerance, grace
  periods, and idempotent alerting are the hardest correctness problems
  in the system. They consume real design time.

- **A larger first slice.** The initial vertical slice must include
  expectations, the outbox, and reconciliation — or the system it
  delivers is a different system.

- **Restraint is required.** The guarantee posture tempts toward brokers,
  projections, and real-time push. These must be resisted. The line is
  drawn at single-node; it is held.

## Notes

- This ADR is the anchor of the project. Every other ADR traces to it.
- ADR-0014 (Authority Chain) is its sibling anchor: SafePick SA models two
  chains — custody and authority — with equal rigor. This ADR establishes
  the custody chain's posture; ADR-0014 establishes the authority chain's.
- The distinction between "records" and "guarantees" is not academic. For a
  system that governs the transport of children, "records" is arguably
  negligent. A parent's question — *"you knew she was supposed to be
  picked up; why didn't you notice she wasn't?"* — has no answer under a
  recording system, and a clear one under a guaranteeing system.