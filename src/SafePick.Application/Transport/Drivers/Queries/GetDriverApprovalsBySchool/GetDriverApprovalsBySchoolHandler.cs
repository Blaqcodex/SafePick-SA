using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Transport;

namespace SafePick.Application.Transport.Drivers.Queries.GetDriverApprovalsBySchool;

public sealed class GetDriverApprovalsBySchoolHandler
    : IRequestHandler<GetDriverApprovalsBySchoolQuery, IReadOnlyList<DriverApprovalItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDriverApprovalsBySchoolHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DriverApprovalItemDto>> Handle(
        GetDriverApprovalsBySchoolQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.DriverApprovals
            .Where(a => a.SchoolId == request.SchoolId);

        if (request.Status is not null)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var approvals = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        if (approvals.Count == 0)
        {
            return Array.Empty<DriverApprovalItemDto>();
        }

        var driverIds = approvals.Select(a => a.DriverId).Distinct().ToList();

        var drivers = await _context.Drivers
            .Where(d => driverIds.Contains(d.Id))
            .Include(d => d.Vehicle)
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        var results = new List<DriverApprovalItemDto>(approvals.Count);

        foreach (var approval in approvals)
        {
            if (!drivers.TryGetValue(approval.DriverId, out var driver))
            {
                continue;
            }

            results.Add(new DriverApprovalItemDto(
                ApprovalId: approval.Id,
                DriverId: driver.Id,
                DriverFullName: driver.FullName,
                DriverEmail: driver.Email,
                DriverPhone: driver.Phone,
                Vehicle: new VehicleDto(
                    VehicleId: driver.Vehicle.Id,
                    Registration: driver.Vehicle.RegistrationNumber,
                    Make: driver.Vehicle.Make,
                    Model: driver.Vehicle.Model,
                    Capacity: driver.Vehicle.Capacity),
                SchoolId: approval.SchoolId,
                Status: MapStatus(approval.Status),
                RequestedAt: approval.CreatedAt,
                DecidedAt: approval.DecidedAt,
                DecidedByUserId: approval.DecidedByUserId,
                Reason: approval.Reason));
        }

        return results;
    }

    private static DriverApprovalStatusDto MapStatus(DriverApproval.ApprovalStatus status)
    {
        return status switch
        {
            DriverApproval.ApprovalStatus.Pending => DriverApprovalStatusDto.Pending,
            DriverApproval.ApprovalStatus.Approved => DriverApprovalStatusDto.Approved,
            DriverApproval.ApprovalStatus.Rejected => DriverApprovalStatusDto.Rejected,
            DriverApproval.ApprovalStatus.Revoked => DriverApprovalStatusDto.Revoked,
            _ => DriverApprovalStatusDto.Pending
        };
    }
}