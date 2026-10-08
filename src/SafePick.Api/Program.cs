using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SafePick.Api.DependencyInjection;
using SafePick.Api.Middleware;
using SafePick.Application.Identity.Commands.SeedOperator;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Controllers + JSON options.
//
// JsonStringEnumConverter makes System.Text.Json serialize enum values
// as their names ("Approved", "PickedUp") instead of their numeric values
// (1, 2). This matters for the frontend, which types statuses as string
// unions — and for anyone reading the API responses, where "Approved" is
// self-describing and 1 is not.
// ---------------------------------------------------------------------------
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ---------------------------------------------------------------------------
// CORS — allow the Angular dev server to call the API.
//
// The Angular development server runs on http://localhost:4200. The browser
// will block cross-origin requests unless the API explicitly permits them.
//
// In production, this would be restricted to the actual frontend domain.
// For development, localhost:4200 is the origin.
// ---------------------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("SafePickWeb", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ---------------------------------------------------------------------------
// Swagger with JWT bearer support.
//
// AddSecurityDefinition registers the "Bearer" scheme so Swagger UI renders
// an Authorize button. AddSecurityRequirement tells Swagger UI to send the
// token as "Authorization: Bearer <token>" on every request after the user
// authorizes.
// ---------------------------------------------------------------------------
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT access token (no 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---------------------------------------------------------------------------
// JWT authentication configuration.
// ---------------------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["SigningKey"]
    ?? throw new InvalidOperationException("JWT SigningKey is not configured.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException(
        "JWT SigningKey must contain at least 32 UTF-8 bytes.");

var seedOperatorPassword = builder.Configuration["Seed:OperatorPassword"];
if (string.IsNullOrWhiteSpace(seedOperatorPassword))
    throw new InvalidOperationException(
        "Seed:OperatorPassword must be configured before starting SafePick.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// ---------------------------------------------------------------------------
// Migrations are NOT applied here.
//
// The running application connects as 'safepick_app' — a least-privilege role
// that intentionally lacks DDL permissions. Migrations are a deployment
// concern and must be applied separately, as the bootstrap role.
//
// Seeding, however, runs at startup — the app role has the necessary
// INSERT privilege on the 'users' table, and seeding is idempotent.
// ---------------------------------------------------------------------------

using (var scope = app.Services.CreateScope())
{
    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var email = config["Seed:OperatorEmail"]!;
    var password = seedOperatorPassword;
    var displayName = config["Seed:OperatorDisplayName"]!;

    await mediator.Send(new SeedOperatorCommand(email, password, displayName));
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ---------------------------------------------------------------------------
// Middleware pipeline.
//
// Order matters:
//
//   1. HTTPS redirection  — redirect HTTP → HTTPS
//   2. CORS               — must run BEFORE authentication so preflight
//                           requests are answered. Must run AFTER HTTPS
//                           redirection. Placement here is correct.
//   3. Exception handling — wraps everything below it, translating
//                           domain exceptions into clean HTTP responses
//   4. Authentication     — validates the JWT
//   5. Authorization      — checks [Authorize] attributes
//   6. Controllers        — runs the endpoint
//
// Putting UseCors after UseAuthorization breaks preflight requests —
// the browser's OPTIONS preflight would be rejected by the auth layer
// before CORS ever had a chance to respond.
// ---------------------------------------------------------------------------
app.UseHttpsRedirection();

app.UseCors("SafePickWeb");

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "SafePick SA API is running.");

app.Run();