import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from './core/guards';

// ============================================================
// SafePick SA — Route table
//
// Structure:
//   /login                   → LoginComponent (no shell)
//   /accept-invitation       → AcceptInvitationComponent (no shell)
//   /register-driver         → RegisterDriverComponent (no shell, public)
//   /operator                → Shell
//     ├── /operator          → OperatorComponent (home)
//     └── /operator/invite   → Redirects to /operator
//   /admin                   → Shell
//     ├── /admin             → AdminComponent (home)
//     ├── /admin/approvals   → ApprovalsComponent
//     ├── /admin/learners    → LearnersComponent
//     ├── /admin/assignments → AssignmentsComponent
//     ├── /admin/monitoring  → MonitoringComponent
//     └── /admin/gaps        → GapsComponent
//   /driver                  → Shell
//     ├── /driver            → DriverComponent (home)
//     └── /driver/roster     → RosterComponent
// ============================================================

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login',
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login.component').then(
        (m) => m.LoginComponent
      ),
  },
  {
    path: 'accept-invitation',
    loadComponent: () =>
      import('./features/auth/accept-invitation/accept-invitation.component').then(
        (m) => m.AcceptInvitationComponent
      ),
  },
  {
    path: 'register-driver',
    loadComponent: () =>
      import('./features/auth/register-driver/register-driver.component').then(
        (m) => m.RegisterDriverComponent
      ),
  },
  {
    path: 'operator',
    canActivate: [authGuard, roleGuard(['Operator'])],
    loadComponent: () =>
      import('./layout/shell/shell.component').then(
        (m) => m.ShellComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/operator/operator.component').then(
            (m) => m.OperatorComponent
          ),
      },
      {
        // Backward compatibility — the old /operator/invite route
        // now redirects to the board.
        path: 'invite',
        redirectTo: '',
      },
    ],
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard(['SchoolAdmin', 'SchoolStaff'])],
    loadComponent: () =>
      import('./layout/shell/shell.component').then(
        (m) => m.ShellComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/admin/admin.component').then(
            (m) => m.AdminComponent
          ),
      },
      {
        path: 'approvals',
        loadComponent: () =>
          import('./features/admin/approvals/approvals.component').then(
            (m) => m.ApprovalsComponent
          ),
      },
      {
        path: 'learners',
        loadComponent: () =>
          import('./features/admin/learners/learners.component').then(
            (m) => m.LearnersComponent
          ),
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./features/admin/assignments/assignments.component').then(
            (m) => m.AssignmentsComponent
          ),
      },
      {
        path: 'monitoring',
        loadComponent: () =>
          import('./features/admin/monitoring/monitoring.component').then(
            (m) => m.MonitoringComponent
          ),
      },
      {
        path: 'gaps',
        loadComponent: () =>
          import('./features/admin/gaps/gaps.component').then(
            (m) => m.GapsComponent
          ),
      },
    ],
  },
  {
    path: 'driver',
    canActivate: [authGuard, roleGuard(['Driver'])],
    loadComponent: () =>
      import('./layout/shell/shell.component').then(
        (m) => m.ShellComponent
      ),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/driver/driver.component').then(
            (m) => m.DriverComponent
          ),
      },
      {
        path: 'roster',
        loadComponent: () =>
          import('./features/driver/roster/roster.component').then(
            (m) => m.RosterComponent
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];