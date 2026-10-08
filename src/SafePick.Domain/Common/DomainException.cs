namespace SafePick.Domain.Common;

/// <summary>
/// Raised when a domain invariant is violated.
///
/// This exception is the domain's way of saying: "the requested operation
/// would put the system into an invalid state." It is never used for
/// validation of external input (that is the Application layer's job) —
/// only for violations of rules the domain itself guarantees.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}