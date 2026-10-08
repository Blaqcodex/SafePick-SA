using SafePick.Domain.Common;
using SafePick.Domain.Identity;
using SafePick.Domain.Identity.Events;

namespace SafePick.Domain.Tests.Identity;

public sealed class InvitationTests
{
    private static readonly DateTime IssuedAt =
        new(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc);

    private static Invitation Issue(TimeSpan? validFor = null) =>
        Invitation.Issue(
            "admin@example.com",
            Role.SchoolAdmin,
            Guid.NewGuid(),
            new InvitationToken("token-hash"),
            validFor ?? TimeSpan.FromHours(2),
            Guid.NewGuid(),
            IssuedAt);

    [Fact]
    public void IssueCreatesPendingInvitationAndRaisesIssuedEvent()
    {
        var invitation = Issue();

        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.Equal(IssuedAt.AddHours(2), invitation.ExpiresAt);
        Assert.IsType<InvitationIssued>(Assert.Single(invitation.DomainEvents));
    }

    [Fact]
    public void OperatorsCannotBeInvited()
    {
        Assert.Throws<DomainException>(() => Invitation.Issue(
            "operator@example.com",
            Role.Operator,
            Guid.NewGuid(),
            new InvitationToken("token-hash"),
            TimeSpan.FromHours(1),
            Guid.NewGuid(),
            IssuedAt));
    }

    [Fact]
    public void InvitationValidityMustBePositive()
    {
        Assert.Throws<DomainException>(() => Issue(TimeSpan.Zero));
    }

    [Fact]
    public void AcceptChangesStatusAndRejectsSecondAcceptance()
    {
        var invitation = Issue();
        var acceptedAt = IssuedAt.AddMinutes(5);

        invitation.Accept(acceptedAt);

        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.Equal(acceptedAt, invitation.AcceptedAt);
        Assert.Throws<DomainException>(() => invitation.Accept(acceptedAt));
    }

    [Fact]
    public void AcceptAfterExpiryMarksInvitationExpiredAndRaisesEvent()
    {
        var invitation = Issue(TimeSpan.FromMinutes(5));

        Assert.Throws<DomainException>(() =>
            invitation.Accept(IssuedAt.AddMinutes(6)));

        Assert.Equal(InvitationStatus.Expired, invitation.Status);
        Assert.Contains(invitation.DomainEvents, domainEvent =>
            domainEvent is InvitationExpired);
    }
}
