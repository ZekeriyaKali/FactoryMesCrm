using FactoryMesCrm.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FactoryMesCrm.Application.Features.Crm.Commands.ApproveCustomerOrder;

public record ApproveCustomerOrderCommand(Guid CustomerOrderId) : IRequest<bool>;

public class ApproveCustomerOrderCommandValidator : AbstractValidator<ApproveCustomerOrderCommand>
{
    public ApproveCustomerOrderCommandValidator()
    {
        RuleFor(x => x.CustomerOrderId).NotEmpty().WithMessage("Sipariş kimliği zorunludur.");
    }
}

public class ApproveCustomerOrderCommandHandler : IRequestHandler<ApproveCustomerOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ApproveCustomerOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveCustomerOrderCommand request, CancellationToken cancellationToken)
    {
        // Aggregate Root ve ilişkili Child koleksiyonu yükle
        var order = await _context.CustomerOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.CustomerOrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Id'si {request.CustomerOrderId} olan müşteri siparişi bulunamadı.");
        }

        // Domain Metodunu Çağır (Bu metot CustomerOrderApprovedEvent fırlatır!)
        order.Approve();

        // SaveChangesAsync çağrıldığında DbContext Override mekanizmamız 
        // bu Domain Event'i yakalayacak ve 'OutboxMessages' tablosuna otomatik kaydedecektir!
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}