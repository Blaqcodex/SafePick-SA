namespace SafePick.Application.Common.Interfaces;

/// <summary>
/// Answers: "who is invoking this use case?"
///
/// Implemented in Infrastructure by reading the authenticated user from
/// the HTTP context (via JWT claims). The Application layer uses this for
/// authorization checks and audit attribution.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>The authenticated user's id, or null if anonymous.</summary>
    Guid? UserId { get; }

    /// <summary>The authenticated user's email, or null if anonymous.</summary>
    string? Email { get; }

    /// <summary>The authenticated user's role, or null if anonymous.</summary>
    string? Role { get; }

    /// <summary>The school the authenticated user is bound to, or null.</summary>
    Guid? SchoolId { get; }

    bool IsAuthenticated { get; }
}