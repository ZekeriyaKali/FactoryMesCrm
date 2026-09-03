using FactoryMesCrm.Application.Common.Interfaces;
using FactoryMesCrm.Domain.Entities.Crm;
using FluentValidation;
using MediatR;

namespace FactoryMesCrm.Application.Features.Crm.Commands.CreateCustomerOrder;

public record CreateCustomerOrderItemDto(string ProductCode, int Quantity, decimal UnitPrice);

public record CreateCustomerOrderCommand(
    string OrderNumber,
    Guid CustomerId,
    List<CreateCustomerOrderItemDto> Items
) : IRequest<Guid>;

public class CreateCustomerOrderCommandValidator : AbstractValidator<CreateCustomerOrderCommand>
{
    public CreateCustomerOrderCommandValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty().WithMessage("Sipariş numarası zorunludur.")
            .MaximumLength(50).WithMessage("Sipariş numarası 50 karakterden uzun olamaz.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Geçerli bir müşteri kimliği girilmelidir.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş en az bir ürün kalemi içermelidir.");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductCode).NotEmpty().WithMessage("Ürün kodu boş olamaz.");
            items.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
            items.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Birim fiyat negatif olamaz.");
        });
    }
}

public class CreateCustomerOrderCommandHandler : IRequestHandler<CreateCustomerOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCustomerOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Domain Factory Metodu ile Siparişi Oluştur
        var customerOrder = CustomerOrder.Create(request.OrderNumber, request.CustomerId);

        // 2. Kalemleri Ekle
        foreach (var item in request.Items)
        {
            customerOrder.AddItem(item.ProductCode, item.Quantity, item.UnitPrice);
        }

        // 3. Veritabanına Kaydet
        _context.CustomerOrders.Add(customerOrder);
        await _context.SaveChangesAsync(cancellationToken);

        return customerOrder.Id;
    }
}