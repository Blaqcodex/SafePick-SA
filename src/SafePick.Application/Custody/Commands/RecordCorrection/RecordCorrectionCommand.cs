using MediatR;

namespace SafePick.Application.Custody.Commands.RecordCorrection;

/// <summary>
/// Records a correction to a prior custody event. The original event is
/// never modified — the correction is a new event that references the
/// original by id (ADR-0011).
/// </summary>
public sealed record RecordCorrectionCommand(
    Guid OriginalEventId,
    Guid SchoolId,
    Guid LearnerId,
    string Reason,
    Guid ClientGeneratedId) : IRequest<Guid>;