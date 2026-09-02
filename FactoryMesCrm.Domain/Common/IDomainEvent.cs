namespace FactoryMesCrm.Domain.Common;

/// <summary>
/// Sistemde gerçekleşen tüm Domain Event'lerin uyması gereken temel kontrat.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Olayın UTC zaman diliminde gerçekleştiği tarih ve saat.
    /// </summary>
    DateTime OccurredOn { get; }
}