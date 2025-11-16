using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Coupon")]
    [Authorize(Roles = "ADMIN")]
    public class CouponController : Controller
    {
        private readonly DataContext _datacontext;
        public CouponController(DataContext context)
        {
            _datacontext = context;

        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var coupon_list = await _datacontext.Coupons.ToListAsync();
            ViewBag.Coupons = coupon_list;
            return View();
        }
        [Route("Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponModel coupon)
        {

            if (ModelState.IsValid)
            {
               

                _datacontext.Add(coupon);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Thêm coupon thành công";
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

            return View();
        }
        [Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            CouponModel coupon = await _datacontext.Coupons.FindAsync(Id);
            return View(coupon);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponModel coupon)
        {

            if (ModelState.IsValid)
            {


                _datacontext.Update(coupon);
                await _datacontext.SaveChangesAsync();
                TempData["success"] = "Cập nhật Coupon thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Lỗi khi cập nhật Coupon";
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

            return View(coupon);
        }
        public async Task<IActionResult> Delete(int Id)
        {
            CouponModel coupon = await _datacontext.Coupons.FindAsync(Id);
            if (coupon == null)
            {
                return NotFound();
            }
            _datacontext.Coupons.Remove(coupon);
            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Xóa Coupon thành công";
            return RedirectToAction("Index");
        }
    }
}
