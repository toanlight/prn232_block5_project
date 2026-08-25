using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClonEbay_CoreAPI.Models
{
    [Table("Notification")]
    public partial class Notification
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; } // NULL = broadcast to all

        [MaxLength(50)]
        public string? UserRole { get; set; } // 'All', 'Buyer', 'Seller', 'Admin', NULL

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "InApp"; // "Order", "Return", "Promotion", "System"

        [MaxLength(50)]
        public string Status { get; set; } = "Sent"; // 'Pending', 'Sent', 'Scheduled'

        public DateTime? ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        public int? CreatedBy { get; set; } // Admin ID

        public bool IsRead { get; set; } = false;

        [MaxLength(500)]
        public string? LinkUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User? Creator { get; set; }
    }
}
