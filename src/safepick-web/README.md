# SafePick web client

Angular 17 standalone application for school staff, transport operators, and
drivers. It is served locally at `http://localhost:4200` during development.

## Requirements

- Node.js 20
- npm
- The SafePick API running at `http://localhost:5000` for API-backed flows

## Run locally

From this directory:

```sh
npm ci
npm start
```

The development server uses `proxy.conf.json` to forward `/api` requests to
the local API. Set up the API and its required secrets using the instructions
in the repository [README](../../README.md).

## Production build

```sh
npm run build
```

The compiled application is written to `dist/safepick-web`.

## Tests

The repository does not yet contain frontend spec files. Backend domain tests
are in `tests/SafePick.Domain.Tests` and run with `dotnet test` from the
repository root.
