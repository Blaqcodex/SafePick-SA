namespace SafePick.Application.Common.Exceptions;

/// <summary>
/// Raised when the current user is not permitted to perform an action.
/// Translated by the API's exception middleware into an HTTP 403.
/// </summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message) : base(message)
    {
    }

    public ForbiddenAccessException() : base("Access to this resource is forbidden.")
    {
    }
}