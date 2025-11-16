using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Models.ViewModels;
using WebTMDT_DACN.Repository;
namespace WebTMDT_DACN.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _datacontext;
        public ProductController(DataContext context)
        {
            _datacontext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //public async Task<IActionResult> Details(int Id)
        //{


        //    if (Id == null) return RedirectToAction("Index");

        //    var productsById = _datacontext.Products.Include(p => p.Ratings).Where(p => p.Id == Id).FirstOrDefault(); //category = 4
        //                                                 //related product


        //    var relatedProducts = await _datacontext.Products
        //    .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
        //    .Take(4)
        //    .ToListAsync();

        //    ViewBag.RelatedProducts = relatedProducts;

        //    var viewModel = new ProductDetailsViewModel
        //    {
        //        ProductDetails = productsById

        //    };


        //    return View(viewModel);

        //}
        public async Task<IActionResult> Details(int Id)
        {
            // 1. Kiểm tra Id đầu vào. Nếu là int, nó không thể là null, 
            //    vì vậy hãy kiểm tra xem nó có phải là giá trị mặc định (0) hay không.
            if (Id == 0)
            {
                return RedirectToAction("Index");
            }

            // 2. Lấy sản phẩm (dùng FirstOrDefaultAsync)
            var productsById = await _datacontext.Products
                                    .Include(p => p.Ratings)
                                    .FirstOrDefaultAsync(p => p.Id == Id);

            // 3. === THÊM BƯỚC KIỂM TRA NULL QUAN TRỌNG Ở ĐÂY ===
            if (productsById == null)
            {
                // Nếu không tìm thấy sản phẩm, trả về trang 404
                return NotFound();
            }
            // ===============================================

            // 4. Bây giờ 'productsById' chắc chắn không null
            var relatedProducts = await _datacontext.Products
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productsById
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _datacontext.Products
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .ToListAsync();

            ViewBag.Keyword = searchTerm;

            return View(products);
        }

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if(ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Comment = rating.Comment,
                    Name = rating.Name,
                    Email = rating.Email,
                    Star = rating.Star
                };
                _datacontext.Ratings.Add(ratingEntity);
                await _datacontext.SaveChangesAsync();

                TempData["success"] = "Đánh giá sản phẩm thành công";
                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["error"] = "Đánh giá sản phẩm thất bại. Vui lòng kiểm tra lại thông tin.";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return RedirectToAction("Details", new { Id = rating.ProductId });
            }
            return Redirect(Request.Headers["Referer"]);
        }
    }
}
