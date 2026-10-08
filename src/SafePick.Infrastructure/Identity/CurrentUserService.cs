using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SafePick.Application.Common.Interfaces;

namespace SafePick.Infrastructure.Identity;

/// <summary>
/// Reads the authenticated user from the HTTP context's claims principal.
/// The Application layer uses this for authorization and audit attribution.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? Principal?.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirst(ClaimTypes.Email)?.Value
                         ?? Principal?.FindFirst("email")?.Value;

    public string? Role => Principal?.FindFirst(ClaimTypes.Role)?.Value;

    public Guid? SchoolId
    {
        get
        {
            var school = Principal?.FindFirst("school_id")?.Value;
            return Guid.TryParse(school, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}