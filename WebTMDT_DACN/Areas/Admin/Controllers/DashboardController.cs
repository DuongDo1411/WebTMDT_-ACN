using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : Controller
    {
        private readonly DataContext _datacontext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _datacontext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var count_product = _datacontext.Products.Count();
            var count_order = _datacontext.Orders.Count();
            var count_category = _datacontext.Categories.Count();
            var count_user = _datacontext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetChartData()
        {
            var data = _datacontext.Statisticals
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }
    }
}
