import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { School } from '@app/core/models';

// ============================================================
// SchoolService
//
// Lists schools. Used by the Operator to see what's been
// provisioned.
//
// Endpoint:
//   GET /api/schools?includeInactive=...
// ============================================================

@Injectable({ providedIn: 'root' })
export class SchoolService {
  private readonly api = inject(ApiService);

  getSchools(includeInactive = false): Observable<School[]> {
    return this.api.get<School[]>('/schools', { includeInactive });
  }
}