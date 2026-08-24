namespace ClonEbay_CoreAPI.DTOs.Order;

public sealed class OrderHistoryPageDto
{
    public IReadOnlyList<OrderHistoryItemDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public string StatusFilter { get; init; } = "all";
}

public sealed class OrderHistoryItemDto
{
    public int OrderId { get; init; }
    public DateTime? OrderDate { get; init; }
    public int ItemCount { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusKey { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
    public string? PaymentMethod { get; init; }
    public string? PaymentStatus { get; init; }
    public string? ShippingStatus { get; init; }
    public IReadOnlyList<OrderHistoryPreviewItemDto> PreviewItems { get; init; } = [];
}

public sealed class OrderHistoryPreviewItemDto
{
    public int ProductId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int Quantity { get; init; }
}

public sealed class OrderDetailDto
{
    public int OrderId { get; init; }
    public DateTime? OrderDate { get; init; }
    public int ItemCount { get; init; }
    public decimal Subtotal { get; init; }
    public decimal ShippingFee { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusKey { get; init; } = string.Empty;
    public string StatusLabel { get; init; } = string.Empty;
    public bool CanReview { get; init; }
    public bool CanRequestReturn { get; init; }
    public OrderHistoryAddressDto? Address { get; init; }
    public OrderHistoryPaymentDto? Payment { get; init; }
    public OrderHistoryShippingDto? Shipping { get; init; }
    public IReadOnlyList<OrderDetailItemDto> Items { get; init; } = [];
    public IReadOnlyList<OrderTimelineStepDto> Timeline { get; init; } = [];
}

public sealed class OrderDetailItemDto
{
    public int ProductId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal => UnitPrice * Quantity;
}

public sealed class OrderHistoryAddressDto
{
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string? State { get; init; }
    public string Country { get; init; } = string.Empty;
    public string? PostalCode { get; init; }
}

public sealed class OrderHistoryPaymentDto
{
    public string Method { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime? PaidAt { get; init; }
}

public sealed class OrderHistoryShippingDto
{
    public string Carrier { get; init; } = string.Empty;
    public string? TrackingNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? EstimatedArrival { get; init; }
}

public sealed class OrderTimelineStepDto
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime? Timestamp { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsCurrent { get; init; }
    public bool IsCancelled { get; init; }
}
