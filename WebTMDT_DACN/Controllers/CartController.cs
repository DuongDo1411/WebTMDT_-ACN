using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Models.ViewModels;   
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _datacontext;
        public CartController(DataContext context)
        {
            _datacontext = context;
        }
        public IActionResult Index()
        {
            List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            // Nhận shipping giá từ cookie

            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            if (shippingPriceCookie != null)
            {
                var shippingPriceJson = shippingPriceCookie;
                shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
            }
            CartItemViewModel cartVM = new()
            {
                CartItems = cartItems,
                SubTotal = cartItems.Sum(x => x.Price * x.Quantity),
                ShippingCost = shippingPrice,
                GrandToTal = cartItems.Sum(x => x.Price * x.Quantity) + shippingPrice

            };
            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }
        public async Task<IActionResult> Add(long Id)
        {
            ProductModel product = await _datacontext.Products.FindAsync(Id);
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
            CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItems == null)
            {
                cart.Add(cartItems = new CartItemModel(product));
            }
            else
            {
                cartItems.Quantity += 1;
            }
            HttpContext.Session.SetJson("Cart", cart);
            TempData["success"] = "Thêm Vào Giỏ Hàng Thành Công";
            return Json(new { success = true });
        }
        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItems.Quantity > 1)
            {
                --cartItems.Quantity;

            }
            else
            {
                cart.RemoveAll(c => c.ProductId == Id);
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Increase(int Id)
        {
            ProductModel product = await _datacontext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();
            if (cartItems.Quantity >= 1 && product.Quantity > cartItems.Quantity)
            {
                ++cartItems.Quantity;
                TempData["success"] = "Cập Nhật Giỏ Hàng Thành Công";
            }
            else
            {
                cartItems.Quantity = product.Quantity;
                TempData["error"] = "Số Lượng Hàng Trong Kho Không Đủ";
            }
            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            return RedirectToAction("Index");

        }
        public async Task<IActionResult> Remove(int Id)
        {
            List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
            cart.RemoveAll(c => c.ProductId == Id);
            if(cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }
            TempData["success"] = "Xóa Khỏi Giỏ Hàng Thành Công";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear(int Id)
        {
            HttpContext.Session.Remove("Cart");
            TempData["success"] = "Xóa Toàn Bộ Giỏ Hàng Thành Công";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Cart/GetShipping")]
        public async Task<IActionResult> GetShipping(ShippingModel shippingModel, string quan, string tinh, string phuong)
        {
            var existingShipping = await _datacontext.Shippings
                .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);

            decimal shippingPrice = 0; // Set mac dinh gia tien

            if (existingShipping != null)
            {
                shippingPrice = existingShipping.Price;
            }
            else
            {
                //Set mac dinh gia tien neu ko tim thay
                shippingPrice = 50000;
            }

            var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);

            try
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    Secure = true // using HTTPS
                };

                Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
            }
            catch (Exception ex)
            {
                //
                Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
            }

            return Json(new { shippingPrice });
        }
        // 1. Sửa thành [HttpPost]
        [HttpPost]
        // 2. XÓA BỎ [Route("...")] để nó nhận đúng tên hàm
        public IActionResult DeleteShipping()
        {
            Response.Cookies.Delete("ShippingPrice");

            // 3. Trả về JSON để AJAX biết đã thành công
            return Json(new { success = true });
        }


    }
}
