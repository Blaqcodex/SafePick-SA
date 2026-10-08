# features/auth/

The login page — the flagship "wow" screen.

- Three-image rotating background
- Animated logo and brand treatment
- Reactive login form (email + password)
- Session-expiry handling (returning user to their prior route)
- Redirects by role after successful login:
  - Operator → /operator
  - SchoolAdmin → /admin
  - Driver → /driver
  - SchoolStaff → /admin (read-only)