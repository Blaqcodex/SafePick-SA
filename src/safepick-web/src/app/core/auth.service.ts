import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { ApiService } from './api.service';
import type {
  AuthSession,
  LoginRequest,
  LoginResponse,
  Role,
} from './models';

// ============================================================
// AuthService
//
// The single source of truth for "who is logged in."
//
// Design:
//  - The runtime source of truth is the `session` signal.
//  - sessionStorage is the persistence layer for reload survival.
//  - The two are kept in sync ONLY at login, logout,
//    hydration, and expiry — nowhere else.
//
// Every guard, interceptor, and feature reads from this signal.
// No component ever touches sessionStorage directly.
// ============================================================

const STORAGE_KEY = 'safepick.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  // ---------- state ----------

  /** The current session, or null if not authenticated. */
  private readonly _session = signal<AuthSession | null>(this.hydrate());

  /** Read-only signal — the public API. Components read this. */
  readonly session = this._session.asReadonly();

  /** Convenience: is anyone logged in? */
  readonly isAuthenticated = computed(() => this._session() !== null);

  /** Convenience: the current role, or null. */
  readonly role = computed<Role | null>(() => this._session()?.role ?? null);

  /** Convenience: the current school (SchoolAdmin / SchoolStaff only). */
  readonly schoolId = computed<string | null>(
    () => this._session()?.schoolId ?? null
  );

  /** Convenience: the current driver (Driver only). */
  readonly driverId = computed<string | null>(
    () => this._session()?.driverId ?? null
  );

  /** Convenience: the current user's email. */
  readonly email = computed<string | null>(
    () => this._session()?.email ?? null
  );

  // ---------- actions ----------

  /**
   * Authenticate with the API, then persist the session.
   * On success, the session signal updates and the caller
   * is expected to navigate based on role.
   */
  login(credentials: LoginRequest): Observable<AuthSession> {
    return this.api
      .post<LoginResponse>('/auth/login', credentials)
      .pipe(
        map((response): AuthSession => {
          const session: AuthSession = {
            accessToken: response.accessToken,
            email: response.email,
            role: response.role,
            schoolId: response.schoolId,
            driverId: response.driverId,
          };
          this.setSession(session);
          return session;
        })
      );
  }

  /**
   * Clear the session.
   *
   * @param redirectTo  If provided, navigate to this path after
   *                    clearing. If null, do not navigate — the
   *                    caller is responsible.
   */
  logout(redirectTo: string | null = '/login'): void {
    this.clearSession();
    if (redirectTo !== null) {
      void this.router.navigate([redirectTo]);
    }
  }

  /**
   * The token for the current session, or null.
   * Used by the auth interceptor. Components never call this.
   */
  currentToken(): string | null {
    return this._session()?.accessToken ?? null;
  }

  // ---------- internals ----------

  private setSession(session: AuthSession): void {
    this._session.set(session);
    this.persist(session);
  }

  private clearSession(): void {
    this._session.set(null);
    try {
      sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      // sessionStorage can throw in private-mode Safari. Ignore.
    }
  }

  private persist(session: AuthSession): void {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session));
    } catch {
      // A failure to persist is not fatal.
      // The in-memory signal is still correct.
    }
  }

  private hydrate(): AuthSession | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) return null;
      const parsed = JSON.parse(raw) as unknown;
      if (this.isValidSession(parsed)) {
        return parsed;
      }
      sessionStorage.removeItem(STORAGE_KEY);
      return null;
    } catch {
      return null;
    }
  }

  /**
   * Runtime shape guard for hydrated sessions.
   * If the session shape changes in a future version, old
   * persisted sessions get discarded rather than crashing.
   */
  private isValidSession(value: unknown): value is AuthSession {
    if (typeof value !== 'object' || value === null) return false;
    const v = value as Record<string, unknown>;
    return (
      typeof v['accessToken'] === 'string' &&
      typeof v['email'] === 'string' &&
      typeof v['role'] === 'string' &&
      (v['schoolId'] === null || typeof v['schoolId'] === 'string') &&
      (v['driverId'] === null || typeof v['driverId'] === 'string')
    );
  }
}