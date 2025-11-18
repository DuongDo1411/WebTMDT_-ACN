using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    [Authorize(Roles = "ADMIN")]
    public class OrderController : Controller
    {
        private readonly DataContext _datacontext;
        public OrderController(DataContext context)
        {
            _datacontext = context;

        }
        [HttpGet]
        [Route("Order/Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<OrderModel> order = _datacontext.Orders.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = order.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = order.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.paper = pager;

            return View(data);
        }
        [HttpGet]
        [Route("Order/ViewOrder")]
        //public async Task<IActionResult> ViewOrder(string ordercode)
        //{
        //    //var DetailsOrder = await _datacontext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
        //    var DetailsOrder = await _datacontext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
        //    var ShippingCost =  _datacontext.Orders.Where(o => o.OrderCode == ordercode).First();
        //    ViewBag.ShippingCost = ShippingCost.ShippingCost;
        //    return View("ViewOrder",DetailsOrder);
        //}
        public async Task<IActionResult> ViewOrder(string ordercode)
        {
            // Sửa lỗi: Lấy thông tin đơn hàng chính trước
            var order = await _datacontext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            // Lấy chi tiết đơn hàng
            var DetailsOrder = await _datacontext.OrderDetails
                                        .Include(od => od.Product)
                                        .Where(od => od.OrderCode == ordercode)
                                        .ToListAsync();

            // Truyền các thông tin cần thiết qua ViewBag
            ViewBag.ShippingCost = order.ShippingCost;
            ViewBag.OrderCode = order.OrderCode;
            ViewBag.Status = order.Status;

            return View("ViewOrder", DetailsOrder);
        }
        [HttpPost]
        [Route("UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _datacontext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;

            try
            {
                await _datacontext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật tình trạng đơn hàng thành công" });
            }
            catch (Exception)
            {


                return StatusCode(500, "An error occurred while updating the order status.");
            }
        }
        [HttpGet]
        [Route("Order/Delete")]
        public async Task<IActionResult> Delete(string ordercode) // <-- Đã đổi từ int Id sang string ordercode
        {
            // 1. Tìm đơn hàng chính bằng OrderCode
            var order = await _datacontext.Orders
                                .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            try
            {
                // 2. Tìm tất cả chi tiết đơn hàng liên quan (cũng bằng OrderCode)
                var orderDetails = await _datacontext.OrderDetails
                                            .Where(od => od.OrderCode == order.OrderCode)
                                            .ToListAsync();

                // 3. Xóa tất cả chi tiết đơn hàng trước (nếu có)
                if (orderDetails.Any())
                {
                    _datacontext.OrderDetails.RemoveRange(orderDetails);
                }

                // 4. Xóa đơn hàng chính
                _datacontext.Orders.Remove(order);

                // 5. Lưu thay đổi
                await _datacontext.SaveChangesAsync();

                TempData["success"] = "Đã xóa đơn hàng thành công.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Đã xảy ra lỗi khi xóa đơn hàng.";
            }

            // 6. Quay lại trang Index
            return RedirectToAction("Index");
        }
    }
}
