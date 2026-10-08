import type { Role } from './models';

// ============================================================
// Role → home route mapping.
//
// Used by:
//  - roleGuard (redirect wrong-role users to their own home)
//  - guestGuard (redirect logged-in users away from login)
//  - login component (after successful auth, navigate here)
//
// One source of truth. If a role's home ever changes, this
// is the only file that changes.
// ============================================================

export const ROLE_HOME: Record<Role, string> = {
  Operator: '/operator',
  SchoolAdmin: '/admin',
  SchoolStaff: '/admin',
  Driver: '/driver',
};

/**
 * Returns the home route for a role, or '/login' if the role
 * is unknown. Defensive — a corrupt session with a bogus role
 * should not send the user to 'undefined'.
 */
export function homeForRole(role: Role | null | undefined): string {
  if (!role) return '/login';
  return ROLE_HOME[role] ?? '/login';
}