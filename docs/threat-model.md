# Threat model

This is a high-level design note for a local-development project, not an
independent security review or a complete threat assessment.

## Assets

- Learner names, school membership, transport assignments, and pickup history.
- Parent/guardian contact details.
- Account credentials, invitation tokens, and JWT signing material.
- Custody events and the status of unresolved custody gaps.

## Trust boundaries

- The Angular client and browser are untrusted; the API must validate all
  input and derive identity from authenticated claims.
- The HTTP API is the boundary between users and application use cases.
- PostgreSQL stores identity, transport, and custody records.
- Email and other integrations cross the application boundary and require
  protected credentials and careful data minimization.
- Local development tooling and configuration are not production security
  controls.

## Principal risks

- Unauthorized access to learner information across school boundaries.
- Account takeover through weak credentials, leaked secrets, or insecure
  deployment configuration.
- Forged or replayed pickup actions and tampering with event history.
- Reuse or exposure of invitation tokens.
- Disclosure of learner or contact data in logs, email, backups, or test data.
- Loss of data or inability to investigate an incident due to missing
  operational backups and audit procedures.

## Existing design measures

- The API requires a configured JWT signing key of at least 32 UTF-8 bytes
  and an operator bootstrap password; it fails at startup otherwise.
- Invitation and role provisioning rules are represented in the domain
  model; parents are contacts, not accounts.
- Custody gaps have explicit domain lifecycle rules, and custody corrections
  are intended to be represented as compensating records.
- The repository ignores local development settings and common secret files.
- The Compose database is intended for local development only.

These measures do not demonstrate that every endpoint enforces tenant
isolation or that the application is production secure. Authorization,
tenant isolation, dependency maintenance, transport security, secret
rotation, observability, database permissions, backups, and incident
response must be reviewed before deployment.

## Safe use

Do not publish real learner data, production credentials, database dumps,
or secret-bearing configuration. Do not expose the local Compose services
to a public network or reuse their development credentials.
