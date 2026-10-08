# ADR 0006: Use a web application as the primary interface

- **Status:** Accepted

## Context

School staff and drivers need a browser-accessible interface for transport
administration and daily custody workflows without separate native clients.

## Decision

Build the primary user interface as a responsive Angular web application
that communicates with the ASP.NET Core API over HTTP.

## Consequences

One web client can serve the supported user roles. The API remains the
authority for validation and authorization; browser state is not trusted for
security decisions. Offline support is not implied by this decision.
