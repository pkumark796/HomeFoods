using Microsoft.AspNetCore.Mvc;
using HomeFoods.Application.Services;
using HomeFoods.Application.DTOs;

namespace HomeFoods.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid orderId)
    {
        var dto = await _orderService.GetByIdAsync(orderId);
        if (dto == null) return NotFound(new { error = "Order not found" });
        return Ok(dto);
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<OrderResponseDto>> GetByNumber(string orderNumber)
    {
        var dto = await _orderService.GetByOrderNumberAsync(orderNumber);
        if (dto == null) return NotFound(new { error = "Order not found" });
        return Ok(dto);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetByCustomer(Guid customerId)
    {
        var list = await _orderService.GetByCustomerIdAsync(customerId);
        return Ok(list);
    }
}
