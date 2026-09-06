namespace HomeFoods.Application.DTOs;

public class OrderResponseDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public PaymentResponseDto? Payment { get; set; }
}

public class PaymentResponseDto
{
    public Guid PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? PhonePeUrl { get; set; }
}
