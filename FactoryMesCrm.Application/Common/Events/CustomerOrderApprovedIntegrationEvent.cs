namespace FactoryMesCrm.Application.Common.Events;

public record OrderApprovedItemIntegrationDto(string ProductCode, int Quantity);

public record CustomerOrderApprovedIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    List<OrderApprovedItemIntegrationDto> Items,
    DateTime OccurredOnUtc
);