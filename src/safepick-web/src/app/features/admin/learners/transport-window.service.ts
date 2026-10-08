import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type {
  TransportWindow,
  SetTransportWindowRequest,
  SetTransportWindowResult,
} from '@app/core/models';

// ============================================================
// TransportWindowService
//
// Reads and sets the school's transport window.
//
// Endpoints:
//   GET  /api/schools/{schoolId}/transport-window
//   PUT  /api/schools/{schoolId}/transport-window
// ============================================================

@Injectable({ providedIn: 'root' })
export class TransportWindowService {
  private readonly api = inject(ApiService);

  getWindow(schoolId: string): Observable<TransportWindow | null> {
    return this.api.get<TransportWindow | null>(
      `/schools/${schoolId}/transport-window`
    );
  }

  setWindow(
    schoolId: string,
    request: SetTransportWindowRequest
  ): Observable<SetTransportWindowResult> {
    return this.api.put<SetTransportWindowResult>(
      `/schools/${schoolId}/transport-window`,
      request
    );
  }
}