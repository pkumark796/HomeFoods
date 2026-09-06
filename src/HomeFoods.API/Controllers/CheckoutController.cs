using Microsoft.AspNetCore.Mvc;
using HomeFoods.Application.Services;
using HomeFoods.Application.DTOs;

namespace HomeFoods.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Checkout([FromBody] CheckoutDto checkoutDto)
    {
        try
        {
            var result = await _checkoutService.ProcessCheckout(checkoutDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred during checkout", details = ex.Message });
        }
    }
}
