import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';

// ============================================================
// ApiService
//
// The single boundary between the app and the SafePick API.
// Every HTTP call goes through one of these methods.
//
// Why:
//  - The base URL lives in exactly one place.
//  - Feature services read like prose, not plumbing.
//  - Interceptors (auth, session-expiry) attach to every
//    request without any feature code knowing they exist.
// ============================================================

type QueryValue = string | number | boolean | null | undefined;

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  /**
   * GET request.
   * @param path  Path relative to the API base (e.g. '/custody/roster/abc')
   * @param query Optional query parameters — null/undefined values are skipped.
   */
  get<T>(path: string, query?: Record<string, QueryValue>): Observable<T> {
    return this.http.get<T>(this.url(path), {
      params: this.buildParams(query),
    });
  }

  /**
   * POST request.
   * @param path Path relative to the API base.
   * @param body Request body. For commands, this carries the idempotency key.
   */
  post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<T>(this.url(path), body);
  }

  /**
   * PUT request.
   */
  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(this.url(path), body);
  }

  // ---------- internals ----------

  private url(path: string): string {
    // Normalize: ensure exactly one slash between base and path.
    const base = this.baseUrl.replace(/\/$/, '');
    const suffix = path.startsWith('/') ? path : `/${path}`;
    return `${base}${suffix}`;
  }

  private buildParams(
    query?: Record<string, QueryValue>
  ): HttpParams | undefined {
    if (!query) return undefined;
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value === null || value === undefined) continue;
      params = params.set(key, String(value));
    }
    return params;
  }
}