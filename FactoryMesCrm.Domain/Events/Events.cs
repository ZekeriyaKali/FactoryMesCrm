using FactoryMesCrm.Domain.Common;

namespace FactoryMesCrm.Domain.Events;

/// <summary>
/// Yeni bir İş Emri (Work Order) taslak olarak oluşturulduğunda fırlatılır.
/// </summary>
public record WorkOrderCreatedEvent(
    Guid WorkOrderId, 
    string Code, 
    DateTime OccurredOn
) : IDomainEvent
{
    // Constructor çağrıldığında zaman damgası verilmezse otomatik UtcNow atar.
    public WorkOrderCreatedEvent(Guid workOrderId, string code) 
        : this(workOrderId, code, DateTime.UtcNow) { }
}

/// <summary>
/// İş Emri sahaya (üretim hattına) serbest bırakıldığında fırlatılır.
/// </summary>
public record WorkOrderReleasedEvent(
    Guid WorkOrderId, 
    DateTime OccurredOn
) : IDomainEvent
{
    public WorkOrderReleasedEvent(Guid workOrderId) 
        : this(workOrderId, DateTime.UtcNow) { }
}

/// <summary>
/// İş Emri üzerindeki tüm operasyonlar tamamlandığında fırlatılır.
/// </summary>
public record WorkOrderCompletedEvent(
    Guid WorkOrderId, 
    Guid? CustomerOrderId, 
    DateTime OccurredOn
) : IDomainEvent
{
    public WorkOrderCompletedEvent(Guid workOrderId, Guid? customerOrderId) 
        : this(workOrderId, customerOrderId, DateTime.UtcNow) { }
}

/// <summary>
/// Müşteri siparişi onaylandığında içerdiği ürün kalemlerini taşımak için kullanılan DTO.
/// </summary>
public record OrderApprovedItemDto(string ProductCode, int Quantity);

/// <summary>
/// CRM tarafında Müşteri Siparişi onaylandığında fırlatılır. MES tarafı bu event'i dinleyerek İş Emri oluşturur.
/// </summary>
public record CustomerOrderApprovedEvent(
    Guid CustomerOrderId, 
    string OrderNumber, 
    List<OrderApprovedItemDto> Items, 
    DateTime OccurredOn
) : IDomainEvent
{
    public CustomerOrderApprovedEvent(Guid customerOrderId, string orderNumber, List<OrderApprovedItemDto> items) 
        : this(customerOrderId, orderNumber, items, DateTime.UtcNow) { }
}