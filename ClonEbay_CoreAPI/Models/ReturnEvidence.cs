using System;

namespace ClonEbay_CoreAPI.Models;

public partial class ReturnEvidence
{
    public int Id { get; set; }

    public int ReturnRequestId { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public virtual ReturnRequest? ReturnRequest { get; set; }
}
