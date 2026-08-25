namespace ClonEbay_CoreAPI.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public bool HasPendingReturnRequest { get; set; }
        public string? ShippingCarrier { get; set; }
        public string? ShippingStatus { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? PaymentPaidAt { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
        public bool HasReviewed { get; set; }
        public int? ReviewId { get; set; }
        public int? ReviewRating { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? SellerName { get; set; }
        public int? SellerId { get; set; }
    }
}
