import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

// ============================================================
// authInterceptor
//
// Attaches the JWT to every request whose URL begins with /api.
//
// Scope discipline: the token is NEVER sent to any other host.
// If a future feature calls a third-party API through HttpClient,
// no token is attached. This is a security requirement, not a
// stylistic choice.
//
// Functional interceptor (Angular 15+). Registered in
// app.config.ts via provideHttpClient(withInterceptors([...])).
// ============================================================

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  // Only attach the token to our own API.
  if (!req.url.startsWith('/api')) {
    return next(req);
  }

  const auth = inject(AuthService);
  const token = auth.currentToken();

  // No session — pass through unchanged. A 401 will follow,
  // and the session-expiry interceptor will handle it.
  if (!token) {
    return next(req);
  }

  // Clone the request with the Authorization header.
  // HttpRequest is immutable — we never mutate req directly.
  const authenticated = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authenticated);
};