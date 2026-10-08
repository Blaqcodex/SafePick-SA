import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { homeForRole } from '../role-home';
import type { Role } from '../models';

// ============================================================
// roleGuard
//
// A guard factory. Given a list of allowed roles, returns a
// CanActivateFn that permits only those roles.
//
// If the user is unauthenticated → redirect to /login.
// If the user is authenticated but has the wrong role →
//   redirect to their own home (not login — they ARE logged in).
//
// Usage:
//   { path: 'admin', canActivate: [authGuard, roleGuard(['SchoolAdmin', 'SchoolStaff'])], ... }
// ============================================================

export function roleGuard(allowedRoles: readonly Role[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    const session = auth.session();

    // Unauthenticated → login.
    if (!session) {
      return router.createUrlTree(['/login'], {
        queryParams: { reason: 'auth-required' },
      });
    }

    // Authenticated but wrong role → their own home.
    if (!allowedRoles.includes(session.role)) {
      return router.createUrlTree([homeForRole(session.role)]);
    }

    return true;
  };
}