using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Infrastructure.Identity;
using SafePick.Infrastructure.Notifications;
using SafePick.Infrastructure.Pdf;
using SafePick.Infrastructure.Persistence;
using SafePick.Infrastructure.Persistence.Outbox;
using SafePick.Infrastructure.Time;


namespace SafePick.Api.DependencyInjection;

public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IDateTime, SystemDateTime>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Transactional outbox
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddHostedService<OutboxDispatcher>();

        // Notifications
        services.Configure<NotificationSettings>(
            configuration.GetSection("Notifications"));
        services.AddSingleton<INotificationTemplateRenderer, FileTemplateRenderer>();
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();
        services.AddScoped<NotificationDispatcher>();
        services.AddScoped<IInvitationMailer, InvitationMailer>();

        // PDF receipt generation
        services.AddSingleton<IPdfGenerator, QuestPdfReceiptGenerator>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddSingleton<JwtTokenService>();

        return services;
    }
}