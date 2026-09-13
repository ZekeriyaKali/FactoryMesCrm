using FactoryMesCrm.Application.Features.Crm.Commands.ApproveCustomerOrder;
using FactoryMesCrm.Application.Features.Crm.Commands.CreateCustomerOrder;
using Microsoft.AspNetCore.Mvc;

namespace FactoryMesCrm.Api.Controllers;

public class CustomerOrdersController : ApiControllerBase
{
    // müşteri siparişi oluşturur
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerOrderCommand command)
    {
        var orderId = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = orderId }, orderId);
    }

    // verilen siparişi onaylar ve Outbox üzerinden CustomerOrderApprovedEvent fırlatır.
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid id)
    {
        await Mediator.Send(new ApproveCustomerOrderCommand(id));
        return Ok(new { Message = "Sipariş başarıyla onaylandı ve üretim sürecine aktarılmak üzere Outbox'a işlendi." });
    }
}