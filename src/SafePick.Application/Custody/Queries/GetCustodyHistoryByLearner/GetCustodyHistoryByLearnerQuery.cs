using MediatR;

namespace SafePick.Application.Custody.Queries.GetCustodyHistoryByLearner;

/// <summary>
/// Returns the full custody event history for a learner, newest first.
/// Used for audit and for populating the learner's detail view.
/// </summary>
public sealed record GetCustodyHistoryByLearnerQuery(
    Guid LearnerId,
    int Skip = 0,
    int Take = 100) : IRequest<IReadOnlyList<CustodyHistoryItemDto>>;

public sealed record CustodyHistoryItemDto(
    Guid CustodyEventId,
    string Type,
    DateTime ServerTimestamp,
    DateTime? DeviceTimestamp,
    Guid? DriverId,
    string? DriverName,
    Guid? VehicleId,
    string? VehicleRegistration,
    string? Reason,
    string? Note,
    Guid? OriginalEventId);