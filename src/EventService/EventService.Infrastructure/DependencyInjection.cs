using EventService.Application.Common.Interfaces;
using EventService.Infrastructure.Caching;
using EventService.Infrastructure.Messaging;
using EventService.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EventService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EventDb"))
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IEventDbContext>(sp => sp.GetRequiredService<EventDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });
            });
        });

        return services;
    }
}