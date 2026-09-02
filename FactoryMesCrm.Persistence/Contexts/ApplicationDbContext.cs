using FactoryMesCrm.Domain.Common;
using FactoryMesCrm.Domain.Entities.Crm;
using FactoryMesCrm.Domain.Entities.WorkOrders;
using FactoryMesCrm.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FactoryMesCrm.Persistence.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tüm Configuration sınıflarını assembly'den otomatik bulur ve uygular
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Transaction tamamlanmadan önce değişen Entity'lerdeki Domain Event'leri topla
        ConvertDomainEventsToOutboxMessages();

        // 2. Veritabanına hem ana veriyi hem de Outbox mesajlarını tek bir atomic transaction ile yaz
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ConvertDomainEventsToOutboxMessages()
    {
        // ChangeTracker kullanarak AggregateRoot olan ve içinde DomainEvent barındıran entity'leri yakala
        var domainEvents = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Select(x => x.Entity)
            .Where(x => x.DomainEvents.Any())
            .SelectMany(x =>
            {
                var events = x.DomainEvents.ToList();
                x.ClearDomainEvents(); // Event'leri işlediğimiz için entity üzerinden temizliyoruz
                return events;
            })
            .ToList();

        // Her bir Domain Event'i OutboxMessage formatına çevirip ChangeTracker'a ekle
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
            };

            OutboxMessages.Add(outboxMessage);
        }
    }
}