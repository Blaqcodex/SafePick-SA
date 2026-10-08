using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Identity;
using SafePick.Infrastructure.Identity;

namespace SafePick.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public sealed record LoginRequest(string Email, string Password);

    public sealed record LoginResponse(
        string AccessToken,
        string Email,
        string Role,
        Guid? SchoolId,
        Guid? DriverId);

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials." });

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        var token = _jwtTokenService.IssueToken(user);

        // For drivers, resolve the driver record by matching on email.
        // This lets the driver UI know its own driverId without an
        // extra round-trip.
        Guid? driverId = null;
        if (user.Role == Role.Driver)
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Email == user.Email, cancellationToken);

            driverId = driver?.Id;
        }

        return Ok(new LoginResponse(
            token,
            user.Email,
            user.Role.ToString(),
            user.SchoolId,
            driverId));
    }
}