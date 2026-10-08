using SafePick.Domain.Common;
using SafePick.Domain.Transport.Events;

namespace SafePick.Domain.Transport;

/// <summary>
/// A school learner — the subject of the custody chain.
///
/// Learner is a thin aggregate root, scoped to a SchoolId (ADR-0003).
/// It carries two value objects: ClassGroup (GradeClass) and ParentContact.
///
/// A learner may exist without a ParentContact (the school has the
/// learner on the roll but no contact yet). It may NOT be assigned to
/// a vehicle without one — that invariant is enforced by the Assignment
/// aggregate, not here.
/// </summary>
public sealed class Learner : AggregateRoot<Guid>
{
    public Guid SchoolId { get; private set; }
    public string FullName { get; private set; }
    public GradeClass ClassGroup { get; private set; }
    public ParentContact? ParentContact { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core materialization constructor.
    private Learner() : base(Guid.Empty)
    {
        FullName = string.Empty;
        ClassGroup = null!;
        IsActive = false;
    }

    private Learner(
        Guid id,
        Guid schoolId,
        string fullName,
        GradeClass classGroup) : base(id)
    {
        SchoolId = schoolId;
        FullName = fullName;
        ClassGroup = classGroup;
        IsActive = true;
    }

    /// <summary>
    /// Registers a learner on a school's roll. The learner starts without
    /// a parent contact; the school supplies one via SetParentContact.
    /// </summary>
    public static Learner Register(
        Guid schoolId,
        string fullName,
        GradeClass classGroup,
        DateTime now)
    {
        if (schoolId == Guid.Empty)
            throw new DomainException("SchoolId is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Learner full name must not be empty.");

        ArgumentNullException.ThrowIfNull(classGroup);

        var learner = new Learner(Guid.NewGuid(), schoolId, fullName.Trim(), classGroup);

        learner.Raise(new LearnerRegistered(
            learner.Id,
            learner.SchoolId,
            learner.FullName,
            learner.ClassGroup.ToString(),
            now));

        return learner;
    }

    /// <summary>
    /// Renames the learner, or moves them to a different grade/class.
    /// </summary>
    public void UpdateDetails(string fullName, GradeClass classGroup)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Learner full name must not be empty.");

        ArgumentNullException.ThrowIfNull(classGroup);

        FullName = fullName.Trim();
        ClassGroup = classGroup;
    }

    /// <summary>
    /// Sets or replaces the learner's parent contact. Required before
    /// the learner can be assigned to a vehicle.
    /// </summary>
    public void SetParentContact(ParentContact contact, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(contact);

        ParentContact = contact;

        Raise(new LearnerParentContactSet(
            Id,
            SchoolId,
            contact.Email,
            now));
    }

    /// <summary>
    /// Removes the parent contact. If the learner has active assignments,
    /// the Application layer must handle the consequences.
    /// </summary>
    public void ClearParentContact()
    {
        ParentContact = null;
    }

    /// <summary>
    /// Deactivates the learner — no longer part of transport.
    /// The record is preserved for audit.
    /// </summary>
    public void Deactivate(DateTime now)
    {
        if (!IsActive)
            throw new DomainException("Learner is already inactive.");

        IsActive = false;
        Raise(new LearnerDeactivated(Id, SchoolId, now));
    }

    /// <summary>
    /// Whether this learner can currently be assigned to a vehicle.
    /// </summary>
    public bool CanBeAssigned()
        => IsActive && ParentContact is not null;
}