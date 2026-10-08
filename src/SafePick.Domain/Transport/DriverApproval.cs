using SafePick.Domain.Common;
using SafePick.Domain.Transport.Events;

namespace SafePick.Domain.Transport;

/// <summary>
/// A school's approval of a driver.
///
/// This is the join between a Driver and a School, and it is per-school:
/// the same driver can be approved by School A and rejected by School B.
///
/// DriverApproval is a thin aggregate root, scoped to a SchoolId.
/// Its lifecycle is: Pending → Approved | Rejected → (optionally) Revoked.
/// </summary>
public sealed class DriverApproval : AggregateRoot<Guid>
{
    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Revoked = 3
    }

    public Guid DriverId { get; private set; }
    public Guid SchoolId { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public Guid? DecidedByUserId { get; private set; }
    public DateTime? DecidedAt { get; private set; }
    public string? Reason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core materialization constructor.
    private DriverApproval() : base(Guid.Empty)
    {
    }

    private DriverApproval(
        Guid id,
        Guid driverId,
        Guid schoolId,
        DateTime createdAt) : base(id)
    {
        DriverId = driverId;
        SchoolId = schoolId;
        Status = ApprovalStatus.Pending;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Creates a pending approval — the driver has registered and a
    /// school's attention is required.
    /// </summary>
    public static DriverApproval Request(
        Guid driverId,
        Guid schoolId,
        DateTime now)
    {
        if (driverId == Guid.Empty)
            throw new DomainException("DriverId is required.");

        if (schoolId == Guid.Empty)
            throw new DomainException("SchoolId is required.");

        return new DriverApproval(Guid.NewGuid(), driverId, schoolId, now);
    }

    /// <summary>
    /// Approves the driver for this school. Only valid from Pending.
    /// </summary>
    public void Approve(Guid decidedByUserId, DateTime now)
    {
        if (Status != ApprovalStatus.Pending)
            throw new DomainException(
                $"Cannot approve from status '{Status}'.");

        Status = ApprovalStatus.Approved;
        DecidedByUserId = decidedByUserId;
        DecidedAt = now;

        Raise(new DriverApproved(Id, DriverId, SchoolId, decidedByUserId, now));
    }

    /// <summary>
    /// Rejects the driver for this school. Only valid from Pending.
    /// A reason is required — the driver deserves to know why.
    /// </summary>
    public void Reject(Guid decidedByUserId, string reason, DateTime now)
    {
        if (Status != ApprovalStatus.Pending)
            throw new DomainException(
                $"Cannot reject from status '{Status}'.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A rejection reason is required.");

        Status = ApprovalStatus.Rejected;
        DecidedByUserId = decidedByUserId;
        DecidedAt = now;
        Reason = reason.Trim();

        Raise(new DriverRejected(Id, DriverId, SchoolId, decidedByUserId, Reason, now));
    }

    /// <summary>
    /// Revokes a prior approval. Only valid from Approved.
    /// Used when a school loses trust in a driver mid-term (UC-10).
    /// </summary>
    public void Revoke(Guid decidedByUserId, string reason, DateTime now)
    {
        if (Status != ApprovalStatus.Approved)
            throw new DomainException(
                $"Cannot revoke from status '{Status}'. Only Approved can be revoked.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A revocation reason is required.");

        Status = ApprovalStatus.Revoked;
        DecidedByUserId = decidedByUserId;
        DecidedAt = now;
        Reason = reason.Trim();

        Raise(new DriverApprovalRevoked(Id, DriverId, SchoolId, decidedByUserId, Reason, now));
    }
}