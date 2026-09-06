namespace HomeFoods.Application.DTOs;

public class CheckoutDto
{
    public Guid CustomerId { get; set; }
    public Guid DeliveryAddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<CheckoutItemDto> Items { get; set; } = new();
}

public class CheckoutItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    PhonePe = 3,
    Cash = 4,
    Wallet = 5
}
