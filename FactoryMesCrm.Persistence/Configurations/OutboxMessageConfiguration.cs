using FactoryMesCrm.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryMesCrm.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", "common");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type).IsRequired().HasMaxLength(255);
        builder.Property(o => o.Content).IsRequired().HasColumnType("jsonb"); // PostgreSQL'in native JSONB tipi!

        // Background worker'ın aradığı tek şey "Henüz işlenmemiş mesajlar"dır.
        // Bu sorguyu anında getirmek için Partial Index kullanıyoruz.
        builder.HasIndex(o => o.ProcessedOnUtc)
            .HasFilter("\"ProcessedOnUtc\" IS NULL");
    }
}