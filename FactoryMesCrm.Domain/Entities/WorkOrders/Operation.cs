using FactoryMesCrm.Domain.Common;
using FactoryMesCrm.Domain.Enums;

namespace FactoryMesCrm.Domain.Entities.WorkOrders;

public class Operation : Entity<Guid>
{
    public Guid WorkOrderId { get; private set; }
    public int Sequence { get; private set; } // Operasyon sırası: 10, 20, 30...
    public string Name { get; private set; } // Örn: CNC Kesim, Montaj, Kalite Kontrol
    public Guid WorkCenterId { get; private set; } // Hangi tezgahta/istasyonda yapılacağı
    public OperationStatus Status { get; private set; }
    public int TargetQuantity { get; private set; }
    public int CompletedQuantity { get; private set; }
    public int ScrapQuantity { get; private set; } // Fire miktarı

    private Operation() { } // EF Core

    internal Operation(Guid id, Guid workOrderId, int sequence, string name, Guid workCenterId, int targetQuantity)
        : base(id)
    {
        WorkOrderId = workOrderId;
        Sequence = sequence;
        Name = name;
        WorkCenterId = workCenterId;
        TargetQuantity = targetQuantity;
        Status = OperationStatus.Pending;
        CompletedQuantity = 0;
        ScrapQuantity = 0;
    }

    internal void RecordProduction(int quantityProduced, int scrapProduced)
    {
        if (Status != OperationStatus.Running && Status != OperationStatus.Setup)
            throw new InvalidOperationException("Yalnızca çalışan veya setup durumundaki operasyona üretim kaydı girilebilir.");

        CompletedQuantity += quantityProduced;
        ScrapQuantity += scrapProduced;

        if (CompletedQuantity >= TargetQuantity)
        {
            Status = OperationStatus.Completed;
        }
    }

    internal void Start()
    {
        if (Status == OperationStatus.Completed)
            throw new InvalidOperationException("Tamamlanmış operasyon tekrar başlatılamaz.");

        Status = OperationStatus.Running;
    }
}