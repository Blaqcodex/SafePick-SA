// ============================================================
// SafePick SA — Development environment config
// The proxy (proxy.conf.json) forwards /api to localhost:5000.
// ============================================================

export const environment = {
  production: false,
  apiBaseUrl: '/api',
} as const;