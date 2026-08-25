namespace CloneEbay_FE.Models
{
    public class HomeLandingViewModel
    {
        public List<ProductCardViewModel> FeaturedProducts { get; set; } = new();
        public List<ProductCardViewModel> AuctionProducts { get; set; } = new();
        public List<CategoryViewModel> Categories { get; set; } = new();
        public List<CouponViewModel> ActiveCoupons { get; set; } = new();
    }
}
