import type { Role } from './models';

// ============================================================
// Navigation config
//
// The single source of truth for role-aware navigation links.
// The shell reads this and renders the appropriate set.
// If a link's target ever changes, this file is the only
// one that changes.
// ============================================================

export interface NavLink {
  readonly label: string;
  readonly path: string;       // router path, absolute (e.g. '/admin/gaps')
  readonly icon: string;       // Material icon name
}

/**
 * Links per role. When a user has multiple roles (rare here
 * but possible), we'd concatenate — but the current domain
 * gives each user exactly one role.
 */
export const NAV_LINKS: Record<Role, readonly NavLink[]> = {
  Operator: [
    { label: 'Operator', path: '/operator', icon: 'admin_panel_settings' },
  ],
  
  SchoolAdmin: [
    { label: 'Approvals',   path: '/admin/approvals',   icon: 'how_to_reg' },
    { label: 'Learners',    path: '/admin/learners',    icon: 'groups' },
    { label: 'Assignments', path: '/admin/assignments', icon: 'directions_bus' },
    { label: 'Monitoring',  path: '/admin/monitoring',  icon: 'monitor_heart' },
    { label: 'Gaps',        path: '/admin/gaps',        icon: 'report_problem' },
  ],

  SchoolStaff: [
    { label: 'Monitoring', path: '/admin/monitoring', icon: 'monitor_heart' },
    { label: 'Gaps',       path: '/admin/gaps',       icon: 'report_problem' },
  ],

  Driver: [
    { label: "Today's Roster", path: '/driver/roster', icon: 'list_alt' },
  ],
};

/**
 * Returns the nav links for a role, or an empty array if
 * the role is unknown. Defensive — a corrupt session should
 * never crash the shell.
 */
export function navLinksForRole(role: Role | null | undefined): readonly NavLink[] {
  if (!role) return [];
  return NAV_LINKS[role] ?? [];
}