using System;
using System.Collections.Generic;

namespace ClonEbay_CoreAPI.Models;

public partial class Bid
{
    public int Id { get; set; }

    public int? ProductId { get; set; }

    public int? BidderId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? BidTime { get; set; }

    public virtual User? Bidder { get; set; }

    public virtual Product? Product { get; set; }
}
