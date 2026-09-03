using FactoryMesCrm.Domain.Entities.Crm;
using FactoryMesCrm.Domain.Entities.WorkOrders;
using Microsoft.EntityFrameworkCore;

namespace FactoryMesCrm.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<Operation> Operations { get; }
    DbSet<CustomerOrder> CustomerOrders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}