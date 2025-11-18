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
        [Route("GetChartData")]
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
        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            // Sửa lỗi logic: Thêm 1 ngày vào endDate để bao gồm cả ngày cuối cùng
            endDate = endDate.AddDays(1);

            // BƯỚC 1: LẤY DỮ LIỆU THÔ TỪ DATABASE
            // Chỉ dùng các hàm mà SQL hiểu được (Where, OrderBy)
            var dbData = _datacontext.Statisticals
                .Where(s => s.DateCreated >= startDate && s.DateCreated < endDate)
                .OrderBy(s => s.DateCreated) // Sắp xếp bằng DateCreated (kiểu DateTime)
                .ToList(); // <-- Chạy query, lấy dữ liệu về bộ nhớ

            // BƯỚC 2: ĐỊNH DẠNG DỮ LIỆU TRONG BỘ NHỚ
            // Bây giờ 'dbData' là một List<StatisticalsModel> trong bộ nhớ
            // Chúng ta có thể dùng bất kỳ hàm C# nào (bao gồm .ToString())
            var data = dbData.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"), // <-- Hàm này chạy trong bộ nhớ, không dịch ra SQL
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            }).ToList(); // (ToList() thứ 2 này là của LINQ-to-Objects)

            return Json(data);
        }
    }
}
