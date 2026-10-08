using SafePick.Domain.Common;

namespace SafePick.Domain.Transport;

/// <summary>
/// A learner's parent or guardian contact.
///
/// This is a value object, not a User. The parent does not log in
/// (ADR-0008). They are represented by the contact information the
/// school records at assignment time and receive one-directional
/// notifications (email, later SMS/WhatsApp).
///
/// Email is required because it is the current notification channel.
/// Phone and name are optional but recommended.
/// ConsentRecorded captures POPIA consent for storing this contact.
/// </summary>
public sealed class ParentContact : ValueObject
{
    public string Email { get; }
    public string? Phone { get; }
    public string? Name { get; }
    public bool ConsentRecorded { get; }

    public ParentContact(
        string email,
        string? phone,
        string? name,
        bool consentRecorded)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Parent contact email is required.");

        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
            throw new DomainException("Parent contact email is not valid.");

        if (!consentRecorded)
            throw new DomainException(
                "POPIA consent must be recorded to store a parent contact.");

        Email = email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        ConsentRecorded = true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return Name;
        yield return ConsentRecorded;
    }

    public override string ToString()
        => Name is not null ? $"{Name} <{Email}>" : Email;
}