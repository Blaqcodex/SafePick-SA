using FluentValidation.Results;

namespace SafePick.Application.Common.Exceptions;

/// <summary>
/// Raised when one or more FluentValidation validators reject a request.
/// The dictionary maps property names to their failure messages.
/// </summary>
public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}