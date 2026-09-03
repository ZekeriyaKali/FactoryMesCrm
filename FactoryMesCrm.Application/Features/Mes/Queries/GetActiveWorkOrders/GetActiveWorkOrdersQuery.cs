using FactoryMesCrm.Application.Common.Interfaces;
using FactoryMesCrm.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FactoryMesCrm.Application.Features.Mes.Queries.GetActiveWorkOrders;

public record WorkOrderListDto(
    Guid Id,
    string Code,
    string ProductCode,
    int TargetQuantity,
    string Status,
    DateTime ScheduledStartDate
);

public record GetActiveWorkOrdersQuery() : IRequest<List<WorkOrderListDto>>;

public class GetActiveWorkOrdersQueryHandler : IRequestHandler<GetActiveWorkOrdersQuery, List<WorkOrderListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveWorkOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkOrderListDto>> Handle(GetActiveWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking(): EF Core ChangeTracker nesnelerini takip etmez.
        // Bellek (RAM) ve CPU tüketimi en aza indirilir. High-throughput okuma performansı!
        return await _context.WorkOrders
            .AsNoTracking()
            .Where(w => w.Status == WorkOrderStatus.Released || w.Status == WorkOrderStatus.InProgress)
            .OrderByDescending(w => w.ScheduledStartDate)
            .Select(w => new WorkOrderListDto(
                w.Id,
                w.Code,
                w.ProductCode,
                w.TargetQuantity,
                w.Status.ToString(),
                w.ScheduledStartDate
            ))
            .ToListAsync(cancellationToken);
    }
}