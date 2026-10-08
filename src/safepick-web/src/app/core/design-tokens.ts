// ============================================================
// SafePick SA — Design Tokens (TypeScript mirror)
// Kept in sync with src/styles/_tokens.scss by hand.
// Palette: #1E3A5F (blue) + #10B981 (green) — LOCKED.
// ============================================================

export const DESIGN_TOKENS = {
  color: {
    primary:        '#1E3A5F',
    primaryLight:   '#2C5282',
    primaryDark:    '#142A45',
    accent:         '#10B981',
    accentDark:     '#059669',
    accentLight:    '#D1FAE5',
    warning:        '#F59E0B',
    warningLight:   '#FEF3C7',
    danger:         '#DC2626',
    dangerLight:    '#FEE2E2',
    metal1:         '#E5E7EB',
    metal2:         '#9CA3AF',
    metal3:         '#4B5563',
    surface:        '#FFFFFF',
    surfaceAlt:     '#F9FAFB',
    bg:             '#F5F7FA',
    border:         '#E5E7EB',
    text:           '#111827',
    textMuted:      '#6B7280',
    textInverse:    '#FFFFFF',
  },
  radius: {
    sm:   4,
    md:   8,
    lg:   12,
    xl:   16,
    pill: 999,
  },
  motion: {
    fast:     '150ms',
    base:     '250ms',
    slow:     '400ms',
    easeOut:  'cubic-bezier(0.16, 1, 0.3, 1)',
    easeInOut:'cubic-bezier(0.4, 0, 0.2, 1)',
  },
} as const;