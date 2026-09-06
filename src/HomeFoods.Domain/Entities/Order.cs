namespace HomeFoods.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public Guid DeliveryAddressId { get; set; }
    public Address DeliveryAddress { get; set; } = null!;

    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public DateTime OrderDate { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public string? SpecialInstructions { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public Payment? Payment { get; set; }
    public Review? Review { get; set; }
}
