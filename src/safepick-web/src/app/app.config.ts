import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth.interceptor';
import { sessionExpiryInterceptor } from './core/session-expiry.interceptor';

// ============================================================
// Application configuration
//
// Interceptor order matters:
//   1. authInterceptor            — attaches the JWT
//   2. sessionExpiryInterceptor   — watches for 401s
//
// provideAnimationsAsync() enables Angular's animation system
// lazily — required by Angular Material 17 for its form field
// label transitions, ripples, and overlay animations.
// ============================================================

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([authInterceptor, sessionExpiryInterceptor])
    ),
    provideAnimationsAsync(),
  ],
};