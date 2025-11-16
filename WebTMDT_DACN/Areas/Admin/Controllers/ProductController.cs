using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    [Authorize(Roles = "ADMIN")]
    public class ProductController : Controller
    {
        private readonly DataContext _datacontext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _datacontext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            //List<ProductModel> product = _datacontext.Products.ToList(); //33 datas
            var products = await _datacontext.Products
                .OrderByDescending(p => p.Id)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .ToListAsync();
            var soldData = await _datacontext.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalSold);

            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = products.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = products.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.paper = pager;

            return View(data);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_datacontext.Categories.ToList(), "Id", "Name");
            ViewBag.Brands = new SelectList(_datacontext.Brands.ToList(), "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel product)
        {
            ViewBag.Categories = new SelectList(_datacontext.Categories.ToList(), "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_datacontext.Brands.ToList(), "Id", "Name", product.BrandId);
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _datacontext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if(slug != null)
                {
                    ModelState.AddModelError("", "Tên sản phẩm đã tồn tại");
                    return View(product);
                }
                if (product.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }
                _datacontext.Add(product);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi thêm sản phẩm";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(long Id)
        {   
            ProductModel product = await _datacontext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_datacontext.Categories.ToList(), "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_datacontext.Brands.ToList(), "Id", "Name", product.BrandId);
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long Id , ProductModel product)
        {
            ViewBag.Categories = new SelectList(_datacontext.Categories.ToList(), "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_datacontext.Brands.ToList(), "Id", "Name", product.BrandId);
            var existed_product = _datacontext.Products.Find(product.Id);
            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _datacontext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != product.Id);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên sản phẩm đã tồn tại");
                    return View(product);
                }
                if (product.ImageUpload != null)
                {
                    //Upload new image
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);
                    //Delete old image
                    string oldImagePath = Path.Combine(uploadsDir, existed_product.Image);
                    try
                    {
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Lỗi khi xóa ảnh cũ");
                    }
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_product.Image = imageName;

                    
                }
                existed_product.Name = product.Name;
                existed_product.Description = product.Description;
                existed_product.Price = product.Price;
                existed_product.BrandId = product.BrandId;
                existed_product.CategoryId = product.CategoryId;
                
                _datacontext.Update(existed_product);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi cập nhật sản phẩm";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(product);
        }
        public async Task<IActionResult> Delete(int Id)
        {
            ProductModel product = await _datacontext.Products.FindAsync(Id);
            if (product == null)
            {
                return NotFound();
            }
            string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
            string oldImagePath = Path.Combine(uploadsDir, product.Image);
            try
            {
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi xóa ảnh cũ");
            }
            _datacontext.Products.Remove(product);
            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
        [Route("CreateProductQuantity")]
        [HttpGet]
        public async Task<IActionResult> CreateProductQuantity(long Id)
        {
            var productbyquantity = await _datacontext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productbyquantity;
            ViewBag.ProductId = Id;
            return View();
        }

        [Route("UpdateMoreQuantity")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMoreQuantity(ProductQuantityModel productQuantityModel)
        {
            // Get the product to update
            var product = _datacontext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound(); // Handle product not found scenario
            }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreated = DateTime.Now;


            _datacontext.Add(productQuantityModel);
            _datacontext.SaveChangesAsync();
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("CreateProductQuantity", "Product", new { Id = productQuantityModel.ProductId });
        }

    }
}
