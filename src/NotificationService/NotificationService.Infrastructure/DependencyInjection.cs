using EventPlatform.Contracts.Messages;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Common.Interfaces;
using NotificationService.Application.Consumers;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationDb"))
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<INotificationDbContext>(sp => sp.GetRequiredService<NotificationDbContext>());
        services.AddScoped<IEmailSender, MailKitEmailSender>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<EventCreatedConsumer>();
            x.AddConsumer<EventCreatedFaultConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("event-created-queue", e =>
                {
                    // Reintentos con backoff: 3 intentos, 5s de intervalo.
                    // Si los 3 fallan, MassTransit mueve el mensaje a
                    // "event-created-queue_error" automáticamente (DLQ)
                    // y publica Fault<EventCreatedMessage>, capturado por
                    // EventCreatedFaultConsumer.
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    e.ConfigureConsumer<EventCreatedConsumer>(context);
                });

                cfg.ReceiveEndpoint("event-created-fault-queue", e =>
                {
                    e.ConfigureConsumer<EventCreatedFaultConsumer>(context);
                });
            });
        });

        return services;
    }
}