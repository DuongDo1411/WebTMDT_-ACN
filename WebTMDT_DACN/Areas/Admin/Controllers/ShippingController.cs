using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    [Authorize(Roles = "ADMIN")]
    public class ShippingController : Controller
    {
        private readonly DataContext _datacontext;
        public ShippingController(DataContext context)
        {
            _datacontext = context;

        }
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var shippings = await _datacontext.Shippings.ToListAsync();
            ViewBag.Shippings = shippings;
            return View();
        }

        [HttpPost]
        [Route("StoreShipping")]
        public async Task<IActionResult> StoreShipping(ShippingModel shippingModel, string tinh, string quan, string phuong, decimal price)
        {
            // 1. Gán các giá trị nhận được vào đối tượng shippingModel
            shippingModel.City = tinh;
            shippingModel.District = quan;
            shippingModel.Ward = phuong;
            shippingModel.Price = price;

            try
            {
                // 2. Kiểm tra xem dữ liệu đã tồn tại trong database chưa
                var existingShipping = await _datacontext.Shippings
                    .AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

                if (existingShipping)
                {
                    // Nếu trùng, trả về kết quả { duplicate = true }
                    return Ok(new { duplicate = true, message = "Dữ liệu trùng lặp." });
                }

                // 3. Nếu không trùng, thêm mới vào database
                _datacontext.Shippings.Add(shippingModel);
                await _datacontext.SaveChangesAsync();

                // Trả về kết quả { success = true }
                return Ok(new { success = true, message = "Thêm shipping thành công" });
            }
            catch (Exception)
            {
                // Xử lý nếu có lỗi server
                return StatusCode(500, "An error occurred while adding shipping.");
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            ShippingModel shipping = await _datacontext.Shippings.FindAsync(Id);

            _datacontext.Shippings.Remove(shipping);
            await _datacontext.SaveChangesAsync();
            TempData["success"] = "Shipping đã được xóa thành công";
            return RedirectToAction("Index");
        }
    }
}
