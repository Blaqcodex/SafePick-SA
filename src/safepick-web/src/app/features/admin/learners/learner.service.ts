import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@app/core/api.service';
import type { Learner, Assignment } from '@app/core/models';

// ============================================================
// LearnerService
//
// The school admin's window into learner management.
//
// Endpoints:
//   GET  /api/learners?schoolId=...&includeInactive=...
//   POST /api/learners                          { schoolId, fullName, grade, classLetter }
//   PUT  /api/learners/{learnerId}/parent-contact { email, phone, name, consentRecorded }
//   POST /api/learners/{learnerId}/mark-absent    { schoolId, note, clientGeneratedId }
//
//   GET  /api/assignments?schoolId=...&includeTerminated=...
//   POST /api/assignments                        { learnerId, driverId, validFrom }
//   POST /api/assignments/reassign               { learnerId, newDriverId, validFrom }
//   POST /api/assignments/unassign               { assignmentId, terminatedAt }
// ============================================================

export interface AddLearnerRequest {
  schoolId: string;
  fullName: string;
  grade: number;
  classLetter: string;
}

export interface SetParentContactRequest {
  email: string;
  phone: string | null;
  name: string | null;
  consentRecorded: boolean;
}

export interface AssignLearnerRequest {
  learnerId: string;
  driverId: string;
  validFrom: string | null;
}

@Injectable({ providedIn: 'root' })
export class LearnerService {
  private readonly api = inject(ApiService);

  // ---------- Learners ----------

  getLearners(
    schoolId: string,
    includeInactive = false
  ): Observable<Learner[]> {
    return this.api.get<Learner[]>('/learners', {
      schoolId,
      includeInactive,
    });
  }

  addLearner(request: AddLearnerRequest): Observable<string> {
    return this.api.post<string>('/learners', request);
  }

  setParentContact(
    learnerId: string,
    request: SetParentContactRequest
  ): Observable<void> {
    return this.api.put<void>(
      `/learners/${learnerId}/parent-contact`,
      request
    );
  }

  // ---------- Assignments ----------

  getAssignments(
    schoolId: string,
    includeTerminated = false
  ): Observable<Assignment[]> {
    return this.api.get<Assignment[]>('/assignments', {
      schoolId,
      includeTerminated,
    });
  }

  assign(request: AssignLearnerRequest): Observable<string> {
    return this.api.post<string>('/assignments', request);
  }

  reassign(
    learnerId: string,
    newDriverId: string,
    validFrom: string | null = null
  ): Observable<string> {
    return this.api.post<string>('/assignments/reassign', {
      learnerId,
      newDriverId,
      validFrom,
    });
  }

  unassign(assignmentId: string, terminatedAt: string | null = null): Observable<void> {
    return this.api.post<void>('/assignments/unassign', {
      assignmentId,
      terminatedAt,
    });
  }
}