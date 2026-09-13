using FactoryMesCrm.Application.Common.Events;
using FactoryMesCrm.Domain.Events;
using FactoryMesCrm.Persistence;
using FactoryMesCrm.Persistence.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FactoryMesCrm.Infrastructure.Outbox;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;
    private const int BatchSize = 20; // Bellek tüketimini sınırlandırmak için batching

    public OutboxProcessorService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Background Service başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox mesajları işlenirken beklenmeyen bir hata oluştu.");
            }

            // Polling interval (Maliyet ve CPU dostu 5 saniye bekleme süresi)
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        // 1. İşlenmemiş mesajları Batch olarak çek (Partial Index sayesinde O(1) okuma)
        var unprocessedMessages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.Error == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(stoppingToken);

        if (unprocessedMessages.Count == 0)
            return;

        _logger.LogInformation("{Count} adet işlenmemiş Outbox mesajı bulundu.", unprocessedMessages.Count);

        foreach (var message in unprocessedMessages)
        {
            try
            {
                // 2. Mesaj Tipi Deserialization & Event Publishing
                if (message.Type == nameof(CustomerOrderApprovedEvent))
                {
                    var domainEvent = JsonConvert.DeserializeObject<CustomerOrderApprovedEvent>(message.Content);

                    if (domainEvent != null)
                    {
                        var integrationEvent = new CustomerOrderApprovedIntegrationEvent(
                            domainEvent.CustomerOrderId,
                            domainEvent.OrderNumber,
                            domainEvent.Items.Select(i => new OrderApprovedItemIntegrationDto(i.ProductCode, i.Quantity)).ToList(),
                            message.OccurredOnUtc
                        );

                        // MassTransit üzerinden RabbitMQ Broker'a dökülür
                        await publishEndpoint.Publish(integrationEvent, stoppingToken);
                    }
                }

                // 3. Başarılı İşaretleme
                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox mesajı {MessageId} RabbitMQ'ya yayınlanırken hata oluştu.", message.Id);
                message.Error = ex.Message;
            }
        }

        // 4. Değişiklikleri Veritabanına Tek Transaction Olarak Yaz
        await dbContext.SaveChangesAsync(stoppingToken);
    }
}