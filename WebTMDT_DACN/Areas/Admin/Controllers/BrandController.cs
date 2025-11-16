using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Brand")]
    [Authorize(Roles = "ADMIN,PUBLISHER,AUTHOR")]
    public class BrandController : Controller
    {
        private readonly DataContext _datacontext;
        public BrandController(DataContext context)
        {
            _datacontext = context;

        }
        [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<BrandModel> brand = _datacontext.Brands.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = brand.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = brand.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.paper = pager;

            return View(data);
        }
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _datacontext.Brands.FindAsync(Id);
            return View(brand);
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandModel brand)
        {

            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _datacontext.Categories.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên nhãn hiệu đã tồn tại");
                    return View(brand);
                }

                _datacontext.Add(brand);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Thêm nhãn hiệu thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi thêm nhãn hiệu";
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

            return View(brand);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {

            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _datacontext.Categories.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên nhãn hiệu đã tồn tại");
                    return View(brand);
                }

                _datacontext.Update(brand);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Cập nhật nhãn hiệu thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi cập nhật nhãn hiệu";
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

            return View(brand);
        }
        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _datacontext.Brands.FindAsync(Id);
            if (brand == null)
            {
                return NotFound();
            }
            _datacontext.Brands.Remove(brand);
            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Xóa nhãn hiệu thành công";
            return RedirectToAction("Index");
        }
    }
}
