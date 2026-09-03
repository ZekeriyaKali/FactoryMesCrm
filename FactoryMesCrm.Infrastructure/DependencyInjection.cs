using FactoryMesCrm.Infrastructure.Outbox;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FactoryMesCrm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. MassTransit & RabbitMQ Konfigürasyonu
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Otomatik Retry Politikası (Network dalgalanmalarına karşı direnç)
                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
            });
        });

        // 2. Outbox Background Processor Kaydı (Hosted Service)
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}