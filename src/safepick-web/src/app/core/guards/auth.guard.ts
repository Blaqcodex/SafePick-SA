import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { AuthService } from '../auth.service';

// ============================================================
// authGuard
//
// Allows only authenticated users to access the route.
// Unauthenticated users are redirected to /login, with the
// attempted route preserved as a returnTo query parameter.
//
// Usage:
//   { path: 'admin', canActivate: [authGuard], ... }
// ============================================================

export const authGuard: CanActivateFn = (
  _route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  // Preserve where they were trying to go.
  return router.createUrlTree(['/login'], {
    queryParams: {
      reason: 'auth-required',
      returnTo: state.url !== '/login' ? state.url : null,
    },
  });
};