using HomeFoods.Application.DTOs;

namespace HomeFoods.Application.Services;

public interface IOrderService
{
    Task<OrderResponseDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderResponseDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderResponseDto>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}
