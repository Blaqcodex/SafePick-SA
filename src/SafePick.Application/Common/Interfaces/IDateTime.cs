namespace SafePick.Application.Common.Interfaces;

/// <summary>
/// Server-authoritative time.
///
/// This is ADR-0012 in code: the domain and application never read the
/// system clock directly. All timestamps flow through this abstraction,
/// which is implemented in Infrastructure and can be substituted in tests.
/// </summary>
public interface IDateTime
{
    /// <summary>The current moment, in UTC.</summary>
    DateTime UtcNow { get; }
}