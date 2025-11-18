using Microsoft.AspNetCore.Mvc;
using WebTMDT_DACN.Models.Vnpay;
using WebTMDT_DACN.Services.Vnpay;

namespace WebTMDT_DACN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }

        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        

    }
}
