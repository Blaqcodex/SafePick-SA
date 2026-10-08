using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Application.Custody.Queries.GetCustodyHistoryByLearner;

public sealed class GetCustodyHistoryByLearnerHandler
    : IRequestHandler<GetCustodyHistoryByLearnerQuery, IReadOnlyList<CustodyHistoryItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustodyHistoryByLearnerHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CustodyHistoryItemDto>> Handle(
        GetCustodyHistoryByLearnerQuery request,
        CancellationToken cancellationToken)
    {
        var events = await _context.CustodyEvents
            .Where(e => e.LearnerId == request.LearnerId)
            .OrderByDescending(e => e.ServerTimestamp.Value)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var driverIds = events
            .Where(e => e.DriverId.HasValue)
            .Select(e => e.DriverId!.Value)
            .Distinct()
            .ToList();

        var drivers = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .Select(d => new
            {
                d.Id,
                d.FullName,
                VehicleId = d.Vehicle.Id,
                Registration = d.Vehicle.RegistrationNumber
            })
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        return events.Select(e =>
        {
            drivers.TryGetValue(e.DriverId ?? Guid.Empty, out var driver);

            return new CustodyHistoryItemDto(
                CustodyEventId: e.Id,
                Type: e.Type.ToString(),
                ServerTimestamp: e.ServerTimestamp.Value,
                DeviceTimestamp: e.DeviceTimestamp,
                DriverId: e.DriverId,
                DriverName: driver?.FullName,
                VehicleId: e.VehicleId,
                VehicleRegistration: driver?.Registration,
                Reason: e.Reason,
                Note: e.Note,
                OriginalEventId: e.OriginalEventId);
        }).ToList();
    }
}