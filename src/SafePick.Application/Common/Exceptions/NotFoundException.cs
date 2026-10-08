namespace SafePick.Application.Common.Exceptions;

/// <summary>
/// Raised when a requested aggregate does not exist.
/// Translated by the API's exception middleware into an HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' ({key}) was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}