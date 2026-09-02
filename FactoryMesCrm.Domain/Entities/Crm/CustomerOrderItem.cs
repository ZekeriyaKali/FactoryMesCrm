using FactoryMesCrm.Domain.Common;

namespace FactoryMesCrm.Domain.Entities.Crm;

public class CustomerOrderItem : Entity<Guid>
{
    public Guid CustomerOrderId { get; private set; }
    public string ProductCode { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    // EF Core için private ctor
    private CustomerOrderItem() { }

    // Sadece CustomerOrder (Aggregate Root) içinden çağrılabilmesi için internal yapıldı.
    // Dış katmanlar doğrudan CustomerOrderItem nesnesi oluşturamaz!
    internal CustomerOrderItem(Guid id, Guid customerOrderId, string productCode, int quantity, decimal unitPrice)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("Ürün kodu boş geçilemez.", nameof(productCode));

        CustomerOrderId = customerOrderId;
        ProductCode = productCode;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}