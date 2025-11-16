using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using WebTMDT_DACN.Areas.Admin.Repository;
using WebTMDT_DACN.Models;
using WebTMDT_DACN.Models.ViewModels;
using WebTMDT_DACN.Repository;

namespace WebTMDT_DACN.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly WebTMDT_DACN.Areas.Admin.Repository.IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(WebTMDT_DACN.Areas.Admin.Repository.IEmailSender emailSender, UserManager<AppUserModel> userManage,
            SignInManager<AppUserModel> signInManager, DataContext context)
        {
            _userManager = userManage;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _dataContext = context;

        }
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel {ReturnUrl = returnUrl});
        }
        public async Task<IActionResult> UpdateAccount()
        {
            if (!(bool)User.Identity.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account"); // Replace "Account" with your controll
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            //get user by user email

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userEmail = User.FindFirstValue(ClaimTypes.Email);
            //get user by user email

            var userById = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            else
            {
                
                // Hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);

                userById.PasswordHash = passwordHash;
                _dataContext.Update(userById);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin tài khoản thành công";
            }
            return RedirectToAction("UpdateAccount", "Account");

        }
        public IActionResult ForgetPass(string returnUrl)
        {
            return View();
        }
        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkuser = await _userManager.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                    TempData["success"] = "Đăng nhập thành công";

                    // 1. Dùng _userManager để tìm user bằng username đã đăng nhập
                    var user = await _userManager.FindByNameAsync(loginVM.Username);

                    // 2. Kiểm tra xem user có tồn tại và có email hay không
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        // 3. Lấy email từ đối tượng user
                        var receiver = user.Email;
                        var subject = "Đăng nhập thiết bị thành công";
                        var message = "Đăng nhập thành công, trải nghiệm dịch vụ nhé.";

                        // 4. Gửi email
                        await _emailSender.SendEmailAsync(receiver, subject, message);
                    }

                    return Redirect(loginVM.ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Sai tài khoản hặc mật khẩu");
            }
            return View(loginVM);
        }

        public IActionResult Create()
        {
            return View();
        }
        //public async Task<IActionResult> History()
        //{
        //    if (!(bool)User.Identity.IsAuthenticated)
        //    {
        //        // User is not logged in, redirect to login
        //        return RedirectToAction("Login", "Account"); // Replace "Account" with your controller name
        //    }

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var userEmail = User.FindFirstValue(ClaimTypes.Email);

        //    var Orders = await _dataContext.Orders
        //        .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
        //    ViewBag.UserEmail = userEmail;
        //    return View(Orders);
        //}
        public async Task<IActionResult> History(int pg = 1)
        {
            if (!(bool)User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            ViewBag.UserEmail = userEmail; // Dòng này của bạn đã đúng

            // 1. Tạo truy vấn cơ sở (chưa chạy)
            var ordersQuery = _dataContext.Orders
                .Where(od => od.UserName == userEmail)
                .OrderByDescending(od => od.Id);

            // === BỔ SUNG LOGIC PHÂN TRANG ===
            const int pageSize = 10; // Đặt số lượng đơn hàng mỗi trang

            // Đếm tổng số đơn hàng
            int recsCount = await ordersQuery.CountAsync();

            // Khởi tạo Pager
            var pager = new Paginate(recsCount, pg, pageSize);

            // Tính số lượng bản ghi cần bỏ qua
            int recSkip = (pg - 1) * pageSize;

            // Lấy đúng dữ liệu cho trang hiện tại
            var data = await ordersQuery.Skip(recSkip).Take(pager.PageSize).ToListAsync();

            // 4. GỬI PAGER SANG VIEW (Đây là phần bị thiếu)
            this.ViewBag.paper = pager;
            // === KẾT THÚC BỔ SUNG ===

            // 5. Trả về danh sách đã được phân trang
            return View(data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel
                {
                    UserName = user.Username,
                    Email = user.Email,
                };
                IdentityResult result = await _userManager.CreateAsync(newUser,user.Password);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Đăng ký thành công";
                    return Redirect("/account/login");
                }
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if (!(bool)User.Identity.IsAuthenticated)
            {
                // User is not logged in, redirect to login
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).FirstAsync();
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while canceling the order.");
            }

            return RedirectToAction("History", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgetPass", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                //update token to user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();

                var receiver = checkMail.Email;
                var subject = "Change password for user " + checkMail.Email;
                var message = "Click on link to change password " +
                    "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "&token=" + token +
                    "'>Click on link to change password</a>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "An email has been sent to your registered email address with password reset instructions.";
            return RedirectToAction("ForgetPass", "Account");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var checkuser = await _userManager.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if (checkuser != null)
            {
                //update user with new password and token
                string newtoken = Guid.NewGuid().ToString();
                // Hash the new password
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(checkuser, user.PasswordHash);

                checkuser.PasswordHash = passwordHash;
                checkuser.Token = newtoken;

                await _userManager.UpdateAsync(checkuser);
                TempData["success"] = "Password updated successfully.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgotPass", "Account");
            }
            return View();
        }

    }
}
