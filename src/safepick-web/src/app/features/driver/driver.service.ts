import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { CurrentDriver } from '@app/core/models';

// ============================================================
// DriverService
//
// The driver's own data.
//
// Endpoint:
//   GET /api/drivers/me
//
// Returns the authenticated driver's details plus their
// approval status for the school they registered with.
// ============================================================

@Injectable({ providedIn: 'root' })
export class DriverService {
  private readonly api = inject(ApiService);

  getMe(): Observable<CurrentDriver> {
    return this.api.get<CurrentDriver>('/drivers/me');
  }
}