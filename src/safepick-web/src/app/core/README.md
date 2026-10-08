# core/

Singletons that live for the lifetime of the application.

- `auth/` — AuthService, session state, JWT handling
- `http/` — interceptors, API base config
- `guards/` — route guards (auth, role, session-expiry)
- `models/` — TypeScript interfaces for API DTOs
- `design-tokens.ts` — the TS mirror of the SCSS design tokens

Nothing here imports from `features/` or `shared/`.