using SafePick.Domain.Common;
using SafePick.Domain.Transport.Events;

namespace SafePick.Domain.Transport;

/// <summary>
/// A school — the tenant root of SafePick SA.
///
/// A School is a *thin* aggregate. It does not own its learners, drivers,
/// or assignments; those are independent aggregates scoped by SchoolId.
/// The School owns only its own configuration — currently its transport
/// windows.
///
/// Multi-tenancy (ADR-0003) is enforced by scoping every other aggregate
/// to a SchoolId. The School itself is the anchor of that scoping.
/// </summary>
public sealed class School : AggregateRoot<Guid>
{
    private readonly List<TransportWindow> _transportWindows = new();

    public string Name { get; private set; }
    public string ContactEmail { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// The school's configured transport windows.
    /// Exposed read-only; modification is through AddTransportWindow
    /// and ClearTransportWindows, which raise domain events.
    /// </summary>
    public IReadOnlyCollection<TransportWindow> TransportWindows
        => _transportWindows.AsReadOnly();

    // EF Core materialization constructor.
    private School() : base(Guid.Empty)
    {
        Name = string.Empty;
        ContactEmail = string.Empty;
        IsActive = false;
    }

    private School(
        Guid id,
        string name,
        string contactEmail) : base(id)
    {
        Name = name;
        ContactEmail = contactEmail;
        IsActive = true;
    }

    /// <summary>
    /// Creates a new school. Invoked by the Operator when issuing the
    /// first School Admin invitation for that school.
    /// </summary>
    public static School Register(string name, string contactEmail, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("School name must not be empty.");

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new DomainException("School contact email must not be empty.");

        var school = new School(Guid.NewGuid(), name.Trim(), contactEmail.Trim().ToLowerInvariant());

        school.Raise(new SchoolRegistered(
            school.Id,
            school.Name,
            school.ContactEmail,
            now));

        return school;
    }

    /// <summary>
    /// Updates the school's name and contact email.
    /// </summary>
    public void UpdateProfile(string name, string contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("School name must not be empty.");

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new DomainException("School contact email must not be empty.");

        Name = name.Trim();
        ContactEmail = contactEmail.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Adds a transport window to the school's configuration.
    /// Duplicate windows (same values) are rejected.
    /// </summary>
    public void AddTransportWindow(TransportWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (_transportWindows.Contains(window))
            throw new DomainException("An identical transport window already exists.");

        _transportWindows.Add(window);
        Raise(new TransportWindowUpdated(Id, window.ToString(), DateTime.UtcNow));
    }

    /// <summary>
    /// Clears all transport windows. Used before replacing them wholesale.
    /// </summary>
    public void ClearTransportWindows(DateTime now)
    {
        if (_transportWindows.Count == 0)
            return;

        _transportWindows.Clear();
        Raise(new TransportWindowUpdated(Id, "(cleared)", now));
    }

    /// <summary>
    /// Deactivates the school. The record is preserved for audit.
    /// </summary>
    public void Deactivate(DateTime now)
    {
        if (!IsActive)
            throw new DomainException("School is already inactive.");

        IsActive = false;
    }
}