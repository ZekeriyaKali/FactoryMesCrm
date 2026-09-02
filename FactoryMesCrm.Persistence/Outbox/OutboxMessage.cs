namespace FactoryMesCrm.Persistence.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Event türü (örn: CustomerOrderApprovedEvent)
    public string Content { get; set; } = string.Empty; // JSON formatında payload
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; } // Işlenme zamanı (Null ise henüz gönderilmedi)
    public string? Error { get; set; } // Hata oluştuysa loglanır
}