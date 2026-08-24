namespace CloneEbay_FE.Models;

public sealed class OrderHistoryPageViewModel
{
    public List<OrderHistoryItemViewModel> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string StatusFilter { get; set; } = "all";
}

public sealed class OrderHistoryItemViewModel
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public int ItemCount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusKey { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string? ShippingStatus { get; set; }
    public List<OrderHistoryPreviewItemViewModel> PreviewItems { get; set; } = [];
}

public sealed class OrderHistoryPreviewItemViewModel
{
    public int ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
}

public sealed class OrderDetailViewModel
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusKey { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public bool CanReview { get; set; }
    public bool CanRequestReturn { get; set; }
    public OrderHistoryAddressViewModel? Address { get; set; }
    public OrderHistoryPaymentViewModel? Payment { get; set; }
    public OrderHistoryShippingViewModel? Shipping { get; set; }
    public List<OrderDetailItemViewModel> Items { get; set; } = [];
    public List<OrderTimelineStepViewModel> Timeline { get; set; } = [];
}

public sealed class OrderDetailItemViewModel
{
    public int ProductId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OrderHistoryAddressViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
}

public sealed class OrderHistoryPaymentViewModel
{
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
}

public sealed class OrderHistoryShippingViewModel
{
    public string Carrier { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EstimatedArrival { get; set; }
}

public sealed class OrderTimelineStepViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsCancelled { get; set; }
}
