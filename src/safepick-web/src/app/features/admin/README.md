# features/admin/

The School Admin's dashboard. Desktop-first.

- Driver approvals (`/api/drivers/{approvalId}/approve|reject|revoke`)
- Learner management (`/api/learners`)
- Assignment management (`/api/assignments`)
- Monitoring board (`GET /api/custody/status/{schoolId}?date=`)
- Gap board (`GET /api/reconciliation/gaps/{schoolId}`)
- Gap lifecycle: acknowledge → investigate → resolve
- Reconciliation trigger (`POST /api/reconciliation/run`)

Motion: transitions and feedback. No table row animations.