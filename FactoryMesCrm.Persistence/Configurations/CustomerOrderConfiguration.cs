using FactoryMesCrm.Domain.Entities.Crm;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FactoryMesCrm.Persistence.Configurations;

public class CustomerOrderConfiguration : IEntityTypeConfiguration<CustomerOrder>
{
    public void Configure(EntityTypeBuilder<CustomerOrder> builder)
    {
        builder.ToTable("CustomerOrders", "crm");

        builder.HasKey(co => co.Id);

        builder.Property(co => co.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(co => co.OrderNumber)
            .IsUnique();

        builder.Metadata
            .FindNavigation(nameof(CustomerOrder.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(co => co.Items)
            .WithOne()
            .HasForeignKey(i => i.CustomerOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}