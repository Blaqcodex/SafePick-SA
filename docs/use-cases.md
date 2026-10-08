# Use cases

This page summarizes the user-facing capabilities represented by the API
controllers and Angular feature areas. The exact authorization and validation
rules live in the implementation.

## Account access

- Sign in with an existing account.
- Register as a driver and review the approval state.
- Accept a valid invitation to create a school account.
- Issue invitations according to the authority chain.

## School transport administration

- View schools and configure a transport window.
- Register learners and maintain parent-contact details.
- Review, approve, reject, or revoke driver approvals.
- Assign, reassign, and unassign learners.

## Daily custody

- View a driver roster for a transport window.
- Confirm a pickup or record that a learner was not in the vehicle.
- Record a school absence or a correction to an earlier event.
- Review a school's custody board and a learner's event history.

## Reconciliation

- Inspect expected pickups.
- Run reconciliation for a school and date.
- Review identified custody gaps.
- Acknowledge and investigate a gap.
- Record a resolution and close the gap.

Not every use case has a dedicated screen or production-ready operational
workflow. This list describes repository capabilities, not a guarantee of
deployment or service availability.
