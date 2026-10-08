# Privacy and POPIA considerations

SafePick concerns learner identity, transport, and parent-contact
information. This document records privacy-aware engineering intentions; it
is not legal advice, a compliance assessment, or evidence that the
application complies with the Protection of Personal Information Act (POPIA).

## Development safeguards

- Use synthetic learner and contact details for development, testing, and
  screenshots.
- Keep local settings, credentials, and environment files out of Git.
- Do not collect or expose more information than a feature requires.
- Restrict operational data by school and user role in application code.
- Preserve a record of custody corrections rather than silently overwriting
  the original event.
- Treat parent contacts as contacts rather than independent login accounts.

## Before any real-world use

An organization considering deployment must independently determine its
lawful basis and responsibilities, document purpose and retention, provide
appropriate notices, assess access controls and disclosures, plan for
data-subject requests and incidents, and review hosting and cross-border
processing. These matters require qualified legal, privacy, and security
review; this repository does not implement or certify them.

The repository does not currently establish production access governance,
retention and deletion operations, backup encryption, incident response,
or an independent privacy impact assessment.
