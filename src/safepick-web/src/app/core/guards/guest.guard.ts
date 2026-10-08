import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { homeForRole } from '../role-home';

// ============================================================
// guestGuard
//
// Allows only UNAUTHENTICATED users. Used on the login page.
//
// If a logged-in user navigates to /login, they are redirected
// to their own home. This prevents the confusing "why am I
// seeing the login page when I'm already signed in?" state.
//
// Usage:
//   { path: 'login', canActivate: [guestGuard], ... }
// ============================================================

export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const session = auth.session();
  if (!session) {
    return true;
  }

  return router.createUrlTree([homeForRole(session.role)]);
};