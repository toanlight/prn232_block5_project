namespace CloneEbay_FE.Models
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserRole { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string Type { get; set; } = "InApp";
        public string Status { get; set; } = "Sent";
        public bool IsRead { get; set; }
        public string? LinkUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UnreadCountViewModel
    {
        public int UnreadCount { get; set; }
    }
}
