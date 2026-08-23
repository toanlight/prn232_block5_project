namespace ClonEbay_CoreAPI.Models;

public partial class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
