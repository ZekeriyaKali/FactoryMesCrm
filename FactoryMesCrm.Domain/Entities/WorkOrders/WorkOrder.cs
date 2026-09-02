using FactoryMesCrm.Domain.Common;
using FactoryMesCrm.Domain.Enums;
using FactoryMesCrm.Domain.Events;

namespace FactoryMesCrm.Domain.Entities.WorkOrders;

public class WorkOrder : AggregateRoot<Guid>
{
    public string Code { get; private set; } // Örn: WO-2026-0001
    public Guid? CustomerOrderId { get; private set; } // CRM Entegrasyon bağlantısı
    public string ProductCode { get; private set; }
    public int TargetQuantity { get; private set; }
    public WorkOrderStatus Status { get; private set; }
    public DateTime ScheduledStartDate { get; private set; }

    private readonly List<Operation> _operations = new();
    public IReadOnlyCollection<Operation> Operations => _operations.AsReadOnly();

    private WorkOrder() { } // EF Core

    public static WorkOrder Create(string code, string productCode, int targetQuantity, DateTime scheduledStartDate, Guid? customerOrderId = null)
    {
        if (targetQuantity <= 0)
            throw new ArgumentException("Hedef üretim miktarı 0'dan büyük olmalıdır.");

        var workOrder = new WorkOrder
        {
            Id = Guid.NewGuid(),
            Code = code,
            ProductCode = productCode,
            TargetQuantity = targetQuantity,
            ScheduledStartDate = scheduledStartDate,
            CustomerOrderId = customerOrderId,
            Status = WorkOrderStatus.Draft
        };

        workOrder.AddDomainEvent(new WorkOrderCreatedEvent(workOrder.Id, workOrder.Code));

        return workOrder;
    }

    public void AddOperation(int sequence, string name, Guid workCenterId)
    {
        if (Status != WorkOrderStatus.Draft)
            throw new InvalidOperationException("Sadece taslak (Draft) durumundaki iş emrine operasyon eklenebilir.");

        var operation = new Operation(Guid.NewGuid(), Id, sequence, name, workCenterId, TargetQuantity);
        _operations.Add(operation);
    }

    public void ReleaseToProduction()
    {
        if (!_operations.Any())
            throw new InvalidOperationException("Operasyonu olmayan iş emri sahaya serbest bırakılamaz.");

        if (Status != WorkOrderStatus.Draft)
            throw new InvalidOperationException("Sadece taslak durumundaki iş emri serbest bırakılabilir.");

        Status = WorkOrderStatus.Released;
        AddDomainEvent(new WorkOrderReleasedEvent(Id));
    }

    public void RecordOperationProgress(Guid operationId, int producedQty, int scrapQty)
    {
        var operation = _operations.FirstOrDefault(o => o.Id == operationId);
        if (operation == null)
            throw new InvalidOperationException("İş emrine ait böyle bir operasyon bulunamadı.");

        operation.RecordProduction(producedQty, scrapQty);

        if (Status == WorkOrderStatus.Released)
        {
            Status = WorkOrderStatus.InProgress;
        }

        // Eğer tüm operasyonlar tamamlandıysa İş Emrini kapat
        if (_operations.All(o => o.Status == OperationStatus.Completed))
        {
            Status = WorkOrderStatus.Completed;
            AddDomainEvent(new WorkOrderCompletedEvent(Id, CustomerOrderId));
        }
    }
}