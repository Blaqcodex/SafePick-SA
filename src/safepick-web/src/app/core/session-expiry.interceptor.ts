import {
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

// ============================================================
// sessionExpiryInterceptor
//
// Watches for 401 Unauthorized responses from our own API and
// handles them gracefully: preserves the current route, clears
// the session, and redirects to login with a returnTo hint.
//
// SCOPE RULES:
//  1. Only /api requests are considered.
//  2. /api/auth/login is EXCLUDED — the login page handles its
//     own 401 (invalid credentials) and must NOT trigger a
//     session-expiry logout.
//
// This is a correctness requirement, not polish: the backend
// issues 30-minute access tokens with no refresh mechanism.
// Without graceful handling, an admin loses their place mid-task.
// ============================================================

export const sessionExpiryInterceptor: HttpInterceptorFn = (req, next) => {
  // Rule 1: only our API.
  if (!req.url.startsWith('/api')) {
    return next(req);
  }

  // Rule 2: the login endpoint is exempt.
  // It returns 401 on bad credentials, and that must be handled
  // by the login form — not by the session-expiry path.
  if (req.url.startsWith('/api/auth/login')) {
    return next(req);
  }

  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        // Preserve the route the user was on, so they can be
        // returned there after re-login. Router.url is the
        // current activated route path (e.g. '/admin/gaps').
        const returnTo = router.url;

        // Clear the session first. This is idempotent — if the
        // session is already gone, it's a no-op.
        auth.logout('/login');

        // Then navigate with the returnTo hint. We use
        // navigate with queryParams so the URL is shareable
        // and survives a reload of the login page.
        void router.navigate(['/login'], {
          queryParams: {
            reason: 'expired',
            returnTo: returnTo !== '/login' ? returnTo : null,
          },
        });
      }
      return throwError(() => error);
    })
  );
};