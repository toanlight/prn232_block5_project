namespace ClonEbay_CoreAPI.Models;

public sealed class FavoriteProduct
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
