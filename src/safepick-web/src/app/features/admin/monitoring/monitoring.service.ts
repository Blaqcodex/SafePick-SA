import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { MonitoringSummary } from '@app/core/models';

// ============================================================
// MonitoringService
//
// The school admin's window into today's custody status.
//
// Endpoint:
//   GET /api/custody/status/{schoolId}?date=YYYY-MM-DD
//
// Returns the summary counts plus one row per expected learner,
// each with the current custody status.
// ============================================================

@Injectable({ providedIn: 'root' })
export class MonitoringService {
  private readonly api = inject(ApiService);

  getStatus(schoolId: string, date: string): Observable<MonitoringSummary> {
    return this.api.get<MonitoringSummary>(
      `/custody/status/${schoolId}`,
      { date }
    );
  }
}