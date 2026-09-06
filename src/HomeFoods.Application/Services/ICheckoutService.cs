using HomeFoods.Application.DTOs;

namespace HomeFoods.Application.Services;

public interface ICheckoutService
{
    Task<OrderResponseDto> ProcessCheckout(CheckoutDto checkoutDto);
    Task<bool> CompletePayment(Guid orderId, string transactionId);
}
