import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import { newIdempotencyKey, deviceTimestamp } from '@app/core/idempotency';
import type {
  RosterRow,
  ConfirmPickupCommand,
  MarkNotInCarCommand,
} from '@app/core/models';

// ============================================================
// DriverRosterService
//
// The driver's window into custody.
//
// Endpoints:
//   GET  /api/custody/roster/{driverId}?date=YYYY-MM-DD
//   POST /api/custody/confirm-pickup
//   POST /api/custody/not-in-car
//
// The command bodies match the backend's ConfirmPickupBody and
// MarkNotInCarBody. Every command carries the learner's schoolId
// (from the roster row), the driverId (from the session), a
// client-generated idempotency key, and a device timestamp.
// ============================================================

@Injectable({ providedIn: 'root' })
export class DriverRosterService {
  private readonly api = inject(ApiService);

  /**
   * Fetches the roster for a driver on a given date (YYYY-MM-DD).
   */
  getRoster(driverId: string, date: string): Observable<RosterRow[]> {
    return this.api.get<RosterRow[]>(
      `/custody/roster/${driverId}`,
      { date }
    );
  }

  /**
   * Confirms a pickup.
   *
   * @param row       the roster row being confirmed (carries schoolId)
   * @param driverId  the authenticated driver's id
   */
  confirmPickup(row: RosterRow, driverId: string): Observable<string> {
    const command: ConfirmPickupCommand = {
      schoolId: row.schoolId,
      learnerId: row.learnerId,
      driverId,
      clientGeneratedId: newIdempotencyKey(),
      deviceTimestamp: deviceTimestamp(),
    };
    return this.api.post<string>('/custody/confirm-pickup', command);
  }

  /**
   * Marks a learner as not collected.
   *
   * @param row       the roster row being marked (carries schoolId)
   * @param driverId  the authenticated driver's id
   * @param reason    the reason for not being in the car
   */
  markNotInCar(
    row: RosterRow,
    driverId: string,
    reason: string
  ): Observable<string> {
    const command: MarkNotInCarCommand = {
      schoolId: row.schoolId,
      learnerId: row.learnerId,
      driverId,
      clientGeneratedId: newIdempotencyKey(),
      reason,
      deviceTimestamp: deviceTimestamp(),
    };
    return this.api.post<string>('/custody/mark-not-in-car', command);
  }
}