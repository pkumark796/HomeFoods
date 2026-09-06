namespace HomeFoods.Application.Services;

public interface IPhonePeService
{
    Task<PhonePePaymentResponse> InitiatePayment(PhonePePaymentRequest request);
    Task<PaymentStatus> VerifyPayment(string transactionId);
}

public class PhonePePaymentRequest
{
    public string MerchantTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}

public class PhonePePaymentResponse
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
