using SafePick.Domain.Common;

namespace SafePick.Domain.Transport;

/// <summary>
/// A learner's grade and class letter — e.g. Grade 9B.
///
/// This is a value object: it has no identity, it is defined by its
/// values, and it validates its own construction. The system cannot hold
/// an invalid grade or class letter.
///
/// Modeled as Grade (int) + Letter (char) rather than a single string
/// such as "9B" so that queries, sorting, and validation are all clean.
/// </summary>
public sealed class GradeClass : ValueObject
{
    public int Grade { get; }
    public char Letter { get; }

    public GradeClass(int grade, char letter)
    {
        if (grade < 1 || grade > 12)
            throw new DomainException("Grade must be between 1 and 12.");

        var normalisedLetter = char.ToUpperInvariant(letter);
        if (normalisedLetter < 'A' || normalisedLetter > 'Z')
            throw new DomainException("Class letter must be a single letter A–Z.");

        Grade = grade;
        Letter = normalisedLetter;
    }

    /// <summary>
    /// Parses a combined form such as "9B" into a GradeClass.
    /// Accepts "9B", "9b", "9 B", and " 9b ".
    /// </summary>
    public static GradeClass Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("GradeClass value must not be empty.");

        var cleaned = value.Replace(" ", string.Empty).ToUpperInvariant();

        if (cleaned.Length < 2)
            throw new DomainException("GradeClass must include a grade and a class letter.");

        var letter = cleaned[^1];
        var gradePart = cleaned[..^1];

        if (!int.TryParse(gradePart, out var grade))
            throw new DomainException($"'{gradePart}' is not a valid grade number.");

        return new GradeClass(grade, letter);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Grade;
        yield return Letter;
    }

    public override string ToString() => $"{Grade}{Letter}";
}