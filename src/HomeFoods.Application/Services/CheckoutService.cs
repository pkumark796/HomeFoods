using HomeFoods.Domain.Entities;
using HomeFoods.Domain.Repositories;
using HomeFoods.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace HomeFoods.Application.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPhonePeService _phonePeService;
    private readonly IConfiguration _configuration;

    public CheckoutService(IUnitOfWork unitOfWork, IPhonePeService phonePeService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _phonePeService = phonePeService;
        _configuration = configuration;
    }

    public async Task<OrderResponseDto> ProcessCheckout(CheckoutDto checkoutDto)
    {
        // Use seeded guest user and address
        var customerId = new Guid("11111111-1111-1111-1111-111111111111");
        var addressId = new Guid("22222222-2222-2222-2222-222222222222");

        decimal subTotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in checkoutDto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product {item.ProductId} not found");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for {product.Name}");

            var price = product.DiscountedPrice ?? product.Price;
            var totalPrice = price * item.Quantity;
            subTotal += totalPrice;

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = price,
                TotalPrice = totalPrice
            });

            // Update stock
            product.StockQuantity -= item.Quantity;
        }

        var deliveryFee = CalculateDeliveryFee(subTotal);
        var tax = CalculateTax(subTotal);
        var total = subTotal + deliveryFee + tax;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customerId,
            DeliveryAddressId = addressId,
            Status = OrderStatus.Pending,
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Tax = tax,
            Total = total,
            OrderDate = DateTime.UtcNow,
            SpecialInstructions = checkoutDto.SpecialInstructions,
            OrderItems = orderItems
        };

        await _unitOfWork.Orders.AddAsync(order);

        // Create payment
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Amount = total,
            Method = (HomeFoods.Domain.Entities.PaymentMethod)checkoutDto.PaymentMethod,
            Status = HomeFoods.Domain.Entities.PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Process payment based on method
        PaymentResponseDto? paymentResponse = null;

        if (checkoutDto.PaymentMethod == DTOs.PaymentMethod.PhonePe)
        {
            var phonePeRequest = new PhonePePaymentRequest
            {
                MerchantTransactionId = order.OrderNumber,
                Amount = total,
                MobileNumber = "9999999999", // Should come from user profile
                CallbackUrl = $"{_configuration["AppUrl"]}/api/payment/callback",
                RedirectUrl = $"{_configuration["AppUrl"]}/order-confirmation/{order.Id}"
            };

            var phonePeResponse = await _phonePeService.InitiatePayment(phonePeRequest);

            if (phonePeResponse.Success)
            {
                payment.TransactionId = phonePeResponse.TransactionId;
                paymentResponse = new PaymentResponseDto
                {
                    PaymentId = payment.Id,
                    Status = "Pending",
                    TransactionId = phonePeResponse.TransactionId,
                    PhonePeUrl = phonePeResponse.PaymentUrl
                };
            }
        }
        else if (checkoutDto.PaymentMethod == DTOs.PaymentMethod.Cash)
        {
            payment.Status = HomeFoods.Domain.Entities.PaymentStatus.Pending;
            order.Status = OrderStatus.Processing;
            paymentResponse = new PaymentResponseDto
            {
                PaymentId = payment.Id,
                Status = "Pending (Cash on Delivery)"
            };
        }

        order.Payment = payment;
        await _unitOfWork.SaveChangesAsync();

        return new OrderResponseDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Total = order.Total,
            Status = order.Status.ToString(),
            OrderDate = order.OrderDate,
            Payment = paymentResponse
        };
    }

    public async Task<bool> CompletePayment(Guid orderId, string transactionId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
        if (order == null || order.Payment == null)
            return false;

        var paymentStatus = await _phonePeService.VerifyPayment(transactionId);

        if (paymentStatus == Application.Services.PaymentStatus.Completed)
        {
            order.Payment.Status = HomeFoods.Domain.Entities.PaymentStatus.Completed;
            order.Payment.CompletedAt = DateTime.UtcNow;
            order.Status = OrderStatus.Processing;
            order.ProcessedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        else if (paymentStatus == Application.Services.PaymentStatus.Failed)
        {
            order.Payment.Status = HomeFoods.Domain.Entities.PaymentStatus.Failed;
            order.Status = OrderStatus.Cancelled;
            order.CancelledAt = DateTime.UtcNow;
            order.CancellationReason = "Payment failed";

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        return false;
    }

    private decimal CalculateDeliveryFee(decimal subTotal)
    {
        return subTotal < 500 ? 50 : 0; // Free delivery above ₹500
    }

    private decimal CalculateTax(decimal subTotal)
    {
        return Math.Round(subTotal * 0.05m, 2); // 5% GST
    }

    private string GenerateOrderNumber()
    {
        return $"ORD{DateTime.UtcNow:yyyyMMdd}{DateTime.UtcNow.Ticks % 100000:D5}";
    }
}
