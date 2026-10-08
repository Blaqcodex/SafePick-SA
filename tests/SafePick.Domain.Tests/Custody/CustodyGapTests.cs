using SafePick.Domain.Common;
using SafePick.Domain.Custody;
using SafePick.Domain.Custody.Events;

namespace SafePick.Domain.Tests.Custody;

public sealed class CustodyGapTests
{
    private static readonly DateTime WindowStart =
        new(2026, 10, 8, 14, 0, 0, DateTimeKind.Utc);

    private static CustodyGap CreateGap() => CustodyGap.Open(
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid(),
        WindowStart,
        WindowStart.AddMinutes(30),
        WindowStart.AddMinutes(31));

    [Fact]
    public void OpenStartsInOpenStateAndRaisesOpenedEvent()
    {
        var gap = CreateGap();

        Assert.Equal(GapState.Open, gap.State);
        Assert.IsType<CustodyGapOpened>(Assert.Single(gap.DomainEvents));
    }

    [Fact]
    public void GapCanBeAcknowledgedInvestigatedResolvedAndClosed()
    {
        var gap = CreateGap();
        var userId = Guid.NewGuid();
        var acknowledgedAt = WindowStart.AddHours(1);
        var resolvedAt = acknowledgedAt.AddHours(1);

        gap.Acknowledge(userId, acknowledgedAt);
        gap.BeginInvestigation(acknowledgedAt.AddMinutes(5));
        gap.Resolve(userId, "  Parent confirmed handover.  ", null, resolvedAt);
        gap.Close(resolvedAt.AddMinutes(1));

        Assert.Equal(GapState.Closed, gap.State);
        Assert.Equal(userId, gap.AcknowledgedByUserId);
        Assert.Equal(acknowledgedAt, gap.AcknowledgedAt);
        Assert.Equal(userId, gap.ResolvedByUserId);
        Assert.Equal("Parent confirmed handover.", gap.Resolution);
        Assert.Equal(resolvedAt, gap.ResolvedAt);
        Assert.Contains(gap.DomainEvents, domainEvent =>
            domainEvent is CustodyGapResolved);
    }

    [Fact]
    public void AcknowledgeRejectsAnEmptyUserId()
    {
        var gap = CreateGap();

        Assert.Throws<DomainException>(
            () => gap.Acknowledge(Guid.Empty, WindowStart));
        Assert.Equal(GapState.Open, gap.State);
    }

    [Fact]
    public void OpenGapCannotBeResolvedByAUser()
    {
        var gap = CreateGap();

        Assert.Throws<DomainException>(() =>
            gap.Resolve(Guid.NewGuid(), "Resolution", null, WindowStart));
        Assert.Equal(GapState.Open, gap.State);
    }

    [Fact]
    public void OpenGapCanBeResolvedByLateCustodyEvent()
    {
        var gap = CreateGap();
        var custodyEventId = Guid.NewGuid();
        var resolvedAt = WindowStart.AddHours(1);

        gap.ResolveByCustodyEvent(custodyEventId, resolvedAt);

        Assert.Equal(GapState.Resolved, gap.State);
        Assert.Equal(custodyEventId, gap.ResolvedByCustodyEventId);
        Assert.Equal("Resolved by a late-arriving custody event.", gap.Resolution);
        Assert.Equal(resolvedAt, gap.ResolvedAt);
    }
}
