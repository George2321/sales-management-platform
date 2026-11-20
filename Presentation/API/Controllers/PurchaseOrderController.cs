using Microsoft.AspNetCore.Mvc;
using SalesManagementPlatform.Api.Models.Requests;
using SalesManagementPlatform.Application.Features.Orders.Dtos;
using SalesManagementPlatform.Application.Features.Orders.Interfaces;

namespace SalesManagementPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError, "application/problem+json")]
public sealed class PurchaseOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public PurchaseOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<OrderListItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderListItemDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<OrderDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetByIdAsync(id, cancellationToken));
    }

    [HttpGet("customers")]
    [ProducesResponseType<IEnumerable<OrderCustomerOptionDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderCustomerOptionDto>>> GetCustomersAsync(
        CancellationToken cancellationToken)
    {
        return Ok(await _orderService.GetCustomersAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<ActionResult<OrderDetailsDto>> CreateAsync(
        [FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var input = new CreateOrderDto(
            request.ClientProvidedExternalReference,
            request.CustomerId,
            request.Currency,
            request.Notes,
            request.LineItems?.Select(item => item is null
                ? null
                : new CreateOrderLineItemDto(item.Sku, item.Name, item.Quantity, item.UnitPrice)).ToArray());

        var order = await _orderService.CreateAsync(input, cancellationToken);
        return CreatedAtRoute(nameof(GetByIdAsync), new { id = order.Id }, order);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType<OrderDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status415UnsupportedMediaType, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity, "application/problem+json")]
    public async Task<ActionResult<OrderDetailsDto>> ChangeStatusAsync(
        Guid id, [FromBody] ChangeOrderStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.Status is not { } status || !Enum.IsDefined(status) ||
            request.ExpectedStatus is not { } expectedStatus || !Enum.IsDefined(expectedStatus))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid order status",
                detail: "Choose a valid order status and try again.");
        }

        return Ok(await _orderService.ChangeStatusAsync(id, expectedStatus, status, cancellationToken));
    }
}
