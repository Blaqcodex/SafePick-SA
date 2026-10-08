using SafePick.Domain.Common;
using SafePick.Domain.Identity.Events;

namespace SafePick.Domain.Identity;

/// <summary>
/// A user of SafePick SA.
///
/// Every login — Operator, School Admin, School Staff, Driver — is a User.
/// What differs is the <see cref="Role"/> and, for non-operators, the
/// <see cref="SchoolId"/> they are bound to.
///
/// A User is an aggregate root: it is the consistency boundary for
/// identity, and the only place from which identity domain events are raised.
/// </summary>
public sealed class User : AggregateRoot<Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }
    public Role Role { get; private set; }

    /// <summary>
    /// The school this user is bound to.
    /// Null for Operators — they are not bound to any single school.
    /// </summary>
    public Guid? SchoolId { get; private set; }

    public bool IsActive { get; private set; }

    private User(
        Guid id,
        string email,
        string passwordHash,
        string displayName,
        Role role,
        Guid? schoolId) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        Role = role;
        SchoolId = schoolId;
        IsActive = true;
    }

    /// <summary>
    /// Creates the Operator — the seeded root of trust.
    /// Invoked exactly once, on first boot, by the infrastructure.
    /// </summary>
    public static User SeedOperator(string email, string passwordHash, string displayName)
    {
        var user = new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            displayName,
            Role.Operator,
            schoolId: null);

        user.Raise(new OperatorSeeded(user.Id, user.Email, DateTime.UtcNow));
        return user;
    }

    /// <summary>
    /// Creates a user from an accepted invitation.
    /// </summary>
    public static User CreateFromInvitation(
        string email,
        string passwordHash,
        string displayName,
        Role role,
        Guid schoolId)
    {
        if (role == Role.Operator)
            throw new DomainException("Operators cannot be created from invitations.");

        var user = new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            displayName,
            role,
            schoolId);

        user.Raise(new InvitationAccepted(
            user.Id,
            user.Email,
            user.Role.ToString(),
            schoolId,
            DateTime.UtcNow));

        return user;
    }

    /// <summary>
    /// Creates a User for a self-registered Driver.
    ///
    /// The driver has no custody powers until a school approves
    /// them. Their User exists immediately so they can log in and
    /// see their "awaiting approval" status — but they cannot
    /// confirm pickups until approved (enforced elsewhere).
    ///
    /// The User's email matches the Driver's email — that is the
    /// linking mechanism used by AuthController to resolve driverId
    /// at login time.
    /// </summary>
    public static User CreateDriver(
        string email,
        string passwordHash,
        string displayName,
        Guid schoolId)
    {
        if (schoolId == Guid.Empty)
            throw new DomainException("SchoolId is required for a driver user.");

        var user = new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            displayName,
            Role.Driver,
            schoolId);

        // No dedicated DriverUserCreated event yet — the DriverRegistered
        // event on the Driver aggregate carries this information.
        // If we later need driver-user lifecycle events, they can be added here.

        return user;
    }

    /// <summary>
    /// Deactivates the user. After this, the user cannot authenticate.
    /// The record is preserved for audit.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("User is already inactive.");

        if (Role == Role.Operator)
            throw new DomainException("The Operator account cannot be deactivated.");

        IsActive = false;
        Raise(new UserDeactivated(Id, Email, DateTime.UtcNow));
    }
}