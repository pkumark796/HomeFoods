using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HomeFoods.Application.Services;
using Microsoft.Extensions.Configuration;

namespace HomeFoods.Infrastructure.Services;

public class PhonePeService : IPhonePeService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _merchantId;
    private readonly string _saltKey;
    private readonly string _saltIndex;
    private readonly string _baseUrl;

    public PhonePeService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _merchantId = configuration["PhonePe:MerchantId"] ?? "MERCHANTUAT";
        _saltKey = configuration["PhonePe:SaltKey"] ?? "099eb0cd-02cf-4e2a-8aca-3e6c6aff0399";
        _saltIndex = configuration["PhonePe:SaltIndex"] ?? "1";
        _baseUrl = configuration["PhonePe:BaseUrl"] ?? "https://api-preprod.phonepe.com/apis/pg-sandbox";
    }

    public async Task<PhonePePaymentResponse> InitiatePayment(PhonePePaymentRequest request)
    {
        try
        {
            var payload = new
            {
                merchantId = _merchantId,
                merchantTransactionId = request.MerchantTransactionId,
                merchantUserId = "MUID" + DateTime.Now.Ticks,
                amount = (long)(request.Amount * 100), // Convert to paise
                redirectUrl = request.RedirectUrl,
                redirectMode = "POST",
                callbackUrl = request.CallbackUrl,
                mobileNumber = request.MobileNumber,
                paymentInstrument = new
                {
                    type = "PAY_PAGE"
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonPayload));

            // Generate checksum for initiate endpoint using payload and api path
            var checksum = GenerateChecksum(base64Payload, "/v3/transaction/initiate");

            var requestData = new
            {
                request = base64Payload
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v3/transaction/initiate")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Add("X-VERIFY", checksum);
            httpRequest.Headers.Add("X-MERCHANT-ID", _merchantId);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PhonePeApiResponse>(responseContent);

                return new PhonePePaymentResponse
                {
                    Success = result?.Success ?? false,
                    PaymentUrl = result?.Data?.InstrumentResponse?.RedirectInfo?.Url,
                    TransactionId = request.MerchantTransactionId,
                    Message = result?.Message
                };
            }

            return new PhonePePaymentResponse
            {
                Success = false,
                Message = $"Payment initiation failed: {responseContent}"
            };
        }
        catch (Exception ex)
        {
            return new PhonePePaymentResponse
            {
                Success = false,
                Message = $"Error initiating payment: {ex.Message}"
            };
        }
    }

    public async Task<PaymentStatus> VerifyPayment(string transactionId)
    {
        try
        {
            var endpoint = $"/v3/transaction/{_merchantId}/{transactionId}/status";
            // Generate checksum for status endpoint using the endpoint path
            var checksum = GenerateChecksum(string.Empty, endpoint);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}{endpoint}");
            httpRequest.Headers.Add("X-VERIFY", checksum);
            httpRequest.Headers.Add("X-MERCHANT-ID", _merchantId);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PhonePeStatusResponse>(responseContent);

                return result?.Data?.State?.ToUpper() switch
                {
                    "COMPLETED" => PaymentStatus.Completed,
                    "FAILED" => PaymentStatus.Failed,
                    _ => PaymentStatus.Pending
                };
            }

            return PaymentStatus.Failed;
        }
        catch
        {
            return PaymentStatus.Failed;
        }
    }

    private string GenerateChecksum(string data, string apiPath)
    {
        // PhonePe checksum is computed over data + apiPath + saltKey
        var checksumString = data + apiPath + _saltKey;
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(checksumString));
        return BitConverter.ToString(hash).Replace("-", "").ToLower() + "###" + _saltIndex;
    }

    private class PhonePeApiResponse
    {
        public bool Success { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public PhonePeData? Data { get; set; }
    }

    private class PhonePeData
    {
        public string? MerchantId { get; set; }
        public string? MerchantTransactionId { get; set; }
        public PhonePeInstrumentResponse? InstrumentResponse { get; set; }
    }

    private class PhonePeInstrumentResponse
    {
        public string? Type { get; set; }
        public PhonePeRedirectInfo? RedirectInfo { get; set; }
    }

    private class PhonePeRedirectInfo
    {
        public string? Url { get; set; }
        public string? Method { get; set; }
    }

    private class PhonePeStatusResponse
    {
        public bool Success { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public PhonePeStatusData? Data { get; set; }
    }

    private class PhonePeStatusData
    {
        public string? State { get; set; }
        public string? ResponseCode { get; set; }
    }
}
