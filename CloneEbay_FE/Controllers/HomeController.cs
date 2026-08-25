using CloneEbay_FE.Models;
using CloneEbay_FE.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CloneEbay_FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IApiClientService apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeLandingViewModel();

            try
            {
                // Fetch products (pageSize=12)
                var productsResponse = await _apiClient.GetAsync<PagedProductViewModel>("products?page=1&pageSize=12");
                if (productsResponse?.Success == true && productsResponse.Data?.Items != null)
                {
                    viewModel.FeaturedProducts = productsResponse.Data.Items;
                    viewModel.AuctionProducts = productsResponse.Data.Items.Where(p => p.IsAuction).ToList();
                }

                // Fetch categories
                var categoriesResponse = await _apiClient.GetAsync<List<CategoryViewModel>>("products/categories");
                if (categoriesResponse?.Success == true && categoriesResponse.Data != null)
                {
                    viewModel.Categories = categoriesResponse.Data;
                }

                // Fetch active coupons
                var couponsResponse = await _apiClient.GetAsync<List<CouponViewModel>>("coupons");
                if (couponsResponse?.Success == true && couponsResponse.Data != null)
                {
                    viewModel.ActiveCoupons = couponsResponse.Data;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu cho trang Landing Page");
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
