using HomeFoods.Domain.Repositories;
using HomeFoods.Application.DTOs;
using HomeFoods.Domain.Entities;

namespace HomeFoods.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderResponseDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return null;
        return MapToDto(order);
    }

    public async Task<OrderResponseDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber, cancellationToken);
        if (order == null) return null;
        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId, cancellationToken);
        return orders.Select(MapToDto);
    }

    private static OrderResponseDto MapToDto(Order order)
    {
        return new OrderResponseDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Total = order.Total,
            Status = order.Status.ToString(),
            OrderDate = order.OrderDate,
            Payment = order.Payment == null ? null : new PaymentResponseDto
            {
                PaymentId = order.Payment.Id,
                Status = order.Payment.Status.ToString(),
                TransactionId = order.Payment.TransactionId,
                PhonePeUrl = null
            }
        };
    }
}
