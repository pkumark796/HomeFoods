using Microsoft.AspNetCore.Mvc;
using HomeFoods.Application.Services;

namespace HomeFoods.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public PaymentController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost("callback")]
    public async Task<IActionResult> PhonePeCallback([FromBody] PhonePeCallbackDto callback)
    {
        try
        {
            var success = await _checkoutService.CompletePayment(callback.OrderId, callback.TransactionId);

            if (success)
            {
                return Ok(new { message = "Payment verified successfully" });
            }

            return BadRequest(new { error = "Payment verification failed" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Payment callback error", details = ex.Message });
        }
    }

    [HttpGet("verify/{orderId}/{transactionId}")]
    public async Task<IActionResult> VerifyPayment(Guid orderId, string transactionId)
    {
        try
        {
            var success = await _checkoutService.CompletePayment(orderId, transactionId);

            return Ok(new { success, message = success ? "Payment verified" : "Payment verification failed" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Verification error", details = ex.Message });
        }
    }
}

public class PhonePeCallbackDto
{
    public Guid OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
