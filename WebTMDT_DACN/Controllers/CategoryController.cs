using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Controllers
{
    public class CategoryController : Controller
    {
        private readonly DataContext _datacontext;
        public CategoryController(DataContext context)
        {
            _datacontext = context;
        }
        //public async Task<IActionResult> Index(string Slug = "")
        //{

        //    CategoryModel category = _datacontext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();

        //    if (category == null) return RedirectToAction("Index");

        //    var productsByCategory = _datacontext.Products.Where(p => p.CategoryId == category.Id);

        //    return View(await productsByCategory.OrderByDescending(p => p.Id).ToListAsync());
        //}
        // (Trong file Controllers/CategoryController.cs)

        public async Task<IActionResult> Index(string Slug = "", string sort_by = "")
        {
            CategoryModel category = _datacontext.Categories.Where(c => c.Slug == Slug).FirstOrDefault();

            if (category == null) return RedirectToAction("Index");

            // SỬA LỖI 1: Gửi tên Category qua ViewBag
            ViewBag.CategoryName = category.Name;

            // Khởi tạo truy vấn
            // SỬA LỖI 2: Thêm .Include(p => p.Category)
            IQueryable<ProductModel> productsByCategory = _datacontext.Products
                                                            .Include(p => p.Category)
                                                            .Where(p => p.CategoryId == category.Id);

            // BỔ SUNG LOGIC SẮP XẾP SẢN PHẨM (Code này của bạn đã đúng)
            switch (sort_by)
            {
                case "price_increase":
                    productsByCategory = productsByCategory.OrderBy(p => p.Price);
                    break;
                case "price_decrease":
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                    break;
                case "oldest":
                    productsByCategory = productsByCategory.OrderBy(p => p.Id);
                    break;
                default:
                    productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
                    break;
            }

            return View(await productsByCategory.ToListAsync());
        }

    }
}
