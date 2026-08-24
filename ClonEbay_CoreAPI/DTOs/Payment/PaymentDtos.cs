namespace ClonEbay_CoreAPI.DTOs.Payment;

public sealed class PayPalPaymentDto
{
    public int OrderId { get; init; }
    public int ItemCount { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string OrderStatus { get; init; } = string.Empty;
    public DateTime? PaidAt { get; init; }
}

public sealed class SimulatePayPalRequestDto
{
    public bool Succeeded { get; init; }
}
