// ============================================================
// Invitations and Schools — Operator + Invitee domain
// ============================================================

export type InvitationStatus =
  | 'Pending'
  | 'Accepted'
  | 'Expired'
  | 'Revoked';

export interface Invitation {
  invitationId: string;
  email: string;
  role: string;
  schoolId: string;
  schoolName: string;
  status: InvitationStatus;
  issuedAt: string;
  expiresAt: string;
  acceptedAt: string | null;
}

export interface CreateInvitationRequest {
  email: string;
  displayName: string;
  schoolName: string;
  schoolContactEmail: string;
}

export interface CreateInvitationResult {
  invitationId: string;
  schoolId: string;
  email: string;
  rawToken: string;
  inviteUrl: string;
  expiresAt: string;
  emailSent: boolean;
}

// ---------- Invitee side ----------

export interface ValidateInvitationResult {
  email: string;
  role: string;
  schoolName: string;
  expiresAt: string;
}

export interface AcceptInvitationRequest {
  token: string;
  password: string;
}

export interface AcceptInvitationResult {
  userId: string;
  email: string;
  role: string;
  schoolId: string;
}

// ---------- Schools ----------

export interface School {
  schoolId: string;
  name: string;
  contactEmail: string;
  isActive: boolean;
  learnerCount: number;
  driverCount: number;
}

/**
 * A minimal school representation used by the public
 * driver-registration dropdown.
 */
export interface PublicSchool {
  schoolId: string;
  name: string;
}