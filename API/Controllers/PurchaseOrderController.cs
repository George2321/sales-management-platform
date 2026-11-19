using Microsoft.AspNetCore.Mvc;
using SalesManagementPlatform.Application.Orders;
using SalesManagementPlatform.Application.Orders.Dtos;

namespace SalesManagementPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PurchaseOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public PurchaseOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetAllAsync(cancellationToken));
    }
}
