using FactoryMesCrm.Domain.Entities.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryMesCrm.Persistence.Configurations;

public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("Operations", "mes");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Istasyon bazlı sorgularda (Saha ekranı: "CNC 1 tezgahındaki işler neler?") hız kazanmak için Index
        builder.HasIndex(o => new { o.WorkCenterId, o.Status });
    }
}