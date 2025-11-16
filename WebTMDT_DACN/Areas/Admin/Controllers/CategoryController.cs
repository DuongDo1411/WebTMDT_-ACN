using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    [Authorize(Roles = "ADMIN,PUBLISHER,AUTHOR")]
    public class CategoryController : Controller
    {
        private readonly DataContext _datacontext;
        public CategoryController(DataContext context)
        {
            _datacontext = context;
            
        }
        [Route("Category/Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _datacontext.Categories.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = category.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.paper = pager;

            return View(data);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel category)
        {
            
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _datacontext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên danh mục đã tồn tại");
                    return View(category);
                }
                
                _datacontext.Add(category);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Thêm danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi thêm danh mục";
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

            return View(category);
        }
        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _datacontext.Categories.FindAsync(Id);
            return View(category);
        }
        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _datacontext.Categories.FindAsync(Id);
            if (category == null)
            {
                return NotFound();
            }
            _datacontext.Categories.Remove(category);
            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Xóa danh mục thành công";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {

            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _datacontext.Products.FirstOrDefaultAsync(p => p.Slug == category.Slug && p.Id != category.Id);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên danh mục đã tồn tại");
                    return View(category);
                }

                _datacontext.Update(category);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi thêm danh mục";
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

            return View(category);
        }
    }
}
