using FactoryMesCrm.Domain.Entities.WorkOrders;
using FactoryMesCrm.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryMesCrm.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        // Tablo adı ve Şema yönetimi:
        // MES ve CRM tablolarını aynı PostgreSQL DB içinde mantıksal şemalarla ayırıyoruz.
        builder.ToTable("WorkOrders", "mes");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.ProductCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.TargetQuantity)
            .IsRequired();

        // Enum dönüşümü: Enum değerlerini DB'de tamsayı yerine string saklıyoruz.
        // Neden? İleride enum sıralaması değişirse DB'deki verilerin bozulmasını önlemek için.
        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // İNDEKS STRATEJİSİ (Performans için kritik)
        // 1. Kod üzerinden arama çok yapılacağı için Unique Index
        builder.HasIndex(w => w.Code)
            .IsUnique();

        // 2. Operatör ekranları sadece aktif (Released, InProgress) iş emirlerini sorgular.
        // Status ve ScheduledStartDate üzerine kompozit indeks atıyoruz.
        builder.HasIndex(w => new { w.Status, w.ScheduledStartDate });

        // Aggregate Root - Child Entity ilişkisi (Backing Field):
        // EF Core'un _operations private listesine erişmesini sağlıyoruz.
        // Public setter olmadığı için encapsulation korunur.
        builder.Metadata
            .FindNavigation(nameof(WorkOrder.Operations))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // One-to-Many İlişki ve Cascade Delete Politikası
        builder.HasMany(w => w.Operations)
            .WithOne()
            .HasForeignKey(o => o.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade); // WorkOrder silinirse operasyonları da silinsin
    }
}