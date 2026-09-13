using FactoryMesCrm.Application.Features.Mes.Queries.GetActiveWorkOrders;
using Microsoft.AspNetCore.Mvc;

namespace FactoryMesCrm.Api.Controllers;

public class WorkOrdersController : ApiControllerBase
{
    // Fabrikada devam eden veya yayınlanmış aktif iş emirlerini (MES) getirir.
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<WorkOrderListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveWorkOrders()
    {
        var result = await Mediator.Send(new GetActiveWorkOrdersQuery());
        return Ok(result);
    }
}