using Microsoft.AspNetCore.Mvc;
using SalesManagementPlatform.Api.Models.Requests;
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
    public async Task<ActionResult<IEnumerable<OrderListItemDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    public async Task<ActionResult<OrderDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetailsDto>> CreateAsync(
        [FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var input = new CreateOrderDto(
            request.ClientProvidedExternalReference,
            request.Currency,
            request.Notes,
            request.LineItems?.Select(item => item is null
                ? null
                : new CreateOrderLineItemDto(item.Sku, item.Name, item.Quantity, item.UnitPrice)).ToArray());

        var order = await _orderService.CreateAsync(input, cancellationToken);
        return CreatedAtRoute(nameof(GetByIdAsync), new { id = order.Id }, order);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderDetailsDto>> ChangeStatusAsync(
        Guid id, [FromBody] ChangeOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.Status is not { } status || request.ExpectedStatus is not { } expectedStatus ||
            !Enum.IsDefined(status) || !Enum.IsDefined(expectedStatus))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid order status",
                detail: "Choose a valid order status and try again.");
        }

        return Ok(await _orderService.ChangeStatusAsync(id, expectedStatus, status, cancellationToken));
    }
}
