// ============================================================
// Drivers, Vehicles, Driver Approvals
// ============================================================

export type DriverApprovalStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Revoked';

export interface Vehicle {
  vehicleId: string;
  registration: string;
  make: string;
  model: string;
  capacity: number;
}

export interface Driver {
  driverId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  vehicle: Vehicle;
}

/** A per-school approval row — one driver may have many. */
export interface DriverApproval {
  approvalId: string;
  driverId: string;
  driverFullName: string;
  driverEmail: string;
  driverPhone: string;
  vehicle: Vehicle;
  schoolId: string;
  status: DriverApprovalStatus;
  requestedAt: string;          // ISO 8601 UTC
  decidedAt: string | null;
  decidedByUserId: string | null;
  reason: string | null;        // rejection reason, if any
}

/** Body for driver self-registration */
export interface RegisterDriverCommand {
  clientGeneratedId: string;
  fullName: string;
  email: string;
  password: string;
  phoneNumber: string;
  vehicle: {
    registration: string;
    make: string;
    model: string;
    capacity: number;
  };
}

/**
 * The authenticated driver's own details, returned by
 * GET /api/drivers/me. Used by the driver's dashboard.
 */
export interface CurrentDriver {
  driverId: string;
  fullName: string;
  email: string;
  phone: string;
  idNumber: string;
  vehicle: CurrentDriverVehicle;
  approvalStatus: DriverApprovalStatus;
  schoolId: string | null;
  schoolName: string | null;
}

export interface CurrentDriverVehicle {
  vehicleId: string;
  registration: string;
  make: string;
  model: string;
  capacity: number;
}