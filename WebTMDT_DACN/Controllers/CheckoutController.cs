using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WebTMDT_DACN.Areas.Admin.Repository;
using WebTMDT_DACN.Migrations;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Repository;
using WebTMDT_DACN.Services.Vnpay;

namespace WebTMDT_DACN.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _datacontext;
        private readonly IEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;
        
        public CheckoutController(IEmailSender emailSender , DataContext context , IVnPayService vnPayService)
        {
            _datacontext = context;
            _emailSender = emailSender;
            _vnPayService = vnPayService;
        }
        public async Task<IActionResult> Checkout(string PaymentMethod , string PaymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if(userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.OrderCode = ordercode;
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }else                 {
                    shippingPrice = 0;
                }
                orderItem.ShippingCost = shippingPrice;
                orderItem.UserName = userEmail;
                if(PaymentMethod == "COD")
                {
                    orderItem.PaymentMethod = PaymentMethod;
                }else if(PaymentMethod == "Vnpay")
                {
                    orderItem.PaymentMethod = "Vnpay" + PaymentId;
                }else
                {
                    orderItem.PaymentMethod = "Other";
                }

                    orderItem.Status = 1;
                orderItem.CreatedDate = DateTime.Now;
                _datacontext.Add(orderItem);
                _datacontext.SaveChanges();
                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderdetails = new OrderDetails();
                    orderdetails.UserName = userEmail;
                    orderdetails.OrderCode = ordercode;
                    orderdetails.ProductId = cart.ProductId;
                    orderdetails.Price = cart.Price;
                    orderdetails.Quantity = cart.Quantity;
                    var product = await _datacontext.Products.Where(p => p.Id == cart.ProductId).FirstOrDefaultAsync();
                    product.Quantity -= cart.Quantity;
                    product.Soldout += cart.Quantity;
                    _datacontext.Update(product);
                    _datacontext.Add(orderdetails);
                    _datacontext.SaveChanges();

                }
                HttpContext.Session.Remove("Cart");
                var receiver = userEmail;
                var subject = "Đặt hàng thành công thành công.";
                var message = "Đặt hàng thành công, trải nghiệm dịch vụ nhé.";

                await _emailSender.SendEmailAsync(receiver, subject, message);
                TempData["success"] = "Đặt Hàng Thành Công , Vui Lòng Chờ Duyệt Đơn Hàng";
                return RedirectToAction("History", "Account");
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VnPayResponseCode == "00") //giao dịch thành công lưu db
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now
                };

                _datacontext.Add(newVnpayInsert);
                await _datacontext.SaveChangesAsync();
                //Tiến hành đặt đơn hàng khi thanh toán momo thành công
                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["success"] = "Giao dịch Vnpay thành công.";
                return RedirectToAction("Index", "Cart");
            }
            //return Json(response);
            return View(response);
        }

    }
}
