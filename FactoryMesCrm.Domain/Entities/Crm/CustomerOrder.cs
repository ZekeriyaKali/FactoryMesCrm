using FactoryMesCrm.Domain.Common;
using FactoryMesCrm.Domain.Events;

namespace FactoryMesCrm.Domain.Entities.Crm;

public class CustomerOrder : AggregateRoot<Guid>
{
    // C# auto-property mantığına uygun olarak { get; private set; } kullanıyoruz.
    // Dışarıdan doğrudan atama engellenerek encapsulation (kapsülleme) korunur.
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public bool IsApproved { get; private set; }
    public DateTime OrderDate { get; private set; }

    // DDD kuralı: Koleksiyon dışarıya IReadOnlyCollection olarak sunulur.
    // _items listesi kapsüllenir, dışarıdan _items.Add() yapılamaz.
    private readonly List<CustomerOrderItem> _items = new();
    public IReadOnlyCollection<CustomerOrderItem> Items => _items.AsReadOnly();

    // EF Core ve ORM araçları için private/protected parametresiz ctor
    private CustomerOrder() { }

    // Factory Method (Sipariş oluşturma iş kuralı tek bir kanaldan yürütülür)
    public static CustomerOrder Create(string orderNumber, Guid customerId)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Sipariş numarası boş olamaz.", nameof(orderNumber));

        if (customerId == Guid.Empty)
            throw new ArgumentException("Müşteri ID geçerli bir GUID olmalıdır.", nameof(customerId));

        return new CustomerOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            IsApproved = false
        };
    }

    // Sipariş Kalemi Ekleme (Domain Metodu)
    public void AddItem(string productCode, int quantity, decimal unitPrice)
    {
        // İş Kuralı 1: Onaylanmış siparişe kalem eklenemez
        if (IsApproved)
            throw new InvalidOperationException("Onaylanmış bir siparişe yeni ürün kalemi eklenemez.");

        // İş Kuralı 2: Negatif veya sıfır miktar kontrolü (Güvenlik & Veri Tutarlılığı)
        if (quantity <= 0)
            throw new ArgumentException("Ürün miktarı 0'dan büyük olmalıdır.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Birim fiyat negatif olamaz.", nameof(unitPrice));

        var item = new CustomerOrderItem(Guid.NewGuid(), Id, productCode, quantity, unitPrice);
        _items.Add(item);
    }

    // Sipariş Onaylama (Domain Metodu & Event Fırlatma)
    public void Approve()
    {
        // Idempotency kontrolü: Zaten onaylıysa tekrar event fırlatma
        if (IsApproved) return;

        // İş Kuralı: Kalemi olmayan boş sipariş onaylanamaz
        if (!_items.Any())
            throw new InvalidOperationException("İçinde ürün kalemi bulunmayan sipariş onaylanamaz.");

        IsApproved = true;

        // MES sistemine bildirilmek üzere Transactional Outbox'a girecek Domain Event!
        AddDomainEvent(new CustomerOrderApprovedEvent(
            Id,
            OrderNumber,
            _items.Select(i => new OrderApprovedItemDto(i.ProductCode, i.Quantity)).ToList()
        ));
    }
}