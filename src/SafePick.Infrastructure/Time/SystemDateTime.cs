using SafePick.Application.Common.Interfaces;

namespace SafePick.Infrastructure.Time;

/// <summary>
/// The production implementation of IDateTime.
///
/// Always returns UTC. There is no local-time logic here on purpose:
/// ADR-0012 says the system is server-authoritative in UTC, and any
/// display conversion is a presentation concern, not an infrastructure one.
/// </summary>
public sealed class SystemDateTime : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}