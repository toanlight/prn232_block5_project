using System;
using System.Collections.Generic;

namespace ClonEbay_CoreAPI.Models;

public partial class ReturnRequest
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? OrderItemId { get; set; }

    public int? ProductId { get; set; }

    public int? UserId { get; set; }

    public string? Reason { get; set; }

    public string? Status { get; set; }

    public decimal? RefundAmount { get; set; }

    public string? RefundType { get; set; } // "Full" hoặc "Partial"

    public string? TrackingNumber { get; set; }

    public bool IsEscalated { get; set; } = false;

    public string? EscalationReason { get; set; }

    public string? AdminNotes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual OrderTable? Order { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<ReturnEvidence> Evidences { get; set; } = new List<ReturnEvidence>();
}
