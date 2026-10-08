# 0014. Authority Chain and Provisioning Model

- **Status:** Accepted
- **Date:** 2026-09-17
- **Deciders:** SafePick SA Architecture Team

## Context

SafePick SA models a **custody chain**: the School holds custody of a
learner, hands it to an approved Driver during transport, and the handover
is recorded as an immutable event. ADR-0007 establishes this posture.

Every custody chain has a prerequisite: **someone must have the authority
to approve the parties involved.** A driver cannot gain custody powers
until a School Administrator approves them. A School Administrator governs
a tenant — approving drivers, assigning learners, viewing every learner's
data within their school.

This raises a question that must be answered before any identity code is
written:

> **Who has the authority to grant the School Administrator role, and how
> did *they* get it?**

Every system has a **root of trust** — the place where authority
originates. If it is not designed explicitly, the system defaults to
self-registration, in which anyone can claim any role. For a system that
governs children's data and transport custody, self-registration is a
security and POPIA failure: a malicious actor could register as the
administrator of any school and immediately access every learner's name,
contact details, and pickup history.

Four provisioning models were considered:

1. **Self-registration** — anyone may register as a School Admin.
   Rejected: insecure, violates POPIA, destroys the trust model.

2. **Invitation-only** — an existing authority invites a School Admin.
   Correct in principle, but requires a "trusted first mover."

3. **Pre-seeded** — admins are inserted directly into the database by a
   seed file. Simple, but there is no workflow by which a *second* school
   can join the system. Multi-tenancy becomes a claim, not a capability.

4. **Hybrid** — a single seeded Operator is the root of trust; the
   Operator invites School Admins; School Admins invite School Staff;
   Drivers self-register but await approval; Parents never register.

## Decision

**SafePick SA adopts the hybrid model (Model D). The system has two
chains — a custody chain and an authority chain — each with a seeded
root, each verifiable, each auditable.**

### The authority chain
